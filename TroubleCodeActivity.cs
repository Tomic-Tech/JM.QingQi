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
        private Task troubleCodeTask;

        public TroubleCodeActivity()
        {
            protocolFuncs = new Dictionary<string, ProtocolFunc>();
            protocolFuncs[ResourceManager.Instance.VehicleDB.GetText("QM125T-8H")] = OnSynerject;
            protocolFuncs[ResourceManager.Instance.VehicleDB.GetText("QM200GY-F")] = OnMikuniProtocol;
            protocolFuncs[ResourceManager.Instance.VehicleDB.GetText("QM250GY")] = OnSynerject;
            protocolFuncs[ResourceManager.Instance.VehicleDB.GetText("QM250T")] = OnSynerject;
            protocolFuncs[ResourceManager.Instance.VehicleDB.GetText("QM200-3D")] = OnMikuniProtocol;
            protocolFuncs[ResourceManager.Instance.VehicleDB.GetText("QM200J-3L")] = OnMikuniProtocol;
            protocolFuncs[ResourceManager.Instance.VehicleDB.GetText("QM250J-2L")] = null;
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
            protocolFuncs[model]();
        }

        protected override void OnStop()
        {
            base.OnStop();
            DialogManager.Instance.HideDialog();
        }

        private void OnMikuniProtocol()
        {
            string[] arrays = new string[2];
            arrays[0] = ResourceManager.Instance.VehicleDB.GetText("Read Current Trouble Code");
            arrays[1] = ResourceManager.Instance.VehicleDB.GetText("Read History Trouble Code");
            ListView.Adapter = new ArrayAdapter<string>(
                this,
                Android.Resource.Layout.SimpleListItem1,
                arrays
            );
            funcs = new Dictionary<string, ProtocolFunc>();
            funcs[arrays[0]] = () =>
            {
                try
                {
                    RunOnUiThread(() => DialogManager.Instance.StatusDialogShow(
                        this,
                        ResourceManager.Instance.VehicleDB.GetText("Communicating")
                    )
                    );
                    Mikuni protocol = new Mikuni(ResourceManager.Instance.VehicleDB, Diag.BoxFactory.Instance.Commbox);
                    Dictionary<string, string> codes = protocol.ReadCurrentTroubleCode();
                    ShowTroubleCode(codes);
                }
                catch (System.IO.IOException ex)
                {
                    RunOnUiThread(() =>
                    {
                        DialogManager.Instance.FatalDialogShow(
                            this,
                            ex.Message,
                            null
                            );
                    });
                }
            };

            funcs[arrays[1]] = () =>
            {
                try
                {
                    RunOnUiThread(() =>
                        DialogManager.Instance.StatusDialogShow(
                        this,
                        ResourceManager.Instance.VehicleDB.GetText("Communicating")
                        )
                        );
                    Mikuni protocol = new Mikuni(ResourceManager.Instance.VehicleDB, Diag.BoxFactory.Instance.Commbox);
                    Dictionary<string, string> codes = protocol.ReadHistoryTroubleCode();
                    ShowTroubleCode(codes);

                }
                catch (System.IO.IOException ex)
                {
                    RunOnUiThread(() =>
                    {
                        DialogManager.Instance.FatalDialogShow(this,
                            ex.Message,
                            null
                            );
                    });
                }
            };

            ListView.ItemClick += OnItemClickMikuni;
        }

        private void OnItemClickMikuni(object sender, AdapterView.ItemClickEventArgs e)
        {
            troubleCodeTask = Task.Factory.StartNew(() => funcs[((TextView)e.View).Text]());
        }

        private void OnSynerject()
        {
            RunOnUiThread(() =>
            DialogManager.Instance.StatusDialogShow(
                this,
                ResourceManager.Instance.VehicleDB.GetText("Communicating")
            )
            );
        }

        private void OnItemClickSynerject(object sender, AdapterView.ItemClickEventArgs e)
        {
            StringBuilder text = new StringBuilder();
            text.Append(model);
            text.Append(" ");
            text.Append(((TextView)e.View).Text);
            Toast.MakeText(
                this,
                ResourceManager.Instance.VehicleDB.GetText(text.ToString()),
                ToastLength.Long
            )
                .Show();
        }

        private void OnTroubleCodeItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            StringBuilder text = new StringBuilder();
            text.Append(model);
            text.Append(" ");
            text.Append(((TextView)e.View).Text);
            Toast.MakeText(
                this,
                ResourceManager.Instance.VehicleDB.GetText(text.ToString()),
                ToastLength.Long
            )
                .Show();
        }

        private void ShowTroubleCode(Dictionary<string, string> codes)
        {
            string[] arrays = new string[codes.Count];
            int i = 0;
            foreach (var item in codes)
            {
                arrays[i++] = item.Key + ": " + item.Value;
            }

            RunOnUiThread(() =>
            {
                ListView.Adapter = new ArrayAdapter<string>(
                this,
                Android.Resource.Layout.SimpleListItem1,
                arrays
                );
                DialogManager.Instance.HideDialog();
            }
            );
            ListView.ItemClick -= OnItemClickMikuni;
            ListView.ItemClick += OnTroubleCodeItemClick;
        }
    }
}