using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace WpfHttpApp.Models
{
    /// <summary>Request count bucket for per-minute/per-hour chart.</summary>
    public partial class RequestBucket : ObservableObject
    {
        public DateTime Time { get; init; }

        [ObservableProperty]
        private int _count;

        public string Label => Time.ToString("HH:mm");
    }
}
