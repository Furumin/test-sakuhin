using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;
using SharpDX.MediaFoundation;
using static ARMAN_DEMO.GameMathematics;
using System.Security.Cryptography.Pkcs;
using System.Linq;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using ARMAN_DEMO.src;
using System.Reflection;

namespace ARMAN_DEMO
{

    public struct Contact
    {
        public Contact(Polygon a, Polygon b, Vector2 p, Vector2 n, Vector2 ea, Vector2 eb, bool isvfc, ContactType type)
        {
            A = a; B = b; P = p; N = n; EA = ea; EB = eb; isVFContact = isvfc; TYPE = type;
        }
        public Polygon A;      //polygon A
        public Polygon B;      //polygon B
        public Vector2 P;      //接点
        public Vector2 N;      //側面の法線ベクトル
        public Vector2 EA;     //polygon Aの側面
        public Vector2 EB;     //polygon Bの側面
        public bool isVFContact; // V = 角, F = 側面
        public enum ContactType
        {
            VF,
            VV,
            FF
        };
        public ContactType TYPE;
    }

    public struct Star
    {
        public Vector3 Pos { get; set; }
        public Color Color { get; set; }
    }
    public class MainGame : Game
    {

        #region arena_variables
        static Vector3 worldCamPos = new(0, 0, 1400);

        public const float screenTop = 1400f;
        public const float screenLeft = -1334f;
        public const float screenRight = 1334f;
        public const float screenBottom = -50f;
        #endregion

        List<Polygon> entities;
        InputHandler inputHandler;
        CollisionResolver collisionResolver;
        ParticleHandler particleHandler;
        ProjectileHandler projectileHandler;

        MouseSprite mouseSprite;

        PolygonHandler stagePolygonHandler;

        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;


        Camera _camera;

        StagePolygon[] boundaries = new StagePolygon[4];
        StagePolygon[] asteroids = new StagePolygon[6];

        StagePolygon asteroid;

        private SpriteFont _font;

        ControlPolygon player;

        TimeSpan previousTime, currentTime;

        Projectile[] projectiles;
        int maxProjectiles = 256;

        Effect bloomEffect;
        Effect appliedBloomEffect;

        VectorGraphics vectorGraphics;

        RenderSystem renderSystem;

        EnemyPolygon testEnemy;

        List<SoundEffect> soundEffects;
        Song backgroundSong;

        Rectangle _arenaBoundary;
        public Rectangle ArenaBoundary { get { return _arenaBoundary; } }

        TimeSpan timeOfDeath;

        public ControlPolygon Player
        {
            get { return player; }
        }

        public MainGame()
        {
            _graphics = new GraphicsDeviceManager(this)
            {
                GraphicsProfile = GraphicsProfile.HiDef
            };
            Content.RootDirectory = "Content";
            IsMouseVisible = false;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            inputHandler = new InputHandler();
            collisionResolver = new CollisionResolver();
            particleHandler = new ParticleHandler();
            stagePolygonHandler = new PolygonHandler();
            projectiles = new Projectile[maxProjectiles];
            projectileHandler = new ProjectileHandler(projectiles);
            entities = new List<Polygon>();

            _graphics.IsFullScreen = false;
            _graphics.PreferredBackBufferWidth = 1366;
            _graphics.PreferredBackBufferHeight = 768;
            _graphics.ApplyChanges();

            //時間に関係ある変数を設定
            timeOfDeath = new TimeSpan();
            const double targetTime = 60d;
            TargetElapsedTime = TimeSpan.FromTicks((long)Math.Round(TimeSpan.TicksPerSecond / targetTime));
            previousTime = new TimeSpan();

            Vector2[] playerVectors =
            {
                new Vector2(0, 0),
                new Vector2(-25, -70),
                new Vector2(25, -70)
            };
            player = new ControlPolygon(playerVectors, new Vector2(0, -35), this);
            player.Push(new Vector2(0, 120));
            player._drawPriority = 1f;
            player._centerInertia = 20f * (50 * 50 + 70 * 70);
            player._mass = 10f;

            #region asteroid_initialize
            // SET ASTEROID 0
            Vector2[] asteroidVec =
            {
                new Vector2(-40, 260),
                new Vector2(-140,220),
                new Vector2(-204,41),
                new Vector2(-100,-80),
                new Vector2(80, -60),
                new Vector2(200,140),
                new Vector2(148,268)
            };

            for (int i = 0; i < asteroidVec.Length; i++)
                asteroidVec[i] /= 5f;
            Vector2 c = Vector2.Zero;
            foreach (var a in asteroidVec)
                c += a;
            c /= asteroidVec.Length;
            asteroid = new StagePolygon(asteroidVec, c, this)
            {
                _mass = player._mass * 2f,
                _centerInertia = player._centerInertia * 3f
            };
            asteroid.Push(new Vector2(0, 500));
            asteroid.SetLineColors(Color.White);
            asteroid.SetFillColors(Color.Black);
            asteroid.SetDrawMode(DrawMethod.LINE);

            asteroids[0] = asteroid;

            c = Vector2.Zero;
            // SET ASTEROID 1
            asteroidVec = new Vector2[]
                {
                    new Vector2(48, 86),
                    new Vector2(260, 80),
                    new Vector2(300, 200),
                    new Vector2(233, 268),
                    new Vector2(113, 240),
                };
            foreach (var a in asteroidVec)
                c += a;
            c /= asteroidVec.Length;

            asteroids[1] = new StagePolygon(
                asteroidVec,
                c,
                this
            )
            {
                _mass = 2300000f,
                _centerInertia = 600000000000f
            };
            asteroids[1].Push(new Vector2(-300, 700));
            asteroids[1].SetLineColors(Color.White);
            asteroids[1].SetFillColors(Color.Black);

            c = Vector2.Zero;
            // SET ASTEROID 2
            asteroidVec = new Vector2[]
                {
                    new Vector2(76, 372),
                    new Vector2(102, 209),
                    new Vector2(185, 90),
                    new Vector2(336, 84),
                    new Vector2(502, 140),
                    new Vector2(545, 267),
                    new Vector2(455, 409),
                    new Vector2(203, 435),
                };
            foreach (var a in asteroidVec)
                c += a;
            c /= asteroidVec.Length;
            asteroids[2] = new StagePolygon(
                asteroidVec,
                c,
                this
            )
            {
                _mass = 230000000f,
                _centerInertia = 600000000000000f
            };
            asteroids[2].Push(new Vector2(300, 700));
            asteroids[2].SetLineColors(Color.White);
            asteroids[2].SetFillColors(Color.Black);

            c = Vector2.Zero;
            // SET ASTEROID 3
            asteroidVec = new Vector2[]
                {
                    new Vector2(80, 260),
                    new Vector2(50, 156),
                    new Vector2(56, 90),
                    new Vector2(72, 25),
                    new Vector2(110, 63),
                    new Vector2(145, 160),
                    new Vector2(141, 260),
                };
            foreach (var a in asteroidVec)
                c += a;
            c /= asteroidVec.Length;

            asteroids[3] = new StagePolygon(
                asteroidVec,
                c,
                this
            )
            {
                _mass = 5000000f,
                _centerInertia = 200000000000f
            };
            asteroids[3].Push(new Vector2(-600, 700));
            asteroids[3].SetLineColors(Color.White);
            asteroids[3].SetFillColors(Color.Black);


            c = Vector2.Zero;
            // SET ASTEROID 4
            asteroidVec = new Vector2[]
                {
                    new Vector2(27, 66),
                    new Vector2(19, 48),
                    new Vector2(32, 28),
                    new Vector2(51, 22),
                    new Vector2(62, 31),
                    new Vector2(57, 44),
                };
            foreach (var a in asteroidVec)
                c += a;
            c /= asteroidVec.Length;

            asteroids[4] = new StagePolygon(
                asteroidVec,
                c,
                this
            )
            {
                _mass = player._mass / 2f,
                _centerInertia = player._centerInertia
            };
            asteroids[4].Push(new Vector2(600, 300));
            asteroids[4].SetLineColors(Color.White);
            asteroids[4].SetFillColors(Color.Black);

            c = Vector2.Zero;
            // SET ASTEROID 5
            asteroidVec = new Vector2[]
                {
                    new Vector2(15, 54),
                    new Vector2(17, 29),
                    new Vector2(35, 30),
                    new Vector2(41, 45),
                    new Vector2(33, 62),
                };
            foreach (var a in asteroidVec)
                c += a;
            c /= asteroidVec.Length;

            asteroids[5] = new StagePolygon(
                asteroidVec,
                c,
                this
            )
            {
                _mass = player._mass / 2f,
                _centerInertia = player._centerInertia
            };
            asteroids[5].Push(new Vector2(-600, 100));
            asteroids[5].SetLineColors(Color.White);
            asteroids[5].SetFillColors(Color.Black);

            for (int i = 0; i < asteroids.Length; i++)
            {
                asteroids[i].SetDrawMode(DrawMethod.LINE | DrawMethod.FILL);
            }

            #endregion

            // SET ENEMY
            Vector2[] enemyVec =
            {
                new Vector2(-45, 745+400),
                new Vector2(-45, 655+400),
                new Vector2(45, 655+400),
                new Vector2(45, 745+400),
            };
            Vector2 v = (enemyVec[0] + enemyVec[1] + enemyVec[2] + enemyVec[3]) / 4;
            testEnemy = new EnemyPolygon(enemyVec, v, this)
            {
                _mass = player._mass * 10f,
                _centerInertia = player._centerInertia * 100f
            };

            soundEffects = new List<SoundEffect>();
            SoundEffect.MasterVolume -= 0.9f;

            MediaPlayer.Volume = 0.03f;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // LOAD GAME CONTENT
            // ファイル読み込みや画面のサイズに依存しているものをここで読み込みます

            //カメラをイニシャライズ
            _camera = new Camera(GraphicsDevice.Viewport, 0, 0, 1f, Rectangle.Empty)
            {
                Pos = worldCamPos
            };


            #region ARENA_INIT

            //輪郭の広さ
            float width = 50f;

            //他のオブジェクトがアクセスできる、輪郭の横縦の情報
            int screenWidth = (int)(screenRight - screenLeft);
            int screenHeight = (int)(screenTop - screenBottom);
            _arenaBoundary = new Rectangle((int)screenLeft, (int)screenBottom, (int)screenWidth, (int)screenHeight);

            Vector2[] topBound = {
            new Vector2(screenLeft-width, screenTop+width),
            new Vector2(screenLeft-width, screenTop),
            new Vector2(screenRight+width, screenTop),
            new Vector2(screenRight+width, screenTop+width)
            };
            Vector2[] rightBound =
            {
                new Vector2(screenRight, screenTop),
                new Vector2(screenRight, screenBottom),
                new Vector2(screenRight+width, screenBottom),
                new Vector2(screenRight+width, screenTop)
            };
            Vector2[] bottomBound =
            {
                new Vector2(screenLeft-width, screenBottom),
                new Vector2(screenLeft-width, screenBottom-width),
                new Vector2(screenRight+width, screenBottom-width),
                new Vector2(screenRight+width, screenBottom)
            };
            Vector2[] leftBound =
            {
                new Vector2(screenLeft-width, screenTop),
                new Vector2(screenLeft-width, screenBottom),
                new Vector2(screenLeft, screenBottom),
                new Vector2(screenLeft, screenTop)
            };
            Color[] fillClr = new Color[]
            {
                Color.Blue,
                Color.DeepPink,
                Color.Purple,
                Color.Yellow
            };

            boundaries[0] = new StagePolygon(topBound, Vector2.Zero, this);
            boundaries[1] = new StagePolygon(rightBound, Vector2.Zero, this);
            boundaries[2] = new StagePolygon(bottomBound, Vector2.Zero, this);
            boundaries[3] = new StagePolygon(leftBound, Vector2.Zero, this);

            //各壁の情報を調整して、その他必要なデーターを設定
            for (int i = 0; i < boundaries.Length; i++)
            {
                boundaries[i].RecalculateCenter();
                boundaries[i]._mass = 1000000000000000000f;
                boundaries[i]._centerInertia = 1000000000000000000f;
                boundaries[i].SetFillColors(fillClr);
                boundaries[i].SetToBoundary();
            }

            #endregion


            entities.Add(player);
            entities.Add(testEnemy);
            entities.AddRange(asteroids);
            entities.AddRange(boundaries);

            //カメラの範囲に移す
            foreach (var e in entities)
                e.Push(new Vector2(_camera.Pos.X, _camera.Pos.Y));

            foreach (var v in asteroids)
                stagePolygonHandler.AddPolygon(v);
            foreach (var v in boundaries)
                stagePolygonHandler.AddPolygon(v);

            //レンダーに必要なオブジェクトを読み込む
            vectorGraphics = new VectorGraphics(_camera, this);
            MouseHandler.SetViewport(GraphicsDevice);
            MouseHandler._vectorGraphics = vectorGraphics;
            mouseSprite = new MouseSprite();

            vectorGraphics.SetStates();

            bloomEffect = Content.Load<Effect>("Bloom");
            appliedBloomEffect = Content.Load<Effect>("appBloom");
            bloomEffect.CurrentTechnique = bloomEffect.Techniques.First();

            renderSystem = new RenderSystem(GraphicsDevice, vectorGraphics);
            renderSystem.SetEffect(Globals.EFFECT_BLUR, bloomEffect);
            renderSystem.SetEffect(Globals.EFFECT_BLOOM, appliedBloomEffect);

            //最後にファイルの読み込みを行う
            soundEffects.Add(Content.Load<SoundEffect>("shoot"));
            backgroundSong = Content.Load<Song>("spacebgm");
            _font = Content.Load<SpriteFont>("Debug");
            SpriteFont _font2 = Content.Load<SpriteFont>("arial");
            SpriteFont[] temp = new SpriteFont[] { _font, _font2 };
            renderSystem.LoadFonts(temp);

            MediaPlayer.Play(backgroundSong);
            MediaPlayer.IsRepeating = true;
        }

        protected override void Update(GameTime gameTime)
        {
            //　インプット、時間を更新
            inputHandler.UpdateStates();
            MouseHandler.Update();
            mouseSprite.Update();
            previousTime = currentTime;
            currentTime = gameTime.TotalGameTime;
            TimeSpan dt = currentTime - previousTime;
            float deltaT = (float)dt.TotalMilliseconds / 1000f;


            if (player.IsDead && timeOfDeath.TotalMilliseconds == 0)
                timeOfDeath = gameTime.TotalGameTime;

            if (inputHandler.KeyHeld(Keys.Escape))
                Exit();

            //Update the current inputed velocity
            //プレイヤーと敵の速度をアップデート
            player.Update(deltaT, inputHandler);
            testEnemy.Update(deltaT);

            // collision detection
            stagePolygonHandler.Tick(collisionResolver.CollisionUpdate, dt, deltaT, entities);
            projectileHandler.Tick(collisionResolver.ProjectileCollisionUpdate, deltaT, entities.ToArray());

            //end collision detection, start applying result velocities
            //衝突判定終了、結果の速度などを計算
            player.ApplyVelocity();
            testEnemy.ApplyVelocity();

            // update particles
            particleHandler.Update(deltaT);

            //check if player is dead/ if game should exit
            float dif = (float)(gameTime.TotalGameTime.TotalSeconds - timeOfDeath.TotalSeconds);
            if (dif > 5 && timeOfDeath.TotalSeconds != 0)
                Exit();

            // レンダーの準備を開始
            renderSystem.Clear();

            renderSystem.AddDrawable(entities);
            renderSystem.AddDrawable(particleHandler.Alive().ToList());
            renderSystem.AddDrawable(projectileHandler.Alive().ToList());
            renderSystem.AddDrawable(mouseSprite.Sprite);

            base.Update(gameTime);
        }


        public void AddProjectile(Projectile p)
        {
            projectileHandler.AddProjectile(p);
        }

        //リストを使って粒子を入れたり抜いたりすると処理が遅いので、arrayを使って、使われていないindexをnullにしている
        public void AddParticle(Polygon p)
        {
            particleHandler.FirstOpenSlot = p;
        }

        protected override void Draw(GameTime gameTime)
        {

            renderSystem.Draw(vectorGraphics, _spriteBatch);

            // 操作キャラが死んだら徐々に画面を黒くする
            if (timeOfDeath.TotalMilliseconds > 0)
            {
                float dif = (float)(gameTime.TotalGameTime.TotalSeconds - timeOfDeath.TotalSeconds);
                Color clr = new(0, 0, 0, dif / 4);
                VertexPositionColor[] v = new VertexPositionColor[]
                {
                    new VertexPositionColor(new Vector3(ArenaBoundary.Left, ArenaBoundary.Top, 100), clr),
                    new VertexPositionColor(new Vector3(ArenaBoundary.Left, ArenaBoundary.Bottom, 100), clr),
                    new VertexPositionColor(new Vector3(ArenaBoundary.Right, ArenaBoundary.Bottom, 100), clr),
                    new VertexPositionColor(new Vector3(ArenaBoundary.Right, ArenaBoundary.Top, 100), clr)
                };

                vectorGraphics.Begin(DrawMethod.LINE);
                GraphicsDevice.BlendState = BlendState.AlphaBlend;
                vectorGraphics.AddQuad(v);
                vectorGraphics.End();

            }

            base.Draw(gameTime);
        }


        public void PlaySFX(int index)
        {
            soundEffects[index].CreateInstance().Play();
        }

    }
}