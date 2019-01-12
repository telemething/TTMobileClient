
// https://docs.microsoft.com/en-us/xamarin/xamarin-forms/app-fundamentals/custom-renderer/map/
// https://developer.xamarin.com/samples/xamarin-forms/customrenderers/map/polyline/
// https://developer.xamarin.com/samples/xamarin-forms/WorkingWithMaps/
// https://docs.microsoft.com/en-us/xamarin/xamarin-forms/user-interface/map
// https://stackoverflow.com/questions/25126433/force-redraw-of-xamarin-forms-view-with-custom-renderer
// maybe some sample code here? https://github.com/TorbenK/TK.CustomMap 

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
