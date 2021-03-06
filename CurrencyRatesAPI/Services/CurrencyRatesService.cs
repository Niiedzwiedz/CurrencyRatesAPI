using CurrencyRatesAPI.Data;
using CurrencyRatesAPI.Entities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace CurrencyRatesAPI.Services
{
    public class CurrencyRatesService : ICurrencyRatesService
    {
        private readonly RatesDbContext _dbContext;
        private static string _apiKey;

        public CurrencyRatesService()
        {
            _dbContext = new RatesDbContext();
        }

        #region Generate_API_Key
        public ActionResult<string> GetApiKey(LoginData data)
        {
            //Find username
            var user = _dbContext.AuthorizationTable.Find(data.Username);
            if (user is null) return null;

            //Encode
            byte[] encodedPassword = new UTF8Encoding().GetBytes(data.Password);
            byte[] hash = ((HashAlgorithm)CryptoConfig.CreateFromName("MD5")).ComputeHash(encodedPassword);
            string encoded = BitConverter.ToString(hash)
               .Replace("-", string.Empty)
               .ToLower();

            //Authorization
            if (user.Password != encoded) return null;

            //Generate Key
            var key = new byte[128];
            using (var generator = RandomNumberGenerator.Create())
            {
                generator.GetBytes(key);
            }
            string apiKey = Convert.ToBase64String(key);

            _apiKey = apiKey;

            //set new Api Key
            user.APIKey = apiKey;
            _dbContext.AuthorizationTable.Update(user);
            _dbContext.SaveChanges();

            return _apiKey;
        }
        #endregion

        #region Get_by_Parameters
        public ActionResult<List<CurrencyRate>> GetRates(SearchParameters parameters)
        {
            var user =_dbContext.AuthorizationTable.FirstOrDefault<User>(v => v.APIKey == parameters.ApiKey);
            if (user is null) return null;

            if (_apiKey != parameters.ApiKey) return null;
            if (parameters.EndDate > DateTime.Today) return null;

            List<CurrencyRate> results = new List<CurrencyRate>();

            foreach (KeyValuePair<string, string> v in parameters.CurrencyCodes)
            {
                CurrencyRate temp = new CurrencyRate()
                {
                    From = v.Key,
                    To = v.Value,
                    Rates = new Dictionary<string, double>()
                };

                for (var currentDate = parameters.StartDate; currentDate <= parameters.EndDate; currentDate = currentDate.AddDays(1))
                {
                    //Console.WriteLine(currentDate);

                    DailyRate key;
                    DailyRate val;

                    if (v.Key == "EUR")
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

                    temp.Rates.Add(
                        currentDate.ToString("yyyy-MM-dd"),
                        ((val.CuerrencyCode == "EUR") ? 1.0 : val.Rate) / ((key.CuerrencyCode == "EUR") ? 1.0 : key.Rate)
                        );
                }

                if (temp.Rates.Count > 0)
                {
                    results.Add(temp);
                }
            }

            while(results.Count == 0)
            {
                var newParameters = parameters;
                newParameters.StartDate = (parameters.StartDate.AddDays(-1));
                newParameters.EndDate = newParameters.StartDate;
                results = GetRates(parameters).Value;
            }

            return results;
        }
        #endregion

        public DailyRate GetFromAPI(string currencyCode, DateTime dateTime)
        {

            //Assembly "Querry"
            int i = 0;
            var querryString = $"https://sdw-wsrest.ecb.europa.eu/service/data/EXR/D";
            querryString += $".{currencyCode}.EUR.SP00.A?startPeriod={dateTime.ToString("yyyy-MM-dd")}&endPeriod={dateTime.ToString("yyyy-MM-dd")}";
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
    }
}
