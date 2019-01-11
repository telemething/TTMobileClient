using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using TransportType = Microsoft.Azure.Devices.TransportType;

namespace AzureIotLib
{
    public class AzureIotDevice
    {
        private RegistryManager _registryManager;

        public delegate void GotMessageCallback(byte[] messagePayload);

        private GotMessageCallback _gotMessageCallback;

        private string _iotHubConnectionString =
            "HostName=tfzhubt1.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=9+7tOF0ntGwoZFTkP7dwStYONKe6WoyXQvioRE3dcnI=";

        private string _iotHubName =
            "tfzhubt1";

        private string _deviceId =
            "ObserverDevice1";

        private string _key =
            "HNtTSw7OIW8eTnB9gqnN1S0SRbxnBTK6SwoenmsWJdQ=";

        /*private string _iotHubConnectionString =
            "HostName=TTIOTHub1.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=MCm7FZbAEJmZD56vaOlqE4HMLPNmk+dLFYV4en+nKB0=";

        private string _iotHubName =
            "TTIOTHub1";

        private string _deviceId =
            "TestDevice1";

        private string _key =
            "zkurodvxaHCbiisBGQ5C3wpK8hxPIyKeHSbbXeRWlJM=";*/

        public void ConnectToIotHub()
        {
            if (null == _registryManager)
                _registryManager = RegistryManager.CreateFromConnectionString(_iotHubConnectionString);
        }

        public async Task<List<Twin>> GetDeviceTwinList()
        {
            ConnectToIotHub();

            var query = _registryManager.CreateQuery(@"Select * from devices", 100);
            var twins = await query.GetNextAsTwinAsync();

            return twins.ToList();
        }

        public async Task<Twin> GetDeviceTwin(String deviceId)
        {
            ConnectToIotHub();

            var query = _registryManager.CreateQuery($"Select * from devices where DeviceiD = '{deviceId}' ", 100);
            var twins = await query.GetNextAsTwinAsync();

            return twins.FirstOrDefault();
        }

        public async Task<Twin> ConnectToTwin(String deviceId)
        {
            ConnectToIotHub();

            var query = _registryManager.CreateQuery($"Select * from devices where DeviceiD = '{deviceId}' ", 100);
            var twins = await query.GetNextAsTwinAsync();

            return twins.FirstOrDefault();
        }

        public DeviceClient GetDevice(string deviceId, string key)
        {
            DeviceClient deviceClient;

            var deviceConnectionString = $"HostName={_iotHubName}.azure-devices.net;DeviceId={deviceId};SharedAccessKey={key}";

            //deviceConnectionString = "HostName=tfzhubt1.azure-devices.net;DeviceId=ObserverDevice1;SharedAccessKey=HNtTSw7OIW8eTnB9gqnN1S0SRbxnBTK6SwoenmsWJdQ=";
            deviceConnectionString = "HostName=tfzhubt1.azure-devices.net;DeviceId=ObserverDevice2;SharedAccessKey=BW9uKrE0SDpfeRIfBq6L8cuALkwsbDddZ52KZ6Qb6I0=";

            //deviceClient = DeviceClient.CreateFromConnectionString(deviceConnectionString, Microsoft.Azure.Devices.Client.TransportType.Amqp);  // iOS ?
            deviceClient = DeviceClient.CreateFromConnectionString(deviceConnectionString, Microsoft.Azure.Devices.Client.TransportType.Mqtt);  // iOS ?

            return deviceClient;
        }

        private DeviceClient _deviceClient;

        public async Task<int> ConnectToDevice(GotMessageCallback callback)
        {
            _gotMessageCallback = callback;

            _deviceClient = GetDevice(_deviceId, _key);
            object context = null;

            //dev.SetDesiredPropertyUpdateCallbackAsync(Callback, context);

            //await dev.SetMethodDefaultHandlerAsync(new MethodCallback(Target), context);



            //var ddd = "{\"state\": {\"reported\": {\"door\": \"OFF\"}}}";

            //var message = new Microsoft.Azure.Devices.Client.Message(Encoding.UTF8.GetBytes(ddd));

            //await dev.SendEventAsync(message);

            return await Listen(_deviceClient);
        }

        public async void SendD2C(string payload)
        {
            if (null == _deviceClient)
            {
                throw new Exception("AzureIotDevice.SendD2C() : _deviceClient = NULL");
            }

            var message = new Microsoft.Azure.Devices.Client.Message(Encoding.UTF8.GetBytes(payload));
            await _deviceClient.SendEventAsync(message);
        }

        private async Task<int> Listen(DeviceClient device)
        {
            while (true)
            {
                try
                {
                    var recvMessage = await device.ReceiveAsync();

                    if (null == recvMessage)
                        continue;

                    try
                    {
                        var messageBytes = recvMessage.GetBytes();
                        string strData = Encoding.UTF8.GetString(messageBytes);
                        //var ff = recvMessage.MessageId;
                        Console.WriteLine($"### Message: '{strData}' ###");

                        //_gotMessageCallback?.BeginInvoke(messageBytes, ar => { }, null);  // works on iOS

                        _gotMessageCallback?.Invoke(messageBytes);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }

                    await device.CompleteAsync(recvMessage);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            return 0;
        }

        private Task<MethodResponse> Target(MethodRequest methodrequest, object usercontext)
        {
            throw new NotImplementedException();
        }

        private Task Callback(TwinCollection desiredproperties, object usercontext)
        {
            var fff = desiredproperties;

            return Task.CompletedTask;
        }
    }
}

