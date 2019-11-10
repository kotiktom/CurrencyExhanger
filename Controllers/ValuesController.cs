using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace CurrencyApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        
        public Rates.Rootobject GetCurrentRates(String resultAs)
        // Fetching the current exchange rates for the given currency. Parameter changed toUppser() in case of non-capital letters in the parameter.
        {
            HttpWebRequest WebReq = (HttpWebRequest)WebRequest.Create(string.Format("https://api.exchangeratesapi.io/latest?base=" + resultAs.ToUpper()));

            WebReq.Method = "GET";

            HttpWebResponse WebResp = (HttpWebResponse)WebReq.GetResponse();

            string jsonString;
            using (Stream stream = WebResp.GetResponseStream())
            {
                StreamReader reader = new StreamReader(stream, System.Text.Encoding.UTF8);
                jsonString = reader.ReadToEnd();
            }

            var currencies = JsonConvert.DeserializeObject<Rates.Rootobject>(jsonString);
            // Deserialize the JSON to a variable and return it

            return currencies;
        }

        public class Payload
        // Return object for the GET request
        {
            public double Amount { get; set; }
            public string Currency { get; set; }
        }

        
        [HttpGet]
        public Payload Add(String[] id, String resultAs)
        // GET Request example: https://localhost:44318/api/values?id=10USD&id=50EUR&resultAs=SEK
        // Parameter id is returned as list and iterated throught.
        {

            double total = 0;

            for (int i = 0; i < id.Length; i++ )
            {
                string original = id[i].ToUpper();
                
                string withoutNumbers = Regex.Replace(original, "[0-9]", "");
                // Remove numbers from the parameter. Used for input currency
                string withoutLetters = Regex.Replace(original, "([a-zA-Z,_ ]+|(?<=[a-zA-Z ])[/-])", "");
                // Remove letters from the parameter. Used as input amount

                Rates.Rates Currencies = new Rates.Rates();
                Currencies = GetCurrentRates(resultAs).rates;
                // Call the fetch function and set the return value to a local object

                var CurrentRate = Currencies.GetType().GetProperty(withoutNumbers).GetValue(Currencies);
                // Get current rate for the current given parameter in the list. In this case it would be USD
                
                Console.WriteLine(CurrentRate + " Current rate as " + withoutNumbers + " to " + resultAs);
                Console.WriteLine("Given amount " + withoutLetters);
                
                double rate = Convert.ToDouble(CurrentRate);
                double amount = Convert.ToDouble(withoutLetters);
                // Convert to double


                // Counting the given amounts again their exhange rates
                // Console writelines for testing/showing amounts, rates and totals. 
                if (rate > 1)
                {
                    total = total + (amount / rate);
                    Console.WriteLine("Exhange rate is " + rate);
                    Console.WriteLine("Current total: " + total);
                    Console.WriteLine("");
                }

                else if  (rate < 1 && rate != 0 )
                {
                    total = total + (amount / rate);
                    Console.WriteLine("Exhange rate is " + rate);
                    Console.WriteLine("Current total: " + total);
                    Console.WriteLine("");
                }

                // In case of one of the parameters is of the same currency as resultAs. The exchangeratesapi doesnt return a value for that currency so
                // the currency exhange rate is set to 1.
                else if (rate == 0 || rate == 1)
                {
                    total = total + amount;
                    Console.WriteLine("rate set to 1");
                    Console.WriteLine("Current total: " + total);
                    Console.WriteLine("");
                }           
            }

            var model = new Payload
            {
                Amount = Math.Round(total, 2),
                Currency = resultAs
            };
            return model;
        }

    }
}