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

namespace JM.QingQi
{
    [Activity(Label = "Active Test")]
    public class ActiveTestActivity : ListActivity
    {
        private delegate void ProtocolFunc();

        private Dictionary<string, ProtocolFunc> protocolFuncs;
        private string model;

        public ActiveTestActivity()
        {
            protocolFuncs = new Dictionary<string, ProtocolFunc>();
            protocolFuncs[ResourceManager.Instance.VehicleDB.GetText("QM125T-8H")] = OnSynerject;
            protocolFuncs[ResourceManager.Instance.VehicleDB.GetText("QM200GY-F")] = null;
            protocolFuncs[ResourceManager.Instance.VehicleDB.GetText("QM250GY")] = OnSynerject;
            protocolFuncs[ResourceManager.Instance.VehicleDB.GetText("QM250T")] = OnSynerject;
            protocolFuncs[ResourceManager.Instance.VehicleDB.GetText("QM200-3D")] = null;
            protocolFuncs[ResourceManager.Instance.VehicleDB.GetText("QM200J-3L")] = null;
            protocolFuncs[ResourceManager.Instance.VehicleDB.GetText("QM250J-2L")] = null;
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
        }

        private void OnSynerject()
        {
            string[] arrays = new string[3];
            arrays[0] = ResourceManager.Instance.VehicleDB.GetText("Injector");
            arrays[1] = ResourceManager.Instance.VehicleDB.GetText("Ignition Coil");
            arrays[2] = ResourceManager.Instance.VehicleDB.GetText("Fuel Pump");

            ListAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, arrays);
            ListView.ItemClick += OnItemClickSynerject;
        }

        private void OnItemClickSynerject(object sender, AdapterView.ItemClickEventArgs e)
        {
            if (((TextView)e.View).Text == ResourceManager.Instance.VehicleDB.GetText("Injector"))
            {
                string[] arrays = new string[]
                {
                    ResourceManager.Instance.VehicleDB.GetText("Injector On Test"),
                    ResourceManager.Instance.VehicleDB.GetText("Injector Off Test"),
                };
                DialogManager.Instance.ListDialogShow(this, arrays, (sender2, e2) =>
                {
                    Task task = Task.Factory.StartNew(() =>
                    {
                        Synerject protocol = new Synerject(ResourceManager.Instance.VehicleDB, Diag.BoxFactory.Instance.Commbox);
                        string result = protocol.Active(ResourceManager.Instance.VehicleDB.GetText("Injector"), e2.Which == 0 ? true : false);
                        RunOnUiThread(() => DialogManager.Instance.FatalDialogShow(this, result, null));
                    }
                    );
                }
                );
            }
            else if (((TextView)e.View).Text == ResourceManager.Instance.VehicleDB.GetText("Ignition Coil"))
            {
                string[] arrays = new string[]
                {
                    ResourceManager.Instance.VehicleDB.GetText("Ingition Coil On Test"),
                    ResourceManager.Instance.VehicleDB.GetText("Ingition Coil Off Test"),
                };
                DialogManager.Instance.ListDialogShow(this, arrays, (sender2, e2) =>
                {
                    Task task = Task.Factory.StartNew(() =>
                    {
                        Synerject protocol = new Synerject(ResourceManager.Instance.VehicleDB, Diag.BoxFactory.Instance.Commbox);
                        string result = protocol.Active(ResourceManager.Instance.VehicleDB.GetText("Ignition Coil"), e2.Which == 0 ? true : false);
                        RunOnUiThread(() => DialogManager.Instance.FatalDialogShow(this, result, null));
                    }
                    );
                }
                );
            }
            else if (((TextView)e.View).Text == ResourceManager.Instance.VehicleDB.GetText("Fuel Pump"))
            {
                string[] arrays = new string[]
                {
                    ResourceManager.Instance.VehicleDB.GetText("Fuel Pump On Test"),
                    ResourceManager.Instance.VehicleDB.GetText("Fuel Pump Off Test"),
                };
                DialogManager.Instance.ListDialogShow(this, arrays, (sender2, e2) =>
                {
                    Task task = Task.Factory.StartNew(() =>
                    {
                        Synerject protocol = new Synerject(ResourceManager.Instance.VehicleDB, Diag.BoxFactory.Instance.Commbox);
                        string result = protocol.Active(ResourceManager.Instance.VehicleDB.GetText("Fuel Pump"), e2.Which == 0 ? true : false);
                        RunOnUiThread(() => DialogManager.Instance.FatalDialogShow(this, result, null));
                    }
                    );
                }
                );
            }
        }
    }
}

