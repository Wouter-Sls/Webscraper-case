using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Threading;
using OpenQA.Selenium.Safari;
using System.Collections.Generic;
using System.Web;
using OpenQA.Selenium.DevTools.V105.Runtime;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Channels;
using System.Xml.Linq;
using System.Diagnostics;
using Webscraper_case;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace Webscraper
{
    class Scraper
    {
        //global variables
        static int count = 0;
        static string choice = "1";

        public static void Main(string[] args)
        {
            while (!choice.Equals("0"))
            {
                GetInput();
                StartScrapMethod();
            }
        }

        //get scrape option from user
        static void GetInput()
        {
            Console.WriteLine("\n\n=================================");
            Console.WriteLine("   WEBSITE SCRAPER APPLICATION   ");
            Console.WriteLine("=================================\n\n");

            Console.WriteLine("0: Stop application\n1: Youtube video's\n2: Jobsite\n3: Bol.com");
            Console.Write("Choose which website to scrape (0, 1, 2 or 3): ");
            choice = Console.ReadLine(); 
            //didn't parse to int on purpose to make it easier to identify wrong input. All input other then 0,1,2,3 is wrong. See startScrapeMethod.
        }

        //start scrape option
        static void StartScrapMethod()
        {
            //start method based on choice
            switch (choice)
            {
                case "0": Console.WriteLine("Aplication has been succesfully stopped."); return; break;
                case "1": GenerateYTVideos(); break;
                case "2": GenerateJobs(); break;
                case "3": GenerateBolproducts(); break;
                default: Console.WriteLine("\n" + choice + " -> is no option. Please type a correct option."); GetInput(); break;
            }
        }

        //scrape methods
        static void GenerateYTVideos()
        {
            //give search term
            Console.Write("Give a Youtube search term: ");
            string searchTerm = Console.ReadLine();

            //navigate to youtube
            IWebDriver driver = new ChromeDriver();
            driver.Navigate().GoToUrl("https://www.youtube.com/");

            //wait till page is refreshed
            Thread.Sleep(2000);

            //accept cookies
            var cookieButton = driver.FindElement(By.XPath("//*[@id=\"content\"]/div[2]/div[6]/div[1]/ytd-button-renderer[2]/yt-button-shape/button"));
            cookieButton.Click();

            //wait till page is refreshed
            Thread.Sleep(2000);

            //search on search term
            var search = driver.FindElement(By.XPath("//input[@id=\"search\"]"));
            search.SendKeys(searchTerm);
            search.Submit();

            //wait till page is refreshed
            Thread.Sleep(2000);

            //select the first 5 videos
            var videos = driver.FindElements(By.XPath("//*[@id=\"dismissible\"]")).Take(5);

            //create list
            List<YoutubeVideo> listYoutubeVideos = new List<YoutubeVideo>();

            //for each video get URL, title, views and uploader
            foreach (var video in videos)
            {
                //get details
                var videoURL = video.FindElement(By.XPath(".//*[@id=\"thumbnail\"]")).GetAttribute("href");
                var title = video.FindElement(By.XPath(".//*[@id=\"video-title\"]/yt-formatted-string")).Text;
                var views = video.FindElement(By.XPath(".//*[@id=\"metadata-line\"]/span[1]")).Text;
                var channel = video.FindElement(By.XPath(".//*[@id=\"channel-name\"]"));
                var uploader = channel.FindElement(By.XPath(".//*[@id=\"text\"]/a")).GetAttribute("innerText");

                //declaration variabel
                string viewsAmount = "";

                //filter views to just numbers
                switch (views)
                {
                    case string a when a.Contains('K'): //when video < 1mln views
                        if (a.Contains(','))
                        {
                            viewsAmount = Regex.Replace(a, @"K.*", "00");
                            viewsAmount = viewsAmount.Replace(",", "");
                        }
                        else
                        {
                            viewsAmount = Regex.Replace(a, @"K.*", "000");
                        }
                        break;
                    case string b when b.Contains("mln."): //when video +mln views
                        if (b.Contains(','))
                        {
                            viewsAmount = Regex.Replace(b, @" mln.*", "00000");
                            viewsAmount = viewsAmount.Replace(",", "");
                        }
                        else
                        {
                            viewsAmount = Regex.Replace(b, @" mln.*", "000000");
                        }
                        break;
                    default:
                        viewsAmount = Regex.Replace(views, @"\s.*", ""); //when views > 1000

                        if (!viewsAmount.All(char.IsDigit))
                        { //when views is "Geen"
                            viewsAmount = "0";
                        }
                        break;
                }

                //add video to list
                listYoutubeVideos.Add(new YoutubeVideo(videoURL.ToString(), title, Convert.ToInt32(viewsAmount), uploader));
            }

            //to display special characters
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            //order on views
            var orderedList = listYoutubeVideos.OrderBy(v => v.Views);

            //write data to json file
            var fileName = "../../../../jsonFiles/YoutubeVideos.json";
            JsonFileUtils.SimpleWrite(orderedList, fileName);

            Console.WriteLine("\n\n================================================");
            Console.WriteLine("   YOUTUBE VIDEO'S ORDERED BY AMOUNT OF VIEWS   ");
            Console.WriteLine("================================================\n\n");

            //display video details
            foreach (var video in orderedList)
            {
                count++;
                Console.WriteLine("YOUTUBE VIDEO DETAILS {0}:", count);
                Console.WriteLine("URL: {0}", video.Url);
                Console.WriteLine("TITLE: {0}", video.Title);
                Console.WriteLine("VIEWS: {0}", video.Views);
                Console.WriteLine("CHANNEL: {0}", video.ChannelName);
                Console.WriteLine("\n");
            }

            //Quit driver after session
            driver.Quit();
        }

        static void GenerateJobs()
        {
            //give search term
            Console.Write("Give a job search term: ");
            string searchTerm = Console.ReadLine();

            //navigate to jobsite
            IWebDriver driver = new ChromeDriver();
            driver.Navigate().GoToUrl("https://www.ictjob.be/");

            //wait till page is refreshed
            Thread.Sleep(2000);

            //search on keyword
            var search = driver.FindElement(By.XPath("//*[@id=\"keywords-input\"]"));
            search.SendKeys(searchTerm);
            search.Submit();

            //wait till page is refreshed
            Thread.Sleep(2000);
            try
            {
                //get results
                var results = driver.FindElement(By.XPath("//*[@id=\"search-result-body\"]"));

                //take first 5 vacancies
                var vacancies = results.FindElements(By.XPath(".//li[contains(@class, 'search-item  clearfix')]")).Take(5);


                List<JobsiteVacancy> vacanciesList = new List<JobsiteVacancy>();
                foreach (var vacancy in vacancies)
                {
                    //get data
                    var title = vacancy.FindElement(By.XPath(".//span[2]/a/h2"));
                    var company = vacancy.FindElement(By.XPath(".//span[2]/span[1]")).Text;
                    var location = vacancy.FindElement(By.XPath(".//span[2]/span[2]/span[2]/span/span")).Text;
                    var url = vacancy.FindElement(By.XPath(".//span[2]/a")).GetAttribute("href");

                    //declaration variables
                    var keywords = title;
                    string keywordsText;

                    //try catch when vacancy has no keywords
                    try
                    {
                        keywords = vacancy.FindElement(By.XPath(".//span[2]/span[3]"));
                        keywordsText = keywords.Text;
                    }
                    catch (NoSuchElementException e)
                    {
                        keywordsText = "None";
                    }

                    vacanciesList.Add(new JobsiteVacancy(title.Text, company, location, keywordsText, url));
                }

                //write data to json file
                var fileName = "../../../../jsonFiles/Jobsite.json";
                JsonFileUtils.SimpleWrite(vacanciesList, fileName);

                Console.WriteLine("\n\n========================================");
                Console.WriteLine("          JOBSITE VACANCIES             ");
                Console.WriteLine("========================================\n\n");

                //print data
                foreach (var vacancy in vacanciesList)
                {
                    count++;

                    Console.WriteLine("JOB VACANCIE DETAILS {0}:", count);
                    Console.WriteLine("TITLE: {0}", vacancy.Title);
                    Console.WriteLine("COMPANY: {0}", vacancy.Company);
                    Console.WriteLine("LOCATION: {0}", vacancy.Location);
                    Console.WriteLine("KEYWORDS: {0}", vacancy.Keywords);
                    Console.WriteLine("URL: {0}", vacancy.Url);
                    Console.WriteLine("\n");
                }
                //quit driver after session
                driver.Quit();

            }
            catch (NoSuchElementException e)
            {
                Console.WriteLine("No results found.");

                //quit driver after session
                driver.Quit();

                return;
            }

        }

        static void GenerateBolproducts()
        {

            //give search term
            Console.Write("Give a product search term: ");
            string searchTerm = Console.ReadLine();

            //navigate to bol.com
            IWebDriver driver = new ChromeDriver();
            driver.Navigate().GoToUrl("https://www.bol.com/be/nl/");

            //accept privacy preference
            var privacyButton = driver.FindElement(By.XPath("//*[@id=\"js-first-screen-accept-all-button\"]"));
            privacyButton.Click();

            //wait till page is refreshed
            Thread.Sleep(2000);

            //search on keyword
            var search = driver.FindElement(By.XPath("//*[@id=\"searchfor\"]"));
            search.SendKeys(searchTerm);
            search.Submit();

            //wait till page is refreshed
            Thread.Sleep(2000);

            try
            {
                //get results
                var results = driver.FindElement(By.XPath("//*[@id=\"js_items_content\"]"));

                //take first 5 products
                var products = results.FindElements(By.XPath(".//li[contains(@class, 'product-item--row js_item_root')]")).Take(5);

                List<BolProduct> productsList = new List<BolProduct>();
                foreach (var product in products)
                {
                    //get all product details
                    var brand = product.FindElement(By.XPath(".//div[2]/div/ul[1]/li/a")).Text;
                    var productName = product.FindElement(By.XPath(".//div[2]/div/div[1]/wsp-analytics-tracking-event/a")).Text;
                    string productPrice = product.FindElement(By.XPath(".//div[2]/wsp-buy-block/div[1]/section/div[1]/div/meta")).GetAttribute("content");


                    string starRating = "";
                    try
                    {
                        starRating = product.FindElement(By.XPath(".//div[2]/div/div[3]/div")).GetAttribute("title");
                    }
                    catch (NoSuchElementException e)
                    {
                        starRating = "There are no reviews for this product yet";
                    }



                    //to display € sign
                    Console.OutputEncoding = System.Text.Encoding.UTF8;

                    //convert string to double
                    double price = Convert.ToDouble(productPrice.Replace('.', ','));

                    //add product to list
                    productsList.Add(new BolProduct(productName, brand, price, starRating));
                }

                //order on price
                var orderedList = productsList.OrderBy(p => p.Price);

                //write data to json file
                var fileName = "../../../../jsonFiles/BolProducts.json";
                JsonFileUtils.SimpleWrite(orderedList, fileName);

                Console.WriteLine("\n\n=================================================");
                Console.WriteLine("   BOL.COM PRODUCTS ORDERED BY ASCENDING PRICE   ");
                Console.WriteLine("=================================================\n\n");

                //print all details
                foreach (var bolProduct in orderedList)
                {
                    count++;
                    Console.WriteLine("PRODUCTS {0}:", count);
                    Console.WriteLine("PRODUCT NAME: {0}", bolProduct.ProductName);
                    Console.WriteLine("PRODUCT BRAND: {0}", bolProduct.Brand);
                    Console.WriteLine(String.Format("PRODUCT PRICE: €{0:0.00}", bolProduct.Price));
                    Console.WriteLine("STAR RATING: {0}", bolProduct.Rating);
                    Console.WriteLine("\n");
                }

                //quit driver after session
                driver.Quit();

            }
            catch (NoSuchElementException e)
            {
                Console.WriteLine("No results found.");

                //quit driver after session
                driver.Quit();

                return;
            }
        }

    }
}
