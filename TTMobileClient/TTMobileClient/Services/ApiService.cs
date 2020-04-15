using System;
using System.Collections.Generic;
using System.Text;

namespace TTMobileClient.Services
{
    public class ApiService
    {
        WebApiLib.WebApiServer _was = null;
        string _webApiUrl = AppSettings.WebApiUrl;

        public void StartService()
        {
            _was = new WebApiLib.WebApiServer();
            _was.StartServer(_webApiUrl, GotMessageCallback);
        }

        private WebApiLib.Response GotMessageCallback(WebApiLib.Request data)
        {
            var resp = new WebApiLib.Response(WebApiLib.ResultEnum.ok, new List<WebApiLib.Argument>(), null);
            return resp;
        }

    }
}
