using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace TTMobileClient
{
    public class AppSettings
    {
        //private static string _defaultRobotRosbridgeUrl = "ws://theflyingzephyr.ddns.net:9090";
        private static string _defaultRobotRosbridgeUrl = "ws://192.168.1.30:9090";
        private static string _defaultRobotRosVideoUrl = "http://192.168.1.30:8080";
        private static double _defaultGeoCoordsLat = 47.6062;
        private static double _defaultGeoCoordsLon = -122.3321;

        public static string DefaultRobotRosbridgeUrl
        {
            get { return _defaultRobotRosbridgeUrl; }
        }

        public static string DefaultRobotRosVideoUrl
        {
            get { return _defaultRobotRosVideoUrl; }
        }

        public static double DefaultGeoCoordsLat
        {
            get { return _defaultGeoCoordsLat; }
        }

        public static double DefaultGeoCoordsLon
        {
            get { return _defaultGeoCoordsLon; }
        }
    }
}
