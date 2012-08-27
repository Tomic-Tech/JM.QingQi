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
            string[] arrays = new string[7];
             arrays[0] = ResourceManager.Instance.VehicleDB.GetText("QM125T-8H");
            arrays[1] = ResourceManager.Instance.VehicleDB.GetText("QM200GY-F");
            arrays[2] = ResourceManager.Instance.VehicleDB.GetText("QM250GY");
            arrays[3] = ResourceManager.Instance.VehicleDB.GetText("QM250T");
            arrays[4] = ResourceManager.Instance.VehicleDB.GetText("QM200-3D");
            arrays[5] = ResourceManager.Instance.VehicleDB.GetText("QM200J-3L");
            arrays[6] = ResourceManager.Instance.VehicleDB.GetText("QM250J-2L");
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
                intent.PutExtra("MenuClick", ((TextView)args.View).Text);
                StartActivity(intent);
            };
        }

    }
}