using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Foundation;
using UIKit;
using MapKit;

namespace TTMobileClient.iOS
{
    public class CustomMKAnnotationView : MKAnnotationView
    {
        public bool isWaypoint { get; set; }

        public string Id { get; set; }

        public string Url { get; set; }

        public CustomMKAnnotationView(IMKAnnotation annotation, string id)
            : base(annotation, id)
        {
        }
    }
}