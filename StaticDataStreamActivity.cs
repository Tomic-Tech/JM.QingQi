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
    [Activity(Theme = "@style/Theme.Default", Label = "Static Data Stream Activity")]
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
            protocolFuncs = new Dictionary<string, ProtocolFunc>();
            protocolFuncs[ResourceManager.Instance.VehicleDB.GetText("QM125T-8H")] = OnSynerjectProtocol;
            protocolFuncs[ResourceManager.Instance.VehicleDB.GetText("QM200GY-F")] = OnMikuniProtocol;
            protocolFuncs[ResourceManager.Instance.VehicleDB.GetText("QM250GY")] = OnSynerjectProtocol;
            protocolFuncs[ResourceManager.Instance.VehicleDB.GetText("QM250T")] = OnSynerjectProtocol;
            protocolFuncs[ResourceManager.Instance.VehicleDB.GetText("QM200-3D")] = OnMikuniProtocol;
            protocolFuncs[ResourceManager.Instance.VehicleDB.GetText("QM200J-3L")] = OnMikuniProtocol;
            protocolFuncs[ResourceManager.Instance.VehicleDB.GetText("QM250J-2L")] = OnVisteonProtocol;

            model = Intent.Extras.GetString("Model");
            protocolFuncs[model]();
        }

        protected override void OnStart()
        {
            base.OnStart();
        }

        private void OnMikuniProtocol()
        {
            status = DialogManager.ShowStatus(this, ResourceManager.Instance.VehicleDB.GetText("Communicating"));
            Core.LiveDataVector vec = ResourceManager.Instance.LiveDataVector;
            task = Task.Factory.StartNew(() =>
            {
                Mikuni protocol = new Mikuni(ResourceManager.Instance.VehicleDB, ResourceManager.Instance.Commbox);
                protocol.StaticDataStream(vec);
            });

            task.ContinueWith((t) =>
            {
                ShowResult(t);
            });
        }

        private void OnSynerjectProtocol()
        {
            status = DialogManager.ShowStatus(this, ResourceManager.Instance.VehicleDB.GetText("Communicating"));
            Core.LiveDataVector vec = ResourceManager.Instance.LiveDataVector;

            for (int i = 0; i < vec.Count; i++)
            {
                if (model == ResourceManager.Instance.VehicleDB.GetText("QM125T-8H"))
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
                else if (model == ResourceManager.Instance.VehicleDB.GetText("QM250GY"))
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
                        (vec[i].ShortName == "LV_SAV"))
                    {
                        vec[i].Enabled = false;
                    }
                }
                else if (model == ResourceManager.Instance.VehicleDB.GetText("QM250T"))
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
                        (vec[i].ShortName == "VS_8") ||
                        (vec[i].ShortName == "V_TPS_1_BAS") ||
                        (vec[i].ShortName == "LV_SAV"))
                    {
                        vec[i].Enabled = false;
                    }
                }
            }
            vec.DeployEnabledIndex();
            task = Task.Factory.StartNew(() =>
            {
                Synerject protocol = new Synerject(ResourceManager.Instance.VehicleDB, ResourceManager.Instance.Commbox);
                protocol.StaticDataStream(vec);
            });

            task.ContinueWith((t) =>
            {
                ShowResult(t);
            });
        }

        private void OnVisteonProtocol()
        {
            status = DialogManager.ShowStatus(this, ResourceManager.Instance.VehicleDB.GetText("Communicating"));
            Core.LiveDataVector vec = ResourceManager.Instance.LiveDataVector;
            task = Task.Factory.StartNew(() =>
            {
                Visteon protocol = new Visteon(ResourceManager.Instance.VehicleDB, ResourceManager.Instance.Commbox);
                protocol.StaticDataStream(vec);
            });

            task.ContinueWith((t) =>
            {
                ShowResult(t);
            });
        }

        private void ShowResult(Task t)
        {
            Core.LiveDataVector vec = ResourceManager.Instance.LiveDataVector;
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
                        arrays[i] = vec[vec.ShowedIndex(i)].Content + " : " + vec[vec.ShowedIndex(i)].Value + " " + vec[vec.ShowedIndex(i)].Unit;
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