using HtmlAgilityPack;
using ScrapySharp.Network;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;

namespace CrawlerDemo
{
    class Program
    {
        static ScrapingBrowser _browser = new ScrapingBrowser();
        static string connetionString = "data source=(localdb)\\MSSQLLocalDB;initial catalog=CrawlerDemo;User ID=sa;Password=P@ssw0rd";
        static void Main(string[] args)
        {
            CrawlerDemo();
        }

        public static HtmlDocument GetHtml(string url)
        {
            WebPage webpage = _browser.NavigateToPage(new Uri(url));
            var htmlinner = webpage.Html.InnerHtml;
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(htmlinner);
            return htmlDocument;
        }

        private static void CrawlerDemo()
        {
            try
            {
                var url = "https://www.mac2hand.com/17/ขาย+iPhone/";
                var httpClient = new HttpClient();
                var htmlDocument = GetHtml(url);

                List<Phone> listPhone = new List<Phone>();
                var listData = htmlDocument.DocumentNode.Descendants("div")
                .Where(node => node.GetAttributeValue("class", "").Equals("productContainer")).ToList();

                foreach (var item in listData)
                {
                    var phone = new Phone
                    {
                        Name = item.Descendants("div").Where(node => node.HasClass("productname")).LastOrDefault().InnerText,
                        Detail = item.Descendants("div").Where(node => node.HasClass("detail")).LastOrDefault().InnerText,
                        Price = item.Descendants("div").Where(node => node.HasClass("priceContainer")).LastOrDefault().InnerText
                    };

                    listPhone.Add(phone);
                }

                try
                {
                    ////////////connect database//////////
                    SqlConnection con = new SqlConnection(connetionString);
                    con.Open();
                    //////////add data on table/////////
                    foreach (var item in listPhone)
                    {

                        string query = "INSERT INTO Phone (Name,Detail,Price) VALUES(@VAL1,@VAL2,@VAL3);";
                        SqlCommand cmd = new SqlCommand(query, con);
                        cmd.Parameters.Add("@VAL1", SqlDbType.VarChar).Value = item.Name;
                        cmd.Parameters.Add("@VAL2", SqlDbType.VarChar).Value = item.Detail;
                        cmd.Parameters.Add("@VAL3", SqlDbType.VarChar).Value = item.Price;
                        SqlDataReader reader = cmd.ExecuteReader();
                        reader.Close();

                    }
                    con.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                //////////end process/////////
                Console.WriteLine("Add Data Successful ");
                Console.WriteLine("Press Check Database...");
                ConsoleKeyInfo keyinfor = Console.ReadKey(true);
                if (keyinfor.Key == ConsoleKey.Enter)
                {
                    System.Environment.Exit(0);
                }

            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
        