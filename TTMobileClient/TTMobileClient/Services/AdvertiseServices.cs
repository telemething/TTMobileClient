using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TThingComLib.Messages;


namespace TTMobileClient
{
    //*************************************************************************
    /// <summary>
    /// 
    /// </summary>
    //*************************************************************************
    class AdvertiseServices
    {
        private TThingComLib.Repeater _telemetryRepeater = 
            new TThingComLib.Repeater();
        private Timer _advertiseTimer;
        private int _serviceAdvertisePeriodSeconds = 
            AppSettings.ServiceAdvertisePeriodSeconds;
        string _udpBroadcaseIP = AppSettings.UdpBroadcastIP;
        int _thingTelemPort = AppSettings.ThingTelemPort;
        private string _addressPrefix = AppSettings.AddressPrefix;
        private bool _sendSelfTelem = true;
        private bool _initialized = false;
        private string _address;
        List<TThingComLib.Messages.NetworkService> _serviceList = new List<NetworkService>();

        private static AdvertiseServices _singleton = null;

        //*********************************************************************
        /// <summary>
        /// The singleton
        /// </summary>
        //*********************************************************************
        public static AdvertiseServices Singleton
        {
            get 
            {
                if (null == _singleton)
                    _singleton = new AdvertiseServices();
                return _singleton;
            }
        }

        //*********************************************************************
        /// <summary>
        /// Private constructor, force instanciation/access through singleton
        /// </summary>
        //*********************************************************************

        private AdvertiseServices()
        {
            Init();
        }

        //*********************************************************************
        /// <summary>
        /// Initialize service
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
        /// Find IP address of device, doesn't seem to work on iOS
        /// </summary>
        //*********************************************************************

        private string FetchIpAddressOld()
        {
            try
            {
                var addresses = System.Net.Dns.GetHostAddresses(System.Net.Dns.GetHostName());

                foreach (var address in addresses)
                    if (address.ToString().Contains(_addressPrefix))
                        return address.ToString();
            }
            catch(Exception ex)
            {
                //logger.DebugLogError("Fatal Error on permissions: " + ex.Message);
                App.Current.MainPage.DisplayAlert(
                    "FetchIpAddress() Exception:", "Error: " + ex.Message, "Ok");
            }

            return null;
        }

        //*********************************************************************
        /// <summary>
        /// Find IP address of device
        /// </summary>
        //*********************************************************************

        public string FetchIpAddress()
        {
            try
            {

                foreach (var netInterface in NetworkInterface.GetAllNetworkInterfaces())
                    if (netInterface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 ||
                        netInterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                        foreach (var addrInfo in netInterface.GetIPProperties().UnicastAddresses)
                            if (addrInfo.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                                if (addrInfo.Address.ToString().Contains(_addressPrefix))
                                    return addrInfo.Address.ToString();
            }
            catch (Exception ex)
            {
                //logger.DebugLogError("Fatal Error on permissions: " + ex.Message);
                App.Current.MainPage.DisplayAlert(
                    "FetchIpAddress() Exception:", "Error: " + ex.Message, "Ok");
            }

            return null;
        }

        //*********************************************************************
        /// <summary>
        /// Start the advertisment service
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
        /// <param name="service"></param>
        //*********************************************************************
        public void AddServiceToAdvertise(TThingComLib.Messages.NetworkService service)
        {
            _serviceList.Add(service);
        }

        //*********************************************************************
        /// <summary>
        /// Called on a periodic basis to broadcast an advertisement
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
                    TThingComLib.Messages.MessageTypeEnum.Config, "GroundStation", "*")
                {
                    //NetworkServices = BuildServiceList()
                    NetworkServices = _serviceList
                }, false); 
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public class ServiceAdvertismentReceivedEventArgs
        {
            public string Sender { get; set; }
            public List<TThingComLib.Messages.NetworkService> NetworkServices { get; set; }
        }
        public delegate void ServiceAdvertismentReceived(
            object sender, ServiceAdvertismentReceivedEventArgs e);
        public event ServiceAdvertismentReceived ServiceAdvertismentReceivedEvent;

        //*********************************************************************
        /// <summary>
        /// Called by external object to indicate that a service advertisment 
        /// has been received
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //*********************************************************************
        public void ServiceAdvertismentReceivedEventHandler(object sender, 
            TThingComLib.Listener.ServiceAdvertismentReceivedEventArgs e)
        {
            ServiceAdvertismentReceivedEvent?.Invoke(this,
                new ServiceAdvertismentReceivedEventArgs
                {
                    Sender = e.Sender,
                    NetworkServices = e.NetworkServices
                } );
        }

    }
}
