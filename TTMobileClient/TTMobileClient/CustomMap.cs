
// https://docs.microsoft.com/en-us/xamarin/xamarin-forms/app-fundamentals/custom-renderer/map/
// https://developer.xamarin.com/samples/xamarin-forms/customrenderers/map/polyline/
// https://developer.xamarin.com/samples/xamarin-forms/WorkingWithMaps/
// https://docs.microsoft.com/en-us/xamarin/xamarin-forms/user-interface/map
// https://stackoverflow.com/questions/25126433/force-redraw-of-xamarin-forms-view-with-custom-renderer
// maybe some sample code here? https://github.com/TorbenK/TK.CustomMap 

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xamarin.Forms.Maps;

namespace TTMobileClient
{
    public class Waypoint : Pin
    {
        public string Url { get; set; }
        public object Tag { get; set; }
        public bool IsActive { get; set; }
    }

    public class TrackedObject : Pin
    {
        public string UniqueId;
        public string Url { get; set; }
        public string Name { get; set; }
        public object Tag { get; set; }
        public object nativeMapElement { get; set; }
    }

    public class SelfObject : Pin
    {
        public string UniqueId;
        public string Url { get; set; }
        public string Name { get; set; }
        public object Tag { get; set; }
        public object nativeMapElement { get; set; }
        public Position PositionFromSensor { get; set; }
        public Position PositionOffset { get; set; }
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
        public enum ChangeTypeEnum { None, Added, Changed, Removed }
        public ChangeTypeEnum ChangeType;
        public int ItemNumber;
        public object SubjectObject;

        public ChangeHappened(object subjectObject, ChangeTypeEnum changeType)
        {
            SubjectObject = subjectObject;
            ChangeType = changeType;
        }
    }

    public class CustomMap : Map
    {
        public event EventHandler<OnMapClickEventArgs> OnMapClick;
        public event EventHandler OnMapReady;

        //*********************
        public List<Position> ShapeCoordinates { get; set; }

        public FaaUasLib.Models.FascilityMap faaFascilityMap { get; set; }

        //*********************

        public ChangeHappened change;
        public List<Position> routeCoordinates;
        private List<Waypoint> _Waypoints;
        private List<TrackedObject> _TrackedObjects;
        private SelfObject _SelfObject;

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

        public List<TrackedObject> TrackedObjects
        {
            get { return _TrackedObjects; }
            set { _TrackedObjects = value; OnPropertyChanged(); }
        }

        public SelfObject SelfObject
        {
            get { return _SelfObject; }
            set { _SelfObject = value; OnPropertyChanged(); }
        }

        public List<Position> RouteCoordinates
        {
            get { return routeCoordinates; }
            set { routeCoordinates = value; OnPropertyChanged(); }
        }

       public CustomMap(MapSpan region) : base(region)
       {
           TrackedObjects = new List<TrackedObject>();
           Waypoints = new List<Waypoint>();
           routeCoordinates = new List<Position>();
       }

       public void AddWaypoint(Waypoint waypoint)
       {
           _Waypoints.Add(waypoint);
           //waypoint.MarkerClicked
           Change = new ChangeHappened(waypoint, ChangeHappened.ChangeTypeEnum.Added);
       }

       public void RemoveWaypoint(Waypoint waypoint)
       {
           Change = new ChangeHappened(waypoint, ChangeHappened.ChangeTypeEnum.Removed);
           _Waypoints.Remove(waypoint);
       }

       public void RemoveWaypoints()
       {
           foreach (var wpt in _Waypoints)
              Change = new ChangeHappened(wpt, ChangeHappened.ChangeTypeEnum.Removed);

           _Waypoints.Clear();
        }

        public void AddTrackedObject(TrackedObject trackedObject)
        {
            _TrackedObjects.Add(trackedObject);
            Change = new ChangeHappened(trackedObject, ChangeHappened.ChangeTypeEnum.Added);
        }

        public void SetSelfObject(SelfObject selfObject)
        {
            ChangeHappened.ChangeTypeEnum change = ChangeHappened.ChangeTypeEnum.None;

            if (null == _SelfObject)
            {
                _SelfObject = selfObject;
                change = ChangeHappened.ChangeTypeEnum.Added;
            }
            else
                change = ChangeHappened.ChangeTypeEnum.Changed;

            Change = new ChangeHappened(selfObject, change);
        }

        public void MapClickCallback(double lat, double lon, double alt)
        {
            OnMapClick?.Invoke(this,
                new OnMapClickEventArgs(lat, lon, alt));
        }

        public void MapReadyCallback()
        {
            OnMapReady?.Invoke(this, new EventArgs());
        }

       public void MapClickCallback(double lat, double lon)
       {
           OnMapClick?.Invoke(this,
               new OnMapClickEventArgs(lat, lon, 0));
       }
    }
}
