using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CurrencyRatesAPI.Entities
{
    public class DailyRate
    {
        public string CuerrencyCode { get; set; }
        public DateTime Date { get; set; }
        public double Rate { get; set; } //Relative to EUR
    }
}
