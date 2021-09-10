# CurrencyRatesAPI

#### It is Web API providing currencies rates in the context of a date.

###### The API recives in Body GET parameters that uses to search data from European Central Bank's (ECP) API called "ECB SDMX 2.1 RESTful web service" documented on https://sdw-wsrest.ecb.europa.eu/help/. Before user will use it, he needs to provide API key, that must by generated and returned by this API during the log in process. API needs default configured SQL Express. To minimalize waiting time all recived data are stored in SQL Express Database. 
###### Example of provided parameters:

```
{
    "CurrencyCodes" : 
    {
        "USD":"JPY",
        "GBP":"EUR"
    },
    "StartDate" : "2021-04-30",
    "EndDate"   : "2021-05-03",
    "ApiKey"    : "QnZK3XcGlOXZFDJbGZibG85GoxYdeE"
}
```

###### Result(JSON) contains array of data blocks built of two currency codes: source and target and Dictionary that contains date as a key and calculated ratio as value that is related to date.
###### Example of recived parameters:

```
[
    {
        "from": "USD",
        "to": "JPY",
        "rates": {
            "2021-04-30": 108.93891739778184,
            "2021-05-03": 109.51511125871805
        }
    },
    {
        "from": "GBP",
        "to": "EUR",
        "rates": {
            "2021-04-30": 1.1512381566374636,
            "2021-05-03": 1.1515695893502844
        }
    }
]
```
