using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace JM.QingQi.AndroidUI
{
    [Activity(Theme = "@style/Theme.Default", Label = "Device Information")]
    public class DevideInfoActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            // Create your application here
            SetContentView(Resource.Layout.DeviceInfo);

            ListView list = (ListView)FindViewById(Resource.Id.deviceInfoListView);

            string[] arrays = new string[2];
            arrays[0] = StaticString.beforeBlank + GetString(Resource.String.Version) + "  :   1.0";
            arrays[1] = StaticString.beforeBlank + GetString(Resource.String.DeviceID) + " :   ABC";
            list.Adapter = new ArrayAdapter<string>(
                this,
                Android.Resource.Layout.SimpleListItem1,
                arrays
            );
        }
    }
}