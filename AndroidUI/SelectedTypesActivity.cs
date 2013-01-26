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
using JM.Core;
using JM.QingQi.Vehicle;

namespace JM.QingQi.AndroidUI
{
    [Activity(Theme = "@style/Theme.Default", Label = "Type Selected")]
    public class SelectedTypesActivity : ListActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Create your application here
            Window.SetFlags(WindowManagerFlags.KeepScreenOn, WindowManagerFlags.KeepScreenOn);
            string[] arrays = new string[7];
            arrays[0] = StaticString.beforeBlank + Database.GetText("QM125T-8H", "QingQi");
            arrays[1] = StaticString.beforeBlank + Database.GetText("QM200J-3L", "QingQi");
            arrays[2] = StaticString.beforeBlank + Database.GetText("QM200GY-F", "QingQi");
            arrays[3] = StaticString.beforeBlank + Database.GetText("QM200-3D", "QingQi");
            arrays[4] = StaticString.beforeBlank + Database.GetText("QM250J-2L", "QingQi");
            arrays[5] = StaticString.beforeBlank + Database.GetText("QM250GY", "QingQi");
            arrays[6] = StaticString.beforeBlank + Database.GetText("QM250T", "QingQi");
            ListAdapter = new ArrayAdapter<string>(
                this,
                Android.Resource.Layout.SimpleListItem1,
                arrays
            );
            ListView.TextFilterEnabled = true;
            ListView.ItemClick += delegate(object sender, Android.Widget.AdapterView.ItemClickEventArgs args)
            {
                Intent intent = new Intent(
                    this,
                    typeof(ModelFunctionsActivity)
                );
                string model = ((TextView)args.View).Text;

                intent.PutExtra("MenuClick", model.TrimStart(' '));
                StartActivity(intent);
            };
        }

    }
}