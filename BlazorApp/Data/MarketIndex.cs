using System.Text.Json.Serialization;

namespace BlazorApp.Data
{
    public class MarketIndex
    {
        [JsonPropertyName("symbol")]
        public string MarketIndexTicker { get; set; }


        [JsonPropertyName("open")]
        public double Open { get; set; }

        [JsonPropertyName("close")]
        public double Close { get; set; }

        [JsonPropertyName("high")]
        public double High { get; set; }

        [JsonPropertyName("low")]
        public double Low { get; set; }



        [JsonPropertyName("from")]
        public DateTime From { get; set; }

        public bool Success { get; set; }

        public MarketIndex()
        {
            this.MarketIndexTicker = string.Empty;
            this.Success = false;
        }

        /*
         {
            "status": "OK",
            "from": "2023-03-10",
            "symbol": "I:OMXS30",
            "open": 2202.7420853221,
            "high": 2213.56492488791,
            "low": 2178.6365054963,
            "close": 2192.06471552532,
            "afterHours": 2192.06471552532,
            "preMarket": 2202.7420853221
        }
         */
    }

    public class PreviousCloseMarketIndexResponseResult
    {
        public string T { get; set; }
        public double o { get; set; }
        public double c { get; set; }
        public double h { get; set; }
        public double l { get; set; }
        public long t { get; set; }
    }

    public class PreviousCloseMarketIndexResponse
    {
        public string ticker { get; set; }
        public int queryCount { get; set; }
        public List<PreviousCloseMarketIndexResponseResult> results { get; set; }
        public string status { get; set; }
        public string request_id { get; set; }
        public int count { get; set; }

        public MarketIndex ConvertToMarketIndex()
        {            
            PreviousCloseMarketIndexResponseResult result = this.results?[0] ?? new PreviousCloseMarketIndexResponseResult();

            return new MarketIndex
            {
                MarketIndexTicker = this.ticker,
                Open = result.o,
                Close = result.c,
                High = result.h,
                Low = result.l
            };
        }
    }
}