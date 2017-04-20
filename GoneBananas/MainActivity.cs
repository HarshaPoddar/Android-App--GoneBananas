using Android.App;
using Android.Widget;
using Android.OS;
using CocosSharp;

namespace GoneBananas
{
    [Activity(Label = "GoneBananas", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            var application = new CCApplication();
            application.ApplicationDelegate = new GoneBananasApplicationDelegate();
            SetContentView(application.AndroidContentView);
            application.StartGame();    
            // Set our view from the "main" layout resource
            // SetContentView (Resource.Layout.Main);
        }
    }
}

