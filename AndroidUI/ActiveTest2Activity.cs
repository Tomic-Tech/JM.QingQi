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
    [Activity(Theme = "@style/Theme.Default", Label = "Active Test")]
    public class ActiveTest2Activity : Activity
    {
        private delegate void ProtocolFunc();
        private Task task;
        private string model;
        private string sub;
        private Dictionary<string, ProtocolFunc> protocolFuncs;
        private Dictionary<string, ProtocolFunc> synerjectFuncs;
        private ProgressDialog status;
        private TableLayout layout;
        private Button positionBtn;
        private Button negativeBtn;
        private JM.Diag.AbstractECU protocol;

        public ActiveTest2Activity()
        {
            protocolFuncs = new Dictionary<string, ProtocolFunc>();
            protocolFuncs[Database.GetText("QM250GY", "QingQi")] = OnSynerject;
            protocolFuncs[Database.GetText("QM250T", "QingQi")] = OnSynerject;

            synerjectFuncs = new Dictionary<string, ProtocolFunc>();
            synerjectFuncs[Database.GetText("Injector", "System")] = () =>
            {
                positionBtn.Text = Database.GetText("Injector On Test", "QingQi");
                negativeBtn.Text = Database.GetText("Injector Off Test", "QingQi");

                task = Task.Factory.StartNew(() => 
                {
                    foreach (var v in Manager.LiveDataVector)
                    {
                        if (v.ShortName == "INJ_MODE")
                        {
                            v.Enabled = true;
                            v.Showed = true;
                        }
                    }

                    PreparePage();

                    if (!Manager.Commbox.Close() || !Manager.Commbox.Open())
                    {
                        throw new IOException(Database.GetText("Open Commbox Fail", "System"));
                    }
                    protocol = new Synerject(Diag.BoxFactory.Instance.Commbox);
                    ((Synerject)protocol).Active(Manager.LiveDataVector, Database.GetText("Injector", "System"));
                }).ContinueWith(ShowResult);
            };

            synerjectFuncs[Database.GetText("Ignition Coil", "System")] = () =>
            {
                positionBtn.Text = Database.GetText("Ignition Coil On Test", "QingQi");
                negativeBtn.Text = Database.GetText("Ignition Coil Off Test", "QingQi");

                task = Task.Factory.StartNew(() =>
                {
                    foreach (var v in Manager.LiveDataVector)
                    {
                        if ((v.ShortName == "CUR_IGC_DIAG_cyl1") ||
                            (v.ShortName == "IGA_1") ||
                            (v.ShortName == "TD_1"))
                        {
                            v.Enabled = true;
                            v.Showed = true;
                        }
                    }
                    PreparePage();

                    if (!Manager.Commbox.Close() || !Manager.Commbox.Open())
                    {
                        throw new IOException(Database.GetText("Open Commbox Fail", "System"));
                    }
                    protocol = new Synerject(Diag.BoxFactory.Instance.Commbox);
                    ((Synerject)protocol).Active(Manager.LiveDataVector, Database.GetText("Ignition Coil", "System"));
                }).ContinueWith(ShowResult);
            };

            synerjectFuncs[Database.GetText("Fuel Pump", "System")] = () =>
            {
                positionBtn.Text = Database.GetText("Fuel Pump On Test", "QingQi");
                negativeBtn.Text = Database.GetText("Fuel Pump Off Test", "QingQi");

                task = Task.Factory.StartNew(() =>
                {
                    foreach (var v in Manager.LiveDataVector)
                    {
                        if (v.ShortName == "STATE_EFP")
                        {
                            v.Enabled = true;
                            v.Showed = true;
                        }
                    }
                    PreparePage();
                    if (!Manager.Commbox.Close() || !Manager.Commbox.Open())
                    {
                        throw new IOException(Database.GetText("Open Commbox Fail", "System"));
                    }
                    protocol = new Synerject(Diag.BoxFactory.Instance.Commbox);
                    ((Synerject)protocol).Active(Manager.LiveDataVector, Database.GetText("Fuel Pump", "System"));
                }).ContinueWith(ShowResult);
            };
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Create your application here
            Window.SetFlags(WindowManagerFlags.KeepScreenOn, WindowManagerFlags.KeepScreenOn);
            SetContentView(Resource.Layout.ActiveTest);

            layout = FindViewById<TableLayout>(Resource.Id.activeTestLayout);
            positionBtn = FindViewById<Button>(Resource.Id.buttonPositive);
            positionBtn.Click += (sender, e) => 
            {
                if (protocol != null)
                    protocol.ActiveOn = Diag.AbstractECU.ActiveState.Positive;
            };

            negativeBtn = FindViewById<Button>(Resource.Id.buttonNegative);
            negativeBtn.Click += (sender, e) => 
            {
                if (protocol != null)
                    protocol.ActiveOn = Diag.AbstractECU.ActiveState.Negative;
            };
        }

        protected override void OnStart()
        {
            base.OnStart();

            layout.RemoveAllViews();
            model = Intent.Extras.GetString("Model");
            sub = Intent.Extras.GetString("Sub");
            protocolFuncs[model]();
        }

        protected override void OnStop()
        {
            base.OnStop();
            if (task != null)
            {
                if (protocol != null)
                    protocol.ActiveOn = Diag.AbstractECU.ActiveState.Stop;
                task.Wait();
            }
        }

        private void OnValueChange(Core.LiveData ld)
        {
            RunOnUiThread(() => 
            {
                Core.LiveDataVector vec = Manager.LiveDataVector;
                int position = vec.ShowedPosition(ld.Position);

                TableRow row = (TableRow)layout.GetChildAt(position);

                TextView v = (TextView)row.GetChildAt(1);
                v.Text = ld.Value;
            });
        }

        private void PreparePage()
        {
            Manager.LiveDataVector.DeployEnabledIndex();
            Manager.LiveDataVector.DeployShowedIndex();
            Core.LiveDataVector vec = Manager.LiveDataVector;
            RunOnUiThread(() => 
            {
                for (int i = 0; i < vec.ShowedCount; ++i)
                {
                    TextView content = new TextView(this);
                    content.Text = StaticString.beforeBlank + vec[vec.ShowedIndex(i)].Content;
                    TextView unit = new TextView(this);
                    unit.Text = vec[vec.ShowedIndex(i)].Unit;
                    TextView value = new TextView(this);
                    value.Text = vec[vec.ShowedIndex(i)].Value;
                    TableRow row = new TableRow(this);
                    row.AddView(content);
                    row.AddView(value);
                    row.AddView(unit);
                    layout.AddView(row);

                    vec[vec.ShowedIndex(i)].PropertyChanged += (sender, e) =>
                    {
                        if (e.PropertyName == "Value")
                        {
                            OnValueChange((Core.LiveData)sender);
                        }
                    };
                }
                status.Dismiss();
            });
        }

        private void OnSynerject()
        {
            status = DialogManager.ShowStatus(this, Database.GetText("Communicating", "System"));
            Manager.LiveDataVector = Database.GetLiveData("Synerject");
            foreach (var v in Manager.LiveDataVector)
            {
                v.Enabled = false;
            }
            synerjectFuncs[sub]();
        }

        private void ShowResult(Task t)
        {
            RunOnUiThread(() => 
            {
                if (t.IsFaulted)
                {
                    DialogManager.ShowFatal(this, t.Exception.InnerException.Message, null);
                }
            });
        }
    }
}