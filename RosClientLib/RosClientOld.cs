using System;
using System.Collections.Generic;
using System.Text;
using RosSharp.RosBridgeClient;

namespace RosClientLibBroke
{
    using System;
    using System.Net.WebSockets;
    using System.Threading;
    using RosbridgeNet.RosbridgeClient.Common;
    using RosbridgeNet.RosbridgeClient.Common.Interfaces;
    using RosbridgeNet.RosbridgeClient.ProtocolV2;
    using RosbridgeNet.RosbridgeClient.ProtocolV2.Generics;
    using RosbridgeNet.RosbridgeClient.ProtocolV2.Generics.Interfaces;
    using RosbridgeNet.RosbridgeClient.Common.Attributes;
    using RosClientLib;
    using System.Threading.Tasks;

    /*public class RosClient : RosClientLib.IRosClient
    {
        public IRosbridgeMessageDispatcher MessageDispatcher { get; private set; }
        private ISocket _socket;
        private IRosbridgeMessageSerializer _messageSerializer;
        private string _webSocketUri;
        private CancellationTokenSource _cts;
        private int _timeoutMs = 10000;

        //*********************************************************************
        //*
        //*
        //*
        //*********************************************************************

        public RosClient()
        {
        }

        //*********************************************************************
        //*
        //*
        //*
        //*********************************************************************

        public RosClient(string uri)
        {
            Connect(uri, null);
        }

        //*********************************************************************
        //*
        //* "ws://localhost:9090"; "ws://192.168.1.30:9090";
        //*
        //*********************************************************************

        public void Connect(string uri, CancellationTokenSource cts)
        {
            _webSocketUri = uri;
            _cts = cts ?? new CancellationTokenSource();

            Connect(new Uri(_webSocketUri));
        }

        //*********************************************************************
        //*
        //*
        //*
        //*********************************************************************

        IRosbridgeMessageDispatcher Connect(Uri webSocketAddress)
        {
            _socket = new Socket(new ClientWebSocket(), webSocketAddress, _cts);
            _messageSerializer = new RosbridgeMessageSerializer();
            MessageDispatcher = new RosbridgeMessageDispatcher(_socket, _messageSerializer);

            MessageDispatcher.StartAsync().Wait(_timeoutMs, _cts.Token);

            return MessageDispatcher;
        }

        //*********************************************************************
        //*
        //*
        //*
        //*********************************************************************

        IRosbridgeMessageDispatcher Connect(Uri webSocketAddress, CancellationTokenSource cts)
        {
            _socket = new Socket(new ClientWebSocket(), webSocketAddress, cts);
            _messageSerializer = new RosbridgeMessageSerializer();
            MessageDispatcher = new RosbridgeMessageDispatcher(_socket, _messageSerializer);

            MessageDispatcher.StartAsync().Wait(_timeoutMs, cts.Token);

            return MessageDispatcher;
        }

        //*********************************************************************
        //*
        //*
        //*
        //*********************************************************************

        public async Task<object> CallServiceAsync(RosClientLib.IRosOp rosOp)
        {
            return await rosOp.CallServiceAsync(this);
        }

        #region Tests

        public static void WaypointTest()
        {
            IRosClient rc = new RosClientLib.RosClient("ws://192.168.1.30:9090");
            var wp = new RosClientLib.Waypoints();
            wp.CreateTestWaypoints();
            var resp = rc.CallServiceAsync(wp).Result;
        }


        public static void TopicSubscribeTest()
        {
            var rc = new RosClientLibBroke.RosClient("ws://192.168.1.30:9090");
            rc.SubscribeMissionStatusTest();

            Console.WriteLine("Press any key to unsubscribe...");
            Console.ReadKey(true);
        }

        [RosMessageType("geometry_msgs/Twist")]
        public class Twist
        {
            public Vector Linear { get; set; }
            public Vector Angular { get; set; }

            public override string ToString()
            {
                return string.Format("linear: {0}, angular: {1}", Linear.ToString(), Angular.ToString());
            }
        }

        public class Vector
        {
            public float X { get; set; }
            public float Y { get; set; }
            public float Z { get; set; }

            public override string ToString()
            {
                return string.Format("x: {0}, y: {1}, z: {2}", X, Y, Z);
            }
        }

        class Req
        {
            public Req()
            {
            }

            public string Arg1;
        }

        class Resp
        {
            public Resp()
            {
            }

            public string Result;
        }

        class ReqO
        {
            public ReqO()
            {
            }

        }

        [RosMessageType("sensor_msgs/NavSatFix")]
        class RespO
        {
            public RespO()
            {
            }

            public int status;
        }

        [RosMessageType("tt_mavros_wp_mission/MissionStatus")]
        public class MissionStatus
        {
            public MissionStatus()
            { }

            public double x_lat;
            public double y_long;
            public double z_alt;

            public override string ToString()
            {
                return $"lat: {x_lat}, long: {y_long}, alt {z_alt}";
            }
        }


        public void Test2()
        {
            string webSocketUri = "ws://192.168.1.30:9090";
            //string webSocketUri = "ws://localhost:9090";
            _cts = new CancellationTokenSource();

            IRosbridgeMessageDispatcher messageDispatcher = Connect(new Uri(webSocketUri), _cts);

            SubscribeTest(messageDispatcher);
            PublisherTest(messageDispatcher);
            CallServiceTest(messageDispatcher);
        }

        void CallServiceTest(IRosbridgeMessageDispatcher messageDispatcher)
        {
            //IRosServiceClient<req, resp> serviceClient = new RosServiceClient<req, resp>(messageDispatcher, "tt_mission_master/tt_mission");
            var serviceClient = new RosServiceClient<Req, Resp>(messageDispatcher);

            try
            {
                var oo = serviceClient.CallServiceAsync(serviceArgs: new Req() { Arg1 = "hi" },
                    serviceName: "/tt_mission_master/tt_mission_service").Result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        void CallServiceTest<TReqType>(IRosbridgeMessageDispatcher messageDispatcher) where TReqType : class, new()
        {
            IRosServiceClient<Req, Resp> serviceClient = new RosServiceClient<Req, Resp>(messageDispatcher);

            Req request = new Req() { Arg1 = "hi" };

            Task<Resp> serviceTask;

            try
            {
                serviceTask = serviceClient.CallServiceAsync(serviceArgs: request,
                    serviceName: "/tt_mission_master/tt_mission_service");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            serviceTask.Wait();

            if (serviceTask.Status == TaskStatus.RanToCompletion)
            {
                var fff = serviceTask.Result;

                var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<TReqType>(serviceTask.Result.ToString());
                var obj2 = obj as TReqType;
            }
            else
            {
                throw new Exception("Broke");
            }

        }

        RosPublisher<Twist> PublisherTest(IRosbridgeMessageDispatcher messageDispatcher)
        {
            RosPublisher<Twist> publisher = new RosPublisher<Twist>(messageDispatcher, "/turtle1/cmd_vel");
            publisher.AdvertiseAsync();

            publisher.PublishAsync(new Twist()
            {
                Linear = new Vector()
                {
                    X = -5,
                    Y = 0,
                    Z = 0
                },
                Angular = new Vector()
                {
                    X = 0,
                    Y = 0,
                    Z = 0
                }
            });

            return publisher;
        }

        void SubscribeTest(IRosbridgeMessageDispatcher messageDispatcher)
        {
            RosSubscriber<Twist> subscriber = new RosSubscriber<Twist>(messageDispatcher, "/turtle1/cmd_vel");

            subscriber.RosMessageReceived += (s, e) => { Console.WriteLine(e.RosMessage); };

            subscriber.SubscribeAsync();
        }

        public void SubscribeStateTest()
        {
            //state_sub = nh_.subscribe<mavros_msgs::State>
            //    ("mavros/state", 10, &GPMission::state_cb, this);

            RosSubscriber<object> subscriber = new RosSubscriber<object>(MessageDispatcher, "mavros/state");

            subscriber.RosMessageReceived += (s, e) =>
            { Console.WriteLine(e.RosMessage); };

            subscriber.SubscribeAsync();
        }

        public void SubscribeExtendedStateTest()
        {
            //extended_state_sub = nh_.subscribe<mavros_msgs::ExtendedState>
            //    ("mavros/extended_state", 10, &GPMission::extended_state_cb, this);


            RosSubscriber<object> subscriber = new RosSubscriber<object>(MessageDispatcher, "mavros/extended_state");

            subscriber.RosMessageReceived += (s, e) =>
            { Console.WriteLine(e.RosMessage); };

            subscriber.SubscribeAsync();
        }

        public void SubscribeGlobalPoseTest()
        {
            //global_pose_sub = nh_.subscribe<sensor_msgs::NavSatFix>
            //    ("mavros/global_position/global", 1, &GPMission::global_pose_cb, this);

            RosSubscriber<RespO> subscriber = new RosSubscriber<RespO>(MessageDispatcher, "mavros/global_position/global");

            subscriber.RosMessageReceived += (s, e) =>
            { Console.WriteLine(e.RosMessage); };

            subscriber.SubscribeAsync();
        }

        public void SubscribeMissionStatusTest()
        {
            //global_pose_sub = nh_.subscribe<sensor_msgs::NavSatFix>
            //    ("mavros/global_position/global", 1, &GPMission::global_pose_cb, this);

            RosSubscriber<MissionStatus> subscriber = new RosSubscriber<MissionStatus>(MessageDispatcher, "tt_mavros_wp_mission/MissionStatus");

            subscriber.RosMessageReceived += (s, e) =>
            { Console.WriteLine(e.RosMessage); };


            var aa = subscriber.SubscribeAsync();

            aa.Wait();

            Console.WriteLine("Subscribed");

        }

        #endregion

        /// <summary>
        /// callback from interface
        /// </summary>
        /// <typeparam name="Tin"></typeparam>
        /// <typeparam name="Tout"></typeparam>
        /// <param name="service"></param>
        /// <param name="serviceResponseHandler"></param>
        /// <param name="serviceArguments"></param>
        /// <returns></returns>
        public string CallService<Tin, Tout>(string service, ServiceResponseHandler<Tout> serviceResponseHandler, Tin serviceArguments) where Tin : Message where Tout : Message
        {
            throw new NotImplementedException();
        }

    }

}

namespace RosClientLib2
{
    using RosSharp.RosBridgeClient;
    using std_msgs = RosSharp.RosBridgeClient.Messages.Standard;
    using std_srvs = RosSharp.RosBridgeClient.Services.Standard;
    using rosapi = RosSharp.RosBridgeClient.Services.RosApi;

    public class RosClient2
    {
        static readonly string Uri = "ws://192.168.56.102:9090";

        public static void Main1()
        {
            //RosSocket rosSocket = new RosSocket(new RosBridgeClient.Protocols.WebSocketSharpProtocol(uri));
            RosSocket rosSocket = new RosSocket(new RosSharp.RosBridgeClient.Protocols.WebSocketNetProtocol(Uri));

            // Publication:
            std_msgs.String message = new std_msgs.String("publication test message data");

            string publicationId = rosSocket.Advertise<std_msgs.String>("publication_test");
            rosSocket.Publish(publicationId, message);


            // Subscription:
            string subscriptionId = rosSocket.Subscribe<std_msgs.String>("/subscription_test", SubscriptionHandler);
            //subscription_id = rosSocket.Subscribe<std_msgs.String>("/subscription_test", SubscriptionHandler);

            // Service Call:
            rosSocket.CallService<rosapi.GetParamRequest, rosapi.GetParamResponse>("/rosapi/get_param",
                ServiceCallHandler, new rosapi.GetParamRequest("/rosdistro", "default"));

            // Service Response:
            string serviceId =
                rosSocket.AdvertiseService<std_srvs.TriggerRequest, std_srvs.TriggerResponse>("/service_response_test",
                    ServiceResponseHandler);

            Console.WriteLine("Press any key to unsubscribe...");
            Console.ReadKey(true);
            rosSocket.Unadvertise(publicationId);
            rosSocket.Unsubscribe(subscriptionId);
            rosSocket.UnadvertiseService(serviceId);

            Console.WriteLine("Press any key to close...");
            Console.ReadKey(true);
            rosSocket.Close();
        }

        private static void SubscriptionHandler(std_msgs.String message)
        {
            Console.WriteLine((message).data);
        }

        private static void ServiceCallHandler(rosapi.GetParamResponse message)
        {
            Console.WriteLine("ROS distro: " + message.value);
        }

        private static bool ServiceResponseHandler(std_srvs.TriggerRequest arguments,
            out std_srvs.TriggerResponse result)
        {
            result = new std_srvs.TriggerResponse(true, "service response message");
            return true;
        }

    }
}

namespace RosClientLiby
{
    using RosSharp.RosBridgeClient;
    using std_msgs = RosSharp.RosBridgeClient.Messages.Standard;
    using std_srvs = RosSharp.RosBridgeClient.Services.Standard;
    using rosapi = RosSharp.RosBridgeClient.Services.RosApi;

    public class RosClient
    {
        static readonly string Uri = "ws://192.168.1.30:9090";

        public static void Test()
        {
            //RosSocket rosSocket = new RosSocket(new RosBridgeClient.Protocols.WebSocketSharpProtocol(uri));
            RosSocket rosSocket = new RosSocket(new RosSharp.RosBridgeClient.Protocols.WebSocketNetProtocol(Uri));

            // Publication:
            //std_msgs.String message = new std_msgs.String("publication test message data");

            //string publicationId = rosSocket.Advertise<std_msgs.String>("publication_test");
            //rosSocket.Publish(publicationId, message);


            // Subscription:
            string subscriptionId = rosSocket.Subscribe<RosSharp.RosBridgeClient.Messages.Test.MissionStatus>("/tt_mavros_wp_mission/MissionStatus", SubscriptionHandler);

            // Service Call:
            rosSocket.CallService<RosSharp.RosBridgeClient.Messages.Test.Req, RosSharp.RosBridgeClient.Messages.Test.Resp>
                ("/tt_mavros_wp_mission/StartMission_service", ServiceCallHandler, new RosSharp.RosBridgeClient.Messages.Test.Req("hey"));

            // Service Response:
            //string serviceId = rosSocket.AdvertiseService<std_srvs.TriggerRequest, std_srvs.TriggerResponse>
            //    ("/service_response_test", ServiceResponseHandler);

            Console.WriteLine("Press any key to unsubscribe...");
            Console.ReadKey(true);
            //rosSocket.Unadvertise(publicationId);
            rosSocket.Unsubscribe(subscriptionId);
            //rosSocket.UnadvertiseService(serviceId);

            Console.WriteLine("Press any key to close...");
            Console.ReadKey(true);
            rosSocket.Close();
        }
        public static void TestOrig()
        {
            //RosSocket rosSocket = new RosSocket(new RosBridgeClient.Protocols.WebSocketSharpProtocol(uri));
            RosSocket rosSocket = new RosSocket(new RosSharp.RosBridgeClient.Protocols.WebSocketNetProtocol(Uri));

            // Publication:
            std_msgs.String message = new std_msgs.String("publication test message data");

            string publicationId = rosSocket.Advertise<std_msgs.String>("publication_test");
            rosSocket.Publish(publicationId, message);


            // Subscription:
            string subscriptionId = rosSocket.Subscribe<std_msgs.String>("/subscription_test", SubscriptionHandler);

            // Service Call:
            rosSocket.CallService<RosSharp.RosBridgeClient.Messages.Test.Req, RosSharp.RosBridgeClient.Messages.Test.Resp>
                ("/tt_mission_master/tt_mission_service", ServiceCallHandler, new RosSharp.RosBridgeClient.Messages.Test.Req("hey"));

            // Service Response:
            string serviceId = rosSocket.AdvertiseService<std_srvs.TriggerRequest, std_srvs.TriggerResponse>
                ("/service_response_test", ServiceResponseHandler);

            Console.WriteLine("Press any key to unsubscribe...");
            Console.ReadKey(true);
            rosSocket.Unadvertise(publicationId);
            rosSocket.Unsubscribe(subscriptionId);
            rosSocket.UnadvertiseService(serviceId);

            Console.WriteLine("Press any key to close...");
            Console.ReadKey(true);
            rosSocket.Close();
        }
        private static void SubscriptionHandler(std_msgs.String message)
        {
            Console.WriteLine((message).data);
        }

        private static void SubscriptionHandler(RosSharp.RosBridgeClient.Messages.Test.MissionStatus message)
        {
            Console.WriteLine((message).z_alt);
        }


        private static void ServiceCallHandler(RosSharp.RosBridgeClient.Messages.Test.Resp message)
        {
            Console.WriteLine("response: " + message.result);
        }

        private static bool ServiceResponseHandler(std_srvs.TriggerRequest arguments, out std_srvs.TriggerResponse result)
        {
            result = new std_srvs.TriggerResponse(true, "service response message");
            return true;
        }

    }*/

}
