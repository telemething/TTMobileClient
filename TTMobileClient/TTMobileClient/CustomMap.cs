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
        public List<Position> routeCoordinates;
        public List<CustomPin> customPins;

        public List<CustomPin> CustomPins
        {
            get { return customPins; }
            set { customPins = value; OnPropertyChanged(); }
        }

        public List<Position> RouteCoordinates
        {
            get { return routeCoordinates; }
            set { routeCoordinates = value; OnPropertyChanged(); }
        }

       public CustomMap(MapSpan region) : base(region)
       {
           CustomPins = new List<CustomPin>();
           routeCoordinates = new List<Position>();
        }
    }
}
