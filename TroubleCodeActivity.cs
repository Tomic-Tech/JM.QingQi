using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace JM.QingQi
{
    [Activity(Theme = "@style/Theme.Default", Label = "Trouble Code")]
    public class TroubleCodeActivity : ListActivity
    {
        public delegate void ProtocolFunc();

        private Dictionary<string, ProtocolFunc> protocolFuncs;
        private Dictionary<string, ProtocolFunc> funcs;
        private string model;
        Dictionary<string, string> codes = null;
        ProgressDialog status = null;

        public TroubleCodeActivity()
        {
            protocolFuncs = new Dictionary<string, ProtocolFunc>();
            protocolFuncs[ResourceManager.Instance.VehicleDB.GetText("QM125T-8H")] = OnSynerject;
            protocolFuncs[ResourceManager.Instance.VehicleDB.GetText("QM200GY-F")] = OnMikuniProtocol;
            protocolFuncs[ResourceManager.Instance.VehicleDB.GetText("QM250GY")] = OnSynerject;
            protocolFuncs[ResourceManager.Instance.VehicleDB.GetText("QM250T")] = OnSynerject;
            protocolFuncs[ResourceManager.Instance.VehicleDB.GetText("QM200-3D")] = OnMikuniProtocol;
            protocolFuncs[ResourceManager.Instance.VehicleDB.GetText("QM200J-3L")] = OnMikuniProtocol;
            protocolFuncs[ResourceManager.Instance.VehicleDB.GetText("QM250J-2L")] = OnVisteonProtocol;
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            // Create your application here
        }

        protected override void OnStart()
        {
            base.OnStart();
            model = Intent.Extras.GetString("Model");
            ListView.ItemClick -= OnTroubleCodeItemClick;
            ListView.ItemClick -= OnItemClickMikuni;
            ListView.ItemClick -= OnItemClickSynerject;
            ListView.ItemClick -= OnItemClickVisteon;
            protocolFuncs[model]();
        }

        protected override void OnStop()
        {
            base.OnStop();
        }

        private void OnMikuniProtocol()
        {
            string[] arrays = new string[2];
            arrays[0] = ResourceManager.Instance.VehicleDB.GetText("Read Current Trouble Code");
            arrays[1] = ResourceManager.Instance.VehicleDB.GetText("Read History Trouble Code");
            ListView.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, arrays);
            funcs = new Dictionary<string, ProtocolFunc>();
            funcs[arrays[0]] = () =>
            {
                status = DialogManager.ShowStatus(this, ResourceManager.Instance.VehicleDB.GetText("Communicating"));
                Task task = Task.Factory.StartNew(() =>
                {
                    Mikuni protocol = new Mikuni(ResourceManager.Instance.VehicleDB, Diag.BoxFactory.Instance.Commbox);
                    codes = protocol.ReadCurrentTroubleCode();
                });

                task.ContinueWith(ShowResult);
            };

            funcs[arrays[1]] = () =>
            {
                status = DialogManager.ShowStatus(this, ResourceManager.Instance.VehicleDB.GetText("Communicating"));
                Dictionary<string, string> codes = null;

                Task task = Task.Factory.StartNew(() =>
                {
                    Mikuni protocol = new Mikuni(ResourceManager.Instance.VehicleDB, Diag.BoxFactory.Instance.Commbox);
                    codes = protocol.ReadHistoryTroubleCode();
                });

                task.ContinueWith(ShowResult);
            };

            ListView.ItemClick += OnItemClickMikuni;
        }

        private void OnItemClickMikuni(object sender, AdapterView.ItemClickEventArgs e)
        {
            funcs[((TextView)e.View).Text]();
        }

        private void OnSynerject()
        {
            status = DialogManager.ShowStatus(this, ResourceManager.Instance.VehicleDB.GetText("Communicating"));
            Task task = Task.Factory.StartNew(() =>
            {
                Synerject protocol = new Synerject(ResourceManager.Instance.VehicleDB, ResourceManager.Instance.Commbox);
                codes = protocol.ReadTroubleCode();
            });

            task.ContinueWith(ShowResult);
        }

        private void OnItemClickSynerject(object sender, AdapterView.ItemClickEventArgs e)
        {
            StringBuilder text = new StringBuilder();
            text.Append(model);
            text.Append(" ");
            text.Append(((TextView)e.View).Text);
            Toast.MakeText(this, ResourceManager.Instance.VehicleDB.GetText(text.ToString()), ToastLength.Long).Show();
        }

        private void OnVisteonProtocol()
        {
            status = DialogManager.ShowStatus(this, ResourceManager.Instance.VehicleDB.GetText("Communicating"));
            Task task = Task.Factory.StartNew(() =>
            {
                Visteon protocol = new Visteon(ResourceManager.Instance.VehicleDB, ResourceManager.Instance.Commbox);
                Dictionary<string, string> codes = protocol.ReadTroubleCode();
            });

            task.ContinueWith(ShowResult);
        }

        private void OnItemClickVisteon(object sender, AdapterView.ItemClickEventArgs e)
        {
            StringBuilder text = new StringBuilder();
            text.Append(model);
            text.Append(" ");
            text.Append(((TextView)e.View).Text);
            Toast.MakeText(this, ResourceManager.Instance.VehicleDB.GetText(text.ToString()), ToastLength.Long).Show();
        }

        private void OnTroubleCodeItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            StringBuilder text = new StringBuilder();
            text.Append(model);
            text.Append(" ");
            text.Append(((TextView)e.View).Text);
            Toast.MakeText(this, ResourceManager.Instance.VehicleDB.GetText(text.ToString()), ToastLength.Long).Show();
        }

        private void ShowTroubleCode()
        {
            if (codes == null || codes.Count == 0)
                return;

            string[] arrays = new string[codes.Count];
            int i = 0;
            foreach (var item in codes)
            {
                arrays[i++] = item.Key + ": " + item.Value;
            }

            RunOnUiThread(() =>
            {
                ListView.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, arrays);
            }
            );
            ListView.ItemClick -= OnItemClickMikuni;
            ListView.ItemClick += OnTroubleCodeItemClick;
        }

        private void ShowResult(Task t)
        {
            RunOnUiThread(() =>
            {
                status.Dismiss();
                if (t.IsFaulted)
                {
                    DialogManager.ShowFatal(this, t.Exception.InnerException.Message, (sender, e) =>
                    {
                        this.Finish();
                    });
                }
                else
                {
                    ShowTroubleCode();
                }
            });
        }
    }
}