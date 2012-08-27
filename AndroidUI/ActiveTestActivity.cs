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

        private void ShowResult(Task t)
        {
            RunOnUiThread(() =>
            {
                status.Dismiss();
                if (t.IsFaulted)
                {
                    DialogManager.ShowFatal(this, t.Exception.InnerException.Message, null);
                }
                else
                {
                    DialogManager.ShowFatal(this, result, null);
                }
            });
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
                DialogManager.ShowList(this, arrays, (sender2, e2) =>
                {
                    if (DialogManager.Which == -1)
                    {
                        return;
                    }

                    status = DialogManager.ShowStatus(this, ResourceManager.Instance.VehicleDB.GetText("Communicating"));
                    
                    Task task = Task.Factory.StartNew(() =>
                    {
                        Synerject protocol = new Synerject(ResourceManager.Instance.VehicleDB, Diag.BoxFactory.Instance.Commbox);
                        result = protocol.Active(ResourceManager.Instance.VehicleDB.GetText("Injector"), DialogManager.Which == 0 ? true : false);
                    });

                    task.ContinueWith(ShowResult);
                }
                );
            }
            else if (((TextView)e.View).Text == ResourceManager.Instance.VehicleDB.GetText("Ignition Coil"))
            {
                string[] arrays = new string[]
                {
                    ResourceManager.Instance.VehicleDB.GetText("Ignition Coil On Test"),
                    ResourceManager.Instance.VehicleDB.GetText("Ignition Coil Off Test"),
                };
                DialogManager.ShowList(this, arrays, (sender2, e2) =>
                {
                    if (DialogManager.Which == -1)
                    {
                        return;
                    }

                    status = DialogManager.ShowStatus(this, ResourceManager.Instance.VehicleDB.GetText("Communicating"));
                    
                    Task task = Task.Factory.StartNew(() =>
                    {
                        Synerject protocol = new Synerject(ResourceManager.Instance.VehicleDB, Diag.BoxFactory.Instance.Commbox);
                        result = protocol.Active(ResourceManager.Instance.VehicleDB.GetText("Ignition Coil"), DialogManager.Which == 0 ? true : false);
                    }
                    );
                    task.ContinueWith(ShowResult);
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
                DialogManager.ShowList(this, arrays, (sender2, e2) =>
                {
                    if (DialogManager.Which == -1)
                    {
                        return;
                    }

                    status = DialogManager.ShowStatus(this, ResourceManager.Instance.VehicleDB.GetText("Communicating"));

                    Task task = Task.Factory.StartNew(() =>
                    {
                        Synerject protocol = new Synerject(ResourceManager.Instance.VehicleDB, Diag.BoxFactory.Instance.Commbox);
                        result = protocol.Active(ResourceManager.Instance.VehicleDB.GetText("Fuel Pump"), DialogManager.Which == 0 ? true : false);
                    }
                    );

                    task.ContinueWith(ShowResult);
                }
                );
            }
        }
    }
}

