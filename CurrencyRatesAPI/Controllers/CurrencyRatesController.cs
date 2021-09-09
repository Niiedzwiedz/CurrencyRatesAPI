using CurrencyRatesAPI.Data;
using CurrencyRatesAPI.Entities;
using CurrencyRatesAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Xml;

namespace CurrencyRatesAPI.Controllers
{
    [ApiController]
    [Route("api/currencyRates")]
    public class CurrencyRatesController : ControllerBase
    {
        private readonly ICurrencyRatesService _service;
        private readonly ILogger<CurrencyRatesService> _logger;
        private static string _apiKey;

        public CurrencyRatesController(ILogger<CurrencyRatesService> logger, ICurrencyRatesService service)
        {
            _logger = logger;
            _service = service;
        }

        #region Generate_API_Key
        [HttpGet("apiKey")]
        public ActionResult<string> GetApiKey()
        {
            var result = _service.GetApiKey();
            if (result is null) return NotFound();
            else
            {
                _apiKey = result.Value;
                return Ok(result.Value);
            }
        }
        #endregion

        #region Get_by_Parameters
        [HttpGet("parameters")]
        public ActionResult<List<CurrencyRate>> GetRates([FromBody] SearchParameters parameters)
        {
            if (_apiKey != parameters.ApiKey) return Unauthorized();
            if (parameters.EndDate > DateTime.Today) return NotFound();
            if (parameters.EndDate < parameters.StartDate) return NotFound();

            var result = _service.GetRates(parameters);
            if (result is null) return NotFound();
            else return Ok(result.Value);
        }
        #endregion
    }
}
