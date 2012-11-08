using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    [Activity(Theme = "@style/Theme.Default", Label = "Active Test")]
    public class ActiveTestActivity : ListActivity
    {
        private delegate void ProtocolFunc();

        private Dictionary<string, ProtocolFunc> protocolFuncs;
        private string model;
        private ProgressDialog status;
        private string result = "";

        public ActiveTestActivity()
        {
            protocolFuncs = new Dictionary<string, ProtocolFunc>();
            protocolFuncs[StaticString.beforeBlank + Database.GetText("QM125T-8H", "QingQi")] = OnSynerject;
            protocolFuncs[StaticString.beforeBlank + Database.GetText("QM200GY-F", "QingQi")] = null;
            protocolFuncs[StaticString.beforeBlank + Database.GetText("QM250GY", "QingQi")] = OnSynerject;
            protocolFuncs[StaticString.beforeBlank + Database.GetText("QM250T", "QingQi")] = OnSynerject;
            protocolFuncs[StaticString.beforeBlank + Database.GetText("QM200-3D", "QingQi")] = null;
            protocolFuncs[StaticString.beforeBlank + Database.GetText("QM200J-3L", "QingQi")] = null;
            protocolFuncs[StaticString.beforeBlank + Database.GetText("QM250J-2L", "QingQi")] = null;
        }

        protected override void OnStart()
        {
            base.OnStart();

            ListView.ItemClick -= OnItemClickSynerject;
            model = Intent.Extras.GetString("Model");

            protocolFuncs[model]();
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Create your application here
            Window.SetFlags(WindowManagerFlags.KeepScreenOn, WindowManagerFlags.KeepScreenOn);
        }

        private void OnSynerject()
        {
            string[] arrays = new string[3];
            arrays[0] = StaticString.beforeBlank + Database.GetText("Injector", "System");
            arrays[1] = StaticString.beforeBlank + Database.GetText("Ignition Coil", "System");
            arrays[2] = StaticString.beforeBlank + Database.GetText("Fuel Pump", "System");

            ListAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, arrays);
            ListView.ItemClick += OnItemClickSynerject;
        }

        //private void ShowResult(Task t)
        //{
        //    RunOnUiThread(() =>
        //    {
        //        status.Dismiss();
        //        if (t.IsFaulted)
        //        {
        //            DialogManager.ShowFatal(this, t.Exception.InnerException.Message, null);
        //        }
        //        else
        //        {
        //            DialogManager.ShowFatal(this, result, null);
        //        }
        //    });
        //}

        private void OnItemClickSynerject(object sender, AdapterView.ItemClickEventArgs e)
        {
            Intent intent = new Intent(this, typeof(ActiveTest2Activity));
            intent.PutExtra("Model", model);
            intent.PutExtra("Sub", ((TextView)e.View).Text);
            StartActivity(intent);
            //if (((TextView)e.View).Text == Database.GetText("Injector", "System"))
            //{
            //    string[] arrays = new string[]
            //    {
            //        Database.GetText("Injector On Test", "QingQi"),
            //        Database.GetText("Injector Off Test", "QingQi"),
            //    };
            //    DialogManager.ShowList(this, arrays, (sender2, e2) =>
            //    {
            //        if (DialogManager.Which == -1)
            //        {
            //            return;
            //        }

            //        status = DialogManager.ShowStatus(this, Database.GetText("Communicating", "System"));
                    
            //        Task task = Task.Factory.StartNew(() =>
            //        {
            //            Synerject protocol = new Synerject(Diag.BoxFactory.Instance.Commbox);
            //            result = protocol.Active(Database.GetText("Injector", "System"), DialogManager.Which == 0 ? true : false);
            //        });

            //        task.ContinueWith(ShowResult);
            //    }
            //    );
            //}
            //else if (((TextView)e.View).Text == Database.GetText("Ignition Coil", "System"))
            //{
            //    string[] arrays = new string[]
            //    {
            //        Database.GetText("Ignition Coil On Test", "QingQi"),
            //        Database.GetText("Ignition Coil Off Test", "QingQi"),
            //    };
            //    DialogManager.ShowList(this, arrays, (sender2, e2) =>
            //    {
            //        if (DialogManager.Which == -1)
            //        {
            //            return;
            //        }

            //        status = DialogManager.ShowStatus(this, Database.GetText("Communicating", "System"));
                    
            //        Task task = Task.Factory.StartNew(() =>
            //        {
            //            Synerject protocol = new Synerject(Diag.BoxFactory.Instance.Commbox);
            //            result = protocol.Active(Database.GetText("Ignition Coil", "System"), DialogManager.Which == 0 ? true : false);
            //        }
            //        );
            //        task.ContinueWith(ShowResult);
            //    }
            //    );
            //}
            //else if (((TextView)e.View).Text == Database.GetText("Fuel Pump", "System"))
            //{   
            //    string[] arrays = new string[]
            //    {
            //        Database.GetText("Fuel Pump On Test", "QingQi"),
            //        Database.GetText("Fuel Pump Off Test", "QingQi"),
            //    };
            //    DialogManager.ShowList(this, arrays, (sender2, e2) =>
            //    {
            //        if (DialogManager.Which == -1)
            //        {
            //            return;
            //        }

            //        status = DialogManager.ShowStatus(this, Database.GetText("Communicating", "System"));

            //        Task task = Task.Factory.StartNew(() =>
            //        {
            //            Synerject protocol = new Synerject(Diag.BoxFactory.Instance.Commbox);
            //            result = protocol.Active(Database.GetText("Fuel Pump", "System"), DialogManager.Which == 0 ? true : false);
            //        }
            //        );

            //        task.ContinueWith(ShowResult);
            //    }
            //    );
            //}
        }
    }
}

