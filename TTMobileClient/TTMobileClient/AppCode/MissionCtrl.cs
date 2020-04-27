using Plugin.Geolocator.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace TTMobileClient
{
    //*************************************************************************
    /// <summary>
    /// 
    /// </summary>
    //*************************************************************************
    public class MissionCtrl
    {
        Xamarin.Forms.CollectionView cView;
        ObservableCollection<Waypoint> _wayPoints = 
            new ObservableCollection<Waypoint>();

        public Xamarin.Forms.View viewCtrl => cView;

        //*********************************************************************
        /// <summary>
        /// 
        /// </summary>
        //*********************************************************************
        public MissionCtrl()
        {
            cView = new CollectionView();
            //AddFakeData();
            CreateDisplayTemplate();

            cView.ItemsSource = _wayPoints;
        }

        //*********************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="wayPoint"></param>
        //*********************************************************************
        public async void AddWaypoint(Waypoint wayPoint)
        {
            _wayPoints.Add(wayPoint);
            cView.ItemsSource = _wayPoints;
        }

        //*********************************************************************
        /// <summary>
        /// 
        /// </summary>
        //*********************************************************************
        private void CreateDisplayTemplate()
        {
            cView.ItemTemplate = new DataTemplate(() =>
            {
                SwipeItem favoriteSwipeItem = new SwipeItem
                {
                    Text = "Favorite",
                    IconImageSource = "RoutePin.png",
                    BackgroundColor = Color.LightGreen
                };
                favoriteSwipeItem.Invoked += OnFavoriteSwipeItemInvoked;

                SwipeItem deleteSwipeItem = new SwipeItem
                {
                    Text = "Delete",
                    IconImageSource = "RoutePin.png",
                    BackgroundColor = Color.LightPink
                };
                deleteSwipeItem.Invoked += OnDeleteSwipeItemInvoked;

                List<SwipeItem> swipeItems = new List<SwipeItem>() { favoriteSwipeItem, deleteSwipeItem };

                Grid grid = new Grid { Padding = 10 };
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                //Image image = new Image { Aspect = Aspect.AspectFill, HeightRequest = 60, WidthRequest = 60 };
                //image.SetBinding(Image.SourceProperty, "ImageUrl");

                Label nameLabel = new Label { FontAttributes = FontAttributes.Bold };
                nameLabel.SetBinding(Label.TextProperty, "Label");

                Label locationLabel = new Label { FontAttributes = FontAttributes.Italic, VerticalOptions = LayoutOptions.End };
                locationLabel.SetBinding(Label.TextProperty, "Address");

                //Grid.SetRowSpan(image, 2);

                //grid.Children.Add(image);
                grid.Children.Add(nameLabel, 1, 0);
                grid.Children.Add(locationLabel, 1, 1);

                SwipeView swipeView = new SwipeView
                {
                    LeftItems = new SwipeItems(swipeItems),
                    Content = grid
                };

                //return grid;
                return swipeView;
            });
        }

        //*********************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //*********************************************************************
        private void OnDeleteSwipeItemInvoked(object sender, EventArgs e)
        {
            //TODO
        }

        //*********************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //*********************************************************************
        private void OnFavoriteSwipeItemInvoked(object sender, EventArgs e)
        {
            //TODO
        }

        //*********************************************************************
        /// <summary>
        /// 
        /// </summary>
        //*********************************************************************
        private void AddFakeData()
        {
            double lat = 100.0;
            double lon = 200.0;
            double alt = 300.0;

            _wayPoints.Add(new Waypoint
            {
                Type = Xamarin.Forms.Maps.PinType.Place,
                Position = new Xamarin.Forms.Maps.Position(lat, lon),
                Label = "Waypoint",
                Address = $"Lat: {lat}, Lon: {lon}, alt: {alt}",
                Id = "Waypoint",
                Url = "http://www.telemething.com/",
                IsActive = true
            });

            lat += 100.0;
            lon += 10.0;
            alt += 1.0;

            _wayPoints.Add(new Waypoint
            {
                Type = Xamarin.Forms.Maps.PinType.Place,
                Position = new Xamarin.Forms.Maps.Position(lat, lon),
                Label = "Waypoint",
                Address = $"Lat: {lat}, Lon: {lon}, alt: {alt}",
                Id = "Waypoint",
                Url = "http://www.telemething.com/",
                IsActive = true
            });

            lat += 100.0;
            lon += 10.0;
            alt += 1.0;

            _wayPoints.Add(new Waypoint
            {
                Type = Xamarin.Forms.Maps.PinType.Place,
                Position = new Xamarin.Forms.Maps.Position(lat, lon),
                Label = "Waypoint",
                Address = $"Lat: {lat}, Lon: {lon}, alt: {alt}",
                Id = "Waypoint",
                Url = "http://www.telemething.com/",
                IsActive = true
            });

        }
    }

}
