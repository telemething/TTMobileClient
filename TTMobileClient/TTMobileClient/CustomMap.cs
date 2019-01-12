using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms.Maps;

namespace TTMobileClient
{
    public class CustomPin : Pin
    {
        public string Url { get; set; }
    }

    public class CustomMap : Map
    {
        public CustomMap(MapSpan region) : base(region)
        {
        }

        public List<CustomPin> CustomPins { get; set; }
    }
}
