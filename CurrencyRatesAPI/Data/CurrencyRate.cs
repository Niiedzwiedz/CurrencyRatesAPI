using System;
using System.Collections.Generic;

namespace CurrencyRatesAPI.Data
{
    public class CurrencyRate
    {

        public string From { get; set; }

        public string To { get; set; }

        public Dictionary<string, double> Rates { get; set; }
    }
}
