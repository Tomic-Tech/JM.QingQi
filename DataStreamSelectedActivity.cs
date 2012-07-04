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
    [Activity(Theme = "@style/Theme.Default", Label = "Select Data Stream")]
    public class DataStreamSelectedActivity : ListActivity
    {
        private string model = null;
        private Button valueBtn = null;
        private LinearLayout layout1 = null;
        private LinearLayout layout2 = null;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            ResourceManager.Instance.LiveDataVector.DeployEnabledIndex();

            // Create your application here
            ListView.ChoiceMode = ChoiceMode.Multiple;
            ListView.Focusable = false;
            ListView.ItemsCanFocus = false;
            ListView.ItemClick += (sender, e) =>
            {
                int i = ResourceManager.Instance.LiveDataVector.EnabledIndex(e.Position);
                ResourceManager.Instance.LiveDataVector[i].Showed = !ResourceManager.Instance.LiveDataVector[i].Showed;
            };

            layout1 = new LinearLayout(this);
            layout1.Orientation = Orientation.Horizontal;

            valueBtn = new Button(this);
            valueBtn.Text = Core.SysDB.GetText("Value");
            valueBtn.Gravity = GravityFlags.CenterVertical;
            valueBtn.Click += new EventHandler(ValueBtnClick);

            layout1.AddView(valueBtn, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent, ViewGroup.LayoutParams.FillParent));

            layout2 = new LinearLayout(this);
            layout2.AddView(layout1, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent));

            ListView.AddFooterView(layout2);

            // Prepare the loader. Either re-connect with an existing one,
            // or start a new one.

        }

        void ValueBtnClick(object sender, EventArgs e)
        {
            ResourceManager.Instance.LiveDataVector.DeployShowedIndex();
            Intent intent = new Intent(this, typeof(DataStreamActivity));
            intent.PutExtra("Model", model);
            StartActivity(intent);
        }

        protected override void OnStart()
        {
            base.OnStart();
            model = Intent.Extras.GetString("Model");
            string[] arrays = new string[ResourceManager.Instance.LiveDataVector.EnabledCount];

            for (int i = 0; i < arrays.Length; i++)
            {
                int index = ResourceManager.Instance.LiveDataVector.EnabledIndex(i);
                Core.LiveData ld = ResourceManager.Instance.LiveDataVector[index];
                arrays[i] = ld.ShortName + " : " + ld.Content;
            }

            ListView.Adapter = new ArrayAdapter<string>(
                this,
                Android.Resource.Layout.SimpleListItemMultipleChoice,
                arrays
            );
        }

        //public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        //{
        //    int code = (int)keyCode;
        //    int value = (int)Keycode.DpadRight;

        //    if (keyCode == Keycode.DpadRight)
        //    {
        //        ResourceManager.Instance.LiveDataVector.DeployShowedIndex();
        //        Intent intent = new Intent(this, typeof(DataStreamActivity));
        //        intent.PutExtra("Model", model);
        //        StartActivity(intent);
        //        return true;
        //    }
        //    return base.OnKeyDown(keyCode, e);
        //}
    }
}