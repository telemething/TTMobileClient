
// https://docs.microsoft.com/en-us/xamarin/xamarin-forms/app-fundamentals/custom-renderer/map/
// https://developer.xamarin.com/samples/xamarin-forms/customrenderers/map/polyline/
// https://developer.xamarin.com/samples/xamarin-forms/WorkingWithMaps/
// https://docs.microsoft.com/en-us/xamarin/xamarin-forms/user-interface/map
// https://stackoverflow.com/questions/25126433/force-redraw-of-xamarin-forms-view-with-custom-renderer
// maybe some sample code here? https://github.com/TorbenK/TK.CustomMap 

using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace TTMobileClient
{
    //*************************************************************************
    /// <summary>
    /// 
    /// </summary>
    //*************************************************************************
    public class Waypoint : Pin
    {
        public string Url { get; set; }
        public object Tag { get; set; }
        public bool IsActive { get; set; }
    }

    //*************************************************************************
    /// <summary>
    /// 
    /// </summary>
    //*************************************************************************
    public class Poipoint : Pin
    {
        public string Url { get; set; }
        public object Tag { get; set; }
        public bool IsActive { get; set; }
    }

    //*************************************************************************
    /// <summary>
    /// 
    /// </summary>
    //*************************************************************************
    public class TrackedObject : Pin
    {
        public string UniqueId;
        public string Url { get; set; }
        public string Name { get; set; }
        public object Tag { get; set; }
        public object nativeMapElement { get; set; }
        public Position PositionFromSensor { get; set; }
        public Position PositionOffset { get; set; }

    }

    //*************************************************************************
    /// <summary>
    /// 
    /// </summary>
    //*************************************************************************
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

    //*************************************************************************
    /// <summary>
    /// 
    /// </summary>
    //*************************************************************************
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

    //*************************************************************************
    /// <summary>
    /// 
    /// </summary>
    //*************************************************************************
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

    //*************************************************************************
    /// <summary>
    /// 
    /// </summary>
    //*************************************************************************
    public class CustomMap : Map
    {
        public event EventHandler<OnMapClickEventArgs> OnMapClick;
        public event EventHandler OnMapReady;

        public Color FillColor = Color.FromRgba(80, 80, 80, 100);
        public Color StrokeColor = Color.FromRgba(255, 192, 203, 100);
        public float StrokeWidth = 1;

        //*********************
        public List<Position> ShapeCoordinates { get; set; }

        public FaaUasLib.Models.FascilityMap faaFascilityMap { get; set; }

        //*********************

        public List<TileServerLib.TileInfo> _geoTileList { get; set; }

        public List<TileServerLib.TileInfo> GeoTileList 
        {
            get { return _geoTileList; }
            set { _geoTileList = value;
                DrawGeoTiles(value, FillColor, StrokeColor, StrokeWidth);
                //OnPropertyChanged(); 
            }
        }

        public ChangeHappened change;
        public List<Position> routeCoordinates;
        private List<Waypoint> _Waypoints;
        private List<Poipoint> _Poipoints;
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

        public List<Poipoint> Poipoints
        {
            get { return _Poipoints; }
            set { _Poipoints = value; OnPropertyChanged(); }
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
            Poipoints = new List<Poipoint>();
            routeCoordinates = new List<Position>();
       }

       public void AddPoipoint(Poipoint poipoint)
       {
           _Poipoints.Add(poipoint);
           //waypoint.MarkerClicked
           Change = new ChangeHappened(poipoint, ChangeHappened.ChangeTypeEnum.Added);
       }

       public void RemovePoipoint(Poipoint poipoint)
       {
           Change = new ChangeHappened(poipoint, ChangeHappened.ChangeTypeEnum.Removed);
           _Poipoints.Remove(poipoint);
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


        private List<Polygon> _geoTilePolygonList = null;

        /// *******************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="geoTileList"></param>
        /// <param name="fillColor"></param>
        /// <param name="strokeColor"></param>
        /// <param name="strokeWidth"></param>
        /// *******************************************************************
        private void DrawGeoTiles(List<TileServerLib.TileInfo> geoTileList,
            Color fillColor, Color strokeColor, float strokeWidth)
        {
            if (null == geoTileList)
            {
                if(null != _geoTilePolygonList)
                    foreach (var poly in _geoTilePolygonList)
                        this.MapElements.Remove(poly);

                return;
            }

            _geoTilePolygonList = GeoTile2Polygon(geoTileList,
                fillColor, strokeColor, strokeWidth);

            foreach (var poly in _geoTilePolygonList)
                this.MapElements.Add(poly);
        }

        /// *******************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="geoTileList"></param>
        /// <param name="fillColor"></param>
        /// <param name="strokeColor"></param>
        /// <param name="strokeWidth"></param>
        /// <returns></returns>
        /// *******************************************************************
        private List<Polygon> GeoTile2Polygon(List<TileServerLib.TileInfo> geoTileList,
            Color fillColor, Color strokeColor, float strokeWidth)
        {
            if (null == geoTileList)
                return null;

            var polyList = new List<Polygon>(geoTileList.Count);

            foreach (var geoTile in geoTileList)
                polyList.Add(GeoTile2Polygon(geoTile,
                    fillColor, strokeColor, strokeWidth));

            return polyList;
        }

        /// *******************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="geoTile"></param>
        /// <param name="fillColor"></param>
        /// <param name="strokeColor"></param>
        /// <param name="strokeWidth"></param>
        /// <returns></returns>
        /// *******************************************************************
        private Polygon GeoTile2Polygon(TileServerLib.TileInfo geoTile, 
            Color fillColor, Color strokeColor, float strokeWidth)
        {
            var bbox = geoTile.BoundingBox();

            return new Polygon
            {
                StrokeColor = StrokeColor,
                StrokeWidth = strokeWidth,
                FillColor = fillColor,
                Geopath =
                {
                    new Position(bbox.NW.Lat, bbox.NW.Lon),
                    new Position(bbox.NE.Lat, bbox.NE.Lon),
                    new Position(bbox.SE.Lat, bbox.SE.Lon),
                    new Position(bbox.SW.Lat, bbox.SW.Lon)
                }
            };
        }

        Polygon msWest;

        /// *******************************************************************
        /// <summary>
        /// simple static test
        /// </summary>
        /// *******************************************************************
        public void TestPologon()
        {

            msWest = new Polygon
            {
                StrokeColor = Color.FromHex("#FF9900"),
                StrokeWidth = 8,
                FillColor = Color.FromHex("#88FF9900"),
                Geopath =
                {
                    new Position(47.6458676, -122.1356007),
                    new Position(47.6458097, -122.142789),
                    new Position(47.6367593, -122.1428104),
                    new Position(47.6368027, -122.1398707),
                    new Position(47.6380172, -122.1376177),
                    new Position(47.640663, -122.1352359),
                    new Position(47.6426148, -122.1347209),
                    new Position(47.6458676, -122.1356007)
                }
            };

            if (!this.MapElements.Contains(msWest))
            {
                this.MapElements.Add(msWest);
            }

        }
    }
}
