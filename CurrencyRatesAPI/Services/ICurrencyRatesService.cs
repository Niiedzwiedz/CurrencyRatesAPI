using CurrencyRatesAPI.Data;
using CurrencyRatesAPI.Entities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace CurrencyRatesAPI.Services
{
    public interface ICurrencyRatesService
    {
        ActionResult<string> GetApiKey(LoginData data);
        ActionResult<List<CurrencyRate>> GetRates(SearchParameters parameters);
    }
}