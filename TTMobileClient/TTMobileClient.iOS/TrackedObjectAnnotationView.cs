using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Foundation;
using UIKit;
using MapKit;

namespace TTMobileClient.iOS
{
    public class TrackedObjectAnnotationView : MKAnnotationView
    {
        public object subjectObject { get; set; }

        public string Id { get; set; }

        public string Url { get; set; }

        public TrackedObjectAnnotationView(IMKAnnotation annotation, string id)
            : base(annotation, id)
        {
        }
    }
}