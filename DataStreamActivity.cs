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
            protocolFuncs[ResourceManager.Instance.VehicleDB.GetText("QM125T-8H")] = OnSynerjectProtocol;
            protocolFuncs[ResourceManager.Instance.VehicleDB.GetText("QM200GY-F")] = OnMikuniProtocol;
            protocolFuncs[ResourceManager.Instance.VehicleDB.GetText("QM250GY")] = OnSynerjectProtocol;
            protocolFuncs[ResourceManager.Instance.VehicleDB.GetText("QM250T")] = OnSynerjectProtocol;
            protocolFuncs[ResourceManager.Instance.VehicleDB.GetText("QM200-3D")] = OnMikuniProtocol;
            protocolFuncs[ResourceManager.Instance.VehicleDB.GetText("QM200J-3L")] = OnMikuniProtocol;
            protocolFuncs[ResourceManager.Instance.VehicleDB.GetText("QM250J-2L")] = OnVisteonProtocol;
            protocolFuncs[ResourceManager.Instance.VehicleDB.GetText("QM250J-2L") + "Freeze"] = OnVisteonFreezeProtocol;

            backFuncs = new Dictionary<string, ProtocolFunc>();
            backFuncs[ResourceManager.Instance.VehicleDB.GetText("QM125T-8H")] = OnSynerjectBack;
            backFuncs[ResourceManager.Instance.VehicleDB.GetText("QM200GY-F")] = OnMikuniBack;
            backFuncs[ResourceManager.Instance.VehicleDB.GetText("QM250GY")] = OnSynerjectBack;
            backFuncs[ResourceManager.Instance.VehicleDB.GetText("QM250T")] = OnSynerjectBack;
            backFuncs[ResourceManager.Instance.VehicleDB.GetText("QM200-3D")] = OnMikuniBack;
            backFuncs[ResourceManager.Instance.VehicleDB.GetText("QM200J-3L")] = OnMikuniBack;
            backFuncs[ResourceManager.Instance.VehicleDB.GetText("QM250J-2L")] = OnVisteonBack;
            backFuncs[ResourceManager.Instance.VehicleDB.GetText("QM250J-2L") + "Freeze"] = OnVisteonFreezeBack;
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
                status = DialogManager.ShowStatus(this, JM.Core.SysDB.GetText("Communicating"));
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
                Core.LiveDataVector vec = ResourceManager.Instance.LiveDataVector;
                int position = vec.ShowedPosition(ld.Index);

                TableRow row = (TableRow)layout.GetChildAt(position);

                TextView v = (TextView)row.GetChildAt(1);
                v.Text = ld.Value;
            }
            );
        }

        private void PreparePage()
        {
            status = DialogManager.ShowStatus(this, ResourceManager.Instance.VehicleDB.GetText("Communicating"));
            Core.LiveDataVector vec = ResourceManager.Instance.LiveDataVector;
            for (int i = 0; i < ResourceManager.Instance.LiveDataVector.ShowedCount; i++)
            {
                TextView content = new TextView(this);
                content.Text = vec[vec.ShowedIndex(i)].Content;
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
                if (t.IsFaulted)
                {
                    DialogManager.ShowFatal(this, t.Exception.InnerException.Message, null);
                }
            });
        }

        private void OnMikuniProtocol()
        {
            PreparePage();

            task = Task.Factory.StartNew(() =>
            {
                mikuni = new Mikuni(ResourceManager.Instance.VehicleDB, Diag.BoxFactory.Instance.Commbox);
                mikuni.ReadDataStream(ResourceManager.Instance.LiveDataVector);
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
            PreparePage();

            task = Task.Factory.StartNew(() =>
            {
                synerject = new Synerject(ResourceManager.Instance.VehicleDB, Diag.BoxFactory.Instance.Commbox);
                synerject.ReadDataStream(ResourceManager.Instance.LiveDataVector);
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
            PreparePage();

            task = Task.Factory.StartNew(() =>
            {
                visteon = new Visteon(ResourceManager.Instance.VehicleDB, Diag.BoxFactory.Instance.Commbox);
                visteon.ReadDataStream(ResourceManager.Instance.LiveDataVector);
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
            status = DialogManager.ShowStatus(this, JM.Core.SysDB.GetText("Communicating"));

            PreparePage();

            task = Task.Factory.StartNew(() =>
            {
                visteon = new Visteon(ResourceManager.Instance.VehicleDB, Diag.BoxFactory.Instance.Commbox);
                visteon.ReadFreezeFrame(ResourceManager.Instance.LiveDataVector);
            });

            task.ContinueWith(ShowFault);
        }

        private void OnVisteonFreezeBack()
        {
            if (task != null)
                task.Wait();
        }
    }
}