using Microsoft.AspNetCore.Http.HttpResults;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Net.Http.Headers;
using Microsoft.Extensions.Caching.Memory;
using System;

namespace BlazorApp.Data
{
    public class IndiceService
    {

        private Indice? indice = new Indice();
        private HttpClient httpClient;

        private IMemoryCache _cache;

        private string apiKey = "Zu3n5JL4sYt8utwlJ2hcsTiDJkvp_4xW";


        public IndiceService(IHttpClientFactory clientFactory, IMemoryCache memoryCache)
        {
            this.httpClient = clientFactory.CreateClient();
            this._cache = memoryCache;
        }

        public async Task<Indice> GetIndicePreviousCloseAsync(string indiceTicker = "I:OMXS30")
        {
            try
            {
                // Try to get today's IndiceTicker from cache
                if (_cache.TryGetValue("PreviousClose_" + indiceTicker, out indice))
                {
                    if (indice is null)
                    {
                        throw new Exception("Cached value was null");
                    }
                    else return indice;
                }


                // Build request URI
                // https://api.polygon.io/v2/aggs/ticker/I:OMXS30/prev?apiKey=Zu3n5JL4sYt8utwlJ2hcsTiDJkvp_4xW
                var uri = $"https://api.polygon.io/v2/aggs/ticker/{indiceTicker}/prev?apiKey={apiKey}";


                // Send request
                var response = await httpClient.GetAsync(uri);

                if (response.IsSuccessStatusCode)
                {
                    using var responseStream = await response.Content.ReadAsStreamAsync();
                    var previousCloseIndiceResponse = await JsonSerializer.DeserializeAsync<PreviousCloseIndiceResponse>(responseStream);

                    if (previousCloseIndiceResponse is null) throw new Exception("Deserialization failed");
                    else
                    {
                        indice = previousCloseIndiceResponse.ConvertToIndice();
                        indice.IndiceTicker = indiceTicker;

                        // Cache todays IndiceTicker response
                        _cache.Set("PreviousClose_" + indiceTicker, indice);
                        return indice;
                    }
                }
                throw new Exception("GET previous close request failed, code: " + response.StatusCode);
            }
            catch (Exception ex)
            {
                // Graceful failure
                Console.WriteLine(ex.ToString());
                return new Indice();
            }
        }

        public async Task<Indice> GetIndiceOpenCloseAsync(DateTime? date, string indiceTicker = "I:OMXS30")
        {
            try
            {
                var currentDateTime = DateTime.Now;
                if (date != null) currentDateTime = (DateTime)date;
                var parsedDate = ParseDateTime(currentDateTime);

                // Try to get today's IndiceTicker from cache
                if (_cache.TryGetValue("OpenClose_" + indiceTicker + parsedDate, out indice))
                {
                    if(indice is null)
                    {
                        throw new Exception("Cached value was null");
                    }
                    else return indice;
                }
                    

                // Build request URI
                var uri = $"https://api.polygon.io/v1/open-close/{indiceTicker}/{parsedDate}?apiKey={apiKey}";

                // Send request
                var response = await httpClient.GetAsync(uri);

                if (response.IsSuccessStatusCode)
                {
                    using var responseStream = await response.Content.ReadAsStreamAsync();
                    indice = await JsonSerializer.DeserializeAsync<Indice>(responseStream);

                    if (indice is null) throw new Exception("Deserialization failed");
                    else
                    {
                        // Cache todays IndiceTicker response
                        _cache.Set("OpenClose_" + indiceTicker + parsedDate, indice);
                        return indice;
                    }
                }
                throw new Exception("GET open-close request failed, code: " + response.StatusCode);
                
            }
            catch (Exception ex)
            {
                // Graceful failure
                Console.WriteLine(ex.ToString());
                return new Indice();
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