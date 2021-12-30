using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using FI.Moprim.Tmd.Sdk;
using FI.Moprim.Tmd.Sdk.Model;


namespace HelloWorld
{
    public class Utils
    {

        private static readonly string NOTIFICATION_CHANNEL_ID = "your.app.tmd.channel";
        private static readonly int NOTIFICATION_ID = 7777; // Unique notification id

        private Utils() { }

        private static void CreateNotificationChannel(Context context)
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

        private static Notification BuildNotification(Context context)
        {
            // Instantiate the builder and set notification elements:
            NotificationCompat.Builder builder = new NotificationCompat.Builder(context, NOTIFICATION_CHANNEL_ID)
                .SetContentTitle("MOPRIM TMD")
                .SetContentText("Congratulations your TMD is running.")
                .SetSmallIcon(Android.Resource.Drawable.IcMediaPlay);

            // Build the notification:
            return builder.Build();
        }


        public static TmdStartReturnCode StartTmd(Context context)
        {
            // https://docs.microsoft.com/en-us/xamarin/android/app-fundamentals/notifications/local-notifications
            CreateNotificationChannel(context);
            Notification notification = BuildNotification(context);
            return TMD.StartForeground(context, NOTIFICATION_ID, notification);
        }
    }
}
