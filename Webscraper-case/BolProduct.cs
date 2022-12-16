using OpenQA.Selenium.DevTools.V105.Emulation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webscraper_case
{
    public class BolProduct
    {
        public string ProductName { get; set; }
        public string Brand { get; set; }
        public double Price { get; set; }
        public string Rating { get; set; }

        public BolProduct(string productName, string brand, double price, string rating)
        {
            this.ProductName = productName;
            this.Brand = brand;
            this.Price = price;
            this.Rating = rating;
        }
    }
}
