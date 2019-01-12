﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using TTMobileClient;
using TTMobileClient.UWP;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Windows.Devices.Geolocation;
using Windows.Storage.Streams;
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
        List<CustomPin> customPins;
        XamarinMapOverlay mapOverlay;
        bool xamarinOverlayShown = false;

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            var ff = sender;
            var tt = e;

            if (e.PropertyName.Equals("RouteCoordinates"))
            {
                var formsMap = (CustomMap)sender;
                nativeMap = Control as MapControl;

                if (0 != formsMap.RouteCoordinates.Count())
                {
                    var coordinates = new List<BasicGeoposition>();
                    foreach (var position in formsMap.RouteCoordinates)
                    {
                        coordinates.Add(new BasicGeoposition()
                            { Latitude = position.Latitude, Longitude = position.Longitude });
                    }

                    var polyline = new MapPolyline();
                    polyline.StrokeColor = Windows.UI.Color.FromArgb(128, 255, 0, 0);
                    polyline.StrokeThickness = 5;
                    polyline.Path = new Geopath(coordinates);
                    nativeMap.MapElements.Add(polyline);
                }
            }
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Map> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null)
            {
                nativeMap.MapElementClick -= OnMapElementClick;
                nativeMap.Children.Clear();
                mapOverlay = null;
                nativeMap = null;
            }

            if (e.NewElement != null)
            {
                var formsMap = (CustomMap)e.NewElement;
                nativeMap = Control as MapControl;
                customPins = formsMap.CustomPins;

                nativeMap.Children.Clear();
                nativeMap.MapElementClick += OnMapElementClick;

                // Pins
                foreach (var pin in customPins)
                {
                    var snPosition = new BasicGeoposition { Latitude = pin.Position.Latitude, Longitude = pin.Position.Longitude };
                    var snPoint = new Geopoint(snPosition);

                    var mapIcon = new MapIcon();
                    mapIcon.Image = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///pin.png"));
                    mapIcon.CollisionBehaviorDesired = MapElementCollisionBehavior.RemainVisible;
                    mapIcon.Location = snPoint;
                    mapIcon.NormalizedAnchorPoint = new Windows.Foundation.Point(0.5, 1.0);

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

                    var polyline = new MapPolyline();
                    polyline.StrokeColor = Windows.UI.Color.FromArgb(128, 255, 0, 0);
                    polyline.StrokeThickness = 5;
                    polyline.Path = new Geopath(coordinates);
                    nativeMap.MapElements.Add(polyline);
                }
            }
        }

        private void OnMapElementClick(MapControl sender, MapElementClickEventArgs args)
        {
            var mapIcon = args.MapElements.FirstOrDefault(x => x is MapIcon) as MapIcon;
            if (mapIcon != null)
            {
                if (!xamarinOverlayShown)
                {
                    var customPin = GetCustomPin(mapIcon.Location.Position);
                    if (customPin == null)
                    {
                        throw new Exception("Custom pin not found");
                    }

                    if (customPin.Id.ToString() == "Xamarin")
                    {
                        if (mapOverlay == null)
                        {
                            mapOverlay = new XamarinMapOverlay(customPin);
                        }

                        var snPosition = new BasicGeoposition { Latitude = customPin.Position.Latitude, Longitude = customPin.Position.Longitude };
                        var snPoint = new Geopoint(snPosition);

                        nativeMap.Children.Add(mapOverlay);
                        MapControl.SetLocation(mapOverlay, snPoint);
                        MapControl.SetNormalizedAnchorPoint(mapOverlay, new Windows.Foundation.Point(0.5, 1.0));
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

        CustomPin GetCustomPin(BasicGeoposition position)
        {
            var pos = new Position(position.Latitude, position.Longitude);
            foreach (var pin in customPins)
            {
                if (pin.Position == pos)
                {
                    return pin;
                }
            }
            return null;
        }
    }
}
