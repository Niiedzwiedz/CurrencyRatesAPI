using System;

namespace CurrencyRatesAPI.Data
{
    public class CurrencyRate
    {
        public DateTime Date { get; set; }

        public string From { get; set; }

        public string To { get; set; }

        public double Value { get; set; }
    }
}
