using CurrencyRatesAPI.Data;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

namespace CurrencyRatesAPI.Controllers
{
    [Route("api/currencyRates")]
    public class CurrencyRatesController : ControllerBase
    {
        [HttpGet("parameters")]
        public ActionResult<List<CurrencyRate>> GetAll([FromBody] SearchParameters parameters)
        {
            //Test String
            //https://sdw-wsrest.ecb.europa.eu/service/data/EXR/D.USD.EUR.SP00.A?startPeriod=2021-03-01&endPeriod=2021-03-05
            List<CurrencyRate> rates = new List<CurrencyRate>();

            //Assembly "Querry"
            int i = 0;
            var querryString = $"https://sdw-wsrest.ecb.europa.eu/service/data/EXR/D";
            foreach (KeyValuePair<string, string> v in parameters.CurrencyCodes)
            {
                if(++i == 1) querryString += $".{v.Key}";
                else querryString += $"+{v.Key}";
            }
            querryString+= $".EUR.SP00.A?startPeriod={parameters.StartDate.ToString("yyyy-MM-dd")}&endPeriod={parameters.EndDate.ToString("yyyy-MM-dd")}";
            //Console.WriteLine(querryString);

            var doc = new XmlDocument();
            XmlNamespaceManager nsmgr;
            XmlNodeList nodes;
            try
            {
                doc.Load(querryString);

                nsmgr = new XmlNamespaceManager(doc.NameTable);
                nsmgr.AddNamespace("message", "http://www.sdmx.org/resources/sdmxml/schemas/v2_1/message");
                nsmgr.AddNamespace("generic", "http://www.sdmx.org/resources/sdmxml/schemas/v2_1/data/generic");

                //Get series
                nodes = doc.DocumentElement.SelectNodes("//message:GenericData/message:DataSet/generic:Series", nsmgr);
            }
            catch
            {
                return BadRequest();
            }

            foreach (XmlNode node in nodes)
            {
                string from = "";
                string to = "";

                //Get FROM and TO codes
                foreach (XmlNode keys in node.SelectNodes("generic:SeriesKey/generic:Value", nsmgr))
                {
                    if (keys.Attributes["id"].Value == "CURRENCY") 
                        to = keys.Attributes["value"].Value;
                    if (keys.Attributes["id"].Value == "CURRENCY_DENOM") 
                        from = keys.Attributes["value"].Value;
                }

                foreach (XmlNode dailesNode in node.SelectNodes("generic:Obs", nsmgr))
                {
                    DateTime date = new DateTime();
                    double value = 0.0;

                    //Get Date
                    var dateNode = dailesNode.SelectSingleNode("generic:ObsDimension", nsmgr);
                    if (dateNode != null)
                    {
                        date = DateTime.ParseExact(
                            dateNode.Attributes["value"].Value, 
                            "yyyy-MM-dd", 
                            CultureInfo.InvariantCulture
                            );
                    }

                    //Get Rate
                    var rateNode = dailesNode.SelectSingleNode("generic:ObsValue", nsmgr);
                    if (rateNode != null)
                    {
                        //if Rate Record == NaN skip
                        if (
                            rateNode.Attributes["value"].Value == null || 
                            rateNode.Attributes["value"].Value.ToString() == "NaN"
                            ) continue;

                        value = (double)Decimal.Parse
                            (
                            rateNode.Attributes["value"].Value, 
                            NumberStyles.Float, 
                            new CultureInfo("en-Us")
                            );
                    }

                    //Fine Check
                    if (
                        value == 0.0            || 
                        date == new DateTime()  || 
                        from.Length == 0        || 
                        to.Length == 0
                        ) continue;

                    //Save
                    var rate = new CurrencyRate()
                    {
                        From = from,
                        To = to,
                        Value = value,
                        Date = date
                    };

                    rates.Add(rate);
                }
            }

            return Ok(rates);
        }
    }
}
