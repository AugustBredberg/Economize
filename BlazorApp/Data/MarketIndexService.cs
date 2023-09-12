using Microsoft.AspNetCore.Http.HttpResults;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Net.Http.Headers;
using Microsoft.Extensions.Caching.Memory;
using System;

namespace BlazorApp.Data
{
    public class MarketIndexService
    {

        private MarketIndex? marketIndex = new MarketIndex();
        private HttpClient httpClient;

        private IMemoryCache _cache;

        private string apiKey = "Zu3n5JL4sYt8utwlJ2hcsTiDJkvp_4xW";

        // Flag icons: https://www.flaticon.com/free-icon/sweden_197564?related_id=197564&origin=pack

        static Dictionary<string, string> marketIndexLookup = new Dictionary<string, string>
        {
            {"I:OMXS30", "OMX"},
            {"I:NDX", "NDX"},
            {"I:SPX", "SPX"}
        };


        public MarketIndexService(IHttpClientFactory clientFactory, IMemoryCache memoryCache)
        {
            this.httpClient = clientFactory.CreateClient();
            this._cache = memoryCache;
        }

        public async Task<MarketIndex> GetMarketIndexPreviousCloseAsync(string marketIndexTicker = "I:OMXS30")
        {
            try
            {
                // Try to get today's MarketIndexTicker from cache
                if (_cache.TryGetValue("PreviousClose_" + marketIndexTicker, out marketIndex))
                {
                    if (marketIndex is null)
                    {
                        throw new Exception("Cached value was null");
                    }
                    else return marketIndex;
                }


                // Build request URI
                var uri = $"https://api.polygon.io/v2/aggs/ticker/{marketIndexTicker}/prev?apiKey={apiKey}";


                // Send request
                var response = await httpClient.GetAsync(uri);

                if (response.IsSuccessStatusCode)
                {
                    using var responseStream = await response.Content.ReadAsStreamAsync();
                    var previousCloseMarketIndexResponse = await JsonSerializer.DeserializeAsync<PreviousCloseMarketIndexResponse>(responseStream);

                    if (previousCloseMarketIndexResponse is null) throw new Exception("Deserialization failed");
                    else
                    {
                        marketIndex = previousCloseMarketIndexResponse.ConvertToMarketIndex();
                        marketIndex.MarketIndexTicker = marketIndexTicker;
                        marketIndex.Success = true;

                        // Cache todays MarketIndexTicker response
                        _cache.Set("PreviousClose_" + marketIndexTicker, marketIndex);
                        return marketIndex;
                    }
                }
                throw new Exception("GET previous close request failed, code: " + response.StatusCode);
            }
            catch (Exception ex)
            {
                // Graceful failure
                Console.WriteLine(ex.ToString());
                return new MarketIndex();
            }
        }

        public async Task<MarketIndex> GetMarketIndexOpenCloseAsync(DateTime? date, string marketIndexTicker = "I:OMXS30")
        {
            try
            {
                var currentDateTime = DateTime.Now;
                if (date != null) currentDateTime = (DateTime)date;
                var parsedDate = ParseDateTime(currentDateTime);

                // Try to get today's MarketIndexTicker from cache
                if (_cache.TryGetValue("OpenClose_" + marketIndexTicker + parsedDate, out marketIndex))
                {
                    if(marketIndex is null)
                    {
                        throw new Exception("Cached value was null");
                    }
                    else return marketIndex;
                }
                    

                // Build request URI
                var uri = $"https://api.polygon.io/v1/open-close/{marketIndexTicker}/{parsedDate}?apiKey={apiKey}";

                // Send request
                var response = await httpClient.GetAsync(uri);

                if (response.IsSuccessStatusCode)
                {
                    using var responseStream = await response.Content.ReadAsStreamAsync();
                    marketIndex = await JsonSerializer.DeserializeAsync<MarketIndex>(responseStream);

                    if (marketIndex is null) throw new Exception("Deserialization failed");
                    else
                    {
                        // Cache todays MarketIndexTicker response
                        marketIndex.Success = true;
                        _cache.Set("OpenClose_" + marketIndexTicker + parsedDate, marketIndex);
                        return marketIndex;
                    }
                }
                throw new Exception("GET open-close request failed, code: " + response.StatusCode);
                
            }
            catch (Exception ex)
            {
                // Graceful failure
                Console.WriteLine(ex.ToString());
                return new MarketIndex();
            }
        }

        public static string LookupMarketIndexTickerName(string marketIndexTicker)
        {
            if (marketIndexTicker is null) return "";
            string result;
            if (marketIndexLookup.TryGetValue(marketIndexTicker, out result))
            {
                return result;
            }
            else
            {
                return marketIndexTicker;
            }
        }

        private string ParseDateTime(DateTime date)
        {
            var year = date.Year.ToString();
            var month = date.Month.ToString();
            var day = (date.Day - 1).ToString();

            var parsedDate = year + "-" +
                (month.Length < 2 ? "0" + month : month) + "-" +
                (day.Length < 2 ? "0" + day : day);

            return parsedDate;
        }

    }
}