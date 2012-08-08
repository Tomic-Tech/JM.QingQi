using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace JM.QingQi
{
    public class BootBroadcastReceiver : BroadcastReceiver
    {
        const string Action = "android.intent.action.BOOT_COMPLETED";

        public override void OnReceive(Context context, Intent intent)
        {
            if (intent.Action.Equals(Action))
            {
                Intent qq = new Intent(context, typeof(QingQiActivity));
                qq.AddFlags(ActivityFlags.NewTask);
                context.StartActivity(intent);
            }
        }
    }
}