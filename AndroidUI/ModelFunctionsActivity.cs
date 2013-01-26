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
using JM.Core;
using JM.Vehicles;
using JM.QingQi.Vehicle;

namespace JM.QingQi.AndroidUI
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
            protocolFuncs[Database.GetText("QM125T-8H", "QingQi")] = OnSynerject;
            protocolFuncs[Database.GetText("QM200GY-F", "QingQi")] = OnMikuniProtocol;
            protocolFuncs[Database.GetText("QM250GY", "QingQi")] = OnSynerject;
            protocolFuncs[Database.GetText("QM250T", "QingQi")] = OnSynerject;
            protocolFuncs[Database.GetText("QM200-3D", "QingQi")] = OnMikuniProtocol;
            protocolFuncs[Database.GetText("QM200J-3L", "QingQi")] = OnMikuniProtocol;
            protocolFuncs[Database.GetText("QM250J-2L", "QingQi")] = OnVisteon;
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            // Create your application here
            Window.SetFlags(WindowManagerFlags.KeepScreenOn, WindowManagerFlags.KeepScreenOn);
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
            arrays.Add(StaticString.beforeBlank + Database.GetText("Read Trouble Code", "System"));
            arrays.Add(StaticString.beforeBlank + Database.GetText("Clear Trouble Code", "System"));
            arrays.Add(StaticString.beforeBlank + Database.GetText("Read Data Stream", "System"));
            //arrays.Add(StaticString.beforeBlank + Database.GetText("Static Data Stream", "System"));
            //            arrays.Add(Database.GetText("Activity Test"));
            arrays.Add(StaticString.beforeBlank + Database.GetText("TPS Idle Adjustment", "Mikuni"));
            arrays.Add(StaticString.beforeBlank + Database.GetText("ISC Learn Value Initialize", "Mikuni"));
            arrays.Add(StaticString.beforeBlank + Database.GetText("Long Term Learn Value Zone Initialization", "Mikuni"));
            arrays.Add(StaticString.beforeBlank + Database.GetText("ECU Version", "System"));

            funcs = new Dictionary<string, ProtocolFunc>();
            funcs.Add(Database.GetText("Read Trouble Code", "System"), () =>
            {
                Intent intent = new Intent(this, typeof(TroubleCodeActivity));
                intent.PutExtra("Model", model);
                StartActivity(intent);
            }
            ); // Read Trouble Code

            funcs.Add(Database.GetText("Clear Trouble Code", "System"), () =>
            {
                ProgressDialog status = DialogManager.ShowStatus(this, Database.GetText("Clearing Trouble Code, Please Wait", "System")); ;
                AlertDialog fatal;

                Task task = Task.Factory.StartNew(() =>
                {
                    if (!Manager.Commbox.Close() || !Manager.Commbox.Open())
                    {
                        throw new IOException(Database.GetText("Open Commbox Fail", "System"));
                    }
                    Diag.MikuniOptions options = new Diag.MikuniOptions();
                    options.Parity = Diag.MikuniParity.Even;
                    Mikuni protocol = new Mikuni(Diag.BoxFactory.Instance.Commbox, options);
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
                            fatal = DialogManager.ShowFatal(this, Database.GetText("Clear Trouble Code Finish", "System"), null);
                        }
                    });
                });

            }
            ); // Clear Trouble Code

            funcs.Add(Database.GetText("Read Data Stream", "System"), () =>
            {
                Intent intent = new Intent(this, typeof(DataStreamSelectedActivity));
                intent.PutExtra("Model", model);
                StartActivity(intent);
            }
            ); // Read Data Stream

            //funcs.Add(StaticString.beforeBlank + Database.GetText("Static Data Stream", "System"), () =>
            //{
            //    Manager.LiveDataVector = Database.GetLiveData("Mikuni");
            //    Intent intent = new Intent(this, typeof(StaticDataStreamActivity));
            //    intent.PutExtra("Model", model);
            //    StartActivity(intent);
            //});

            funcs.Add(Database.GetText("Activity Test", "System"), () =>
            {

            }
            ); // Activity Test

            funcs.Add(Database.GetText("ECU Version", "System"), () =>
            {
                ProgressDialog status = DialogManager.ShowStatus(this, Database.GetText("Reading ECU Version, Please Wait", "System"));
                AlertDialog fatal;
                Mikuni.ChineseVersion version = new Mikuni.ChineseVersion();
                Task task = Task.Factory.StartNew(() =>
                {
                    if (!Manager.Commbox.Close() || !Manager.Commbox.Open())
                    {
                        throw new IOException(Database.GetText("Open Commbox Fail", "System"));
                    }
                    Diag.MikuniOptions options = new Diag.MikuniOptions();
                    options.Parity = Diag.MikuniParity.Even;
                    Mikuni protocol = new Mikuni(Diag.BoxFactory.Instance.Commbox, options);
                    version = Mikuni.FormatECUVersionForChina(protocol.GetECUVersion());
                });

                task.ContinueWith((t) =>
                {
                    RunOnUiThread(() =>
                    {
                        status.Dismiss();
                        string text = "";
                        if (t.IsFaulted)
                        {
                            fatal = DialogManager.ShowFatal(this, t.Exception.InnerException.Message, null);
                        }
                        else
                        {
							if (model == (Database.GetText("QM200GY-F", "QingQi")))
							{
                                text = "M16-02\n";
                            }
                            else if (model == (Database.GetText("QM200J-3L", "QingQi")))
							{
                                text = "M16-01\n";
							}
                            else if (model == (Database.GetText("QM200-3D", "QingQi")))
							{
                                text = "M16-03\n";
							}
                            text += version.Hardware + "\nV" + version.Software;
                            fatal = DialogManager.ShowFatal(this, text, null);
                        }
                        
                    });
                });
            }
            ); // ECU Version

            funcs.Add(Database.GetText("TPS Idle Adjustment", "Mikuni"), () =>
            {
                ProgressDialog status = DialogManager.ShowStatus(this, Database.GetText("Communicating", "System"));
                AlertDialog fatal;

                Task task = Task.Factory.StartNew(() =>
                {
                    if (!Manager.Commbox.Close() || !Manager.Commbox.Open())
                    {
                        throw new IOException(Database.GetText("Open Commbox Fail", "System"));
                    }
                    Diag.MikuniOptions options = new Diag.MikuniOptions();
                    options.Parity = Diag.MikuniParity.Even;
                    Mikuni protocol = new Mikuni(Diag.BoxFactory.Instance.Commbox, options);
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
                            fatal = DialogManager.ShowFatal(this, Database.GetText("TPS Idle Setting Success", "Mikuni"), null);
                        }
                    });
                });
            }
            ); // TPS Idle

            funcs.Add(Database.GetText("Long Term Learn Value Zone Initialization", "Mikuni"), () =>
            {
                ProgressDialog status = DialogManager.ShowStatus(this, Database.GetText("Communicating", "System"));
                AlertDialog fatal;

                Task task = Task.Factory.StartNew(() =>
                {
                    if (!Manager.Commbox.Close() || !Manager.Commbox.Open())
                    {
                        throw new IOException(Database.GetText("Open Commbox Fail", "System"));
                    }
                    Diag.MikuniOptions options = new Diag.MikuniOptions();
                    options.Parity = Diag.MikuniParity.Even;
                    Mikuni protocol = new Mikuni(Diag.BoxFactory.Instance.Commbox, options);
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
                            fatal = DialogManager.ShowFatal(this, Database.GetText("Long Term Learn Value Zone Initialization Success", "Mikuni"), null);
                        }
                    });
                });
            }
            ); // Long Term

            funcs.Add(Database.GetText("ISC Learn Value Initialize", "Mikuni"), () =>
            {
                ProgressDialog status = DialogManager.ShowStatus(this, Database.GetText("Communicating", "System"));
                AlertDialog fatal;

                Task task = Task.Factory.StartNew(() =>
                {
                    if (!Manager.Commbox.Close() || !Manager.Commbox.Open())
                    {
                        throw new IOException(Database.GetText("Open Commbox Fail", "System"));
                    }
                    Diag.MikuniOptions options = new Diag.MikuniOptions();
                    options.Parity = Diag.MikuniParity.Even;
                    Mikuni protocol = new Mikuni(Diag.BoxFactory.Instance.Commbox, options);
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
                            fatal = DialogManager.ShowFatal(this, Database.GetText("ISC Learn Value Initialization Success", "Mikuni"), null);
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
            arrays.Add(StaticString.beforeBlank + Database.GetText("Read Trouble Code", "System"));
            arrays.Add(StaticString.beforeBlank + Database.GetText("Clear Trouble Code", "System"));
            arrays.Add(StaticString.beforeBlank + Database.GetText("Read Data Stream", "System"));
            //arrays.Add(StaticString.beforeBlank + Database.GetText("Static Data Stream", "System"));
            if (model != Database.GetText("QM125T-8H", "QingQi"))
                arrays.Add(StaticString.beforeBlank + Database.GetText("Activity Test", "System"));
            arrays.Add(StaticString.beforeBlank + Database.GetText("ECU Version", "System"));

            funcs = new Dictionary<string, ProtocolFunc>();
            funcs.Add(Database.GetText("Read Trouble Code", "System"), () =>
            {
                Intent intent = new Intent(this, typeof(TroubleCodeActivity));
                intent.PutExtra("Model", model);
                StartActivity(intent);
            }
            );

            funcs.Add(Database.GetText("Clear Trouble Code", "System"), () =>
            {
                ProgressDialog status = DialogManager.ShowStatus(this, Database.GetText("Clearing Trouble Code, Please Wait", "System"));
                AlertDialog fatal;

                Task task = Task.Factory.StartNew(() =>
                {
                    if (!Manager.Commbox.Close() || !Manager.Commbox.Open())
                    {
                        throw new IOException(Database.GetText("Open Commbox Fail", "System"));
                    }
                    Synerject protocol = new Synerject(Diag.BoxFactory.Instance.Commbox);
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
                            fatal = DialogManager.ShowFatal(this, Database.GetText("Clear Trouble Code Finish", "System"), null);
                        }
                    });
                });
            });

            funcs.Add(Database.GetText("Read Data Stream", "System"), () =>
            {
                Intent intent = new Intent(this, typeof(DataStreamSelectedActivity));
                intent.PutExtra("Model", model);
                StartActivity(intent);
            }
            ); // Read Data Stream

            //funcs.Add(StaticString.beforeBlank + Database.GetText("Static Data Stream", "System"), () =>
            //{
            //    Manager.LiveDataVector = Database.GetLiveData("Synerject");
            //    Intent intent = new Intent(this, typeof(StaticDataStreamActivity));
            //    intent.PutExtra("Model", model);
            //    StartActivity(intent);
            //});

            funcs.Add(Database.GetText("Activity Test", "System"), () =>
            {
                Intent intent = new Intent(this, typeof(ActiveTestActivity));
                intent.PutExtra("Model", model);
                StartActivity(intent);
            }
            ); // Activity Test

            funcs.Add(Database.GetText("ECU Version", "System"), () =>
            {
                ProgressDialog status = DialogManager.ShowStatus(this, Database.GetText("Reading ECU Version, Please Wait", "System"));
                AlertDialog fatal;
                string version = "";

                Task task = Task.Factory.StartNew(() =>
                {
                    if (!Manager.Commbox.Close() || !Manager.Commbox.Open())
                    {
                        throw new IOException(Database.GetText("Open Commbox Fail", "System"));
                    }
                    Synerject protocol = new Synerject(Diag.BoxFactory.Instance.Commbox);
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
                            fatal = DialogManager.ShowFatal(this, StaticString.beforeBlank + version, null);
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
            arrays.Add(StaticString.beforeBlank + Database.GetText("Read Trouble Code", "System"));
            arrays.Add(StaticString.beforeBlank + Database.GetText("Clear Trouble Code", "System"));
            arrays.Add(StaticString.beforeBlank + Database.GetText("Read Data Stream", "System"));
            //arrays.Add(StaticString.beforeBlank + Database.GetText("Static Data Stream", "System"));
            arrays.Add(StaticString.beforeBlank + Database.GetText("Read Freeze Frame", "System"));
            //arrays.Add(Database.GetText("Activity Test"));
            //arrays.Add(Database.GetText("ECU Version"));

            funcs = new Dictionary<string, ProtocolFunc>();
            funcs.Add(Database.GetText("Read Trouble Code", "System"), () =>
            {
                Intent intent = new Intent(this, typeof(TroubleCodeActivity) );
                intent.PutExtra("Model", model);
                StartActivity(intent);
            }
            );

            funcs.Add(Database.GetText("Clear Trouble Code", "System"), () =>
            {
                ProgressDialog status = DialogManager.ShowStatus(this, Database.GetText("Clearing Trouble Code, Please Wait", "System"));
                AlertDialog fatal;

                Task task = Task.Factory.StartNew(() =>
                {
                    if (!Manager.Commbox.Close() || !Manager.Commbox.Open())
                    {
                        throw new IOException(Database.GetText("Open Commbox Fail", "System"));
                    }
                    Visteon protocol = new Visteon(Diag.BoxFactory.Instance.Commbox);
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
                            fatal = DialogManager.ShowFatal(this, Database.GetText("Clear Trouble Code Finish", "System"), null);
                        }
                    });
                });
            });

            funcs.Add(Database.GetText("Read Data Stream", "System"), () =>
            {
                //Intent intent = new Intent(this, typeof(DataStreamSelectedActivity));
                Intent intent = new Intent(this, typeof(DataStreamActivity));
                intent.PutExtra("Model", model);
                StartActivity(intent);

            }
            ); // Read Data Stream

            //funcs.Add(StaticString.beforeBlank + Database.GetText("Static Data Stream", "System"), () =>
            //{
            //    Manager.LiveDataVector = Database.GetLiveData("Visteon");
            //    Intent intent = new Intent(this, typeof(StaticDataStreamActivity));
            //    intent.PutExtra("Model", model);
            //    StartActivity(intent);
            //});

            funcs.Add(Database.GetText("Read Freeze Frame", "System"), () =>
            {
                Intent intent = new Intent(this, typeof(DataStreamActivity));
                intent.PutExtra("Model", model + "Freeze");
                StartActivity(intent);
            }
            ); // Freeze Frame

            //funcs.Add(Database.GetText("ECU Version"), () =>
            //{
            //    try
            //    {
            //        RunOnUiThread(() => DialogManager.Instance.StatusDialogShow(this, Database.GetText("Reading ECU Version, Please Wait")));
            //        string version = "";
            //        Synerject protocol = new Synerject(Database, Diag.BoxFactory.Instance.Commbox);
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
            //ProgressDialog status = DialogManager.ShowStatus(this, Database.GetText("OpenCommbox", "System"));
            //AlertDialog fatal;

            //Task task = Task.Factory.StartNew(() =>
            //{
            //    if (!Manager.Commbox.Close() ||
            //        !Manager.Commbox.Open())
            //    {
            //        throw new IOException(Database.GetText("Open Commbox Fail", "System"));
            //    }
            //});

            //task.ContinueWith((t) =>
            //{
            //    RunOnUiThread(() =>
            //    {
            //        status.Dismiss();
            //        if (t.IsFaulted)
            //        {
            //            fatal = DialogManager.ShowFatal(this, t.Exception.InnerException.Message, null);
            //        }
            //        else
            //        {
            //            funcs[((TextView)e.View).Text]();
            //        }
            //        //funcs[((TextView)e.View).Text]();
            //    });
            //});
            string sel = ((TextView)e.View).Text;
            funcs[sel.TrimStart(' ')]();
        }
    }
}