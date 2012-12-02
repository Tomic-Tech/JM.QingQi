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

using JM.QingQi.Vehicle;
using JM.Core;

namespace JM.QingQi.AndroidUI
{
    [Activity(Theme = "@style/Theme.StaticDataStream", Label = "Detail Data Stream Activity")]
    public class DetailDataStreamActivity : ListActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Create your application here
            Window.SetFlags(WindowManagerFlags.KeepScreenOn, WindowManagerFlags.KeepScreenOn);
            string model = Intent.Extras.GetString("Model");
            int position = Intent.Extras.GetInt("Index");
            Core.LiveDataVector vec = Manager.LiveDataVector;
            int index = vec.ShowedIndex(position);

            string[] arrays = new string[4];
            arrays[0] = StaticString.beforeBlank + vec[index].Content;
            arrays[1] = StaticString.beforeBlank + vec[index].Value + vec[index].Unit;
            arrays[2] = StaticString.beforeBlank + Database.GetText("Range", "System") + " : " + vec[index].DefaultValue;
            StringBuilder description = new StringBuilder();
#if TOMIC_ANDROID
            if (vec[index].Description.Length > 23)
            {
                for (int i = 0; i < vec[index].Description.Length; )
                {
                    if (i + 23 > vec[index].Description.Length)
                    {
                        description.Append(StaticString.beforeBlank + vec[index].Description.Substring(i) + "\n");
                    }
                    else
                    {
                        description.Append(StaticString.beforeBlank + vec[index].Description.Substring(i, 23) + "\n");
                    }
                    i += 23;
                }
            }
            else
            {
                description.Append(StaticString.beforeBlank + vec[index].Description);
            }
#else
            description.Append(StaticString.beforeBlank + vec[index].Description);
#endif
            //arrays[3] = StaticString.beforeBlank + vec[index].Description;
            arrays[3] = description.ToString();

            ListAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, arrays);
        }
    }
}