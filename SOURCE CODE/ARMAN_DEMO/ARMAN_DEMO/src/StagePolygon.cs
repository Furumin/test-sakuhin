using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;
using SharpDX.MediaFoundation;
using static ARMAN_DEMO.GameMathematics;
using System.Security.Cryptography.Pkcs;
using SharpDX.Direct3D11;
using System.Linq;
using Microsoft.Xna.Framework.Content;

namespace ARMAN_DEMO
{
    public class StagePolygon : Polygon
    {
        bool isBoundary;

        public bool IsBoundary { get { return isBoundary; } }   
        public StagePolygon(Vector2[] d, Vector2 c, MainGame game) : base(d, c, game)
        {
            isBoundary = false;
        }

        public override void Update(float deltaT)
        {
            base.Update(deltaT);
            CapVelocity(700);
            CapAngularVelocity(10);
        }

        public override void OnDeath()
        {
            base.OnDeath();
            Color clr = Color.White;
            Vector3 pos = new(_center.X, _center.Y, _drawPriority + 1);
            Random random = new();
            int n = 80;
            for (int i = 0; i < n; i++)
            {
                float velo = random.Next(90, 500) * 1.7f;
                float ang = (random.Next(0, 361) / 180f) * (MathHelper.Pi);
                float size = random.Next(7, 12);
                Vector2 vel = new Vector2((float)Math.Cos(ang), (float)Math.Sin(ang)) * velo;
                SpawnParticle(size, pos, vel, -vel * 0.16f, clr, 120, 1, Shape.Triangle);
            }
        }

        public override void OnCollision(Vector2 p, float scale = 1)
        {
            base.OnCollision(p, scale);
            Color clr = Color.White;
            Vector3 pos = new(p.X, p.Y, _drawPriority + 1);
            Random random = new();
            int n = 30;
            for (int i = 0; i < n; i++)
            {
                float velo = random.Next(50, 200) * 1.3f;
                float ang = (random.Next(0, 361) / 180f) * (MathHelper.Pi);
                float size = random.Next(2, 7);
                Vector2 vel = new Vector2((float)Math.Cos(ang), (float)Math.Sin(ang)) * velo;
                SpawnParticle(size, pos, vel, -vel * 0.20f, clr, 180, 3, Shape.Square, 0);
            }
        }

        public void SetToBoundary()
        {
            isBoundary = true;
        }

        //アリーナを構成するふたつのStagePolygonの衝突は無視して良い
        public override bool CanCollide(Polygon p)
        {
            if (p is StagePolygon)
            {
                return !(isBoundary && ((StagePolygon)p).IsBoundary);
            }
            return true;

        }

        public override void Draw(VectorGraphics vectorGraphics)
        {
            if (IsDead)
                return;
            base.Draw(vectorGraphics);
        }
    }
}
