using System;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace Function.Services
{
    public class ScrapeWeb
    {
        private ILogger _log;
        public ScrapeWeb(ILogger log) => _log = log;
        public async Task<bool> HasPriceDroppedAsync()
        {
            var GOS_URL = Environment.GetEnvironmentVariable("GOS_PS5");    // Ghost of Tshushima
            var SM_URL = Environment.GetEnvironmentVariable("SM_PS5");      // Spiderman

            var gosPrice = await FindPriceAsync(GOS_URL, ".//span[text()='PS5']");
            var smPrice = await FindPriceAsync(SM_URL, ".//h3[contains(text(), 'Ultimate')]");

            _log.LogInformation($"Ghost Of Tshushima: INR: {gosPrice}");
            _log.LogInformation($"Spiderman: INR: {smPrice}");
    

            var PRICE_THRESHOLD = decimal.Parse(Environment.GetEnvironmentVariable("PRICE_THRESHOLD"));

            return (gosPrice <= PRICE_THRESHOLD) || (smPrice <= PRICE_THRESHOLD);

        }

        public async Task<decimal> FindPriceAsync(string url, string xpath)
        {
            HtmlWeb web = new HtmlWeb();

            var htmlDoc = await web.LoadFromWebAsync(url);
            _log.LogInformation($"Downloaded HTML from: {url}");

            var document = htmlDoc.DocumentNode;
            
            var articles = document.SelectNodes("//article");
            var ps5_article = articles.Where(node => node.SelectSingleNode(xpath) != null).FirstOrDefault();
            var ps5Price = ps5_article.SelectSingleNode(".//span[contains(@data-qa, 'finalPrice')]").InnerText;
            var ps5Price_final = ps5Price.Replace("Rs", "").Replace(",", "").Trim();
            
            return string.IsNullOrWhiteSpace(ps5Price_final) ? decimal.MaxValue : decimal.Parse(ps5Price_final);
        }
    }
}