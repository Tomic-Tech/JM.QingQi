using System;
using System.Collections.Generic;
using System.Text;
using Android.App;
using Android.Content;
using JM.Core;
using Android.Views;

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
                .SetPositiveButton(Database.GetText("OK", "System"),listener)
                .Create();
            fatal.Show();
            return fatal;
        }

        public static AlertDialog ShowList(Context cxt, string[] arrays, EventHandler<DialogClickEventArgs> listener)
        {
            Which = -1;
            AlertDialog list = new AlertDialog.Builder(new ContextThemeWrapper(cxt, Resource.Style.Theme_AlertDialogCustom)).SetSingleChoiceItems(arrays, -1, (dialog, w) =>
            {
                which = w.Which;
            })
                .SetPositiveButton(Database.GetText("OK", "System"), listener)
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
