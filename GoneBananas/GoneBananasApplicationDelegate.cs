using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CocosSharp;
using CocosDenshion;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace GoneBananas
{
    class GoneBananasApplicationDelegate : CCApplicationDelegate
    {
        public override void ApplicationDidFinishLaunching(CCApplication application, CCWindow mainWindow)
        {
            application.PreferMultiSampling = false;
            application.ContentRootDirectory = "Content";

            mainWindow.SupportedDisplayOrientations.IsPortrait = CCDisplayOrientation.Portrait;
            application.ContentSearchPaths.Add("hd");

            CCSimpleAudioEngine.SharedEngine.PreloadEffect("Sounds/tap");

            CCScene scene = GameStartLayer.GameStartLayerScene(mainWindow);
            mainWindow.RunWithScene(scene);
        }
        public override void ApplicationDidEnterBackground(CCApplication application)
        {
            // stop all of the animation actions that are running.
            application.Paused = true;
            //pausing the simple audio
            CCSimpleAudioEngine.SharedEngine.PauseBackgroundMusic();
        }   
        public override void ApplicationWillEnterForeground(CCApplication application)
        {
            //resuming all paused applications
            application.Paused = false;
            //resuming all sounds
            CCSimpleAudioEngine.SharedEngine.ResumeBackgroundMusic();
        }
    }
}