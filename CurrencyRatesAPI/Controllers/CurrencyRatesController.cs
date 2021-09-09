using CurrencyRatesAPI.Data;
using CurrencyRatesAPI.Entities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Xml;

namespace CurrencyRatesAPI.Controllers
{
    [Route("api/currencyRates")]
    public class CurrencyRatesController : ControllerBase
    {
        private readonly RatesDbContext _dbContext;
        private static string _apiKey;

        public CurrencyRatesController()
        {
            _dbContext = new RatesDbContext();
        }

        #region Generate_API_Key
        [HttpGet("apiKey")]
        public ActionResult<string> GetApiKey()
        {
            var key = new byte[128];
            using (var generator = RandomNumberGenerator.Create())
            {
                generator.GetBytes(key);
            }
            string apiKey = Convert.ToBase64String(key);

            _apiKey = apiKey;

            return Ok(apiKey);
        }
        #endregion

        #region Get_by_Parameters
        [HttpGet("parameters")]
        public ActionResult<List<CurrencyRate>> GetRates([FromBody] SearchParameters parameters)
        {
            if (_apiKey != parameters.ApiKey) return Unauthorized();
            if (parameters.EndDate > DateTime.Today) return NotFound();

            List<CurrencyRate> results = new List<CurrencyRate>();

            foreach(KeyValuePair<string,string> v in parameters.CurrencyCodes)
            {
                for(var currentDate = parameters.StartDate; currentDate <= parameters.EndDate; currentDate = currentDate.AddDays(1))
                {
                    Console.WriteLine(currentDate);

                    DailyRate key;
                    DailyRate val;

                    if(v.Key == "EUR")
                    {
                        key = new DailyRate()
                        {
                            CuerrencyCode = "EUR",
                            Date = currentDate,
                            Rate = 1.0
                        };
                    }
                    else 
                    {
                        key = _dbContext
                        .DailyRates
                        .FirstOrDefault<DailyRate>(r => r.CuerrencyCode == v.Key && r.Date == currentDate);

                        if (key is null)
                        {
                            //GET FROM API
                            key = GetFromAPI(v.Key, currentDate);
                            //SAVE in DB
                            if (key != null) _dbContext.DailyRates.Add(key);
                        };
                    }

                    if (v.Value == "EUR")
                    {
                        val = new DailyRate()
                        {
                            CuerrencyCode = "EUR",
                            Date = currentDate,
                            Rate = 1.0
                        };
                    }
                    else
                    {
                        val = _dbContext
                        .DailyRates
                        .FirstOrDefault<DailyRate>(r => r.CuerrencyCode == v.Value && r.Date == currentDate);

                        if (val is null)
                        {
                            //GET FROM API
                            val = GetFromAPI(v.Value, currentDate);
                            //SAVE in DB
                            if (val != null) _dbContext.DailyRates.Add(val);
                        };
                    }

                    if (val == null || key == null)
                    {
                        continue;
                    }

                    _dbContext.SaveChanges();

                    results.Add(
                        new CurrencyRate()
                        {
                            From = key.CuerrencyCode,
                            To = val.CuerrencyCode,
                            Date = currentDate,
                            Value = (
                                ((val.CuerrencyCode == "EUR") ? 1.0 : val.Rate) / ((key.CuerrencyCode == "EUR") ? 1.0 : key.Rate)
                            )
                        }    
                    );

                }
            }

            return Ok(results);
        }
        #endregion

        public DailyRate GetFromAPI(string currencyCode, DateTime dateTime)
        {

            //Assembly "Querry"
            int i = 0;
            var querryString = $"https://sdw-wsrest.ecb.europa.eu/service/data/EXR/D";
            querryString += $".{currencyCode}.EUR.SP00.A?startPeriod={dateTime.ToString("yyyy-MM-dd")}&endPeriod={dateTime.ToString("yyyy-MM-dd")}";
            Console.WriteLine(querryString);

            var doc = new XmlDocument();
            XmlNamespaceManager nsmgr;
            XmlNodeList nodes;

            try
            {
                doc.Load(querryString);
                nsmgr = new XmlNamespaceManager(doc.NameTable);
                nsmgr.AddNamespace("message", "http://www.sdmx.org/resources/sdmxml/schemas/v2_1/message");
                nsmgr.AddNamespace("generic", "http://www.sdmx.org/resources/sdmxml/schemas/v2_1/data/generic");
            }
            catch
            {
                return null;
            }

            DateTime date = new DateTime();
            double value = 0.0;

            string begin = "//message:GenericData/message:DataSet/generic:Series/generic:Obs";

            var dateNode = doc.DocumentElement.SelectSingleNode(begin + "/generic:ObsDimension", nsmgr);
            if (dateNode == null) return null;
            date = DateTime.ParseExact(
                dateNode.Attributes["value"].Value,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture
                );

            var rateNode = doc.DocumentElement.SelectSingleNode(begin + "/generic:ObsValue", nsmgr);
            if (rateNode == null) return null;

            if (
                rateNode.Attributes["value"].Value == null ||
                rateNode.Attributes["value"].Value.ToString() == "NaN"
                ) return null;

            value = (double)Decimal.Parse
                (
                rateNode.Attributes["value"].Value,
                NumberStyles.Float,
                new CultureInfo("en-Us")
                );

            //Fine Check
            if (
                value == 0.0 ||
                date == new DateTime()
                ) return null;

            return new DailyRate()
            {
                CuerrencyCode = currencyCode,
                Rate = value,
                Date = date
            };
        }

        #region DbTests

        [HttpGet("dbtest")]
        public ActionResult<List<CurrencyRate>> GetDbAll()
        {
            //_dbContext.DailyRates.RemoveRange(_dbContext.DailyRates);
            //_dbContext.SaveChanges();

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
