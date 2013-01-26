using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    [Activity(Theme = "@style/Theme.StaticDataStream", Label = "Static Data Stream Activity")]
    public class StaticDataStreamActivity : ListActivity
    {
        private delegate void ProtocolFunc();

        private Task task;
        private Dictionary<string, ProtocolFunc> protocolFuncs;
        private string model = null;
        private ProgressDialog status = null;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Create your application here
            Window.SetFlags(WindowManagerFlags.KeepScreenOn, WindowManagerFlags.KeepScreenOn);
            protocolFuncs = new Dictionary<string, ProtocolFunc>();
            protocolFuncs[Database.GetText("QM125T-8H", "QingQi")] = OnSynerjectProtocol;
            protocolFuncs[Database.GetText("QM200GY-F", "QingQi")] = OnMikuniProtocol;
            protocolFuncs[Database.GetText("QM250GY", "QingQi")] = OnSynerjectProtocol;
            protocolFuncs[Database.GetText("QM250T", "QingQi")] = OnSynerjectProtocol;
            protocolFuncs[Database.GetText("QM200-3D", "QingQi")] = OnMikuniProtocol;
            protocolFuncs[Database.GetText("QM200J-3L", "QingQi")] = OnMikuniProtocol;
            protocolFuncs[Database.GetText("QM250J-2L", "QingQi")] = OnVisteonProtocol;

            model = Intent.Extras.GetString("Model");
            protocolFuncs[model]();
        }

        protected override void OnStart()
        {
            base.OnStart();
        }

        private void OnMikuniProtocol()
        {
            status = DialogManager.ShowStatus(this, Database.GetText("Communicating", "System"));
            Manager.LiveDataVector = Database.GetLiveData("QingQi");
            for (int i = 0; i < Manager.LiveDataVector.Count; i++)
            {
                if ((Manager.LiveDataVector[i].ShortName == "TS")
                    || (Manager.LiveDataVector[i].ShortName == "ERF")
                    || (Manager.LiveDataVector[i].ShortName == "IS"))
                {
                    Manager.LiveDataVector[i].Enabled = false;
                }
            }
            Core.LiveDataVector vec = Manager.LiveDataVector;
            vec.DeployEnabledIndex();
            vec.DeployShowedIndex();
            task = Task.Factory.StartNew(() =>
            {
                if (!Manager.Commbox.Close() || !Manager.Commbox.Open())
                {
                    throw new IOException(Database.GetText("Open Commbox Fail", "System"));
                }
                Diag.MikuniOptions options = new Diag.MikuniOptions();
                options.Parity = Diag.MikuniParity.Even;
                Mikuni protocol = new Mikuni(Manager.Commbox, options);
                protocol.StaticDataStream(vec);
            });

            task.ContinueWith((t) =>
            {
                ShowResult(t);
            });
        }

        private void OnSynerjectProtocol()
        {
            status = DialogManager.ShowStatus(this, Database.GetText("Communicating", "System"));
            //Manager.LiveDataVector = Database.GetLiveData("Synerject");
            Manager.LiveDataVector = Database.GetLiveData(model);
            Core.LiveDataVector vec = Manager.LiveDataVector;

            for (int i = 0; i < vec.Count; i++)
            {
                if ((vec[i].ShortName == "N") ||
                    (vec[i].ShortName == "VBK_MMV") ||
                    (vec[i].ShortName == "STATE_EFP") ||
                    (vec[i].ShortName == "TI_LAM_COR") ||
                    (vec[i].ShortName == "IGA_1") ||
                    (vec[i].ShortName == "VLS_UP_1") ||
                    (vec[i].ShortName == "AMP") ||
                    (vec[i].ShortName == "TCO") ||
                    (vec[i].ShortName == "TPS_MTC_1"))
                {
                    vec[i].Enabled = true;
                    vec[i].Showed = true;
                }
                else
                {
                    vec[i].Enabled = false;
                }
            }
            vec.DeployEnabledIndex();
            vec.DeployShowedIndex();
            task = Task.Factory.StartNew(() =>
            {
                if (!Manager.Commbox.Close() || !Manager.Commbox.Open())
                {
                    throw new IOException(Database.GetText("Open Commbox Fail", "System"));
                }
                Synerject protocol = new Synerject(Manager.Commbox);
                protocol.StaticDataStream(vec);
            });

            task.ContinueWith((t) =>
            {
                ShowResult(t);
            });
        }

        private void OnVisteonProtocol()
        {
            status = DialogManager.ShowStatus(this, Database.GetText("Communicating", "System"));
            Manager.LiveDataVector = Database.GetLiveData("Visteon");
            Core.LiveDataVector vec = Manager.LiveDataVector;
            task = Task.Factory.StartNew(() =>
            {
                if (!Manager.Commbox.Close() || !Manager.Commbox.Open())
                {
                    throw new IOException(Database.GetText("Open Commbox Fail", "System"));
                }
                Visteon protocol = new Visteon(Manager.Commbox);
                protocol.StaticDataStream(vec);
            });

            task.ContinueWith((t) =>
            {
                ShowResult(t);
            });
        }

        private void ShowResult(Task t)
        {
            Core.LiveDataVector vec = Manager.LiveDataVector;
            RunOnUiThread(() =>
            {
                if (t.IsFaulted)
                {
                    DialogManager.ShowFatal(this, t.Exception.InnerException.Message, (sender, e) =>
                    {
                        this.Finish();
                    });
                }
                else
                {
                    string[] arrays = new string[vec.ShowedCount];
                    for (int i = 0; i < vec.ShowedCount; i++)
                    {
                        arrays[i] = StaticString.beforeBlank + vec[vec.ShowedIndex(i)].Content + " : " + vec[vec.ShowedIndex(i)].Value + " " + vec[vec.ShowedIndex(i)].Unit;
                    }
                    ListAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, arrays);
                    ListView.ItemClick += OnItemClick;
                    status.Dismiss();
                }
            });
        }

        private void OnItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            Intent intent = new Intent(this, typeof(DetailDataStreamActivity));
            intent.PutExtra("Model", model);
            intent.PutExtra("Index", e.Position);
            StartActivity(intent);
        }

    }
}