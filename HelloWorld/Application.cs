
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
