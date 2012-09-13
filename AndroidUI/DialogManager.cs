using System;
using System.Collections.Generic;
using System.Text;
using Android.App;
using Android.Content;

namespace JM.QingQi.AndroidUI
{
    internal class DialogManager
    {
        public static ProgressDialog ShowStatus(Context cxt, string msg)
        {
            ProgressDialog status = new ProgressDialog(cxt);
            status.SetMessage(msg);
            status.Show();
            return status;
        }

        public static AlertDialog ShowFatal(Context cxt, string msg, EventHandler<DialogClickEventArgs> listener)
        {
            AlertDialog fatal = new AlertDialog.Builder(cxt).SetMessage(msg)
                .SetPositiveButton(Vehicle.ResourceManager.Instance.VehicleDB.GetText("OK"),listener)
                .Create();
            fatal.Show();
            return fatal;
        }

        public static AlertDialog ShowList(Context cxt, string[] arrays, EventHandler<DialogClickEventArgs> listener)
        {
            Which = -1;
            AlertDialog list = new AlertDialog.Builder(cxt).SetSingleChoiceItems(arrays, -1, (dialog, w) =>
            {
                which = w.Which;
            })
                .SetPositiveButton(Vehicle.ResourceManager.Instance.VehicleDB.GetText("OK"), listener)
                .Create();
            list.Show();
            return list;
        }

        private static int which;
        public static int Which
        {
            get { return which; }
            private set { which = value; }
        }
    }
}
