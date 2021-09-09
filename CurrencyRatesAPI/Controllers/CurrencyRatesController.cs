using CurrencyRatesAPI.Data;
using CurrencyRatesAPI.Entities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;

namespace CurrencyRatesAPI.Controllers
{
    [Route("api/currencyRates")]
    public class CurrencyRatesController : ControllerBase
    {
        private readonly RatesDbContext _dbContext;

        public CurrencyRatesController()
        {
            _dbContext = new RatesDbContext();
        }


        #region Get_by_Parameters
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
                if (++i == 1) querryString += $".{v.Key}";
                else querryString += $"+{v.Key}";
            }
            querryString += $".EUR.SP00.A?startPeriod={parameters.StartDate.ToString("yyyy-MM-dd")}&endPeriod={parameters.EndDate.ToString("yyyy-MM-dd")}";
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
                        value == 0.0 ||
                        date == new DateTime() ||
                        from.Length == 0 ||
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
        #endregion

        #region DbTests

        [HttpGet("dbtest")]
        public ActionResult<List<CurrencyRate>> GetDbAll()
        {
            var dailyRates = _dbContext.DailyRates.ToList();

            List<CurrencyRate> result = new List<CurrencyRate>();
            foreach (DailyRate v in dailyRates)
            {
                var rate = new CurrencyRate()
                {
                    From = "EUR",
                    To = v.CuerrencyCode,
                    Value = v.Rate,
                    Date = v.Date
                };
                result.Add(rate);
            }

            return Ok(result);
        }

        [HttpGet("dbtest/find")]
        public ActionResult<CurrencyRate> GetDbAll([FromQuery]string currency, [FromQuery] DateTime date)
        {

            var rate = _dbContext
                .DailyRates
                .FirstOrDefault<DailyRate>(r => r.CuerrencyCode == currency && r.Date == date) ;

            if (rate is null) return NotFound();

            CurrencyRate result = new CurrencyRate()
            {
                From = "EUR",
                To = rate.CuerrencyCode,
                Value = rate.Rate,
                Date = rate.Date
            };

            return Ok(result);
        }

        [HttpPost("dbtest/add")]
        public ActionResult AddToDb([FromQuery] string currency, [FromQuery] DateTime date, [FromQuery] double rate)
        {
            var check = _dbContext
                .DailyRates
                .FirstOrDefault<DailyRate>(r => r.CuerrencyCode == currency && r.Date == date);

            if (check != null) return BadRequest();

            _dbContext.DailyRates.Add(
                new DailyRate()
                {
                    CuerrencyCode = currency,
                    Date = date,
                    Rate = rate
                }
            );

            _dbContext.SaveChanges();
            return Ok();
        }

        #endregion
    }
}
