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

namespace JM.QingQi.AndroidUI
{
	[BroadcastReceiver]
    public class BootCompleteReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
#if TOMIC_ANDROID
            if (intent.Action == Intent.ActionBootCompleted)
            {
                Intent activity = new Intent(context, typeof(SplashActivity));
                activity.AddFlags(ActivityFlags.NewTask);
                activity.AddFlags(ActivityFlags.SingleTop);
                context.StartActivity(activity);
            }
#endif
        }
    }
}