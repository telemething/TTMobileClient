using System;
using System.Collections.Generic;
using System.Text;

namespace TTMobileClient.Services
{
    //*************************************************************************
    /// <summary>
    /// 
    /// </summary>
    //*************************************************************************
    public class ApiService
    {
        //signature of rerquest handling methods
        public delegate List<WebApiLib.Argument> MethodCallback(List<WebApiLib.Argument> args);

        //list of request handling methods
        Dictionary<string, MethodCallback> _methodList = new Dictionary<string, MethodCallback>();
        
        //The WebApi server on which to listen
        WebApiLib.WebApiServer _was = null;
        
        //The URLof the WebApi Server
        string _webApiUrl = AppSettings.WebApiUrl;

        //*************************************************************************
        /// <summary>
        /// Start API service
        /// </summary>
        //*************************************************************************
        public void StartService()
        {
            CreateTestApiMethods();

            _was = new WebApiLib.WebApiServer();
            _was.StartServer(_webApiUrl, GotMessageCallback);
        }

        //*************************************************************************
        /// <summary>
        /// Called by the WebApi server when a request arrives
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        //*************************************************************************
        private WebApiLib.Response GotMessageCallback(WebApiLib.Request req)
        {
            try
            {
                if (!_methodList.TryGetValue(req.MethodName, out MethodCallback method))
                    return new WebApiLib.Response(WebApiLib.ResultEnum.notfound, null, null);

                return new WebApiLib.Response(WebApiLib.ResultEnum.ok, method.Invoke(req.Arguments), null);
            }
            catch( Exception ex)
            {
                return new WebApiLib.Response(WebApiLib.ResultEnum.exception, null, ex);
            }
        }

        //*************************************************************************
        /// <summary>
        /// Register an Api method handler to be invoked by name
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="methodCallback"></param>
        //*************************************************************************
        public void AddApiMethod(string methodName, MethodCallback methodCallback)
        {
            _methodList.Add(methodName, methodCallback);
        }

        //*************************************************************************
        /// <summary>
        /// Create some Api method handlers for testing
        /// </summary>
        //*************************************************************************
        public void CreateTestApiMethods()
        {
            AddApiMethod("Test1", TestMethod1);
            AddApiMethod("Test2", TestMethod2);
        }

        //*************************************************************************
        /// <summary>
        /// Test method 1
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        //*************************************************************************
        List<WebApiLib.Argument> TestMethod1(List<WebApiLib.Argument> args)
        {
            return new List<WebApiLib.Argument>();
        }

        //*************************************************************************
        /// <summary>
        /// Test method 2
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        //*************************************************************************
        List<WebApiLib.Argument> TestMethod2(List<WebApiLib.Argument> args)
        {
            return new List<WebApiLib.Argument>();
        }
    }
}
