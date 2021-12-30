using Android.Content;
using FI.Moprim.Tmd.Sdk;


namespace HelloWorld
{
    [BroadcastReceiver]
    public class RestartTmdBroadcastReceiver : BroadcastReceiver
    {
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
                Utils.StartTmd(context);
            }  
        }
    }
}
