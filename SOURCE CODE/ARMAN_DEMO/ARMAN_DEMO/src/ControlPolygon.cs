using System;
using ARMAN_DEMO;
using ARMAN_DEMO.src;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ARMAN_DEMO
{
    public class ControlPolygon : Polygon
    {
        public float _angle;
        public Vector2 _grav;
        public Vector2 currentForce;
        public Color fillColor;
     
        int stunned;
        int shootCooldown;
        Color[] lineColors;
        Vector2 movementAccel;


        /// <summary>
        /// Always a rectangle!!!
        /// </summary>
        /// <param name="v"></param>
        /// <param name="c"></param>
        public ControlPolygon(Vector2[] v, Vector2 c, MainGame game) : base(v, c, game)
        {
            //速度などに必要な変数をイニシャライズ
            _angle = 0;
            currentForce = Vector2.Zero;
            _grav = new Vector2(0, 0f);
            _mass = 1f;
            stunned = 0;
            shootCooldown = 0;
            movementAccel = new Vector2(500, 500);
            _killTime = -1;

            //配色情報を設定
            fillColor = Color.White;
            SetFillColors(fillColor);
            lineColors = new Color[] { Color.Red, Color.Blue, Color.Blue };
            _renderInfo = new RenderInfo(0);
            SetDrawMode(DrawMethod.FILL | DrawMethod.LINE);
        }

        public int StunCounter
        {
            get { return stunned; }
            set { stunned = value; }
        }

        public void Update(float dt, InputHandler inputHandler)
        {
            if (IsDead)
            {
                base.Update(dt);
                return;
            }
            if (shootCooldown++ > 60)
                shootCooldown = 60;
            //行動ができるかをチェック
            if (stunned-- < 1)
            {
                bool horiMov = true, vertMov = true, spin = true;

                //インプットを読んで動き方をきめる
                if (inputHandler.KeyHeld(Keys.W))
                {
                    _acceleration.Y += movementAccel.Y;
                    if (_velocity.Y < 0)
                        _acceleration.Y += -_velocity.Y * 4f;
                }
                else if (inputHandler.KeyHeld(Keys.S))
                {
                    _acceleration.Y += -movementAccel.Y;
                    if (_velocity.Y > 0)
                        _acceleration.Y += -_velocity.Y * 4f;
                }
                else
                {
                    _acceleration.Y = 0;
                    vertMov = false;
                }
                if (inputHandler.KeyHeld(Keys.D))
                {
                    _acceleration.X += movementAccel.X;
                    if (_velocity.X < 0)
                        _acceleration.X += -_velocity.X * 4f;
                }
                else if (inputHandler.KeyHeld(Keys.A))
                {
                    _acceleration.X += -movementAccel.X;
                    if (_velocity.X > 0)
                        _acceleration.X += -_velocity.X * 4f;
                }
                else
                {
                    _acceleration.X = 0;
                    horiMov = false;
                }


                Vector3 mPos = MouseHandler.GetTransformedPosition();
                float angleToMouse = (float)Math.Atan2(mPos.Y - _center.Y, mPos.X - _center.X);
                if (angleToMouse < 0f)
                    angleToMouse = MathHelper.Pi * 2f + angleToMouse;
                _orientation %= MathHelper.Pi * 2f;
                float dif1 = angleToMouse - _orientation;

                //逆方向の回転を防ぐために
                if (dif1 < -MathHelper.Pi)
                    dif1 += MathHelper.Pi * 2f;
                else if (dif1 >= MathHelper.Pi)
                    dif1 -= MathHelper.Pi * 2f;

                _angVel = dif1 / dt;

                //インプットがない場合はゆっくりと速度をおとす
                if (!vertMov)
                {
                    _acceleration.Y = -_velocity.Y * 2.5f;
                }
                if (!horiMov)
                {
                    _acceleration.X = -_velocity.X * 2.5f;
                }
                if (!spin && _angVel != 0f)
                    _angAcc = -_angVel * 4f;

                CapVelocity(500);
                CapAngularVelocity(20);

                //弾丸を発射
                if (MouseHandler.LeftHold() && shootCooldown >= 14)
                {
                    shootCooldown = 0;
                    Vector2[] temp = new Vector2[1];
                    Projectile p = new(temp, 11, Vertices[0], Shape.Triangle, game);
                    p.Velocity = new Vector2((float)Math.Cos(_orientation), (float)Math.Sin(_orientation)) * 1400;
                    p.AngularVelocity = -5f;
                    p.SetFillColors(Color.Aqua);
                    p.SetLineColors(Color.Crimson);
                    p._mass = 100000f;
                    p._centerInertia = 100000f;
                    p.data.knockback = 0.8f;
                    p.SetTrailColors(new Color[] { Color.Red, Color.PaleGoldenrod });
                    p.SetKillTime(200);
                    p.data.friendly = true;
                    p.SetRenderInfo(1);
                    game.AddProjectile(p);
                    game.PlaySFX(0);
                }
            }
            //行動ができない
            else
            {
                _acceleration = Vector2.Zero;
                _angAcc = 0f;
            }

            if (stunned < 0)
                stunned = 0;

            Update(dt);
            UpdatePositions();
        }

        public override void Draw(VectorGraphics vectorGraphics)
        {
            if (IsDead) return;
            _edgeColors = lineColors;
            switch(vectorGraphics.drawMethod)
            {
                case DrawMethod.LINE:
                    vectorGraphics.AddShape_LINE(ToVPC(lineColors));
                    break;
                case DrawMethod.FILL:
                    vectorGraphics.AddTriangle(ToVPC(Color.White));
                    break;
            }
        }

        //衝突時に粒子を生み出す
        public override void OnCollision(Vector2 p, float scale = 1f)
        {
            Color[] clrs = new Color[] { Color.White, Color.Cyan, Color.LightPink, Color.LightYellow };
            Vector3 pos = new(p.X, p.Y, _drawPriority + 1);
            Random random = new();
            int n = random.Next(10, 26);
            for (int i = 0; i < n; i++)
            {
                float velo = random.Next(230, 270) * scale;
                float ang = (random.Next(0, 361) / 180f) * (MathHelper.Pi);
                float size = random.Next(4, 10);
                int c = random.Next(4);
                Vector2 vel = new Vector2((float)Math.Cos(ang), (float)Math.Sin(ang)) * velo;
                SpawnParticle(size, pos, vel, -vel * 0.25f, clrs[c], 60, 15);
            }
            base.OnCollision(p);
        }

        //死ぬと粒子を生み出す
        public override void OnDeath()
        {
            base.OnDeath();
            Color[] clrs = new Color[] { Color.White, Color.Red, Color.Blue, Color.Red, Color.Blue };
            Vector3 pos = new(_center.X, _center.Y, _drawPriority + 1);
            Random random = new();
            int n = 50;
            for (int i = 0; i < n; i++)
            {
                float velo = random.Next(390, 490) * 1.7f;
                float ang = (random.Next(0, 361) / 180f) * (MathHelper.Pi);
                float size = random.Next(7, 12);
                int c = random.Next(4);
                Vector2 vel = new Vector2((float)Math.Cos(ang), (float)Math.Sin(ang)) * velo;
                SpawnParticle(size, pos, vel, -vel * 0.25f, clrs[c], 120, 1, Shape.Triangle);
            }
        }

    }
}
