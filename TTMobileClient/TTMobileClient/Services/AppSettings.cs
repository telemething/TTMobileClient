using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using Xamarin.Forms;

namespace TTMobileClient
{
    public class AppSettings
    {
        //private static string _defaultRobotRosbridgeUrl = "ws://theflyingzephyr.ddns.net:9090";
        //private static string _defaultRobotRosbridgeUrl = "ws://192.168.1.30:9090";
        private static string _defaultRobotRosbridgeUrl = "ws://192.168.1.38:9090";
        private static string _defaultRobotRosVideoUrl = "http://192.168.1.30:8080";
        private static double _defaultGeoCoordsLat = 47.6062;
        private static double _defaultGeoCoordsLon = -122.3321;

        public static int ServiceAdvertisePeriodSeconds { get; } = 1;    
        public static string UdpBroadcastIP { get; } = "192.168.1.255"; 
        public static int ThingTelemPort { get; } = 45679; 
        public static string AddressPrefix { get; } = "192.168.1";

        public static int HeartbeatPeriodSeconds { get; } = 30;   
        public static int SelfTelemPeriodSeconds { get; } = 1;
        public static string WebApiUrl { get; } = "http://*:8877";

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

    public class SPT1
    {
        private LocalFileSettingsProvider _sp;

        public static void T1()
        {

        }
    }

    //*************************************************************************
    /// <summary>
    /// App setting
    /// </summary>
    //*************************************************************************

    public class AppSetting
    {
        private string _name;
        private string _description;
        private object _value;
        private System.Type _type;
        private bool _changed = false;

        public string name => _name;
        //public object Value => _value;
        public object Description => _description;
        public System.Type Type => _type;
        public object Value { set { _value = value; _changed = true; } get => _value; }
        public bool Changed => _changed;

        public AppSetting(string name, object value, string description)
        {
            _name = name;
            _value = value;
            _description = description;
            _type = value.GetType();
        }
    }

    //*************************************************************************
    /// <summary>
    /// Container of app settings
    /// </summary>
    //*************************************************************************

    public class AppSettingCollection
    {
        private string _name;
        private string _description;
        private List<AppSetting> _appSettings;

        public string name => _name;
        //public object Value => _value;
        public object Description => _description;
        public List<AppSetting> AppSettings => _appSettings;

        public AppSettingCollection(string name, string description, List<AppSetting> appSettings)
        {
            _name = name;
            _description = description;
            _appSettings = appSettings;
        }
    }

    //*************************************************************************
    /// <summary>
    /// Container of portable app settings
    /// </summary>
    //*************************************************************************

    public class PortableAppSettings
    {
        private string _name;
        private string _description;
        private List<AppSettingCollection> _appSettingCollections;

        public string name => _name;
        //public object Value => _value;
        public object Description => _description;
        public List<AppSettingCollection> AppSettingCollections => _appSettingCollections;

        //*********************************************************************
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="appSettingCollections"></param>
        //*********************************************************************
        public PortableAppSettings(string name, string description,
            List<AppSettingCollection> appSettingCollections)
        {
            _name = name;
            _description = description;
            _appSettingCollections = appSettingCollections;
        }

        //*********************************************************************
        /// <summary>
        /// Hydrate an instance of this class from serialized data
        /// </summary>
        /// <param name="serializedData"></param>
        /// <returns></returns>
        //*********************************************************************
        public static PortableAppSettings Deserialize(string serializedData)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<PortableAppSettings>(serializedData);
        }

        //*********************************************************************
        /// <summary>
        /// Create serialized data from this instance
        /// </summary>
        /// <returns></returns>
        //*********************************************************************
        public string Serialize()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }

        //*********************************************************************
        /// <summary>
        /// Find the appsetting of the given longname
        /// </summary>
        /// <param name="automationId"></param>
        /// <returns></returns>
        //*********************************************************************
        public AppSetting FindAppSetting(string longname)
        {
            if (null == longname)
                return null;

            foreach (var settingsCollection in _appSettingCollections)
            {
                if (longname.Contains(settingsCollection.name))
                    foreach (var setting in settingsCollection.AppSettings)
                    {
                        if ((settingsCollection.name + setting.name).Equals(longname))
                            return setting;
                    }
            }

            return null;
        }

        //*********************************************************************
        /// <summary>
        /// Update the value of the app setting of the given longname
        /// </summary>
        /// <param name="longName"></param>
        /// <param name="settingValue"></param>
        //*********************************************************************

        public void UpdateValue(string longName, object settingValue)
        {
            var appSetting = FindAppSetting(longName);

            if (null == appSetting)
                return;

            switch (appSetting.Type)
            {
                case Type tipe when tipe == typeof(int):
                    appSetting.Value = Convert.ToInt32(settingValue);
                    break;
                case Type tipe when tipe == typeof(Int64):
                    appSetting.Value = Convert.ToInt64(settingValue);
                    break;
                case Type tipe when tipe == typeof(bool):
                    appSetting.Value = Convert.ToBoolean(settingValue);
                    break;
                case Type tipe when tipe == typeof(float):
                    appSetting.Value = Convert.ToDouble(settingValue);
                    break;
                case Type tipe when tipe == typeof(double):
                    appSetting.Value = Convert.ToDouble(settingValue);
                    break;
                case Type tipe when tipe == typeof(string):
                    appSetting.Value = Convert.ToString(settingValue);
                    break;
                default:
                    break;
            }
        }

        //*********************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        //*********************************************************************
        public static PortableAppSettings GetTestData()
        {
            string data = "{ \"name\":\"TThingUnity\",\"Description\":\"TThing Unity\",\"AppSettingCollections\":[{\"name\":\"Things Manager\",\"Description\":\"Things Manager Settings\",\"AppSettings\":[{\"name\":\"TelemetryPort\",\"Description\":\"The UDP port on which to monitor telemetry messages\",\"Type\":\"System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\",\"Value\":45679}]},{\"name\":\"Game\",\"Description\":\"Game Settings\",\"AppSettings\":[{\"name\":\"PlaceObjectsAboveTerrain\",\"Description\":\"PlaceObjectsAboveTerrain\",\"Type\":\"System.Boolean, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\",\"Value\":true},{\"name\":\"DefaultLerpTimeSpan\",\"Description\":\"DefaultLerpTimeSpan\",\"Type\":\"System.Single, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\",\"Value\":1.0},{\"name\":\"UseFlatTerrain\",\"Description\":\"If true, will use a flat plane, otherwise will use a GEO plane\",\"Type\":\"System.Boolean, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\",\"Value\":false},{\"name\":\"HaloScaleFactor\",\"Description\":\"HaloScaleFactor\",\"Type\":\"System.Single, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\",\"Value\":16.0}]},{\"name\":\"UAV\",\"Description\":\"UAV Settings\",\"AppSettings\":[{\"name\":\"AltitudeOffset\",\"Description\":\"AltitudeOffset\",\"Type\":\"System.Single, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\",\"Value\":2.0}]},{\"name\":\"Self\",\"Description\":\"Self Settings\",\"AppSettings\":[{\"name\":\"MyThingId\",\"Description\":\"The ID of the self object in telemetry messages\",\"Type\":\"System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\",\"Value\":\"self\"},{\"name\":\"MainCameraAltitudeOverTerrainOffset\",\"Description\":\"MainCameraAltitudeOverTerrainOffset\",\"Type\":\"System.Single, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\",\"Value\":2.0}]},{\"name\":\"Terrain\",\"Description\":\"Terrain Settings\",\"AppSettings\":[{\"name\":\"TerrainZoomLevel\",\"Description\":\"The zoom level fetched from the tile server\",\"Type\":\"System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\",\"Value\":18},{\"name\":\"TerrainTilesPerSide\",\"Description\":\"The number of tiles per edge (-1 because center tile)\",\"Type\":\"System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\",\"Value\":9}]}]}";
            return Deserialize(data);
        }
    }
}


