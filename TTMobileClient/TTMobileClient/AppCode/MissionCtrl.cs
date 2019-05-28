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
        Xamarin.Forms.ListView lVliew;

        public MissionCtrl()
        {
            lVliew = new ListView();
        }

        public Xamarin.Forms.View viewCtrl => lVliew;
    }
}
