using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TThingComLib.Messages;

namespace TTMobileClient
{
    class AdvertiseServices
    {
        private TThingComLib.Repeater _telemetryRepeater = new TThingComLib.Repeater();
        private Timer _advertiseTimer;
        private int _serviceAdvertisePeriodSeconds = AppSettings.ServiceAdvertisePeriodSeconds;
        string _udpBroadcaseIP = AppSettings.UdpBroadcastIP;
        int _thingTelemPort = AppSettings.ThingTelemPort;
        private string _addressPrefix = AppSettings.AddressPrefix;
        private bool _sendSelfTelem = true;
        private bool _initialized = false;
        private string _address;

        //*********************************************************************
        /// <summary>
        /// 
        /// </summary>
        //*********************************************************************

        public AdvertiseServices()
        {
            Init();
        }

        //*********************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        //*********************************************************************

        private async Task<bool> Init()
        {
            try
            {
                _telemetryRepeater.AddTransport(
                    TThingComLib.Repeater.TransportEnum.UDP,
                    TThingComLib.Repeater.DialectEnum.ThingTelem,
                    _udpBroadcaseIP, _thingTelemPort, 500);
                _initialized = true;
            }
            catch (Exception ex)
            {
                //logger.DebugLogError("Fatal Error on permissions: " + ex.Message);
                await App.Current.MainPage.DisplayAlert(
                    "StartRepeater() Exception:", "Error: " + ex.Message, "Ok");
                return false;
            }

            _address = FetchIpAddress();

            return true;
        }

        //*********************************************************************
        /// <summary>
        /// 
        /// </summary>
        //*********************************************************************

        private string FetchIpAddress()
        {
            var addresses = System.Net.Dns.GetHostAddresses(System.Net.Dns.GetHostName());

            foreach(var address in addresses)
                if (address.ToString().Contains(_addressPrefix))
                    return address.ToString();

            return null;
        }

        //*********************************************************************
        /// <summary>
        /// 
        /// </summary>
        //*********************************************************************

        public void StartAdvertising()
        {
            object obj = null;
            if (_sendSelfTelem)
            {
                _advertiseTimer = new Timer(SelfTelemTimerCallback, obj,
                    new TimeSpan(0, 0, 0, 0),
                    new TimeSpan(0, 0, 0, _serviceAdvertisePeriodSeconds));
            }
        }

        //*********************************************************************
        /// <summary>
        /// 
        /// </summary>
        //*********************************************************************

        private List<TThingComLib.Messages.NetworkService> BuildServiceList()
        {
            var serviceList = new List<TThingComLib.Messages.NetworkService>();

            serviceList.Add(new NetworkService($"http://{_address}:8080/config",
                NetworkTypeEnum.UDP, ServiceTypeEnum.Config, ServiceRoleEnum.Server));

            serviceList.Add(new NetworkService($"http://{_address}:8080/tiles",
                NetworkTypeEnum.UDP, ServiceTypeEnum.GeoTile, ServiceRoleEnum.Server));

            return serviceList;
        }

        //*********************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        //*********************************************************************

        private void SelfTelemTimerCallback(object state)
        {
            if (!_initialized)
                return;

            try
            {
                _telemetryRepeater?.Send(new TThingComLib.Messages.Message(
                    TThingComLib.Messages.MessageTypeEnum.Telem, "GroundStation", "*")
                {
                    NetworkServices = BuildServiceList()
                }, false); 
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
