using System;

namespace WpfHttpApp.Models
{
    /// <summary>Request count bucket for per-minute/per-hour chart.</summary>
    public class RequestBucket
    {
        public DateTime Time { get; init; }
        public int Count { get; set; }
        public string Label => Time.ToString("HH:mm");
    }
}
