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

using JM.Core;
using JM.Vehicles;
using JM.QingQi.Vehicle;

namespace JM.QingQi.AndroidUI
{
    [Activity(Theme = "@style/Theme.Default", Label = "Data Stream")]
    public class DataStreamActivity : Activity
    {
        private delegate void ProtocolFunc();

        private Task task;
        private Dictionary<string, ProtocolFunc> protocolFuncs;
        private Dictionary<string, ProtocolFunc> backFuncs;
        private string model = null;
        private Mikuni mikuni = null;
        private Synerject synerject = null;
        private Visteon visteon = null;
        private TableLayout layout = null;
        private ProgressDialog status = null;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Create your application here
            SetContentView(Resource.Layout.DataStream);
            layout = FindViewById<TableLayout>(Resource.Id.tableLayout);
            layout.RemoveAllViews();

            protocolFuncs = new Dictionary<string, ProtocolFunc>();
            protocolFuncs[StaticString.beforeBlank + Database.GetText("QM125T-8H", "QingQi")] = OnSynerjectProtocol;
            protocolFuncs[StaticString.beforeBlank + Database.GetText("QM200GY-F", "QingQi")] = OnMikuniProtocol;
            protocolFuncs[StaticString.beforeBlank + Database.GetText("QM250GY", "QingQi")] = OnSynerjectProtocol;
            protocolFuncs[StaticString.beforeBlank + Database.GetText("QM250T", "QingQi")] = OnSynerjectProtocol;
            protocolFuncs[StaticString.beforeBlank + Database.GetText("QM200-3D", "QingQi")] = OnMikuniProtocol;
            protocolFuncs[StaticString.beforeBlank + Database.GetText("QM200J-3L", "QingQi")] = OnMikuniProtocol;
            protocolFuncs[StaticString.beforeBlank + Database.GetText("QM250J-2L", "QingQi")] = OnVisteonProtocol;
            protocolFuncs[StaticString.beforeBlank + Database.GetText("QM250J-2L", "QingQi") + "Freeze"] = OnVisteonFreezeProtocol;

            backFuncs = new Dictionary<string, ProtocolFunc>();
            backFuncs[StaticString.beforeBlank + Database.GetText("QM125T-8H", "QingQi")] = OnSynerjectBack;
            backFuncs[StaticString.beforeBlank + Database.GetText("QM200GY-F", "QingQi")] = OnMikuniBack;
            backFuncs[StaticString.beforeBlank + Database.GetText("QM250GY", "QingQi")] = OnSynerjectBack;
            backFuncs[StaticString.beforeBlank + Database.GetText("QM250T", "QingQi")] = OnSynerjectBack;
            backFuncs[StaticString.beforeBlank + Database.GetText("QM200-3D", "QingQi")] = OnMikuniBack;
            backFuncs[StaticString.beforeBlank + Database.GetText("QM200J-3L", "QingQi")] = OnMikuniBack;
            backFuncs[StaticString.beforeBlank + Database.GetText("QM250J-2L", "QingQi")] = OnVisteonBack;
            backFuncs[StaticString.beforeBlank + Database.GetText("QM250J-2L", "QingQi") + "Freeze"] = OnVisteonFreezeBack;
        }

        protected override void OnStart()
        {
            base.OnStart();
            model = Intent.Extras.GetString("Model");
            protocolFuncs[model]();
        }

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            if (keyCode == Keycode.Back)
            {
                status = DialogManager.ShowStatus(this, Database.GetText("Communicating", "System"));
                backFuncs[model]();
                this.Finish();
                return true;
            }
            return base.OnKeyDown(keyCode, e);
        }

        private void OnValueChange(Core.LiveData ld)
        {
            RunOnUiThread(() =>
            {
                Core.LiveDataVector vec = Manager.LiveDataVector;
                int position = vec.ShowedPosition(ld.Index);

                TableRow row = (TableRow)layout.GetChildAt(position);

                TextView v = (TextView)row.GetChildAt(1);
                v.Text = ld.Value;
            }
            );
        }

        private void PreparePage()
        {
            status = DialogManager.ShowStatus(this, Database.GetText("Communicating", "System"));
            Core.LiveDataVector vec = Manager.LiveDataVector;
            for (int i = 0; i < vec.ShowedCount; i++)
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
        }

        private void ShowFault(Task t)
        {
            RunOnUiThread(() =>
            {
                status.Dismiss();
                if (t.IsFaulted)
                {
                    try
                    {
                        mikuni.StopReadDataStream();
                        synerject.StopReadDataStream();
                        visteon.StopReadDataStream();
                    }
                    catch
                    {
                    }
                    DialogManager.ShowFatal(this, t.Exception.InnerException.Message, (sender, e) => 
                    {
                        this.Finish();
                    });
                }
            });
        }

        private void OnMikuniProtocol()
        {
            Manager.LiveDataVector.DeployEnabledIndex();
            Manager.LiveDataVector.DeployShowedIndex();
            PreparePage();

            task = Task.Factory.StartNew(() =>
            {
                mikuni = new Mikuni(Diag.BoxFactory.Instance.Commbox);
                mikuni.ReadDataStream(Manager.LiveDataVector);
            });

            task.ContinueWith(ShowFault);
        }

        private void OnMikuniBack()
        {
            if (mikuni != null)
                mikuni.StopReadDataStream();
            if (task != null)
                task.Wait();
        }

        private void OnSynerjectProtocol()
        {
            Core.LiveDataVector vec = Manager.LiveDataVector;
            for (int i = 0; i < vec.Count; i++)
            {
                if (model == StaticString.beforeBlank + Database.GetText("QM125T-8H", "QingQi"))
                {
                    if ((vec[i].ShortName == "CRASH") ||
                        (vec[i].ShortName == "DIST_ACT_MIL") ||
                        (vec[i].ShortName == "ISA_AD_T_DLY") ||
                        (vec[i].ShortName == "ISA_ANG_DUR_MEC") ||
                        (vec[i].ShortName == "ISA_CTL_IS") ||
                        (vec[i].ShortName == "ISC_ISA_AD_MV") ||
                        (vec[i].ShortName == "LV_EOL_EFP_PRIM") ||
                        (vec[i].ShortName == "LV_EOL_EFP_PRIM_ACT") ||
                        (vec[i].ShortName == "LV_IMMO_PROG") ||
                        (vec[i].ShortName == "LV_IMMO_ECU_PROG") ||
                        (vec[i].ShortName == "LV_LOCK_IMOB") ||
                        (vec[i].ShortName == "LV_VIP") ||
                        (vec[i].ShortName == "LV_EOP") ||
                        (vec[i].ShortName == "TCOPWM") ||
                        (vec[i].ShortName == "VS_8") ||
                        (vec[i].ShortName == "V_TPS_1_BAS") ||
                        (vec[i].ShortName == "LV_SAV"))
                    {
                        vec[i].Enabled = false;
                    }
                }
                else if (model == StaticString.beforeBlank + Database.GetText("QM250GY", "QingQi"))
                {
                    if ((vec[i].ShortName == "CRASH") ||
                        (vec[i].ShortName == "DIST_ACT_MIL") ||
                        (vec[i].ShortName == "ISA_AD_T_DLY") ||
                        (vec[i].ShortName == "ISA_ANG_DUR_MEC") ||
                        (vec[i].ShortName == "ISA_CTL_IS") ||
                        (vec[i].ShortName == "ISC_ISA_AD_MV") ||
                        (vec[i].ShortName == "LV_EOL_EFP_PRIM") ||
                        (vec[i].ShortName == "LV_EOL_EFP_PRIM_ACT") ||
                        (vec[i].ShortName == "LV_IMMO_PROG") ||
                        (vec[i].ShortName == "LV_IMMO_ECU_PROG") ||
                        (vec[i].ShortName == "LV_LOCK_IMOB") ||
                        (vec[i].ShortName == "LV_LSH_UP_1") ||
                        (vec[i].ShortName == "LV_VIP") ||
                        (vec[i].ShortName == "LV_EOP") ||
                        (vec[i].ShortName == "TCOPWM") ||
                        (vec[i].ShortName == "VS_8") ||
                        (vec[i].ShortName == "LV_SAV"))
                    {
                        vec[i].Enabled = false;
                    }
                }
                else if (model == StaticString.beforeBlank + Database.GetText("QM250T", "QingQi"))
                {
                    if ((vec[i].ShortName == "CRASH") ||
                        (vec[i].ShortName == "DIST_ACT_MIL") ||
                        (vec[i].ShortName == "ISA_AD_T_DLY") ||
                        (vec[i].ShortName == "ISA_ANG_DUR_MEC") ||
                        (vec[i].ShortName == "ISA_CTL_IS") ||
                        (vec[i].ShortName == "ISC_ISA_AD_MV") ||
                        (vec[i].ShortName == "LV_EOL_EFP_PRIM") ||
                        (vec[i].ShortName == "LV_EOL_EFP_PRIM_ACT") ||
                        (vec[i].ShortName == "LV_IMMO_PROG") ||
                        (vec[i].ShortName == "LV_IMMO_ECU_PROG") ||
                        (vec[i].ShortName == "LV_LOCK_IMOB") ||
                        (vec[i].ShortName == "LV_LSH_UP_1") ||
                        (vec[i].ShortName == "LV_VIP") ||
                        (vec[i].ShortName == "LV_EOP") ||
                        (vec[i].ShortName == "VS_8") ||
                        (vec[i].ShortName == "V_TPS_1_BAS") ||
                        (vec[i].ShortName == "LV_SAV"))
                    {
                        vec[i].Enabled = false;
                    }
                }
            }
            vec.DeployEnabledIndex();
            vec.DeployShowedIndex();
            PreparePage();

            task = Task.Factory.StartNew(() =>
            {
                synerject = new Synerject(Diag.BoxFactory.Instance.Commbox);
                synerject.ReadDataStream(Manager.LiveDataVector);
            });

            task.ContinueWith(ShowFault);
        }

        private void OnSynerjectBack()
        {
            if (synerject != null)
                synerject.StopReadDataStream();
            if (task != null)
                task.Wait();
        }

        private void OnVisteonProtocol()
        {
            Manager.LiveDataVector.DeployEnabledIndex();
            Manager.LiveDataVector.DeployShowedIndex();
            PreparePage();

            task = Task.Factory.StartNew(() =>
            {
                visteon = new Visteon(Diag.BoxFactory.Instance.Commbox);
                visteon.ReadDataStream(Manager.LiveDataVector);
            });

            task.ContinueWith(ShowFault);
        }

        private void OnVisteonBack()
        {
            if (visteon != null)
                visteon.StopReadDataStream();
            if (task != null)
                task.Wait();
        }

        private void OnVisteonFreezeProtocol()
        {
			Manager.LiveDataVector.DeployEnabledIndex();
			Manager.LiveDataVector.DeployShowedIndex();
            PreparePage();

            status = DialogManager.ShowStatus(this, Database.GetText("Communicating", "System"));

            task = Task.Factory.StartNew(() =>
            {
                visteon = new Visteon(Diag.BoxFactory.Instance.Commbox);
                visteon.ReadFreezeFrame(Manager.LiveDataVector);
            });

            task.ContinueWith(ShowFault);
        }

        private void OnVisteonFreezeBack()
        {
            //if (task != null)
            //    task.Wait();
        }
    }
}