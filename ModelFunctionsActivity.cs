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
using System.IO;

namespace JM.QingQi
{
    [Activity(Theme = "@style/Theme.Default", Label = "Function Select")]
    public class ModelFunctionsActivity : ListActivity
    {
        private delegate void ProtocolFunc();

        private Dictionary<string, ProtocolFunc> protocolFuncs;
        private string model = null;
        private Dictionary<string, ProtocolFunc> funcs;

        public ModelFunctionsActivity()
        {
            protocolFuncs = new Dictionary<string, ProtocolFunc>();
            protocolFuncs[ResourceManager.Instance.VehicleDB.GetText("QM125T-8H")] = OnSynerject;
            protocolFuncs[ResourceManager.Instance.VehicleDB.GetText("QM200GY-F")] = OnMikuniProtocol;
            protocolFuncs[ResourceManager.Instance.VehicleDB.GetText("QM250GY")] = OnSynerject;
            protocolFuncs[ResourceManager.Instance.VehicleDB.GetText("QM250T")] = OnSynerject;
            protocolFuncs[ResourceManager.Instance.VehicleDB.GetText("QM200-3D")] = OnMikuniProtocol;
            protocolFuncs[ResourceManager.Instance.VehicleDB.GetText("QM200J-3L")] = OnMikuniProtocol;
            protocolFuncs[ResourceManager.Instance.VehicleDB.GetText("QM250J-2L")] = OnVisteon;
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

        protected override void OnRestart()
        {
            base.OnRestart();
        }

        protected override void OnResume()
        {
            base.OnResume();
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
            arrays.Add(ResourceManager.Instance.VehicleDB.GetText("TPS Idle Adjustment"));
            arrays.Add(ResourceManager.Instance.VehicleDB.GetText("Long Term Learn Value Zone Initialization"));
            arrays.Add(ResourceManager.Instance.VehicleDB.GetText("ISC Learn Value Initialize"));
            arrays.Add(ResourceManager.Instance.VehicleDB.GetText("ECU Version"));

            funcs = new Dictionary<string, ProtocolFunc>();
            funcs.Add(ResourceManager.Instance.VehicleDB.GetText("Read Trouble Code"), () =>
            {
                Intent intent = new Intent(this, typeof(TroubleCodeActivity));
                intent.PutExtra("Model", model);
                StartActivity(intent);
            }
            ); // Read Trouble Code

            funcs.Add(ResourceManager.Instance.VehicleDB.GetText("Clear Trouble Code"), () =>
            {
                ProgressDialog status = DialogManager.ShowStatus(this, ResourceManager.Instance.VehicleDB.GetText("Clearing Trouble Code, Please Wait"));;
                AlertDialog fatal;

                Task task = Task.Factory.StartNew(() =>
                {
                    Mikuni protocol = new Mikuni(ResourceManager.Instance.VehicleDB, Diag.BoxFactory.Instance.Commbox);
                    protocol.ClearTroubleCode();
                });

                task.ContinueWith((t) =>
                {
                    RunOnUiThread(() =>
                    {
                        status.Dismiss();
                        if (t.IsFaulted)
                        {
                            fatal = DialogManager.ShowFatal(this, t.Exception.InnerException.Message, null);
                        }
                        else
                        {
                            fatal = DialogManager.ShowFatal(this, ResourceManager.Instance.VehicleDB.GetText("Clear Trouble Code Finish"), null);
                        }
                    });
                });

            }
            ); // Clear Trouble Code

            funcs.Add(ResourceManager.Instance.VehicleDB.GetText("Read Data Stream"), () =>
            {
                ResourceManager.Instance.VehicleDB.LDCatalog = "Mikuni";
                ResourceManager.Instance.LiveDataVector = ResourceManager.Instance.VehicleDB.GetLiveData();
                Intent intent = new Intent(this, typeof(DataStreamSelectedActivity));
                intent.PutExtra("Model", model);
                StartActivity(intent);
            }
            ); // Read Data Stream

            funcs.Add(ResourceManager.Instance.VehicleDB.GetText("Activity Test"), () =>
            {

            }
            ); // Activity Test

            funcs.Add(ResourceManager.Instance.VehicleDB.GetText("ECU Version"), () =>
            {
                ProgressDialog status = DialogManager.ShowStatus(this, ResourceManager.Instance.VehicleDB.GetText("Reading ECU Version, Please Wait"));
                AlertDialog fatal;
                string version = "";
                Task task = Task.Factory.StartNew(() =>
                {
                    Mikuni protocol = new Mikuni(ResourceManager.Instance.VehicleDB, Diag.BoxFactory.Instance.Commbox);
                    version = protocol.GetECUVersion();
                });

                task.ContinueWith((t) =>
                {
                    RunOnUiThread(() =>
                    {
                        status.Dismiss();
                        if (t.IsFaulted)
                        {
                            fatal = DialogManager.ShowFatal(this, t.Exception.InnerException.Message, null);
                        }
                        else
                        {
                            fatal = DialogManager.ShowFatal(this, version, null);
                        }
                    });
                });
            }
            ); // ECU Version

            funcs.Add(ResourceManager.Instance.VehicleDB.GetText("TPS Idle Adjustment"), () =>
            {
                ProgressDialog status = DialogManager.ShowStatus(this, ResourceManager.Instance.VehicleDB.GetText("Communicating"));
                AlertDialog fatal;

                Task task = Task.Factory.StartNew(() =>
                {
                    Mikuni protocol = new Mikuni(ResourceManager.Instance.VehicleDB, Diag.BoxFactory.Instance.Commbox);
                    protocol.TPSIdleSetting();
                });

                task.ContinueWith((t) =>
                {
                    RunOnUiThread(() =>
                    {
                        status.Dismiss();
                        if (t.IsFaulted)
                        {
                            fatal = DialogManager.ShowFatal(this, t.Exception.InnerException.Message, null);
                        }
                        else
                        {
                            fatal = DialogManager.ShowFatal(this, ResourceManager.Instance.VehicleDB.GetText("TPS Idle Setting Success"), null);
                        }
                    });
                });
            }
            ); // TPS Idle

            funcs.Add(ResourceManager.Instance.VehicleDB.GetText("Long Term Learn Value Zone Initialization"), () =>
            {
                ProgressDialog status = DialogManager.ShowStatus(this, ResourceManager.Instance.VehicleDB.GetText("Communicating"));
                AlertDialog fatal;

                Task task = Task.Factory.StartNew(() =>
                {
                    Mikuni protocol = new Mikuni(ResourceManager.Instance.VehicleDB, Diag.BoxFactory.Instance.Commbox);
                    protocol.LongTermLearnValueZoneInitialization();
                });

                task.ContinueWith((t) =>
                {
                    RunOnUiThread(() =>
                    {
                        status.Dismiss();
                        if (t.IsFaulted)
                        {
                            fatal = DialogManager.ShowFatal(this, t.Exception.InnerException.Message, null);
                        }
                        else
                        {
                            fatal = DialogManager.ShowFatal(this, ResourceManager.Instance.VehicleDB.GetText("Long Term Learn Value Zone Initialization Success"), null);
                        }
                    });
                });
            }
            ); // Long Term

            funcs.Add(ResourceManager.Instance.VehicleDB.GetText("ISC Learn Value Initialize"), () =>
            {
                ProgressDialog status = DialogManager.ShowStatus(this, ResourceManager.Instance.VehicleDB.GetText("Communicating"));
                AlertDialog fatal;

                Task task = Task.Factory.StartNew(() =>
                {
                    Mikuni protocol = new Mikuni(ResourceManager.Instance.VehicleDB, Diag.BoxFactory.Instance.Commbox);
                    protocol.ISCLearnValueInitialize();
                });

                task.ContinueWith((t) =>
                {
                    RunOnUiThread(() =>
                    {
                        status.Dismiss();
                        if (t.IsFaulted)
                        {
                            fatal = DialogManager.ShowFatal(this, t.Exception.InnerException.Message, null);
                        }
                        else
                        {
                            fatal = DialogManager.ShowFatal(this, ResourceManager.Instance.VehicleDB.GetText("ISC Learn Value Initialization Success"), null);
                        }
                    });
                });
            }
            ); // ISC Learn


            ListAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, arrays);
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
                Intent intent = new Intent(
                this,
                typeof(TroubleCodeActivity)
                );
                intent.PutExtra("Model", model);
                StartActivity(intent);
            }
            );

            funcs.Add(ResourceManager.Instance.VehicleDB.GetText("Clear Trouble Code"), () =>
            {
                ProgressDialog status = DialogManager.ShowStatus(this, ResourceManager.Instance.VehicleDB.GetText("Clearing Trouble Code, Please Wait"));
                AlertDialog fatal;

                Task task = Task.Factory.StartNew(() =>
                {
                    Synerject protocol = new Synerject(ResourceManager.Instance.VehicleDB, Diag.BoxFactory.Instance.Commbox);
                    protocol.ClearTroubleCode();
                });

                task.ContinueWith((t) =>
                {
                    RunOnUiThread(() =>
                    {
                        status.Dismiss();
                        if (t.IsFaulted)
                        {
                            fatal = DialogManager.ShowFatal(this, t.Exception.InnerException.Message, null);
                        }
                        else
                        {
                            fatal = DialogManager.ShowFatal(this, ResourceManager.Instance.VehicleDB.GetText("Clear Trouble Code Finish"), null);
                        }
                    });
                });
            });

            funcs.Add(ResourceManager.Instance.VehicleDB.GetText("Read Data Stream"), () =>
            {
                ResourceManager.Instance.VehicleDB.LDCatalog = "Synerject";
                ResourceManager.Instance.LiveDataVector = ResourceManager.Instance.VehicleDB.GetLiveData();
                Intent intent = new Intent(this, typeof(DataStreamSelectedActivity));
                intent.PutExtra("Model", model);
                StartActivity(intent);
            }
            ); // Read Data Stream

            funcs.Add(ResourceManager.Instance.VehicleDB.GetText("Activity Test"), () =>
            {
                Intent intent = new Intent(this, typeof(ActiveTestActivity));
                intent.PutExtra("Model", model);
                StartActivity(intent);
            }
            ); // Activity Test

            funcs.Add(ResourceManager.Instance.VehicleDB.GetText("ECU Version"), () =>
            {
                ProgressDialog status = DialogManager.ShowStatus(this, ResourceManager.Instance.VehicleDB.GetText("Reading ECU Version, Please Wait"));
                AlertDialog fatal;
                string version = "";

                Task task = Task.Factory.StartNew(() =>
                {
                    Synerject protocol = new Synerject(ResourceManager.Instance.VehicleDB, Diag.BoxFactory.Instance.Commbox);
                    version = protocol.ReadECUVersion();
                });

                task.ContinueWith((t) =>
                {
                    RunOnUiThread(() =>
                    {
                        status.Dismiss();
                        if (t.IsFaulted)
                        {
                            fatal = DialogManager.ShowFatal(this, t.Exception.InnerException.Message, null);
                        }
                        else
                        {
                            fatal = DialogManager.ShowFatal(this, version, null);
                        }
                    });
                });
            }
            ); // ECU Version

            ListAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, arrays);
            ListView.ItemClick += OnItemClick;
        }

        private void OnVisteon()
        {
            List<string> arrays = new List<string>();
            arrays.Add(ResourceManager.Instance.VehicleDB.GetText("Read Trouble Code"));
            arrays.Add(ResourceManager.Instance.VehicleDB.GetText("Clear Trouble Code"));
            arrays.Add(ResourceManager.Instance.VehicleDB.GetText("Read Data Stream"));
            arrays.Add(ResourceManager.Instance.VehicleDB.GetText("Read Freeze Frame"));
            //arrays.Add(ResourceManager.Instance.VehicleDB.GetText("Activity Test"));
            //arrays.Add(ResourceManager.Instance.VehicleDB.GetText("ECU Version"));

            funcs = new Dictionary<string, ProtocolFunc>();
            funcs.Add(ResourceManager.Instance.VehicleDB.GetText("Read Trouble Code"), () =>
            {
                Intent intent = new Intent(
                this,
                typeof(TroubleCodeActivity)
                );
                intent.PutExtra("Model", model);
                StartActivity(intent);
            }
            );

            funcs.Add(ResourceManager.Instance.VehicleDB.GetText("Clear Trouble Code"), () =>
            {
                ProgressDialog status = DialogManager.ShowStatus(this, ResourceManager.Instance.VehicleDB.GetText("Clearing Trouble Code, Please Wait"));
                AlertDialog fatal;

                Task task = Task.Factory.StartNew(() =>
                {
                    Visteon protocol = new Visteon(ResourceManager.Instance.VehicleDB, Diag.BoxFactory.Instance.Commbox);
                    protocol.ClearTroubleCode();
                });

                task.ContinueWith((t) =>
                {
                    RunOnUiThread(() =>
                    {
                        status.Dismiss();
                        if (t.IsFaulted)
                        {
                            fatal = DialogManager.ShowFatal(this, t.Exception.InnerException.Message, null);
                        }
                        else
                        {
                            fatal = DialogManager.ShowFatal(this, ResourceManager.Instance.VehicleDB.GetText("Clear Trouble Code Finish"), null);
                        }
                    });
                });
            });

            funcs.Add(ResourceManager.Instance.VehicleDB.GetText("Read Data Stream"), () =>
            {
                ResourceManager.Instance.VehicleDB.LDCatalog = "Visteon";
                ResourceManager.Instance.LiveDataVector = ResourceManager.Instance.VehicleDB.GetLiveData();
                Intent intent = new Intent(this, typeof(DataStreamSelectedActivity));
                intent.PutExtra("Model", model);
                StartActivity(intent);

            }
            ); // Read Data Stream

            funcs.Add(ResourceManager.Instance.VehicleDB.GetText("Read Freeze Frame"), () =>
            {
                ResourceManager.Instance.VehicleDB.LDCatalog = "Visteon Freeze";
                ResourceManager.Instance.LiveDataVector = ResourceManager.Instance.VehicleDB.GetLiveData();
                Intent intent = new Intent(this, typeof(DataStreamActivity));
                intent.PutExtra("Model", model + "Freeze");
                StartActivity(intent);
            }
            ); // Freeze Frame

            //funcs.Add(ResourceManager.Instance.VehicleDB.GetText("ECU Version"), () =>
            //{
            //    try
            //    {
            //        RunOnUiThread(() => DialogManager.Instance.StatusDialogShow(this, ResourceManager.Instance.VehicleDB.GetText("Reading ECU Version, Please Wait")));
            //        string version = "";
            //        Synerject protocol = new Synerject(ResourceManager.Instance.VehicleDB, Diag.BoxFactory.Instance.Commbox);
            //        version = protocol.ReadECUVersion();

            //        RunOnUiThread(() => DialogManager.Instance.FatalDialogShow(this, version, null));
            //    }
            //    catch (System.IO.IOException ex)
            //    {
            //        RunOnUiThread(() => DialogManager.Instance.FatalDialogShow(this, ex.Message, null));
            //    }
            //}
            //); // ECU Version

            ListAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, arrays);
            ListView.ItemClick += OnItemClick;
        }

        private void OnItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            ProgressDialog status = DialogManager.ShowStatus(this, Core.SysDB.GetText("OpenCommbox"));
            AlertDialog fatal;

            Task task = Task.Factory.StartNew(() =>
            {
                if (!ResourceManager.Instance.Commbox.Close() ||
                    !ResourceManager.Instance.Commbox.Open())
                {
                    throw new IOException(Core.SysDB.GetText("Open Commbox Fail"));
                }
            });

            task.ContinueWith((t) =>
            {
                RunOnUiThread(() =>
                {
                    status.Dismiss();
                    if (t.IsFaulted)
                    {
                        fatal = DialogManager.ShowFatal(this, t.Exception.InnerException.Message, null);
                    }
                    else
                    {
                        funcs[((TextView)e.View).Text]();
                    }
                });
            });
        }
    }
}