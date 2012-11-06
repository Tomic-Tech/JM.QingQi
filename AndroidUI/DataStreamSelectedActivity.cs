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
using JM.Vehicles;
using JM.QingQi.Vehicle;

namespace JM.QingQi.AndroidUI
{
    [Activity(Theme = "@style/Theme.Default", Label = "Select Data Stream")]
    public class DataStreamSelectedActivity : ListActivity
    {
        private string model = null;
        private delegate void Func();
        private Dictionary<string, Func> funcs;

        //private void PrepareLiveDataVector()
        //{
        //    if ((model == (StaticString.beforeBlank + Database.GetText("QM125T-8H", "QingQi"))) ||
        //        (model == (StaticString.beforeBlank + Database.GetText("QM250GY", "QingQi"))) ||
        //        (model == (StaticString.beforeBlank + Database.GetText("QM250T", "QingQi"))))
        //    {
        //        Manager.LiveDataVector = Database.GetLiveData("Synerject");
        //    }
        //    else if ((model == (StaticString.beforeBlank + Database.GetText("QM200GY-F", "QingQi"))) ||
        //        (model == (StaticString.beforeBlank + Database.GetText("QM200-3D", "QingQi"))) ||
        //        (model == (StaticString.beforeBlank + Database.GetText("QM200J-3L", "QingQi"))))
        //    {
        //        Manager.LiveDataVector = Database.GetLiveData("Mikuni");
        //    }
        //    else
        //    {
        //        Manager.LiveDataVector = Database.GetLiveData("Visteon");
        //    }
        //}

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Create your application here
            List<string> arrays = new List<string>();
            arrays.Add(StaticString.beforeBlank + Database.GetText("Dynamic Data Stream", "System"));
            arrays.Add(StaticString.beforeBlank + Database.GetText("Static Data Stream", "System"));

            funcs = new Dictionary<string, Func>();
            
            funcs.Add(StaticString.beforeBlank + Database.GetText("Dynamic Data Stream", "System"), () => 
            {
                //PrepareLiveDataVector();
                Intent intent = new Intent(this, typeof(DataStreamActivity));
                intent.PutExtra("Model", model);
                StartActivity(intent);
            });
            funcs.Add(StaticString.beforeBlank + Database.GetText("Static Data Stream", "System"), () => 
            {
                //PrepareLiveDataVector();
                Intent intent = new Intent(this, typeof(StaticDataStreamActivity));
                intent.PutExtra("Model", model);
                StartActivity(intent);
            });

            ListAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, arrays);

            ListView.ItemClick += (sender, e) => 
            {
                funcs[((TextView)e.View).Text]();
            };
        }

        protected override void OnStart()
        {
            base.OnStart();
            model = Intent.Extras.GetString("Model");
        }

        protected override void OnStop()
        {
            base.OnStop();
        }
    }
}