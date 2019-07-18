using System;
using System.Collections.Generic;
using System.Linq;
using TTMobileClient;
using TTMobileClient.UWP;
using System.ComponentModel;
using Windows.Devices.Geolocation;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Maps;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Maps.UWP;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(CustomMap), typeof(CustomMapRenderer))]
namespace TTMobileClient.UWP
{
    public class CustomMapRenderer : MapRenderer
    {
        MapControl nativeMap;
        private CustomMap formsMap;
        List<Waypoint> _waypoints;
        List<TrackedObject> _trackedObjects;
        XamarinMapOverlay mapOverlay;
        bool xamarinOverlayShown = false;

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        /// 
        //*********************************************************************

        public CustomMapRenderer()
        {
            _waypoints = new List<Waypoint>(4);
            _trackedObjects = new List<TrackedObject>(1);
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        ///
        //*********************************************************************

        protected override void OnElementPropertyChanged(
            object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            var ff = sender;
            var tt = e;

            if (e.PropertyName.Equals("RouteCoordinates"))
            {
                formsMap = (CustomMap)sender;
                nativeMap = Control as MapControl;

                if (0 != formsMap.RouteCoordinates.Count())
                {
                    var coordinates = new List<BasicGeoposition>();
                    foreach (var position in formsMap.RouteCoordinates)
                    {
                        coordinates.Add(new BasicGeoposition()
                        {
                            Latitude = position.Latitude,
                            Longitude = position.Longitude
                        });
                    }

                    var polyline = new MapPolyline();
                    polyline.StrokeColor = Windows.UI.Color.
                        FromArgb(128, 255, 0, 0);
                    polyline.StrokeThickness = 5;
                    polyline.Path = new Geopath(coordinates);
                    nativeMap.MapElements.Add(polyline);
                }
            }

            if (e.PropertyName.Equals("Change"))
            {
                formsMap = (CustomMap)sender;
                nativeMap = Control as MapControl;

                var newObject = formsMap.change.SubjectObject;

                if (newObject is Waypoint newPin)
                    switch (formsMap.change.ChangeType)
                    {
                        case ChangeHappened.ChangeTypeEnum.Added:
                            AddWaypoint(newPin);
                            break;
                        case ChangeHappened.ChangeTypeEnum.Removed:
                            RemoveWaypoint(newPin);
                            break;
                    }

                if (newObject is TrackedObject to)
                    switch (formsMap.change.ChangeType)
                    {
                        case ChangeHappened.ChangeTypeEnum.Added:
                            AddTrackedObject(to);
                            break;
                        case ChangeHappened.ChangeTypeEnum.Changed:
                            ChangeTrackedObject(to);
                            break;
                    }
            }
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        /// <param name="newPin"></param>
        ///
        //*********************************************************************

        private void AddTrackedObject(TrackedObject newTO)
        {
            var mapIcon = new MapIcon
            {
                Image = RandomAccessStreamReference.CreateFromUri(
                    new Uri("ms-appx:///uav.png")),
                CollisionBehaviorDesired =
                    MapElementCollisionBehavior.RemainVisible,
                Location = new Geopoint(
                    new BasicGeoposition
                    {
                        Latitude = newTO.Position.Latitude,
                        Longitude = newTO.Position.Longitude
                    }),
                NormalizedAnchorPoint =
                    new Windows.Foundation.Point(0.5, 1.0)
            };

            newTO.nativeMapElement = mapIcon;

            _trackedObjects.Add(newTO);

            nativeMap.MapElements.Add(mapIcon);
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        /// <param name="changedTO"></param>
        ///
        //*********************************************************************

        private void ChangeTrackedObject(TrackedObject changedTO)
        {
            var trackedObject = GetTrackedObject(changedTO.UniqueId);

            if (trackedObject.nativeMapElement is MapIcon mapIcon)
                mapIcon.Location = new Geopoint(
                    new BasicGeoposition
                    {
                        Latitude = changedTO.Position.Latitude,
                        Longitude = changedTO.Position.Longitude
                    });
            else
                throw new Exception("ChangeTrackedObject() : nativeMapElement is not a MapIcon");
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        /// <param name="newPin"></param>
        ///
        //*********************************************************************

        private void AddWaypoint(Waypoint newPin)
        {
            var snPoint = new Geopoint(
                new BasicGeoposition
                {
                    Latitude = newPin.Position.Latitude,
                    Longitude = newPin.Position.Longitude
                });

            var mapIcon = new MapIcon
            {
                Image = RandomAccessStreamReference.CreateFromUri(
                    new Uri("ms-appx:///pin.png")),
                CollisionBehaviorDesired =
                    MapElementCollisionBehavior.RemainVisible,
                Location = snPoint,
                NormalizedAnchorPoint =
                    new Windows.Foundation.Point(0.5, 1.0),
                Tag = newPin
            };

            nativeMap.MapElements.Add(mapIcon);
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        /// <param name="waypoint"></param>
        ///
        //*********************************************************************

        private void RemoveWaypoint(Waypoint waypoint)
        {
            waypoint.IsActive = false;

            var foundWaypoint = nativeMap.MapElements.FirstOrDefault(x => x.Tag == waypoint);

            if (null == foundWaypoint)
                return;

            nativeMap.MapElements.Remove(foundWaypoint);
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        ///
        //*********************************************************************

        protected override void OnElementChanged(ElementChangedEventArgs<Map> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null)
            {
                nativeMap.MapElementClick -= OnMapElementClick;
                nativeMap.MapTapped -= NativeMapOnMapTapped;
                nativeMap.Children.Clear();
                mapOverlay = null;
                nativeMap = null;
            }

            if (e.NewElement != null)
            {
                formsMap = (CustomMap)e.NewElement;
                nativeMap = Control as MapControl;
                _waypoints = formsMap.Waypoints;

                nativeMap.Children.Clear();

                nativeMap.MapElementClick += OnMapElementClick;
                nativeMap.MapTapped += NativeMapOnMapTapped;
                nativeMap.Loaded += NativeMapOnLoaded;

                // Pins
                foreach (var pin in _waypoints)
                {
                    var snPosition = new BasicGeoposition
                    {
                        Latitude = pin.Position.Latitude,
                        Longitude = pin.Position.Longitude
                    };
                    var snPoint = new Geopoint(snPosition);

                    var mapIcon = new MapIcon();
                    mapIcon.Image = RandomAccessStreamReference.
                        CreateFromUri(new Uri("ms-appx:///pin.png"));
                    mapIcon.CollisionBehaviorDesired = 
                        MapElementCollisionBehavior.RemainVisible;
                    mapIcon.Location = snPoint;
                    mapIcon.NormalizedAnchorPoint = 
                        new Windows.Foundation.Point(0.5, 1.0);

                    nativeMap.MapElements.Add(mapIcon);
                }

                // Polyline
                if (0 != formsMap.RouteCoordinates.Count())
                {
                    var coordinates = new List<BasicGeoposition>();
                    foreach (var position in formsMap.RouteCoordinates)
                    {
                        coordinates.Add(new BasicGeoposition()
                            {Latitude = position.Latitude, Longitude = position.Longitude});
                    }

                    var polyline = new MapPolyline
                    {
                        StrokeColor = Windows.UI.Color.FromArgb(128, 255, 0, 0),
                        StrokeThickness = 5,
                        Path = new Geopath(coordinates)
                    };
                    nativeMap.MapElements.Add(polyline);
                }

                //*************************************
                //*************************************

                if(null != formsMap.faaFascilityMap)
                foreach (var feature in formsMap.faaFascilityMap.features)
                {
                    foreach (var ring in feature.geometry.rings)
                    {
                        var ringCoords = new List<BasicGeoposition>();

                        foreach (var subRing in ring)
                        {
                            ringCoords.Add(new BasicGeoposition() {Latitude = subRing[1], Longitude = subRing[0]});
                        }

                        var ringPolygon = new MapPolygon();
                        //ringPolygon.FillColor = Windows.UI.Colors.LightGray;
                        ringPolygon.FillColor = Windows.UI.Color.FromArgb(100, 80, 80, 80);
                        //ringPolygon.StrokeColor = Windows.UI.Colors.LightPink;
                        ringPolygon.StrokeColor = Windows.UI.Color.FromArgb(100, 255, 192, 203);
                        ringPolygon.StrokeThickness = 1;
                        ringPolygon.Path = new Geopath(ringCoords);
                        nativeMap.MapElements.Add(ringPolygon);
                    }
                }

                //*************************************
                //*************************************
            }
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        ///
        //*********************************************************************

        private void NativeMapOnLoaded(object sender, RoutedEventArgs e)
        {
            formsMap.MapReadyCallback();
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        ///
        //*********************************************************************

        private void NativeMapOnMapTapped(MapControl sender, MapInputEventArgs args)
        {
            formsMap.MapClickCallback(args.Location.Position.Latitude,
                args.Location.Position.Longitude,
                args.Location.Position.Altitude);
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        ///
        //*********************************************************************

        private void OnMapElementClick(MapControl sender, MapElementClickEventArgs args)
        {
            if (args.MapElements.FirstOrDefault(x => x is MapIcon) is MapIcon mapIcon)
            {
                if (!xamarinOverlayShown)
                {
                    var customPin = GetWaypoint(mapIcon.Location.Position);
                    if (customPin == null)
                    {
                        throw new Exception("Custom pin not found");
                    }

                    if (customPin.Id.ToString() == "Waypoint")
                    {
                        if (mapOverlay == null)
                        {
                            mapOverlay = new XamarinMapOverlay(customPin);
                        }

                        var snPosition = new BasicGeoposition
                        {
                            Latitude = customPin.Position.Latitude,
                            Longitude = customPin.Position.Longitude
                        };
                        var snPoint = new Geopoint(snPosition);

                        nativeMap.Children.Add(mapOverlay);
                        MapControl.SetLocation(mapOverlay, snPoint);
                        MapControl.SetNormalizedAnchorPoint(mapOverlay,
                            new Windows.Foundation.Point(0.5, 1.0));
                        xamarinOverlayShown = true;
                    }
                }
                else
                {
                    nativeMap.Children.Remove(mapOverlay);
                    xamarinOverlayShown = false;
                }
            }
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        ///
        //*********************************************************************

        Waypoint GetWaypoint(BasicGeoposition position)
        {
            var pos = new Position(position.Latitude, position.Longitude);
            foreach (var pin in _waypoints)
                if (pin.Position == pos)
                    return pin;

            return null;
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        ///
        //*********************************************************************

        TrackedObject GetTrackedObject(string id)
        {
            foreach (var to in _trackedObjects)
                if (to.UniqueId.Equals(id))
                    return to;

            return null;
        }
    }
}
