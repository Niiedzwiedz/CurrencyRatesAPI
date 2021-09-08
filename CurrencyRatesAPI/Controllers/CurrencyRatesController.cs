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
        [HttpGet("test")]
        public ActionResult<List<CurrencyRate>> GetAll()
        {
            //Test String
            //https://sdw-wsrest.ecb.europa.eu/service/data/EXR/D.USD.EUR.SP00.A?startPeriod=2021-03-01&endPeriod=2021-03-05

            List<CurrencyRate> rates = new List<CurrencyRate>();
            //Dummy Query
            var querryString = "https://sdw-wsrest.ecb.europa.eu/service/data/EXR/D.USD.EUR.SP00.A?startPeriod=2009-05-01&endPeriod=2009-05-31";

            var doc = new XmlDocument();
            doc.Load(querryString);
            var nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("message", "http://www.sdmx.org/resources/sdmxml/schemas/v2_1/message");
            nsmgr.AddNamespace("generic", "http://www.sdmx.org/resources/sdmxml/schemas/v2_1/data/generic");
            XmlNodeList nodes = doc.DocumentElement.SelectNodes("//message:GenericData/message:DataSet/generic:Series/generic:SeriesKey/generic:Value", nsmgr);

            string from = "???";
            string to = "???";
            if (nodes != null)
            {
                foreach (XmlNode node in nodes)
                {
                    if (node.Attributes["id"].Value == "CURRENCY") to = node.Attributes["value"].Value;
                    if (node.Attributes["id"].Value == "CURRENCY_DENOM") from = node.Attributes["value"].Value;
                }
            }

            nodes = doc.DocumentElement.SelectNodes("//message:GenericData/message:DataSet/generic:Series/generic:Obs", nsmgr);
            if (nodes != null)
            {
                foreach (XmlNode node in nodes)
                {

                    DateTime date = new DateTime(1970, 01, 01);
                    double value = 0.0;

                    foreach (XmlNode subnode in node.ChildNodes)
                    {
                        if (subnode.Name == "generic:ObsDimension")
                            date = DateTime.ParseExact(subnode.Attributes["value"].Value, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                        if (subnode.Name == "generic:ObsValue")
                        {
                            if (subnode.Attributes["value"].Value == null || subnode.Attributes["value"].Value.ToString() == "NaN") break;
                            value = (double)Decimal.Parse(subnode.Attributes["value"].Value, NumberStyles.Float, new CultureInfo("en-Us"));
                            break;
                        }
                    }

                    if (value == 0.0) continue;
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
