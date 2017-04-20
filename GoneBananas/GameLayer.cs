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
using Box2D.Dynamics;
using Box2D.Common;
using Box2D.Collision.Shapes;

namespace GoneBananas
{
    class GameLayer : CCLayerColor
    {
        const float Money_Speed = 350.0f;
        const float Game_Duration = 60.0f;
        // point to meter ratio for physics
        const int PTM_RATIO = 32;

        float elapsedTime;
        CCSprite monkey;
        List<CCSprite> visibleBananas;
        List<CCSprite> hitBananas;

        // monkey walking animation
        CCAnimation walkAnim;
        CCRepeatForever walkRepeat;
        CCCallFunc walkAnimStop = new CCCallFuncN(node => node.StopAllActions());

        // physics world
        b2World world;

        // balls sprite batch
        CCSpriteBatchNode ballsBatch;
        CCTexture2D ballTexture;

        //background sprite
        CCSprite grass;

        //particles
        CCParticleSun sun;

        //circle layer behind sun
        CCDrawNode circleNode;

        //parallax node for clouds
        CCParallaxNode parallaxClouds;

        //define banana rotation action
        CCRotateBy rotateBanana = new CCRotateBy(0.8f, 360);

        //remove banana once it hits bottom of screen
        CCCallFuncN moveBananaComplete = new CCCallFuncN(node => node.RemoveFromParent());

        public GameLayer()
        {
            var touchListener = new CCEventListenerTouchAllAtOnce();
            touchListener.OnTouchesEnded = OnTouchesEnded;

            Color = new CCColor3B(CCColor4B.White);
            Opacity = 255;

            // batch node for physics balls
            ballsBatch = new CCSpriteBatchNode("balls", 100);
            ballTexture = ballsBatch.Texture;
            AddChild(ballsBatch, 1, 1);

            visibleBananas = new List<CCSprite>();
            hitBananas = new List<CCSprite>();

            Schedule(t =>
            {
                visibleBananas.Add(AddBanana());
                elapsedTime += t;
                if (ShouldEndGame())
                {
                    EndGame();
                }
            }, 1.0f);

            Schedule(t => CheckCollision());

            AddGrass();
            AddSun();
            AddMonkey();

        }

        void AddGrass()
        {
            grass = new CCSprite("grass");
            AddChild(grass);

        }

        CCSprite AddBanana()
        {
            var spriteSheet = new CCSpriteSheet("animations/monkey.plist");
            var banana = new CCSprite(spriteSheet.Frames.Find((x) => x.TextureFilename.StartsWith("Banana")));

            var position = GetRandomPositon(banana.ContentSize);
            banana.Position = position;
            banana.Scale = 0.5f;

            AddChild(banana);

            return banana;
        }

        void AddMonkey()
        {
            var spriteSheet = new CCSpriteSheet("animations/monkey.plist");
            var animationFrames = spriteSheet.Frames.FindAll((x) => x.TextureFilename.StartsWith("frame"));

            walkAnim = new CCAnimation(animationFrames, 0.1f);
            walkRepeat = new CCRepeatForever(new CCAnimate(walkAnim));
            monkey = new CCSprite(animationFrames.First()) { Name = "Monkey" };
            monkey.Scale = 0.25f;

            AddChild(monkey);
        }

        public void OnTouchesEnded(List<CCTouch> touches, CCEvent touchEvent)
        {
            monkey.StopAllActions();
            var location = touches[0].LocationOnScreen;
            location = WorldToScreenspace(location);
            float ds = CCPoint.Distance(monkey.Position, location);

            var dt = ds / Money_Speed;

            var MoveMonkey = new CCMoveTo(dt, location);

            monkey.RunAction(walkRepeat);
            monkey.RunActions(MoveMonkey, walkAnimStop);
            CCSimpleAudioEngine.SharedEngine.PlayEffect("Sounds/tap");
        }

        void CheckCollision()
        {
            visibleBananas.ForEach(banana =>
           {
               bool hit = banana.BoundingBoxTransformedToParent.IntersectsRect(monkey.BoundingBoxTransformedToParent);
               if (hit)
               {
                   hitBananas.Add(banana);
                   CCSimpleAudioEngine.SharedEngine.PlayEffect("Sounds/tap");
                   Explode(banana.Position);
                   banana.RemoveFromParent();
               }
           });
            hitBananas.ForEach(banana => visibleBananas.Remove(banana));

            int ballHitCount = ballsBatch.Children.Count(ball => ball.BoundingBoxTransformedToParent.IntersectsRect(monkey.BoundingBoxTransformedToParent));

            if (ballHitCount > 0)
            {
                EndGame();
            }
            Schedule(t => {
                visibleBananas.Add(AddBanana());
                elapsedTime += t;
                if (ShouldEndGame())
                {
                    EndGame();
                }
                AddBall();
            }, 1.0f);

            Schedule(t => {
                world.Step(t, 8, 1);

                foreach (CCPhysicsSprite sprite in ballsBatch.Children)
                {
                    if (sprite.Visible && sprite.PhysicsBody.Position.x < 0f || sprite.PhysicsBody.Position.x * PTM_RATIO > ContentSize.Width)
                    {
                        world.DestroyBody(sprite.PhysicsBody);
                        sprite.Visible = false;
                        sprite.RemoveFromParent();
                    }
                    else
                    {
                        sprite.UpdateBallTransform();
                    }
                }
            });
        }

        bool ShouldEndGame()
        {
            return elapsedTime > Game_Duration;
        }

        void EndGame()
        {
            var gameOverScene = GameOverLayer.SceneWithScore(Window, hitBananas.Count());
            var transitionToGameOver = new CCTransitionMoveInR(0.3f, gameOverScene);
            Director.ReplaceScene(transitionToGameOver);
        }

        void AddClouds()
        {
            float h = VisibleBoundsWorldspace.Size.Height;
            parallaxClouds = new CCParallaxNode
            {
                Position = new CCPoint(0, h)
            };
            AddChild(parallaxClouds);

            var cloud1 = new CCSprite("cloud");
            var cloud2 = new CCSprite("cloud");
            var cloud3 = new CCSprite("cloud");

            float yRatio1 = 1.0f;
            float yRatio2 = 0.15f;
            float yRatio3 = 0.5f;

            parallaxClouds.AddChild(cloud1, 0, new CCPoint(1.0f, yRatio1), new CCPoint(100, -100 + h - (h * yRatio1)));
            parallaxClouds.AddChild(cloud2, 0, new CCPoint(1.0f, yRatio2), new CCPoint(250, -200 + h - (h * yRatio2)));
            parallaxClouds.AddChild(cloud3, 0, new CCPoint(1.0f, yRatio3), new CCPoint(400, -150 + h - (h * yRatio3)));

        }

        void Explode(CCPoint point)
        {
            var explosion = new CCParticleExplosion(point);
            explosion.TotalParticles = 10;
            explosion.AutoRemoveOnFinish = true;
            AddChild(explosion);
        }

        void AddSun()
        {
            circleNode = new CCDrawNode();
            circleNode.DrawCircle(CCPoint.Zero, 30.0f, CCColor4B.Yellow);
            AddChild(circleNode);

            sun = new CCParticleSun(CCPoint.Zero);
            sun.StartColor = new CCColor4F(CCColor3B.Red);
            sun.EndColor = new CCColor4F(CCColor4B.Yellow);
            AddChild(sun);
        }

        protected override void AddedToScene()
        {
            base.AddedToScene();
            Scene.SceneResolutionPolicy = CCSceneResolutionPolicy.NoBorder;

            grass.Position = VisibleBoundsWorldspace.Center;
            monkey.Position = VisibleBoundsWorldspace.Center;

            var b = VisibleBoundsWorldspace;
            sun.Position = b.UpperRight.Offset(-100, -100);

            circleNode.Position = sun.Position;

            AddClouds();
        }

        public static CCScene GameScene(CCWindow mainWindow)
        {
            var scene = new CCScene(mainWindow);
            var layer = new CCLayer();

            scene.AddChild(layer);
            return scene;
        }

        void InitPhysics()
        {
            CCSize size = Layer.VisibleBoundsWorldspace.Size;

            var gravity = new b2Vec2(0.0f, -10.0f);
            world = new b2World(gravity);

            world.SetAllowSleeping(true);
            world.SetContinuousPhysics(true);

            var def = new b2BodyDef();
            def.allowSleep = true;
            def.position = b2Vec2.Zero;
            def.type = b2BodyType.b2_staticBody;
            b2Body groundBody = world.CreateBody(def);
            groundBody.SetActive(true);

            b2EdgeShape groundBox = new b2EdgeShape();
            groundBox.Set(b2Vec2.Zero, new b2Vec2(size.Width / PTM_RATIO, 0));
            b2FixtureDef fd = new b2FixtureDef();
            fd.shape = groundBox;
            groundBody.CreateFixture(fd);
        }

        void AddBall()
        {
            int idx = (CCRandom.Float_0_1() > .5 ? 0 : 1);
            int idy = (CCRandom.Float_0_1() > .5 ? 0 : 1);
            var sprite = new CCPhysicsSprite(ballTexture, new CCRect(32 * idx, 32 * idy, 32, 32), PTM_RATIO);

            ballsBatch.AddChild(sprite);

            CCPoint p = GetRandomPosition(sprite.ContentSize);

            sprite.Position = new CCPoint(p.X, p.Y);
            var def = new b2BodyDef();
            def.position = new b2Vec2(p.X / PTM_RATIO, p.Y / PTM_RATIO);
            def.linearVelocity = new b2Vec2(0.0f, -1.0f);
            def.type = b2BodyType.b2_dynamicBody;
            b2Body body = world.CreateBody(def);

            var circle = new b2CircleShape();
            circle.Radius = 0.5f;

            var fd = new b2FixtureDef();
            fd.shape = circle;
            fd.density = 1f;
            fd.restitution = 0.85f;
            fd.friction = 0f;
            body.CreateFixture(fd);

            sprite.PhysicsBody = body;

            Console.WriteLine("sprite batch node count = {0}", ballsBatch.ChildrenCount);
        }

        public override void OnEnter()
        {
            base.OnEnter();

            InitPhysics();
        }



    }
}
            