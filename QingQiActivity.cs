using System;
using System.IO;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace JM.QingQi
{
    [Activity(Theme = "@style/Theme.Default", Label = "JiNanQingQi")]
    public class QingQiActivity : Activity
    {
        private string sdcardPath;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            RequestWindowFeature(WindowFeatures.NoTitle);
            Window.SetFlags(
                WindowManagerFlags.Fullscreen,
                WindowManagerFlags.Fullscreen
            );
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get our button from the layout resource,
            // and attach an event to it
            Button button = FindViewById<Button>(Resource.Id.buttonSelectedTypes);

            button.Click += OnButtonSelectedTypes;

            button = FindViewById<Button>(Resource.Id.buttonDeviceInfo);
            button.Click += OnButtonDeviceInfo;

            CopyDatabase();
        }

        protected void OnButtonSelectedTypes(object sender, EventArgs e)
        {
            Intent intent = new Intent(this, typeof(SelectedTypesActivity));
            StartActivity(intent);
        }

        protected void OnButtonDeviceInfo(object sender, EventArgs e)
        {
            StartActivity(typeof(DevideInfoActivity));
        }

        private void CreateDirectory()
        {
            sdcardPath = "/mnt/sdcard/JMScanner";
            if (!Directory.Exists(sdcardPath))
            {
                try
                {
                    Directory.CreateDirectory(sdcardPath);
                }
                catch (Exception)
                {
                    sdcardPath = "/sdcard/JMScanner";
                    if (!Directory.Exists(sdcardPath))
                    {
                        Directory.CreateDirectory(sdcardPath);
                    }
                }
            }
        }

        private FileStream CreateSysDB()
        {
            if (File.Exists(sdcardPath + "/sys.db"))
            {
                File.Delete(sdcardPath + "/sys.db");
            }
            return File.Create(sdcardPath + "/sys.db");
        }

        private FileStream CreateMikuniDB()
        {
            if (File.Exists(sdcardPath + "/QingQi.db"))
            {
                File.Delete(sdcardPath + "/QingQi.db");
            }
            return File.Create(sdcardPath + "/QingQi.db");
        }

        private FileStream CreateDat()
        {
            if (File.Exists(sdcardPath + "/demo.dat"))
            {
                File.Delete(sdcardPath + "/demo.dat");
            }

            return File.Create(sdcardPath + "/demo.dat");
        }

        private void CopyFile(Stream source, Stream dest)
        {
            byte[] buffer = new byte[0x1000];
            for (int len = source.Read(buffer, 0, buffer.Length); len > 0; len = source.Read(buffer, 0, buffer.Length))
            {
                dest.Write(buffer, 0, len);
            }
            source.Close();
            dest.Flush();
            dest.Close();
        }

        private void CopySysDB(FileStream sysFS)
        {
            // Read the contents of our asset
            Stream sr = Assets.Open(
                "sys.db",
                Android.Content.Res.Access.Buffer
            );
            CopyFile(sr, sysFS);
        }

        private void CopyMikuniDB(FileStream mikuniFS)
        {
            Stream sr = Assets.Open(
                "QingQi.db",
                Android.Content.Res.Access.Buffer
            );
            CopyFile(sr, mikuniFS);
        }

        private void CopyDat(FileStream datFS)
        {
            Stream sr = Assets.Open(
                "demo.dat",
                Android.Content.Res.Access.Buffer
            );
            CopyFile(sr, datFS);
        }

        private void CopyDatabase()
        {
            CreateDirectory();

            FileStream sysFS = CreateSysDB();
            FileStream mikuniFS = CreateMikuniDB();
            FileStream datFS = CreateDat();

            CopySysDB(sysFS);
            CopyMikuniDB(mikuniFS);
            CopyDat(datFS);

            Core.MustCallFirst.Instance.Init(sdcardPath + "/");

            ResourceManager.Instance.VehicleDB = new Core.VehicleDB(sdcardPath + "/QingQi.db");
        }
    }
}

