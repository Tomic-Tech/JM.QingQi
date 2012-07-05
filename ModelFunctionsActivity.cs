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
    [Activity(Theme = "@style/Theme.Default", Label = "Function Select")]
    public class ModelFunctionsActivity : ListActivity
    {
        private delegate void ProtocolFunc();

        private Dictionary<string, ProtocolFunc> protocolFuncs;
        private string model = null;
        Dictionary<string, ProtocolFunc> funcs;

        public ModelFunctionsActivity()
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

            ListView.ItemClick -= OnItemClick;
            model = Intent.GetStringExtra("MenuClick");
            ProtocolSelected();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        private void ProtocolSelected()
        {
            protocolFuncs[model]();
        }

        private void OnMikuniProtocol()
        {
            List<string> arrays = new List<string>();
            arrays.Add(ResourceManager.Instance.VehicleDB.GetText("Read Trouble Code"));
            arrays.Add(ResourceManager.Instance.VehicleDB.GetText("Clear Trouble Code"));
            arrays.Add(ResourceManager.Instance.VehicleDB.GetText("Read Data Stream"));
            //            arrays.Add(ResourceManager.Instance.VehicleDB.GetText("Activity Test"));
            arrays.Add(ResourceManager.Instance.VehicleDB.GetText("ECU Version"));
            arrays.Add(ResourceManager.Instance.VehicleDB.GetText("TPS Idle Adjustment"));
            arrays.Add(ResourceManager.Instance.VehicleDB.GetText("Long Term Learn Value Zone Initialization"));
            arrays.Add(ResourceManager.Instance.VehicleDB.GetText("ISC Learn Value Initialize"));

            funcs = new Dictionary<string, ProtocolFunc>();
            funcs.Add(ResourceManager.Instance.VehicleDB.GetText("Read Trouble Code"), () =>
            {
                try
                {
                    RunOnUiThread(() =>
                    {
                        Intent intent = new Intent(
                        this,
                        typeof(TroubleCodeActivity)
                        );
                        intent.PutExtra("Model", model);
                        StartActivity(intent);
                    }
                    );
                }
                catch (System.IO.IOException ex)
                {
                    RunOnUiThread(() => DialogManager.Instance.FatalDialogShow(
                        this,
                        ex.Message,
                        null
                    )
                    );
                }
            }
            ); // Read Trouble Code

            funcs.Add(ResourceManager.Instance.VehicleDB.GetText("Clear Trouble Code"), () =>
            {
                try
                {
                    RunOnUiThread(() => DialogManager.Instance.StatusDialogShow(
                        this,
                        ResourceManager.Instance.VehicleDB.GetText("Clearing Trouble Code, Please Wait")
                    )
                    );
                    Mikuni protocol = new Mikuni(ResourceManager.Instance.VehicleDB, Diag.BoxFactory.Instance.Commbox);
                    protocol.ClearTroubleCode();

                    RunOnUiThread(() => DialogManager.Instance.FatalDialogShow(
                        this,
                        ResourceManager.Instance.VehicleDB.GetText("Clear Trouble Code Finish"),
                        null
                    )
                    );
                }
                catch (System.IO.IOException ex)
                {
                    RunOnUiThread(() => DialogManager.Instance.FatalDialogShow(
                        this,
                        ex.Message,
                        null
                    )
                    );
                }
            }
            ); // Clear Trouble Code

            funcs.Add(ResourceManager.Instance.VehicleDB.GetText("Read Data Stream"), () =>
            {
                try
                {
                    ResourceManager.Instance.VehicleDB.LDCatalog = "Mikuni";
                    ResourceManager.Instance.LiveDataVector = ResourceManager.Instance.VehicleDB.GetLiveData();
                    RunOnUiThread(() =>
                    {
                        Intent intent = new Intent(
                        this,
                        typeof(DataStreamSelectedActivity)
                        );
                        intent.PutExtra("Model", model);
                        StartActivity(intent);
                    }
                    );
                }
                catch (System.IO.IOException ex)
                {
                    RunOnUiThread(() => DialogManager.Instance.FatalDialogShow(
                        this,
                        ex.Message,
                        null
                    )
                    );
                }
            }
            ); // Read Data Stream

            funcs.Add(ResourceManager.Instance.VehicleDB.GetText("Activity Test"), () =>
            {

            }
            ); // Activity Test

            funcs.Add(ResourceManager.Instance.VehicleDB.GetText("ECU Version"), () =>
            {
                try
                {
                    RunOnUiThread(() => DialogManager.Instance.StatusDialogShow(
                        this,
                        ResourceManager.Instance.VehicleDB.GetText("Reading ECU Version, Please Wait")
                    )
                    );
                    string version = "";
                    Mikuni protocol = new Mikuni(ResourceManager.Instance.VehicleDB, Diag.BoxFactory.Instance.Commbox);
                    version = protocol.GetECUVersion();

                    RunOnUiThread(() => DialogManager.Instance.ListDialogShow(
                        this,
                        new string[] { version },
                        null
                    )
                    );
                }
                catch (System.IO.IOException ex)
                {
                    RunOnUiThread(() => DialogManager.Instance.FatalDialogShow(
                        this,
                        ex.Message,
                        null
                    )
                    );
                }
            }
            ); // ECU Version

            funcs.Add(ResourceManager.Instance.VehicleDB.GetText("TPS Idle Adjustment"), () =>
            {
                try
                {
                    RunOnUiThread(() => DialogManager.Instance.StatusDialogShow(
                        this,
                        ResourceManager.Instance.VehicleDB.GetText("Communicating")
                    )
                    );
                    Mikuni protocol = new Mikuni(ResourceManager.Instance.VehicleDB, Diag.BoxFactory.Instance.Commbox);
                    protocol.TPSIdleSetting();
                    RunOnUiThread(() => DialogManager.Instance.FatalDialogShow(
                        this,
                        ResourceManager.Instance.VehicleDB.GetText("TPS Idle Setting Success"),
                        null
                    )
                    );
                }
                catch (System.IO.IOException ex)
                {
                    RunOnUiThread(() => DialogManager.Instance.FatalDialogShow(
                        this,
                        ex.Message,
                        null
                    )
                    );
                }
            }
            ); // TPS Idle

            funcs.Add(ResourceManager.Instance.VehicleDB.GetText("Long Term Learn Value Zone Initialization"), () =>
            {
                try
                {
                    RunOnUiThread(() => DialogManager.Instance.StatusDialogShow(
                        this,
                        ResourceManager.Instance.VehicleDB.GetText("Communicating")
                    )
                    );
                    Mikuni protocol = new Mikuni(ResourceManager.Instance.VehicleDB, Diag.BoxFactory.Instance.Commbox);
                    protocol.LongTermLearnValueZoneInitialization();

                    RunOnUiThread(() => DialogManager.Instance.FatalDialogShow(
                        this,
                        ResourceManager.Instance.VehicleDB.GetText("Long Term Learn Value Zone Initialization Success"),
                        null
                    )
                    );
                }
                catch (System.IO.IOException ex)
                {
                    RunOnUiThread(() => DialogManager.Instance.FatalDialogShow(
                        this,
                        ex.Message,
                        null
                    )
                    );
                }
            }
            ); // Long Term

            funcs.Add(ResourceManager.Instance.VehicleDB.GetText("ISC Learn Value Initialize"), () =>
            {
                try
                {
                    RunOnUiThread(() => DialogManager.Instance.StatusDialogShow(
                        this,
                        ResourceManager.Instance.VehicleDB.GetText("Communicating")
                    )
                    );
                    Mikuni protocol = new Mikuni(ResourceManager.Instance.VehicleDB, Diag.BoxFactory.Instance.Commbox);
                    protocol.ISCLearnValueInitialize();

                    RunOnUiThread(() => DialogManager.Instance.FatalDialogShow(
                        this,
                        ResourceManager.Instance.VehicleDB.GetText("ISC Learn Value Initialize Success"),
                        null
                    )
                    );
                }
                catch (System.IO.IOException ex)
                {
                    RunOnUiThread(() => DialogManager.Instance.FatalDialogShow(
                        this,
                        ex.Message,
                        null
                    )
                    );
                }
            }
            ); // ISC Learn


            ListAdapter = new ArrayAdapter<string>(
                this,
                Android.Resource.Layout.SimpleListItem1,
                arrays
            );
            ListView.ItemClick += OnItemClick;
        }

        private void OnSynerject()
        {
            List<string> arrays = new List<string>();
            arrays.Add(ResourceManager.Instance.VehicleDB.GetText("Read Trouble Code"));
            arrays.Add(ResourceManager.Instance.VehicleDB.GetText("Clear Trouble Code"));
            arrays.Add(ResourceManager.Instance.VehicleDB.GetText("Read Data Stream"));
            arrays.Add(ResourceManager.Instance.VehicleDB.GetText("Activity Test"));
            arrays.Add(ResourceManager.Instance.VehicleDB.GetText("ECU Version"));

            funcs = new Dictionary<string, ProtocolFunc>();
            funcs.Add(ResourceManager.Instance.VehicleDB.GetText("Read Trouble Code"), () =>
            {
                try
                {
                    RunOnUiThread(() =>
                    {
                        Intent intent = new Intent(
                        this,
                        typeof(TroubleCodeActivity)
                        );
                        intent.PutExtra("Model", model);
                        StartActivity(intent);
                    }
                    );
                }
                catch (System.IO.IOException ex)
                {
                    RunOnUiThread(() => DialogManager.Instance.FatalDialogShow(
                        this,
                        ex.Message,
                        null
                    )
                    );
                }
            }
            );

            ListAdapter = new ArrayAdapter<string>(
                this,
                Android.Resource.Layout.SimpleListItem1,
                arrays
            );
            ListView.ItemClick += OnItemClick;
        }

        private void OnItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            try
            {
                DialogManager.Instance.StatusDialogShow(this, Core.SysDB.GetText("OpenCommbox"));
                Task task = Task.Factory.StartNew(() =>
                {
                    try
                    {
#if !DEBUG
                        ResourceManager.Instance.Commbox.Close();
                        ResourceManager.Instance.Commbox.Open();
#endif
                        funcs[((TextView)e.View).Text]();
                    }
                    catch (System.IO.IOException ex)
                    {
                        RunOnUiThread(() => DialogManager.Instance.FatalDialogShow(
                            this,
                            ex.Message,
                            null
                        )
                        );
                    }
                }
                );
            }
            catch
            {
                DialogManager.Instance.FatalDialogShow(
                    this,
                    Core.SysDB.GetText("Open Commbox Fail!"),
                    null
                );
            }
        }
    }
}