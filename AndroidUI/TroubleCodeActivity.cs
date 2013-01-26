using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

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
    [Activity(Theme = "@style/Theme.StaticDataStream", Label = "Trouble Code")]
    public class TroubleCodeActivity : ListActivity
    {
        public delegate void ProtocolFunc();

        private Dictionary<string, ProtocolFunc> protocolFuncs;
        private Dictionary<string, ProtocolFunc> funcs;
        private string model;
        private List<Core.TroubleCode> codes = null;
        private ProgressDialog status = null;
        private int currentIndex = 0;

        private enum UIStatus
        {
            CurrentHistory,
            Result
        }

        private UIStatus uiStatus;

        public TroubleCodeActivity()
        {
            protocolFuncs = new Dictionary<string, ProtocolFunc>();
            protocolFuncs[Database.GetText("QM125T-8H", "QingQi")] = OnSynerjectProtocol;
            protocolFuncs[Database.GetText("QM200GY-F", "QingQi")] = OnMikuniProtocol;
            protocolFuncs[Database.GetText("QM250GY", "QingQi")] = OnSynerjectProtocol;
            protocolFuncs[Database.GetText("QM250T", "QingQi")] = OnSynerjectProtocol;
            protocolFuncs[Database.GetText("QM200-3D", "QingQi")] = OnMikuniProtocol;
            protocolFuncs[Database.GetText("QM200J-3L", "QingQi")] = OnMikuniProtocol;
            protocolFuncs[Database.GetText("QM250J-2L", "QingQi")] = OnVisteonProtocol;
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            // Create your application here
            Window.SetFlags(WindowManagerFlags.KeepScreenOn, WindowManagerFlags.KeepScreenOn);
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
                    uiStatus = UIStatus.CurrentHistory;
                    if (model == Database.GetText("QM250J-2L", "QingQi"))
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
                        if ((model == Database.GetText("QM125T-8H", "QingQi")) ||
                            (model == Database.GetText("QM250GY", "QingQi")) ||
                            (model == Database.GetText("QM250T", "QingQi")))
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
            arrays[0] = StaticString.beforeBlank + Database.GetText("Read Current Trouble Code", "System");
            arrays[1] = StaticString.beforeBlank + Database.GetText("Read History Trouble Code", "System");
            ListView.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, arrays);
            ListView.SetSelection(currentIndex);
            funcs = new Dictionary<string, ProtocolFunc>();
            funcs[arrays[0].TrimStart(' ')] = () =>
            {
                status = DialogManager.ShowStatus(this, Database.GetText("Communicating", "System"));
                Task task = Task.Factory.StartNew(() =>
                {
                    if (!Manager.Commbox.Close() || !Manager.Commbox.Open())
                    {
                        throw new IOException(Database.GetText("Open Commbox Fail", "System"));
                    }
                    Diag.MikuniOptions options = new Diag.MikuniOptions();
                    options.Parity = Diag.MikuniParity.Even;
                    Mikuni protocol = new Mikuni(Diag.BoxFactory.Instance.Commbox, options);
                    codes = protocol.ReadCurrentTroubleCode("QingQi");
                });

				task.ContinueWith((t) => {ShowResult(t, null);});
            };

            funcs[arrays[1].TrimStart(' ')] = () =>
            {
                status = DialogManager.ShowStatus(this, Database.GetText("Communicating", "System"));

                Task task = Task.Factory.StartNew(() =>
                {
                    if (!Manager.Commbox.Close() || !Manager.Commbox.Open())
                    {
                        throw new IOException(Database.GetText("Open Commbox Fail", "System"));
                    }
                    Diag.MikuniOptions options = new Diag.MikuniOptions();
                    options.Parity = Diag.MikuniParity.Even;
                    Mikuni protocol = new Mikuni(Diag.BoxFactory.Instance.Commbox, options);
                    codes = protocol.ReadHistoryTroubleCode("QingQi");
                });

				task.ContinueWith((t) => {ShowResult(t, null);});
            };

            ListView.ItemClick += OnItemClickMikuni;
        }

        private void OnItemClickMikuni(object sender, AdapterView.ItemClickEventArgs e)
        {
            currentIndex = e.Position;
            string text = ((TextView)e.View).Text;
            funcs[text.TrimStart(' ')]();
        }


        private void OnSynerjectProtocol()
        {
        //    status = DialogManager.ShowStatus(this, Database.GetText("Communicating"));
        //    Task task = Task.Factory.StartNew(() =>
        //    {
        //        Synerject protocol = new Synerject(Database, ResourceManager.Instance.Commbox);
        //        codes = protocol.ReadTroubleCode();
        //    });

        //    task.ContinueWith(ShowResult);
            string[] arrays = new string[2];
            arrays[0] = StaticString.beforeBlank + Database.GetText("Read Current Trouble Code", "System");
            arrays[1] = StaticString.beforeBlank + Database.GetText("Read History Trouble Code", "System");
            ListView.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, arrays);
            ListView.SetSelection(currentIndex);
            funcs = new Dictionary<string, ProtocolFunc>();
            funcs[arrays[0].TrimStart(' ')] = () =>
            {
                status = DialogManager.ShowStatus(this, Database.GetText("Communicating", "System"));
                Task task = Task.Factory.StartNew(() =>
                {
                    if (!Manager.Commbox.Close() || !Manager.Commbox.Open())
                    {
                        throw new IOException(Database.GetText("Open Commbox Fail", "System"));
                    }
                    Synerject protocol = new Synerject(Diag.BoxFactory.Instance.Commbox);
                    codes = protocol.ReadTroubleCode(false);
                });

				task.ContinueWith((t) => {ShowResult(t, null);});
            };

            funcs[arrays[1].TrimStart(' ')] = () =>
            {
                status = DialogManager.ShowStatus(this, Database.GetText("Communicating", "System"));

                Task task = Task.Factory.StartNew(() =>
                {
                    if (!Manager.Commbox.Close() || !Manager.Commbox.Open())
                    {
                        throw new IOException(Database.GetText("Open Commbox Fail", "System"));
                    }
                    Synerject protocol = new Synerject(Diag.BoxFactory.Instance.Commbox);
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
            //Toast.MakeText(this, Database.GetText(text.ToString()), ToastLength.Long).Show();
            currentIndex = e.Position;
            string text = ((TextView)e.View).Text;
            funcs[text.TrimStart(' ')]();
        }

        private void OnVisteonProtocol()
        {
            status = DialogManager.ShowStatus(this, Database.GetText("Communicating", "System"));
            Task task = Task.Factory.StartNew(() =>
            {
                if (!Manager.Commbox.Close() || !Manager.Commbox.Open())
                {
                    throw new IOException(Database.GetText("Open Commbox Fail", "System"));
                }
                Visteon protocol = new Visteon(Manager.Commbox);
                codes = protocol.ReadTroubleCode();
            });

			task.ContinueWith((t) => {ShowResult(t, (sender, e) => this.Finish());});
        }

        private void OnItemClickVisteon(object sender, AdapterView.ItemClickEventArgs e)
        {
            //StringBuilder text = new StringBuilder();
            //text.Append(model);
            //text.Append(" ");
            //text.Append(((TextView)e.View).Text);
            Toast.MakeText(this, codes[e.Position].Description, ToastLength.Long).Show();
        }

        private void OnTroubleCodeItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            //StringBuilder text = new StringBuilder();
            //text.Append(model);
            //text.Append(" ");
            //text.Append(((TextView)e.View).Text);
            Toast.MakeText(this, codes[e.Position].Description, ToastLength.Long).Show();
        }

        private void ShowTroubleCode()
        {
            if (codes == null || codes.Count == 0)
                return;

            uiStatus = UIStatus.Result;
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