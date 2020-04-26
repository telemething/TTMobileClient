using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace TTMobileClient
{
    public class MissionCtrl
    {
        Xamarin.Forms.CollectionView cView;

        public MissionCtrl()
        {
            cView = new CollectionView();
            AddFakeData();
        }

        public Xamarin.Forms.View viewCtrl => cView;
        
        private void AddFakeData()
        {
            cView.ItemsSource = new string[]
            {
                "Baboon",
                "Capuchin Monkey",
                "Blue Monkey",
                "Squirrel Monkey",
                "Golden Lion Tamarin",
                "Howler Monkey",
                "Japanese Macaque"
            };
        }
    }

}
