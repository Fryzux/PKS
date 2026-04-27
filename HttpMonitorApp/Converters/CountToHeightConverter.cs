using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using WpfHttpApp.Models;

namespace WpfHttpApp
{
    /// <summary>
    /// Converts a bucket's Count into a bar height proportional to the max count in the collection.
    /// MultiBinding: [0] = Count (int), [1] = Buckets collection
    /// </summary>
    public class CountToHeightConverter : IMultiValueConverter
    {
        private const double MaxBarHeight = 110.0;
        private const double MinBarHeight = 2.0;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2) return MinBarHeight;
            if (values[0] is not int count) return MinBarHeight;
            if (values[1] is not ObservableCollection<RequestBucket> buckets) return MinBarHeight;

            int max = buckets.Count > 0 ? buckets.Max(b => b.Count) : 1;
            if (max == 0) return MinBarHeight;

            double height = (count / (double)max) * MaxBarHeight;
            return Math.Max(height, count > 0 ? MinBarHeight : 0);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
