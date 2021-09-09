using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CurrencyRatesAPI.Entities
{
    public class User
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string APIKey { get; set; }
    }
}
