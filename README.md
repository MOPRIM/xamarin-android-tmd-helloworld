# Hello World

## Add the Nuget package for MOPRIM's TMD

The MOPRIM's TMD requires a set of keys to function. Please contact MOPRIM to get your SDK access if you do not have one yet.

Then install the package `xamarin-android-tmd-core` version `0.4.0.2` and accept the MOPRIM and Google's licenses.


## Add the location permissions

Edit your `AndroidManifest.xml` file to add the following permission:

* AccessBackgroundLocation `android.permission.ACCESS_BACKGROUND_LOCATION`
* AccessCoarseLocation `android.permission.ACCESS_COARSE_LOCATION`
* AccessFineLocation `android.permission.ACCESS_FINE_LOCATION`
* AccessNetworkState `android.permission.ACCESS_NETWORK_STATE`
* Internet `android.permission.INTERNET`
* WakeLock `android.permission.WAKE_LOCK`
* BootReceiveCompleted `android.permission.RECEIVE_BOOT_COMPLETED`
* ForegroundService `android.permission.FOREGROUND_SERVICE`

## Initialization of the TMD

The TMD library needs to be initialized as early as possible. 
The recommended place to initialize the TMD is in the `onCreate` method of the `Application` class.

### Create an application class

Add a new class `Application.cs` at the root of your app folder with the following content:
```cs
using Android.App;
using Android.Runtime;
using FI.Moprim.Tmd.Sdk;

namespace HelloWorld
{
    [Application]
    public class CustomApplication : Application
    {
        protected CustomApplication(System.IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }
        public override void OnCreate()
        {
            base.OnCreate();

            TmdConfig.Builder builder = new TmdConfig.Builder(this)
                // Mandatory
                .SetEndpoint(GetString(Resource.String.tmd_endpoint))
                .SetKey(GetString(Resource.String.tmd_key));
            TMD.Instance.Init(this, builder.Build());

        }
    }
}
``` 

You need to replace the strings `tmd_endpoint` and `tmd_key` with your own credentials.
> ⚠️ These credentials are confidentials and should not be shared. One could create a new string resource file `config.xml` which is added to `.gitignore` to put these credentials or use something more specific to Xamarin to handle credentials.

### Link your app with the application class

Then modify the `<application>` of your `AndroidManifest.xml` to add the attribute
to reference to the application class.
```
android:name=".Application" 
```

## Request location permissions

The TMD requires access to location permissions to function (fine-grained and background).
You need the user to grant these permissions before starting the TMD, otherwise the mobility of the user will not be recorded.

```
var status = await Permissions.RequestAsync<Permissions.LocationAlways>();
```

## Starting the TMD

The TMD is a foreground service, thus it requires a notification when it is started.

```cs

using AndroidX.Core.App;
using Xamarin.Essentials;
using FI.Moprim.Tmd.Sdk;
using FI.Moprim.Tmd.Sdk.Model;
...

private static readonly string NOTIFICATION_CHANNEL_ID = "your.app.tmd.channel";
private static readonly int NOTIFICATION_ID = 7777; // Unique notification id
...

void CreateNotificationChannel()
{
    if (Build.VERSION.SdkInt < BuildVersionCodes.O)
    {
        // Notification channels are new in API 26 (and not a part of the
        // support library). There is no need to create a notification
        // channel on older versions of Android.
        return;
    }

    var channelName = "Channel name";
    var channelDescription = "Channel description";
    var channel = new NotificationChannel(NOTIFICATION_CHANNEL_ID, channelName, NotificationImportance.Default)
    {
        Description = channelDescription
    };
    channel.SetSound(null, null);
    channel.SetVibrationPattern(null);

    var notificationManager = (NotificationManager)GetSystemService(NotificationService);
    notificationManager.CreateNotificationChannel(channel);
}


Notification BuildNotification()
{
    // Instantiate the builder and set notification elements:
    NotificationCompat.Builder builder = new NotificationCompat.Builder(this, NOTIFICATION_CHANNEL_ID)
        .SetContentTitle("MOPRIM TMD")
        .SetContentText("Congratulations your TMD is running.")
        .SetSmallIcon(Android.Resource.Drawable.IcMediaPlay);

    // Build the notification:
    return builder.Build();
}
```

Then start the TMD with:
```
CreateNotificationChannel();
Notification notification = BuildNotification();
TmdStartReturnCode returnCode = TMD.StartForeground(this, NOTIFICATION_ID, notification);
if (returnCode != TmdStartReturnCode.Started && returnCode != TmdStartReturnCode.AlreadyRunning)
{
    var message = "Start TMD failed because of this reason: " + returnCode.Name();
    Android.Util.Log.Error("MainActivity", message);
}
else
{
    Android.Util.Log.Info("MainActivity", "Start TMD with uuid: " + TMD.Instance.InstallationId);
}
```

Additionally, you can stop the TMD by calling `TMD.Stop(context)` or check if the TMD is running by calling `TMD.IsTmdRunning(context)`

### Restarting the TMD on reboots or app updates

If the phone reboots or the app is update, one may want to restart the TMD if it was already started.

So register a new BroadcastReceiver:
```cs
using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using FI.Moprim.Tmd.Sdk;


namespace HelloWorld
{
    [BroadcastReceiver]
    public class RestartTmdBroadcastReceiver : BroadcastReceiver
    {

        private static readonly string NOTIFICATION_CHANNEL_ID = "your.app.tmd.channel";
        private static readonly int NOTIFICATION_ID = 7777; // Unique notification id

        private bool CheckActionIsValid(string action)
        {
            return "android.intent.action.BOOT_COMPLETED".Equals(action) ||
                "android.intent.action.QUICKBOOT_POWERON".Equals(action) ||
                "android.intent.action.MY_PACKAGE_REPLACED".Equals(action);
        }

        public override void OnReceive(Context context, Intent intent)
        {
            if (CheckActionIsValid(intent.Action) && TMD.WasTmdStarted(context))
            {
                CreateNotificationChannel(context);
                Notification notification = BuildNotification(context);
                TMD.StartForeground(context, NOTIFICATION_ID, notification);
            }  
        }

        private void CreateNotificationChannel(Context context)
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                // Notification channels are new in API 26 (and not a part of the
                // support library). There is no need to create a notification
                // channel on older versions of Android.
                return;
            }

            var channelName = "Channel name";
            var channelDescription = "Channel description";
            var channel = new NotificationChannel(NOTIFICATION_CHANNEL_ID, channelName, NotificationImportance.Default)
            {
                Description = channelDescription
            };
            channel.SetSound(null, null);
            channel.SetVibrationPattern(null);

            var notificationManager = (NotificationManager)context.GetSystemService(Context.NotificationService);
            notificationManager.CreateNotificationChannel(channel);
        }

        private Notification BuildNotification(Context context)
        {
            // Instantiate the builder and set notification elements:
            NotificationCompat.Builder builder = new NotificationCompat.Builder(context, NOTIFICATION_CHANNEL_ID)
                .SetContentTitle("MOPRIM TMD")
                .SetContentText("Congratulations your TMD is running.")
                .SetSmallIcon(Android.Resource.Drawable.IcMediaPlay);

            // Build the notification:
            return builder.Build();
        }
    }
}
```

and update the `AndroidManifest.xml` application tag:
```
<receiver android:name=".RestartTmdBroadcastReceiver" android:exported="false">
    <intent-filter>
        <category android:name="android.intent.category.DEFAULT" />
        <action android:name="android.intent.action.BOOT_COMPLETED" />
        <action android:name="android.intent.action.QUICKBOOT_POWERON" />
        <action android:name="android.intent.action.MY_PACKAGE_REPLACED" />
    </intent-filter>
</receiver>
```

Now the TMD will restart automatically when the user has rebooted his phone or whenever the app gets updated.
