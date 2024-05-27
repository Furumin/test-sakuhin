using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using static ARMAN_DEMO.GameMathematics;

namespace ARMAN_DEMO
{
    public enum Shape
    {
        Triangle,
        Square,
        Circle
    }

    //弾丸の情報
    public struct DamageInformation
    {
        public int damage;
        public float knockback;
        public bool friendly;
    }
    public class Projectile : Polygon
    {
        public DamageInformation data;
        Vector2 trailStart, trailEnd, trailDir;
        Color trailStartColor, trailEndColor;
        Vector2 trailDistance;
        float particleKillTime;
        float trailCounter;
        
        Shape shape;

        bool active;

        public bool Active
        {
            get { return active; }
        }

        public Projectile(Vector2[] dots, float r, Vector2 c, Shape shape, MainGame game) : base(dots, c, game)
        {
            _killTime = -1;
            active = true;
            this.shape = shape;
            this._center = c;

            Vector2 vertex = _center + new Vector2(0, r);
            int n = 0;
            float rotate = 0;
            float start = MathHelper.Pi / 2f;

            switch (shape)
            {
                case Shape.Triangle:
                    n = 3;
                    rotate = (MathHelper.Pi * 2f) / 3f;
                    _fillColors = new Color[]
                    {
                        Color.White,
                        Color.White,
                        Color.White,
                    };
                    break;
                case Shape.Square:
                    n = 4;
                    rotate = (MathHelper.Pi) / 2f;
                    _fillColors = new Color[]
                    {
                        Color.White,
                        Color.White,
                        Color.White,
                        Color.White
                    };
                    break;
                case Shape.Circle:
                    n = 20;
                    rotate = (MathHelper.Pi * 2f) / n;
                    _fillColors = new Color[2] { Color.White, Color.White };
                    break;
            }

            List<Vector2> vertices = new();
            //センターと一定の距離に離れてn数の頂点をセンターを回りながら位置づける
            //簡単に、五角形以上の弾丸も作れる
            for(int i = 0; i < n; i++)
            {
                Vector2 v = RotatedVector(vertex, _center, start + rotate * i);
                vertices.Add(v);
            }
            this.Vertices = vertices.ToArray();
            _normals = new Vector2[VertexCount];
            RecalculateNormals();
            _edgeColors = _fillColors;

            trailStart = _center;
            trailEnd = _center;
            trailStartColor = _fillColors[0];
            trailEndColor = _fillColors[1];
            particleKillTime = 60;
            trailCounter = 0;
            trailDir = Vector2.Zero;

            SetDrawMode(DrawMethod.FILL | DrawMethod.LINE);
        }

        public void SetTrailColors(Color[] clr)
        {
            if (clr.Length > 2 || clr.Length < 1)
                throw new ArgumentException("Wrong length of color array!");
            
            trailStartColor = clr[0];
            if (clr.Length == 1)
                trailEndColor = trailStartColor;
            else trailEndColor = clr[1];
        }


        public override void Update(float deltaT)
        {
            trailEnd = _center;
            base.Update(deltaT);
            
            if (!active)
            {
                if (trailDir == Vector2.Zero)
                {
                    trailDir = trailStart;
                    trailDistance = (trailEnd - trailStart);
                    //trailDir.Normalize();
                }
                _velocity = Vector2.Zero;
                _angVel = 0;
                _acceleration = Vector2.Zero;
                _angAcc = 0f;
                if (trailCounter <= particleKillTime)
                {
                    //痕跡の起点を徐々に爆発の点に移動させる
                    //爆発点を超えないように以下のようになっている
                    trailStart = trailDir + trailDistance * ((float)Math.Sin((MathHelper.Pi / 2f) * (1/particleKillTime) * trailCounter++));
                    if ((trailStart - trailEnd).LengthSquared() < 1.5f)
                        SetKillTime(1);

                }
            }
        }

        public new void UpdateVelocities(float deltaT)
        {
            if (active)
                base.UpdateVelocities(deltaT);
        }
        public new void UpdateVelocities()
        {
            UpdateVelocities(dt);
        }
        public VertexPositionColor[] TrailVPC_LINE()
        {
            VertexPositionColor[] v = new VertexPositionColor[2];
            v[0] = new VertexPositionColor(new Vector3(trailStart.X, trailStart.Y, 0f), trailStartColor);
            v[1] = new VertexPositionColor(new Vector3(trailEnd.X, trailEnd.Y, 0f), trailEndColor);
            return v;
        }

        public override void OnDeath()
        {
            base.OnDeath();
        }

        public void DrawTrail(VectorGraphics vectorGraphics)
        {
            switch (vectorGraphics.drawMethod)
            {
                case DrawMethod.LINE:
                    vectorGraphics.AddShape_LINE(TrailVPC_LINE());
                    break;
            }
        }

        public override VertexPositionColor[] ToVPC(Color[] clr)
        {
            switch (shape)
            {
                case Shape.Circle:
                    return base.ToVPC(clr[0]);
                    
            }

            return base.ToVPC(clr);     

        }

        public override void Draw(VectorGraphics vectorGraphics)
        {
            if (IsDead)
                return;
            base.Draw(vectorGraphics);
            if (vectorGraphics.drawMethod == DrawMethod.LINE) { vectorGraphics.AddShape_LINE(TrailVPC_LINE()); }
        }

        //衝突時に数多くの粒子を生み出す
        public override void OnCollision(Vector2 p, float scale = 1f)
        {
            Color[] clrs = new Color[] { Color.Crimson, Color.DarkMagenta, Color.DarkBlue, Color.DarkGreen };
            Vector3 pos = new(p.X, p.Y, _drawPriority + 1);
            Random random = new();
            int n = random.Next(10, 26);
            for (int i = 0; i < n; i++)
            {
                float velo = random.Next(70, 120) * scale;
                float ang = (random.Next(0, 361) / 180f) * (MathHelper.Pi);
                float size = random.Next(4, 10);
                int c = random.Next(4);
                Shape s = random.Next(0, 2) == 0 ? Shape.Triangle : Shape.Square;
                Vector2 vel = new Vector2((float)Math.Cos(ang), (float)Math.Sin(ang)) * velo;
                SpawnParticle(size, pos, vel, -vel * 0.15f, clrs[c], particleKillTime, 25, s);
            }
            active = false;
            _killTime = 70;
            base.OnCollision(p);
        }
    }
}
