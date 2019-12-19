using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using MavLink4Net.Messages.Common;
using RosSharp.RosBridgeClient.Messages.Test;

namespace RosClientLib
{
    #region Repeater

    //*************************************************************************
    /// <summary>
    /// 
    /// </summary>
    //*************************************************************************

    public class Repeater
    {
        private List<Transport> _TransportList = new List<Transport>(2);

        public enum TransportEnum { Uninit, UDP, TCP }
        public enum DialectEnum { Uninit, Mavlink, ThingTelem }

        public Repeater()
        { }

        //*********************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="transport"></param>
        /// <param name="dialect"></param>
        /// <param name="address"></param>
        /// <param name="port"></param>
        //*********************************************************************

        public void AddTransport(TransportEnum transport,
            DialectEnum dialect, string address, int port, int minimumTimeSpanMs)
        {
            Transport newTransport;
            Dialect newDialect;

            switch (dialect)
            {
                case DialectEnum.ThingTelem:
                    newDialect = new DialectThingTelem();
                    break;
                default:
                    throw new NotImplementedException("Dialect type not implemented");
            }

            switch (transport)
            {
                case TransportEnum.UDP:
                    newTransport = new TransportUdp(address, port, newDialect, minimumTimeSpanMs);
                    _TransportList.Add(newTransport);
                    break;
                default:
                    throw new NotImplementedException("Transport type not implemented");
            }
        }

        //*********************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        //*********************************************************************

        public async Task<bool> Send(RosSharp.RosBridgeClient.Message message)
        {
            foreach (var tp in _TransportList)
            {
                await tp.Send(message);
            }

            return true;
        }
    }

    #endregion

    #region Dialect

    //*************************************************************************
    /// <summary>
    /// 
    /// </summary>
    //*************************************************************************

    abstract class Dialect
    {
        protected Dialect()
        { }

        protected string GetSenderId()
        {
            return "aDrone"; //*** TODO * Obviously this needs to be changed
        }

        public abstract string Translate(RosSharp.RosBridgeClient.Message message);
    }

    //*************************************************************************
    /// <summary>
    /// 
    /// </summary>
    //*************************************************************************

    class DialectThingTelem : Dialect
    {
        public DialectThingTelem()
        { }

        //*********************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        //*********************************************************************

        public override string Translate(RosSharp.RosBridgeClient.Message message)
        {
            string outMessage ="";

            switch (message)
            {
                case MissionStatus ms:
                    //string poseFormat =
                    //    "{{\"type\": \"pose\",\"id\" : \"{0}\", \"tow\" : {1}, \"coord\" : {{ \"lat\": {2}, \"lon\" : {3}, \"alt\" : {4} }}, \"orient\" : {{ \"mag\": {5}, \"true\" : {6}, \"x\" : {7}, \"y\" : {8}, \"z\" : {9}, \"w\" : {10} }}, \"gimbal0\" : {{ \"x\" : {11}, \"y\" : {12}, \"z\" : {13}, \"w\" : {14} }}}}";
                    string poseFormat =
                        "{{\"type\": \"pose\",\"id\" : \"{0}\", \"tow\" : {1}, \"coord\" : {{ \"lat\": {2}, \"lon\" : {3}, \"alt\" : {4} }}}}";
                    outMessage = string.Format(poseFormat, GetSenderId(), DateTime.UtcNow.Second,
                        ms.x_lat, ms.y_long,ms.z_alt);
                    break;
                default:
                    throw new NotImplementedException("Message type translation not implemented");
            }

            return outMessage;
        }
    }

    #endregion

    #region Transport

    //*************************************************************************
    /// <summary>
    /// 
    /// </summary>
    //*************************************************************************

    abstract class Transport
    {
        protected string _destIP = "192.168.1.255"; //***
        protected int _destPort = 45679;
        protected int _MinimumTimeSpanMs = 500;
        protected Dialect _dialect;
        protected Transport _handlerTransport;

        private Stopwatch _stopwatch = new Stopwatch();

        protected Transport()
        {
            _stopwatch.Start();
        }

        //public abstract Task<bool> Send(RosSharp.RosBridgeClient.Message message);

        public abstract Task<bool> Send(Byte[] message);

        public async Task<bool> Send(RosSharp.RosBridgeClient.Message message)
        {
            if (_stopwatch.ElapsedMilliseconds > _MinimumTimeSpanMs)
            {
                _stopwatch.Restart();
                _handlerTransport.Send(Encoding.ASCII.GetBytes(_dialect.Translate(message)));
            }
            
            return true;
        }
    }

    //*************************************************************************
    /// <summary>
    /// 
    /// </summary>
    //*************************************************************************

    class TransportUdp : Transport
    {
        private System.Net.Sockets.Socket sock;
        private System.Net.IPAddress ipaddr;
        private System.Net.IPEndPoint endpoint;

        //*********************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        /// <param name="dialect"></param>
        //*********************************************************************

        public TransportUdp(string address, int port, Dialect dialect, int minimumTimeSpanMs)
        {
            _handlerTransport = this;
            _destIP = address;
            _destPort = port;
            _dialect = dialect;
            _MinimumTimeSpanMs = minimumTimeSpanMs;

            sock = new System.Net.Sockets.Socket(
                System.Net.Sockets.AddressFamily.InterNetwork,
                System.Net.Sockets.SocketType.Dgram,
                System.Net.Sockets.ProtocolType.Udp) 
                {EnableBroadcast = true, ExclusiveAddressUse = false};

            ipaddr = System.Net.IPAddress.Parse(_destIP);
            endpoint = new System.Net.IPEndPoint(ipaddr, _destPort);
        }

        //*********************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        //*********************************************************************

        /*public override async Task<bool> Send(RosSharp.RosBridgeClient.Message message)
        {
            sock.SendTo(Encoding.ASCII.GetBytes(_dialect.Translate(message)), endpoint);
            return true;
        }*/

        public override async Task<bool> Send(Byte[] message)
        {
            sock.SendTo(message, endpoint);
            return true;
        }
    }

    #endregion
}
