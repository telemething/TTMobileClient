
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
    public class Waypoint : Pin
    {
        public string Url { get; set; }
    }

    public class TrackedObject : Pin
    {
        public string Name { get; set; }
        public object tag { get; set; }
    }

    public class OnMapClickEventArgs : EventArgs
    {
        private double _lat;
        private double _lon;
        private double _alt;

        public double Lat
        {
            set => _lat = value;
            get => _lat;
        }

        public double Lon
        {
            set => _lon = value;
            get => _lon;
        }

        public double Alt
        {
            set => _alt = value;
            get => _alt;
        }

        public OnMapClickEventArgs(double lat, double lon, double alt)
        {
            _lat = lat;
            _lon = lon;
            _alt = alt;
        }
    }

    public class ChangeHappened
    {
        public int itemNumber;
        public object addedObject;
        public object removedObject;
    }

    public class CustomMap : Map
    {
        public event EventHandler<OnMapClickEventArgs> OnMapClick;

        public ChangeHappened change;
        public List<Position> routeCoordinates;
        private List<Waypoint> _Waypoints;

        public ChangeHappened Change
        {
            get { return change; }
            set { change = value; OnPropertyChanged(); }
        }

        public List<Waypoint> Waypoints
        {
            get { return _Waypoints; }
            set { _Waypoints = value; OnPropertyChanged(); }
        }

        public List<Position> RouteCoordinates
        {
            get { return routeCoordinates; }
            set { routeCoordinates = value; OnPropertyChanged(); }
        }

       public CustomMap(MapSpan region) : base(region)
       {
           Waypoints = new List<Waypoint>();
           routeCoordinates = new List<Position>();
       }

       public void AddWaypoint(Waypoint waypoint)
       {
            _Waypoints.Add(waypoint);
            Change = new ChangeHappened(){addedObject = waypoint };
       }

       public void MapClickCallback(double lat, double lon, double alt)
       {
           OnMapClick?.Invoke(this,
               new OnMapClickEventArgs(lat, lon, alt));
       }

       public void MapClickCallback(double lat, double lon)
       {
           OnMapClick?.Invoke(this,
               new OnMapClickEventArgs(lat, lon, 0));
       }
    }
}
