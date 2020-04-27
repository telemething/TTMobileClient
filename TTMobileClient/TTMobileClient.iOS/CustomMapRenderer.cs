using System;
using System.Collections.Generic;
using UIKit;
using System.ComponentModel;
using CoreGraphics;
using TTMobileClient;
using TTMobileClient.iOS;
using MapKit;
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
        private readonly int _pinDropDwellTime = 500;

        UIView customPinView;
        readonly List<Waypoint> _waypoints;
        readonly List<TrackedObject> _trackedObjects;
        SelfObject _selfObject;
        MKPolylineRenderer polylineRenderer;
        private readonly UITapGestureRecognizer _tapRecogniser;
        private bool _viewingPinInfo = false;
        private CustomMap _formsMap;

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

            /*if (e.PropertyName.Equals("Change"))
            {
                var formsMap = (CustomMap)sender;
                var newObject = formsMap.change.SubjectObject;

                if (newObject is Waypoint newWaypoint)
                {
                    _waypoints.Add(newWaypoint);
                    formsMap.Pins.Add(newWaypoint);
                }
            }*/

            if (e.PropertyName.Equals("Change"))
            {
                var formsMap = (CustomMap)sender;
                var newObject = formsMap.change.SubjectObject;

                if (newObject is Waypoint newWaypoint)
                    switch (formsMap.change.ChangeType)
                    {
                        case ChangeHappened.ChangeTypeEnum.Added:
                            _waypoints.Add(newWaypoint);
                            formsMap.Pins.Add(newWaypoint);
                            break;
                        case ChangeHappened.ChangeTypeEnum.Removed:
                            //_waypoints.Remove(newWaypoint);
                            newWaypoint.IsActive = false;
                            formsMap.Pins.Remove(newWaypoint);
                            break;
                    }

                if (newObject is SelfObject so)
                    switch (formsMap.change.ChangeType)
                    {
                        case ChangeHappened.ChangeTypeEnum.Added:
                            AddSelfObject(so, formsMap);
                            break;
                        case ChangeHappened.ChangeTypeEnum.Changed:
                            ChangeSelfObject(so);
                            break;
                    }

                if (newObject is TrackedObject to)
                    switch (formsMap.change.ChangeType)
                    {
                        case ChangeHappened.ChangeTypeEnum.Added:
                            AddTrackedObject(to, formsMap);
                            break;
                        case ChangeHappened.ChangeTypeEnum.Changed:
                            ChangeTrackedObject(to);
                            break;
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
        /// <param name="to"></param>
        /// <param name="formsMap"></param>
        /// 
        //*********************************************************************

        private void AddSelfObject(SelfObject so, CustomMap formsMap)
        {
            _selfObject = so;
            formsMap.Pins.Add(so);
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        /// <param name="to"></param>
        ///
        //*********************************************************************

        private void ChangeSelfObject(SelfObject changedTO)
        {
            _selfObject.Position = new Position(
                changedTO.Position.Latitude, changedTO.Position.Longitude);
            _selfObject.PositionFromSensor = new Position(
                changedTO.PositionFromSensor.Latitude, changedTO.PositionFromSensor.Longitude);
            _selfObject.PositionOffset = new Position(
                changedTO.PositionOffset.Latitude, changedTO.PositionOffset.Longitude);
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        /// <param name="to"></param>
        /// <param name="formsMap"></param>
        /// 
        //*********************************************************************

        private void AddTrackedObject(TrackedObject to, CustomMap formsMap)
        {
            _trackedObjects.Add(to);
            formsMap.Pins.Add(to);
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        /// <param name="to"></param>
        ///
        //*********************************************************************

        private void ChangeTrackedObject(TrackedObject changedTO)
        {
            var trackedObject = GetTrackedObject(changedTO.UniqueId);

            trackedObject.Position = new Position(
                changedTO.Position.Latitude, changedTO.Position.Longitude);
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

                nativeMap.GetViewForAnnotation -= GetViewForAnnotation;
                nativeMap.CalloutAccessoryControlTapped -= OnCalloutAccessoryControlTapped;
                nativeMap.DidSelectAnnotationView -= OnDidSelectAnnotationView;
                nativeMap.DidDeselectAnnotationView -= OnDidDeselectAnnotationView;
                nativeMap.MapLoaded -= OnMapLoaded;

                Control.RemoveGestureRecognizer(_tapRecogniser);

            }

            if (e.NewElement != null)
            {
                _formsMap = (CustomMap)e.NewElement;
                var nativeMap = Control as MKMapView;

                nativeMap.GetViewForAnnotation += GetViewForAnnotation;
                nativeMap.CalloutAccessoryControlTapped += OnCalloutAccessoryControlTapped;
                nativeMap.DidSelectAnnotationView += OnDidSelectAnnotationView;
                nativeMap.DidDeselectAnnotationView += OnDidDeselectAnnotationView;
                nativeMap.MapLoaded += OnMapLoaded;

                Control.AddGestureRecognizer(_tapRecogniser);
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

        private void OnMapLoaded(object sender, EventArgs e)
        {
            _formsMap?.MapReadyCallback();
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

            if(_selfObject != null)
            {
                annotationView = mapView.DequeueReusableAnnotation(_selfObject.MarkerId.ToString());
                if (annotationView == null)
                {
                    annotationView = new CustomMKAnnotationView(annotation, _selfObject.MarkerId.ToString())
                    {
                        Image = UIImage.FromFile("self.png"),
                        CalloutOffset = new CGPoint(0, 0),
                        LeftCalloutAccessoryView = new UIImageView(UIImage.FromFile("monkey.png")),
                        RightCalloutAccessoryView = UIButton.FromType(UIButtonType.DetailDisclosure)
                    };

                    ((CustomMKAnnotationView)annotationView).subjectObject = _selfObject;
                    ((CustomMKAnnotationView)annotationView).Id = _selfObject.MarkerId.ToString();
                    ((CustomMKAnnotationView)annotationView).Url = _selfObject.Url;
                }

                annotationView.CanShowCallout = true;
                return annotationView;
            }

            // If we have no waypoints or tracked objects then this is not a waypoint or tracked object
            if (0 == _waypoints.Count && 0 == _trackedObjects.Count)
                return null;

            var waypoint = GetWaypoint(annotation as MKPointAnnotation);
            if (waypoint != null)
            {
                annotationView = mapView.DequeueReusableAnnotation(waypoint.MarkerId.ToString());
                if (annotationView == null)
                {
                    annotationView = new CustomMKAnnotationView(annotation, waypoint.MarkerId.ToString())
                    {
                        Image = UIImage.FromFile("RoutePin.png"),
                        CalloutOffset = new CGPoint(0, 0),
                        LeftCalloutAccessoryView = new UIImageView(UIImage.FromFile("monkey.png")),
                        RightCalloutAccessoryView = UIButton.FromType(UIButtonType.DetailDisclosure)
                    };

                    ((CustomMKAnnotationView)annotationView).subjectObject = waypoint;
                    ((CustomMKAnnotationView)annotationView).Id = waypoint.MarkerId.ToString();
                    ((CustomMKAnnotationView)annotationView).Url = waypoint.Url;
                }

                annotationView.CanShowCallout = true;
                return annotationView;
            }

            //var ddd = new UIImageView(UIImage.FromFile("uav.png"));

            var trackedObject = GetTrackedObject(annotation as MKPointAnnotation);
            if (trackedObject != null)
            {
                annotationView = mapView.DequeueReusableAnnotation(trackedObject.MarkerId.ToString());
                if (annotationView == null)
                {
                    annotationView = new TrackedObjectAnnotationView(annotation, trackedObject.MarkerId.ToString())
                    {
                        Image = UIImage.FromFile("uav.png"),
                        CalloutOffset = new CGPoint(0, 0),
                        LeftCalloutAccessoryView = new UIImageView(UIImage.FromFile("uav.png")),
                        RightCalloutAccessoryView = UIButton.FromType(UIButtonType.DetailDisclosure)
                    };

                    ((TrackedObjectAnnotationView)annotationView).subjectObject = trackedObject;
                    ((TrackedObjectAnnotationView)annotationView).Id = trackedObject.MarkerId.ToString();
                    ((TrackedObjectAnnotationView)annotationView).Url = trackedObject.Url;
                }

                annotationView.CanShowCallout = true;
                return annotationView;
            }

            throw new Exception("Custom pin not found");      
        }

        /*protected override MKAnnotationView GetViewForAnnotation(
            MKMapView mapView, IMKAnnotation annotation)
        {
            MKAnnotationView annotationView = null;

            if (annotation is MKUserLocation)
                return null;

            // If we have no custom pins the this is not a custom pin
            if (0 == _waypoints.Count)
                return null;

            var customPin = GetWaypoint(annotation as MKPointAnnotation);
            if (customPin == null)
            {
                throw new Exception("Custom pin not found");
            }

            annotationView = mapView.DequeueReusableAnnotation(customPin.Id.ToString());
            if (annotationView == null)
            {
                annotationView = new CustomMKAnnotationView(annotation, customPin.Id.ToString())
                {
                    Image = UIImage.FromFile("RoutePin.png"),
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
        }*/

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
            //var customView = e.View as CustomMKAnnotationView;
            customPinView = new UIView();

            if (e.View is CustomMKAnnotationView caView)
            {
                _viewingPinInfo = true;

                customPinView.Frame = new CGRect(0, 0, 200, 84);
                var image = new UIImageView(
                    new CGRect(0, 0, 200, 84))
                    { Image = UIImage.FromFile("xamarin.png")};
                customPinView.AddSubview(image);
                customPinView.Center = new CGPoint(0, -(e.View.Frame.Height + 75));
                e.View.AddSubview(customPinView);
            }
            else if (e.View is TrackedObjectAnnotationView toaView)
            {
                _viewingPinInfo = true;

                customPinView.Frame = new CGRect(0, 0, 200, 84);
                var image = new UIImageView(
                    new CGRect(0, 0, 200, 84))
                    { Image = UIImage.FromFile("uav.png")};
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

        Waypoint GetWaypoint(MKPointAnnotation annotation)
        {
            if (null == annotation)
                return null;

            if (null == _waypoints)
                return null;

            var position = new Position(annotation.Coordinate.Latitude,
                annotation.Coordinate.Longitude);

            foreach (var pin in _waypoints)
                if (pin.Position == position)
                    return pin;

            return null;
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

        TrackedObject GetTrackedObject(MKPointAnnotation annotation)
        {
            if (null == annotation)
                return null;

            if (null == _trackedObjects)
                return null;

            var position = new Position(annotation.Coordinate.Latitude,
                annotation.Coordinate.Longitude);

            foreach (var pin in _trackedObjects)
                if (pin.Position.Equals(position))
                    return pin;

            return null;
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

        TrackedObject GetTrackedObject(string id)
        {
            if (null == id)
                return null;

            if (null == _trackedObjects)
                return null;

            foreach (var to in _trackedObjects)
                if (to.UniqueId.Equals(id))
                    return to;

            return null;
        }
    }
}
