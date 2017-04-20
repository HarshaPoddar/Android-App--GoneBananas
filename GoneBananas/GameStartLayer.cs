﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CocosSharp;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace GoneBananas
{
    class GameStartLayer : CCLayerColor
    {
        public GameStartLayer() : base()
        {
            var touchListener = new CCEventListenerTouchAllAtOnce();
            touchListener.OnTouchesEnded = (touches, ccevent) => Window.DefaultDirector.ReplaceScene(GameLayer.GameScene(Window));

            AddEventListener(touchListener, this);

            Color = CCColor3B.Black;
            Opacity = 255;
        }
        protected override void AddedToScene()
        {
            base.AddedToScene();

            Scene.SceneResolutionPolicy = CCSceneResolutionPolicy.ShowAll;
                
            var label = new CCLabel("Tap Screen to Go Bananas!", "arial", 22)
            {
                Position = VisibleBoundsWorldspace.Center,
                Color = CCColor3B.Green,
                HorizontalAlignment = CCTextAlignment.Center,
                VerticalAlignment = CCVerticalTextAlignment.Center,
                AnchorPoint = CCPoint.AnchorMiddle,
                Dimensions = ContentSize
            };
            AddChild(label);
        }
        public static CCScene GameStartLayerScene(CCWindow mainWindow)
        {
            var scene = new CCScene(mainWindow);
            var layer = new GameStartLayer();

            scene.AddChild(layer);

            return scene;
        }
    }
}