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

using JM.QingQi.Vehicle;

namespace JM.QingQi.AndroidUI
{
    [Activity(Theme = "@style/Theme.StaticDataStream", Label = "Trouble Code")]
    public class TroubleCodeActivity : ListActivity
    {
        public delegate void ProtocolFunc();

        private Dictionary<string, ProtocolFunc> protocolFuncs;
        private Dictionary<string, ProtocolFunc> funcs;
        private string model;
        private List<Core.TroubleCode> codes = null;
        private ProgressDialog status = null;

        private enum UIStatus
        {
            CurrentHistory,
            Result
        }

        private UIStatus uiStatus;

        public TroubleCodeActivity()
        {
            protocolFuncs = new Dictionary<string, ProtocolFunc>();
            protocolFuncs[StaticString.beforeBlank + ResourceManager.Instance.VehicleDB.GetText("QM125T-8H")] = OnSynerjectProtocol;
            protocolFuncs[StaticString.beforeBlank + ResourceManager.Instance.VehicleDB.GetText("QM200GY-F")] = OnMikuniProtocol;
            protocolFuncs[StaticString.beforeBlank + ResourceManager.Instance.VehicleDB.GetText("QM250GY")] = OnSynerjectProtocol;
            protocolFuncs[StaticString.beforeBlank + ResourceManager.Instance.VehicleDB.GetText("QM250T")] = OnSynerjectProtocol;
            protocolFuncs[StaticString.beforeBlank + ResourceManager.Instance.VehicleDB.GetText("QM200-3D")] = OnMikuniProtocol;
            protocolFuncs[StaticString.beforeBlank + ResourceManager.Instance.VehicleDB.GetText("QM200J-3L")] = OnMikuniProtocol;
            protocolFuncs[StaticString.beforeBlank + ResourceManager.Instance.VehicleDB.GetText("QM250J-2L")] = OnVisteonProtocol;
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            // Create your application here
            uiStatus = UIStatus.CurrentHistory;
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

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            if (keyCode == Keycode.Back)
            {
                if (uiStatus == UIStatus.CurrentHistory)
                {
                    this.Finish();
                    return true;
                }
                else if (uiStatus == UIStatus.Result)
                {
                    if (model == StaticString.beforeBlank + ResourceManager.Instance.VehicleDB.GetText("QM250J-2L"))
                    {
                        this.Finish();
                        return true;
                    }
                    else
                    {
                        ListView.ItemClick -= OnTroubleCodeItemClick;
                        ListView.ItemClick -= OnItemClickMikuni;
                        ListView.ItemClick -= OnItemClickSynerject;
                        ListView.ItemClick -= OnItemClickVisteon;
                        if ((model == StaticString.beforeBlank + ResourceManager.Instance.VehicleDB.GetText("QM125T-8H")) ||
                            (model == StaticString.beforeBlank + ResourceManager.Instance.VehicleDB.GetText("QM250GY")) ||
                            (model == StaticString.beforeBlank + ResourceManager.Instance.VehicleDB.GetText("QM250T")))
                        {
                            OnSynerjectProtocol();
                        }
                        else
                        {
                            OnMikuniProtocol();
                        }
                        return true;
                    }
                }
            }
            return base.OnKeyDown(keyCode, e);
        }

        private void OnMikuniProtocol()
        {
            string[] arrays = new string[2];
            arrays[0] = StaticString.beforeBlank + ResourceManager.Instance.VehicleDB.GetText("Read Current Trouble Code");
            arrays[1] = StaticString.beforeBlank + ResourceManager.Instance.VehicleDB.GetText("Read History Trouble Code");
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

				task.ContinueWith((t) => {ShowResult(t, null);});
            };

            funcs[arrays[1]] = () =>
            {
                status = DialogManager.ShowStatus(this, ResourceManager.Instance.VehicleDB.GetText("Communicating"));

                Task task = Task.Factory.StartNew(() =>
                {
                    Mikuni protocol = new Mikuni(ResourceManager.Instance.VehicleDB, Diag.BoxFactory.Instance.Commbox);
                    codes = protocol.ReadHistoryTroubleCode();
                });

				task.ContinueWith((t) => {ShowResult(t, null);});
            };

            ListView.ItemClick += OnItemClickMikuni;
        }

        private void OnItemClickMikuni(object sender, AdapterView.ItemClickEventArgs e)
        {
            funcs[((TextView)e.View).Text]();
        }


        private void OnSynerjectProtocol()
        {
        //    status = DialogManager.ShowStatus(this, ResourceManager.Instance.VehicleDB.GetText("Communicating"));
        //    Task task = Task.Factory.StartNew(() =>
        //    {
        //        Synerject protocol = new Synerject(ResourceManager.Instance.VehicleDB, ResourceManager.Instance.Commbox);
        //        codes = protocol.ReadTroubleCode();
        //    });

        //    task.ContinueWith(ShowResult);
            string[] arrays = new string[2];
            arrays[0] = StaticString.beforeBlank + ResourceManager.Instance.VehicleDB.GetText("Read Current Trouble Code");
            arrays[1] = StaticString.beforeBlank + ResourceManager.Instance.VehicleDB.GetText("Read History Trouble Code");
            ListView.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, arrays);
            funcs = new Dictionary<string, ProtocolFunc>();
            funcs[arrays[0]] = () =>
            {
                status = DialogManager.ShowStatus(this, ResourceManager.Instance.VehicleDB.GetText("Communicating"));
                Task task = Task.Factory.StartNew(() =>
                {
                    Synerject protocol = new Synerject(ResourceManager.Instance.VehicleDB, Diag.BoxFactory.Instance.Commbox);
                    codes = protocol.ReadTroubleCode(false);
                });

				task.ContinueWith((t) => {ShowResult(t, null);});
            };

            funcs[arrays[1]] = () =>
            {
                status = DialogManager.ShowStatus(this, ResourceManager.Instance.VehicleDB.GetText("Communicating"));

                Task task = Task.Factory.StartNew(() =>
                {
                    Synerject protocol = new Synerject(ResourceManager.Instance.VehicleDB, Diag.BoxFactory.Instance.Commbox);
                    codes = protocol.ReadTroubleCode(true);
                });

				task.ContinueWith((t) => {ShowResult(t, null);});
            };

            ListView.ItemClick += OnItemClickSynerject;
        }

        private void OnItemClickSynerject(object sender, AdapterView.ItemClickEventArgs e)
        {
            //StringBuilder text = new StringBuilder();
            //text.Append(model);
            //text.Append(" ");
            //text.Append(((TextView)e.View).Text);
            //Toast.MakeText(this, ResourceManager.Instance.VehicleDB.GetText(text.ToString()), ToastLength.Long).Show();
            funcs[((TextView)e.View).Text]();
        }

        private void OnVisteonProtocol()
        {
            status = DialogManager.ShowStatus(this, ResourceManager.Instance.VehicleDB.GetText("Communicating"));
            Task task = Task.Factory.StartNew(() =>
            {
                Visteon protocol = new Visteon(ResourceManager.Instance.VehicleDB, ResourceManager.Instance.Commbox);
                codes = protocol.ReadTroubleCode();
            });

			task.ContinueWith((t) => {ShowResult(t, (sender, e) => this.Finish());});
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
                arrays[i++] = StaticString.beforeBlank + item.Code + ": " + item.Content;
            }

            RunOnUiThread(() =>
            {
                ListView.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, arrays);
            }
            );
            ListView.ItemClick -= OnItemClickMikuni;
            ListView.ItemClick -= OnItemClickSynerject;
            ListView.ItemClick -= OnItemClickVisteon;
            ListView.ItemClick += OnTroubleCodeItemClick;
        }

		private void ShowResult(Task t, EventHandler<DialogClickEventArgs> listener)
        {
            RunOnUiThread(() =>
            {
                status.Dismiss();
                if (t.IsFaulted)
                {
					DialogManager.ShowFatal(this, StaticString.beforeBlank + t.Exception.InnerException.Message, listener);
                }
                else
                {
                    ShowTroubleCode();
                }
            });
        }
    }
}