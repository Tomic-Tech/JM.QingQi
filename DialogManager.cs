using System;
using System.Collections.Generic;
using System.Text;
using Android.App;
using Android.Content;

namespace JM.QingQi
{
    internal class DialogManager
    {
        private static readonly DialogManager instance = new DialogManager();
        private ProgressDialog statusDialog;
        private AlertDialog fatalDialog;
        private AlertDialog listDialog;

        private DialogManager()
        {
        }

        public static DialogManager Instance
        {
            get { return instance; }
        }

        public void HideDialog()
        {
            if (statusDialog != null)
                statusDialog.Dismiss();
            if (fatalDialog != null)
                fatalDialog.Dismiss();
            if (listDialog != null)
                listDialog.Dismiss();
        }

        public void StatusDialogShow(Context cxt, string msg)
        {
            HideDialog();
            statusDialog = new ProgressDialog(cxt);
            statusDialog.SetMessage(msg);
            statusDialog.SetCancelable(false);
            statusDialog.Show();
        }

        public void FatalDialogShow(Context cxt, string msg, EventHandler<DialogClickEventArgs> listener)
        {
            HideDialog();
            fatalDialog = new AlertDialog.Builder(cxt).SetMessage(msg).SetPositiveButton(
                ResourceManager.Instance.VehicleDB.GetText("OK"),
                listener
            )
                .Create();
            fatalDialog.Show();
        }

        public void ListDialogShow(Context cxt, string[] arrays, EventHandler<DialogClickEventArgs> listener)
        {
            HideDialog();
            listDialog = new AlertDialog.Builder(cxt).SetItems(
                arrays,
                (IDialogInterfaceOnClickListener)null
            )
                .SetPositiveButton(
                ResourceManager.Instance.VehicleDB.GetText("OK"),
                listener
            )
                .Create();
            listDialog.Show();
        }
    }
}
