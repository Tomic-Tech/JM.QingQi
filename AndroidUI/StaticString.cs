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
    static class StaticString
    {
#if TOMIC_ANDROID
        public const string beforeBlank = "     ";
#else
        public const string beforeBlank = "";
#endif
    }
}