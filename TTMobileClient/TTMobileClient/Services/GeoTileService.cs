﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TTMobileClient.Services;

namespace TTMobileClient
{
    //*********************************************************************
    /// <summary>
    /// TileReqArgs
    /// </summary>
    //*********************************************************************
    public class TileReqArgs
    {
        public int ZoomLevel { set; get; }
        public int X { set; get; }
        public int Y { set; get; }

        //*********************************************************************
        /// <summary>
        /// Create an instance of the class given the request arg list
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        //*********************************************************************
        public static TileReqArgs Extract(List<WebApiLib.Argument> args)
        {
            var req = new TileReqArgs();
            bool gotZoom = false;
            bool gotX = false;
            bool gotY = false;

            try
            {
                foreach (var arg in args)
                {
                    switch (arg.Name.ToLower())
                    {
                        case "zoomlevel":
                            req.ZoomLevel = Convert.ToInt32(arg.Value);
                            gotZoom = true;
                            break;
                        case "x":
                            req.X = Convert.ToInt32(arg.Value);
                            gotX = true;
                            break;
                        case "y":
                            req.Y = Convert.ToInt32(arg.Value);
                            gotY = true;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Conversion error: " + ex.Message);
            }

            if (!gotZoom)
                throw new ArgumentException("missing 'zoomlevel' argument");
            if (!gotX)
                throw new ArgumentException("missing 'x' argument");
            if (!gotY)
                throw new ArgumentException("missing 'y' argument");

            return req;
        }
    }

    //*********************************************************************
    /// <summary>
    /// 
    /// </summary>
    //*********************************************************************
    public class GeoTileService
    {
        string _bingAccessKey;
        int _successfulfetchCount = 0;
        int _failedfetchCount = 0;

        //The API service, which forwards settings requests from remote devices
        TTMobileClient.Services.ApiService _was = null;

        //*********************************************************************
        /// <summary>
        /// 
        /// </summary>
        //*********************************************************************
        public GeoTileService()
        {
            //Set the directory for tile caching
            //https://docs.microsoft.com/en-us/xamarin/xamarin-forms/data-cloud/data/files?tabs=windows
            TileServerLib.TileCache.AppDataPath =
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            //Set the Bing map service acccess key
            _bingAccessKey = AppSettings.BingMapAccessKey;
        }

        //*********************************************************************
        /// <summary>
        /// Start the service on WebApi
        /// </summary>
        //*********************************************************************
        public void StartServer()
        {
            _was = TTMobileClient.Services.ApiService.Singleton;

            //register the API callbacks
            _was.AddApiMethod(WebApiMethodNames.Geo_FetchImageTile,
                GeoFetchImageTile);

            _was.AddApiMethod(WebApiMethodNames.Geo_FetchElevationTile,
                GeoFetchElevationTile);

            AdvertiseServices.Singleton.AddServiceToAdvertise(
                new TThingComLib.Messages.NetworkService(
                    $"ws://{AdvertiseServices.Singleton.FetchIpAddress()}:8877/wsapi",
                    TThingComLib.Messages.NetworkTypeEnum.WsAPI,
                    TThingComLib.Messages.ServiceTypeEnum.GeoTile,
                    TThingComLib.Messages.ServiceRoleEnum.Server));
        }

        //*************************************************************************
        /// <summary>
        /// GeoFetchImageTile
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        //*************************************************************************
        List<WebApiLib.Argument> GeoFetchImageTile(List<WebApiLib.Argument> args)
        {
            var req = TileReqArgs.Extract(args);

            var data = FetchImageTile(req.ZoomLevel, req.X, req.Y);

            data.Wait();

            return new List<WebApiLib.Argument>()
                { new WebApiLib.Argument("ImageB64", Convert.ToBase64String(data.Result))};
        }

        //*************************************************************************
        /// <summary>
        /// GeoFetchElevationTile
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        //*************************************************************************
        List<WebApiLib.Argument> GeoFetchElevationTile(List<WebApiLib.Argument> args)
        {
            var req = TileReqArgs.Extract(args);

            var data = FetchElevationTile(req.ZoomLevel, req.X, req.Y);

            data.Wait();

            var retlist = new List<WebApiLib.Argument>();

            var arg = new WebApiLib.Argument("Elevation", data.Result);

            retlist.Add(arg);

            return retlist;

            //return new List<WebApiLib.Argument>() 
            //    { new WebApiLib.Argument("Elevation", data.Result) };
        }


        //*********************************************************************
        /// <summary>
        /// Fetch a single elevation tile
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //*********************************************************************
        private async Task<string> FetchElevationTile(int zoomLevel, int x, int y)
        {
            string message;
            _successfulfetchCount = 0;
            _failedfetchCount = 0;

            try
            {
                TileServerLib.MapTile mt = new TileServerLib.MapTile(_bingAccessKey);
                return await mt.FetchElevationData(zoomLevel, x, y,
                    (fetchStatus) => GotStatusUpdate(fetchStatus));
                //message = string.Format("Success: Length: {0}", resp.Length);
            }
            catch (Exception ex)
            {
                message = ex.Message;
                throw;
            }
        }

        //*********************************************************************
        /// <summary>
        /// Fetch a single image tile
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //*********************************************************************
        private async Task<byte[]> FetchImageTile(int zoomLevel, int x, int y)
        {
            string message;
            _successfulfetchCount = 0;
            _failedfetchCount = 0;

            try
            {
                TileServerLib.MapTile mt = new TileServerLib.MapTile(_bingAccessKey);
                return await mt.FetchImageData(zoomLevel, x, y,
                    (fetchStatus) => GotStatusUpdate(fetchStatus));
                //message = string.Format("Success: Length: {0}", resp.Length);
            }
            catch (Exception ex)
            {
                message = ex.Message;
                throw;
            }
        }

        public List<TileServerLib.MapTile> MapTiles => _mapTiles;
        private List<TileServerLib.MapTile> _mapTiles;

        public List<TileServerLib.TileInfo> TileInfos
        { 
            get 
            {
                var til = new List<TileServerLib.TileInfo>();
                foreach(var mt in _mapTiles)
                    til.Add(mt.TileData);
                return til;
            } 
        }

        //*********************************************************************
        /// <summary>
        /// Fetch elevation and image tiles for an area given zoom, size, and
        /// center coords. All tile data is stored in local cache, making this
        /// useful for prefetch for later field use. The callback is invoked
        /// once for each tile success or failure.
        /// </summary>
        /// <param name="Latitude"></param>
        /// <param name="Longitude"></param>
        /// <param name="ZoomLevel"></param>
        /// <param name="MapSize"></param>
        //*********************************************************************
        public async Task<TileServerLib.FetchStatus> PreFetchMap(
            float Latitude, float Longitude, int ZoomLevel,
            int MapSize, Action<TileServerLib.FetchStatus> callback)
        {
            _successfulfetchCount = 0;
            _failedfetchCount = 0;

            try
            {
                TileServerLib.MapBuilder mb =
                    new TileServerLib.MapBuilder(_bingAccessKey);
                //mb.Test1();

                mb.GetMapData(Latitude, Longitude,
                    ZoomLevel, MapSize, callback,
                    TileServerLib.MapBuilder.FetchTypeEnum.ImageAndElevation);

                _mapTiles = mb.MapTiles;

                return new TileServerLib.FetchStatus(
                    TileServerLib.FetchStatus.DataTypeEnum.Map,
                    TileServerLib.FetchStatus.ResultEnum.Success);
            }
            catch (Exception ex)
            {
                return new TileServerLib.FetchStatus(
                    TileServerLib.FetchStatus.DataTypeEnum.Map,
                    TileServerLib.FetchStatus.ResultEnum.Failure,
                    null, ex.Message);

                //var messageDialog = new MessageDialog(ex.Message);
                //messageDialog.ShowAsync();
            }
        }

        //*********************************************************************
        /// <summary>
        /// Generate tile info without fetching GIS data, useful for displaying
        /// tile boundaries prior to fetching.
        /// </summary>
        /// <param name="Latitude"></param>
        /// <param name="Longitude"></param>
        /// <param name="ZoomLevel"></param>
        /// <param name="MapSize"></param>
        /// <returns></returns>
        //*********************************************************************
        public List<TileServerLib.TileInfo> FetchTileInfo(
            float Latitude, float Longitude, int ZoomLevel,
            int MapSize)
        {
            _successfulfetchCount = 0;
            _failedfetchCount = 0;

            try
            {
                TileServerLib.MapBuilder mb =
                    new TileServerLib.MapBuilder(_bingAccessKey);

                mb.GetMapData(Latitude, Longitude,
                    ZoomLevel, MapSize, null, 
                    TileServerLib.MapBuilder.FetchTypeEnum.JustTileInfo);

                _mapTiles = mb.MapTiles;

                return TileInfos;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        //*********************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="statusUpdate"></param>
        //*********************************************************************
        private void GotStatusUpdate(TileServerLib.FetchStatus statusUpdate)
        {
            switch (statusUpdate.Result)
            {
                case TileServerLib.FetchStatus.ResultEnum.Success:
                    _successfulfetchCount++;
                    break;
                case TileServerLib.FetchStatus.ResultEnum.Failure:
                    _failedfetchCount++;
                    break;
            }
        }

        //*********************************************************************
        /// <summary>
        /// 
        /// </summary>
        //*********************************************************************
        public async void FetchTest()
        {
            try
            {
                int zoomLevel = 12, x = 662, y = 1425;
                var img = await FetchImageTile(zoomLevel, x, y);
                var ele = await FetchElevationTile(zoomLevel, x, y);
            }
            catch(Exception ex)
            {
                var msg = ex.Message;
            }
        }

    }
}
