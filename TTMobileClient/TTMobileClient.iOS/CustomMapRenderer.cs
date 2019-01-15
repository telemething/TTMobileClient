using System;
using System.Collections.Generic;
using UIKit;
using System.ComponentModel;
using CoreGraphics;
using TTMobileClient;
using TTMobileClient.iOS;
using MapKit;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Maps.iOS;
using Xamarin.Forms.Platform.iOS;
using CoreLocation;
using Foundation;

[assembly: ExportRenderer(typeof(CustomMap), typeof(CustomMapRenderer))]
namespace TTMobileClient.iOS
{
    public class CustomMapRenderer : MapRenderer
    {
        private int _pinDropDwellTime = 500;

        UIView customPinView;
        List<Waypoint> customPins;
        MKPolylineRenderer polylineRenderer;
        private readonly UITapGestureRecognizer _tapRecogniser;
        private bool _viewingPinInfo = false;

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        /// 
        //*********************************************************************

        public CustomMapRenderer()
        {
            customPins = new List<Waypoint>(4);
            _tapRecogniser = new UITapGestureRecognizer(OnTap)
            {
                NumberOfTapsRequired = 1,
                NumberOfTouchesRequired = 1
            };
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        /// <param name="recognizer"></param>
        ///
        //*********************************************************************

        private void OnTap(UITapGestureRecognizer recognizer)
        {
            // if user is viewing pin info, then an outside click just clears
            // the pin info, and we don't drop a new pin
            if (_viewingPinInfo)
                return;

            var cgPoint = recognizer.LocationInView(Control);
            var mapView = (MKMapView)Control;
            var location = mapView.ConvertPoint(cgPoint, Control);

            // Absurd hack to accomodate annotation event occuring after pin
            // drop event
            var timer = NSTimer.CreateTimer(
                new TimeSpan(0, 0, 0, 0, _pinDropDwellTime),
                nsTimer =>
                {
                    if (!_viewingPinInfo)
                        ((CustomMap)Element).MapClickCallback(
                            location.Latitude, location.Longitude);
                });

            NSRunLoop.Main.AddTimer(timer, NSRunLoopMode.Common);
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

        protected override void OnElementPropertyChanged(object sender, 
            PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName.Equals("Change"))
            {
                var formsMap = (CustomMap)sender;
                var newObject = formsMap.change.addedObject;

                if (newObject is Waypoint newPin)
                {
                    customPins.Add(newPin);
                    formsMap.Pins.Add(newPin);
                }
            }
            if (e.PropertyName.Equals("RouteCoordinates"))
            {
                var formsMap = (CustomMap)sender;
                //routeCoordinates = formsMap.RouteCoordinates;
                //Control.GetMapAsync(this);

                var nativeMap = Control as MKMapView;

                nativeMap.OverlayRenderer = GetOverlayRenderer;

                CLLocationCoordinate2D[] coords = 
                    new CLLocationCoordinate2D[formsMap.RouteCoordinates.Count];

                int index = 0;
                foreach (var position in formsMap.RouteCoordinates)
                {
                    coords[index] = new CLLocationCoordinate2D(
                        position.Latitude, position.Longitude);
                    index++;
                }

                var routeOverlay = MKPolyline.FromCoordinates(coords);
                nativeMap.AddOverlay(routeOverlay);
            }
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mapView"></param>
        /// <param name="overlayWrapper"></param>
        /// <returns></returns>
        ///
        //*********************************************************************

        MKOverlayRenderer GetOverlayRenderer(MKMapView mapView, IMKOverlay overlayWrapper)
        {
            if (polylineRenderer == null && !Equals(overlayWrapper, null))
            {
                var overlay = ObjCRuntime.Runtime.GetNSObject(overlayWrapper.Handle) as IMKOverlay;
                polylineRenderer = new MKPolylineRenderer(overlay as MKPolyline)
                {
                    FillColor = UIColor.Blue,
                    StrokeColor = UIColor.Red,
                    LineWidth = 3,
                    Alpha = 0.4f
                };
            }
            return polylineRenderer;
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        ///
        //*********************************************************************

        protected override void OnElementChanged(ElementChangedEventArgs<View> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null)
            {
                var nativeMap = Control as MKMapView;
                nativeMap.GetViewForAnnotation = null;
                nativeMap.CalloutAccessoryControlTapped -= OnCalloutAccessoryControlTapped;
                nativeMap.DidSelectAnnotationView -= OnDidSelectAnnotationView;
                nativeMap.DidDeselectAnnotationView -= OnDidDeselectAnnotationView;

                Control.RemoveGestureRecognizer(_tapRecogniser);

            }

            if (e.NewElement != null)
            {
                var formsMap = (CustomMap)e.NewElement;
                var nativeMap = Control as MKMapView;

                //customPins = formsMap.CustomPins;

                //if(e is Waypoint)

                nativeMap.GetViewForAnnotation = GetViewForAnnotation;
                nativeMap.CalloutAccessoryControlTapped += OnCalloutAccessoryControlTapped;
                nativeMap.DidSelectAnnotationView += OnDidSelectAnnotationView;
                nativeMap.DidDeselectAnnotationView += OnDidDeselectAnnotationView;

                Control.AddGestureRecognizer(_tapRecogniser);
            }
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mapView"></param>
        /// <param name="annotation"></param>
        /// <returns></returns>
        /// 
        //*********************************************************************

        protected override MKAnnotationView GetViewForAnnotation(
            MKMapView mapView, IMKAnnotation annotation)
        {
            MKAnnotationView annotationView = null;

            if (annotation is MKUserLocation)
                return null;

            var customPin = GetCustomPin(annotation as MKPointAnnotation);
            if (customPin == null)
            {
                throw new Exception("Custom pin not found");
            }

            annotationView = mapView.DequeueReusableAnnotation(customPin.Id.ToString());
            if (annotationView == null)
            {
                annotationView = new CustomMKAnnotationView(annotation, customPin.Id.ToString())
                {
                    Image = UIImage.FromFile("pin.png"),
                    CalloutOffset = new CGPoint(0, 0),
                    LeftCalloutAccessoryView = new UIImageView(UIImage.FromFile("monkey.png")),
                    RightCalloutAccessoryView = UIButton.FromType(UIButtonType.DetailDisclosure)
                };

                ((CustomMKAnnotationView)annotationView).isWaypoint = true;
                ((CustomMKAnnotationView)annotationView).Id = customPin.Id.ToString();
                ((CustomMKAnnotationView)annotationView).Url = customPin.Url;
            }
            annotationView.CanShowCallout = true;

            return annotationView;
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

        void OnCalloutAccessoryControlTapped(
            object sender, MKMapViewAccessoryTappedEventArgs e)
        {
            var customView = e.View as CustomMKAnnotationView;
            if (!string.IsNullOrWhiteSpace(customView.Url))
            {
                UIApplication.SharedApplication.OpenUrl(new Foundation.NSUrl(customView.Url));
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

        void OnDidSelectAnnotationView(object sender, MKAnnotationViewEventArgs e)
        {
            var customView = e.View as CustomMKAnnotationView;
            customPinView = new UIView();

            //if (customView.Id == "Xamarin")
            if (customView.isWaypoint)
            {
                _viewingPinInfo = true;

                customPinView.Frame = new CGRect(0, 0, 200, 84);
                var image = new UIImageView(new CGRect(0, 0, 200, 84));
                image.Image = UIImage.FromFile("xamarin.png");
                customPinView.AddSubview(image);
                customPinView.Center = new CGPoint(0, -(e.View.Frame.Height + 75));
                e.View.AddSubview(customPinView);
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

        void OnDidDeselectAnnotationView(object sender, MKAnnotationViewEventArgs e)
        {
            if (!e.View.Selected)
            {
                customPinView.RemoveFromSuperview();
                customPinView.Dispose();
                customPinView = null;

                _viewingPinInfo = false;
            }
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        /// <param name="annotation"></param>
        /// <returns></returns>
        ///
        //*********************************************************************

        Waypoint GetCustomPin(MKPointAnnotation annotation)
        {
            var position = new Position(annotation.Coordinate.Latitude, 
                annotation.Coordinate.Longitude);
            foreach (var pin in customPins)
            {
                if (pin.Position == position)
                {
                    return pin;
                }
            }
            return null;
        }
    }
}
