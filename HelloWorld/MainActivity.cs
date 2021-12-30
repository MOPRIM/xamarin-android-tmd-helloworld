using System;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.AppCompat.Widget;
using AndroidX.AppCompat.App;
using Google.Android.Material.FloatingActionButton;
using Google.Android.Material.Snackbar;
using Xamarin.Essentials;
using FI.Moprim.Tmd.Sdk;
using FI.Moprim.Tmd.Sdk.Model;

namespace HelloWorld
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            Toolbar toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            fab.Click += FabOnClick;
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_settings)
            {
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        private async void FabOnClick(object sender, EventArgs eventArgs)
        {
            View view = (View)sender;
            if (TMD.IsTmdRunning(this))
            {
                TMD.Stop(this);
                Snackbar.Make(view, "You stopped the TMD", Snackbar.LengthLong).Show();
            }
            else
            {
                var status = await Permissions.RequestAsync<Permissions.LocationAlways>();

                if (status != PermissionStatus.Granted)
                {
                    var message = "Start of TMD failed because location always is not granted";
                    Android.Util.Log.Error("MainActivity", message);
                    Snackbar.Make(view, message, Snackbar.LengthLong).Show();
                }
                else {
                
                TmdStartReturnCode returnCode = Utils.StartTmd(this);
                if (returnCode != TmdStartReturnCode.Started && returnCode != TmdStartReturnCode.AlreadyRunning)
                {
                    var message = "Start TMD failed because of this reason: " + returnCode.Name();
                    Android.Util.Log.Error("MainActivity", message);
                    Snackbar.Make(view, message, Snackbar.LengthLong).Show();
                }
                else
                {
                    Snackbar.Make(view, "You started the TMD", Snackbar.LengthLong).Show();
                    Android.Util.Log.Info("MainActivity", "Start TMD with uuid: " + TMD.Instance.InstallationId);
                }
                }
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}
