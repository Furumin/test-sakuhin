using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.DirectoryServices.ActiveDirectory;
using System.Xml.Serialization;
using ARMAN_DEMO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SharpDX.WIC;
using static ARMAN_DEMO.GameMathematics;

namespace ARMAN_DEMO
{

    public struct RenderInfo
    {
        int effect;

        public RenderInfo()
        {
            effect = 0;
        }
        public RenderInfo(int b)
        {
            effect = b;
        }

        public int Effect { get { return effect; } }   
    }
    public class Polygon
    {
        [Flags]
        public enum DrawMode
        {
            LINE = 1,
            FILL = 2
        };

        private DrawMethod _drawMode;

        protected MainGame game;

        public Vector2[] Vertices;

        protected Vector2[] _normals;
        public Vector2 _center;

        protected Vector2 _velocity;
        public Vector2 _finalVelocity;
        protected Vector2 _acceleration;
        public Vector2 _finalAcceleration;

        public float _angVel, _angAcc;

        public float _orientation;

        public float _mass;
        public Vector2 _edgeN;
        public float _drawPriority;

        public float _centerInertia;

        protected Color[] _edgeColors, _fillColors;

        protected float dt;
        protected float _killTime;
        private bool _isDead;

        protected RenderInfo _renderInfo;

        public bool drawable;

        public Vector2 Center
        {
            get { return _center; }
            set 
            { 
                for (int i = 0; i < Vertices.Length; ++i) 
                { 
                    Vertices[i] += value - _center; 
                } 
                _center = value;
            }
        }

        public RenderInfo RenderInfo { get { return _renderInfo; } }   

        public bool IsDead
        {
            get { return _isDead; }
        }

        public float KillTime
        {
            get { return _killTime; }
        }

        public Vector2 Velocity
        {
            get { return _velocity; }
            set { _velocity = value; }
        }

        public float AngularVelocity
        {
            get { return _angVel; }
            set { _angVel = value; }
        }
        public float AngularAcceleration
        {
            get { return _angAcc; }
            set { _angAcc = value; }
        }

        public int VertexCount { get { return Vertices.Length; } }

        public DrawMethod RenderMode { get { return _drawMode; } }

        /// <summary>
        /// 様々なデフォルト数値で変数をイニシャライズして、頂点を設定する
        /// </summary>
        /// <param name="points"></param>
        /// <param name="center"></param>
        public Polygon(Vector2[] points, Vector2 center, MainGame game)
        {
            this.game = game;
            Vertices = points;
            this._center = center;
            _velocity = Vector2.Zero;
            _finalVelocity = Vector2.Zero;
            _acceleration = Vector2.Zero;
            _finalAcceleration = Vector2.Zero;
            _mass = 1f;

            _angVel = 0f;
            _angAcc = 0f;

            _orientation = MathHelper.Pi / 2f;

            _drawPriority = 0f;
            _centerInertia = 50f;


            _normals = new Vector2[Vertices.Length];
            for (int i = 0; i < Vertices.Length; i++)
            {
                int j = (i + 1) % Vertices.Length;
                _normals[i] = new Vector2(Vertices[j].Y - Vertices[i].Y, -(Vertices[j].X - Vertices[i].X));
            }

            _edgeColors = new Color[Vertices.Length];
            _fillColors = new Color[Vertices.Length];

            dt = 0f;
            _killTime = -1;

            _drawMode = DrawMethod.FILL;
            drawable = true;
            RecalculateNormals();
        }

        ~Polygon()
        {
            Vertices = null;
            _edgeColors = null;
            _fillColors = null;
            _normals = null;
            //Debug.WriteLine("Object destroyed!");
        }


        public void RecalculateCenter()
        {
            Vector2 c = Vector2.Zero;
            for (int i = 0; i < Vertices.Length; ++i)
            {
                c += Vertices[i];
            }
            c /= VertexCount;
            this._center = c;
        }

        public virtual void UpdatePositions()
        {
            RecalculateNormals();
        }

        //このオブジェクトは死んでいるかをチェック
        public virtual void Update(float deltaT)
        {
            dt = deltaT;
            if (_killTime > 0)
                _killTime--;
            if (_killTime == 0 && !IsDead)
            {
                _killTime-=2;
                _isDead = true;
                OnDeath();
            }
            UpdateVelocities(deltaT);
        }

        //時間の変数を更新するかをチェック
        public void CheckNewDT(float deltaT)
        {
            if (dt >= deltaT)
                dt = deltaT;
        }

        public void UpdateVelocities(float deltaT)
        {
            UpdateLinearVelocity(deltaT);
            UpdateAngularVelocity(deltaT);
        }

        public void UpdateVelocities()
        {
            UpdateLinearVelocity();
            UpdateAngularVelocity();
        }

        private void UpdateLinearVelocity(float deltaT)
        {
            _velocity += deltaT * _acceleration;
        }
        private void UpdateLinearVelocity()
        {
            UpdateLinearVelocity(dt);
        }

        private void UpdateAngularVelocity(float deltaT)
        {
            if (_angAcc > 0)
            {

            }
            _angVel += deltaT * _angAcc;
        }
        private void UpdateAngularVelocity()
        {
            UpdateAngularVelocity(dt);
        }

        //当ポリゴンを移動させる
        //directionは目的位置ではなく、移動する距離をさす
        public void Push(Vector2 direction)
        {
            for (int i = 0; i < Vertices.Length; i++)
            {
                Vertices[i] += direction;
            }

            _center += direction;
            UpdatePositions();
        }
        public void Push(float dt)
        {
            for (int i = 0; i < Vertices.Length; i++)
            {
                Vertices[i] += _velocity*dt;
            }

            _center += _velocity*dt;
            UpdatePositions();
        }

        public void RecalculateNormals()
        {
            for (int i = 0; i < Vertices.Length; i++)
            {
                int j = (i + 1) % Vertices.Length;
                _normals[i] = new Vector2(Vertices[j].Y - Vertices[i].Y, -(Vertices[j].X - Vertices[i].X));
            }
        }

        public virtual void Rotate(float theta)
        {
            Rotate(theta, _center);
        }

        public void ApplyVelocity(float dt)
        {
            ApplyLinearVelocity(dt);
            ApplyAngularVelocity(dt);
            UpdatePositions();
        }
        public void ApplyVelocity()
        {
            ApplyLinearVelocity(dt);
            ApplyAngularVelocity(dt);
            UpdatePositions();
        }

        public void ApplyLinearVelocity(float dt)
        {
            Push(dt);
        }

        public void ApplyAngularVelocity(float dt)
        {
            Rotate(_angVel * dt);
            _orientation += _angVel * dt;
        }

        public void ApplyAngularAcceleration(float dt)
        {
            _angVel += _angAcc * dt;
        }

        // velocity in point p is body velocity + (ang vel * dist from center to p)
        public Vector2 GetPointVelocity(Vector2 p)
        {
            Vector2 pointAngV = AngularPointVelocity(p);
            return pointAngV + _velocity;
        }

        public Vector2 AngularPointVelocity(Vector2 p)
        {
            Vector2 r = p - _center;
            r = new Vector2(-r.Y, r.X);
            return _angVel * r;
        }

        //find this polygons position after timestep dt
        //given current velocity
        //速度を現在の位置に足し、次のフレームに
        //このポリゴンの位置を想定するメソッド
        public Polygon GetLinearPrediction(float dt)
        {
            Vector2[] vec = new Vector2[VertexCount];
            for (int i = 0; i<VertexCount; i++)
                vec[i] = Vertices[i];
            Polygon p = new(vec, _center, game);
            p._velocity = _velocity; p._acceleration = _acceleration;
            p._angVel = _angVel; p._angAcc = _angAcc;
            p._mass = _mass;
            p._edgeN = _edgeN;
            p._centerInertia = _centerInertia;
            p.ApplyVelocity(dt);
            return p;
        }

        /// <summary>
        /// ポリゴンを時計回りに回す
        /// </summary>
        /// <param name="theta"></param>
        /// <param name="anchor"></param>
        public virtual void Rotate(float theta, Vector2 anchor)
    {
        float diffX, diffY, cosTheta, sinTheta;
        for (int i = 0; i < Vertices.Length; i++)
        {
            diffX = Vertices[i].X - anchor.X;
            diffY = Vertices[i].Y - anchor.Y;

            cosTheta = (float)Math.Cos(theta);
            sinTheta = (float)Math.Sin(theta);

            Vertices[i] = new Vector2(
                cosTheta * diffX - sinTheta * diffY + anchor.X,
                sinTheta * diffX + cosTheta * diffY + anchor.Y);

        }
        diffX = _center.X - anchor.X;
        diffY = _center.Y - anchor.Y;

        cosTheta = (float)Math.Cos(theta);
        sinTheta = (float)Math.Sin(theta);

        _center = new Vector2(
            cosTheta * diffX - sinTheta * diffY + anchor.X,
            sinTheta * diffX + cosTheta * diffY + anchor.Y);
        UpdatePositions();

        _edgeN = new Vector2(
            cosTheta * _edgeN.X - sinTheta * _edgeN.Y,
            sinTheta * _edgeN.X + cosTheta * _edgeN.Y);
    }


        public bool PointOnVertex(Vector2 p, ref int index)
        {
            for (int i = 0; i < Vertices.Length; i++)
            {
                float dist = Math.Abs((p - Vertices[i]).LengthSquared());
                if (dist < 0.3f)
                {
                    index = i;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Caps thsi polygon's velocity to the given length.
        /// </summary>
        /// <param name="cap"></param>
        public void CapVelocity(float cap)
        {
            if (_velocity.LengthSquared() <= cap * cap)
                return;
            _velocity.Normalize();
            _velocity *= cap;
        }
        public void CapAcceleration(float cap)
        {
            if (_acceleration.LengthSquared() <= cap * cap)
                return;
            _acceleration.Normalize();
            _acceleration *= cap;
        }

        public void CapAngularVelocity(float cap)
        {
            if (_angVel > cap)
                _angVel = cap;
            else if (_angVel < -cap)
                _angVel = -cap;
        }

        public int[] TriangulatedIndex(Vector2 verts)
        {
            throw new ArgumentException("Not implemented yet!");
        }

        public VertexPositionColor[] ToQuadVPC()
        {
            VertexPositionColor[] v = new VertexPositionColor[Vertices.Length];
            for (int i = 0; i < Vertices.Length; i++)
            {
                v[i] = new VertexPositionColor(new Vector3(Vertices[i].X, Vertices[i].Y, _drawPriority), Color.Yellow);
            }
            return v;
        }
        
        //Vector2である頂点をVertexPositionColorに変換する
        //グラフィックデバイスは、Vector2を頂点データーとして認めないのでVPCに変換する必要あり
        public virtual VertexPositionColor[] ToVPC(Color[] clr)
        {
            if (VertexCount != clr.Length)
                throw new ArgumentException("Wrong size of color array!");
            VertexPositionColor[] v = new VertexPositionColor[Vertices.Length];
            for (int i = 0; i < Vertices.Length; i++)
            {
                v[i] = new VertexPositionColor(new Vector3(Vertices[i].X, Vertices[i].Y, _drawPriority), clr[i]);
            }
            return v;
        }
        public VertexPositionColor[] ToVPC(Color clr)
        {
          
            VertexPositionColor[] v = new VertexPositionColor[Vertices.Length];
            for (int i = 0; i < Vertices.Length; i++)
            {
                v[i] = new VertexPositionColor(new Vector3(Vertices[i].X, Vertices[i].Y, _drawPriority), clr);
            }
            return v;
        }


        public virtual void Draw(VectorGraphics vectorGraphics)
        {
            if (!drawable)
                return;
            switch (vectorGraphics.drawMethod)
            {
                case DrawMethod.LINE:
                    vectorGraphics.AddShape_LINE(ToVPC(_edgeColors));
                    break;
                case DrawMethod.FILL:
                    if (VertexCount == 3)
                        vectorGraphics.AddTriangle(ToVPC(_fillColors));
                    else if (VertexCount == 4) vectorGraphics.AddQuad(ToVPC(_fillColors));
                    else vectorGraphics.AddShape(ToVPC(_fillColors));
                    break;
            }
        }

        public void SetRenderInfo(int bloom)
        {
            _renderInfo = new RenderInfo(bloom);
        }

        public void SetDrawMode(DrawMethod mode)
        {
            _drawMode = mode;
        }

        public void Scale(float factor)
        {
            for (int i = 0; i < VertexCount; i++)
            {
                Vertices[i] = _center + ((Vertices[i] - _center) * factor);
            }

            RecalculateNormals();
        }

        public void SpawnParticle(float size, Vector3 position, Vector2 velocity, 
            Vector2 acceleration, Color color, float killTime, float angularVel = 0f, Shape shape = Shape.Square, int renderEffect = 1)
        {
            Vector2 position2D = new(position.X, position.Y);
            Vector2 vertex = position2D + new Vector2(0, size);
            Color[] clrs = new Color[3];
            float rotate = 0f;
            float start = MathHelper.Pi / 2f;
            int n = 0;
            //パラメーターで指定された形によって、当粒子のデーターを決める
            switch(shape)
            {
                case Shape.Triangle:
                    n = 3;
                    rotate = (MathHelper.Pi * 2f) / 3f;
                    clrs = new Color[]
                    {
                        color,
                        color,
                        color,
                    };
                    break;
                case Shape.Square:
                    n = 4;
                    rotate = (MathHelper.Pi) / 2f;
                    clrs = new Color[]
                    {
                        color,
                        color,
                        color,
                        color
                    };
                    break;
            }
            List<Vector2> vertices = new();
            for (int i = 0; i < n; i++)
            {
                Vector2 v = RotatedVector(vertex, position2D, start + rotate * i);
                vertices.Add(v);
            }
            Vector2[] pos = vertices.ToArray();
            Polygon p = new(pos, position2D, game);
            p.AngularVelocity = angularVel;
            p._velocity = velocity;
            p._acceleration = acceleration;
            p.SetFillColors(clrs);
            p.SetLineColors(clrs);
            p.SetRenderInfo(renderEffect);
            p.SetDrawMode(DrawMethod.FILL);
            p.SetKillTime(killTime);

            //particles.Add(p);
            game?.AddParticle(p);
        }

        public void SetKillTime(float time)
        {
            _killTime = time;
        }

        public void SetLineColors(Color[] clrs)
        {
            _edgeColors = clrs;
        }

        public void SetFillColors(Color[] clrs)
        {
            _fillColors = clrs;
        }
        public void SetFillColors(Color clr)
        {
            Color[] c = new Color[VertexCount];
            for (int i = 0; i < VertexCount; i++)
                c[i] = clr;
            _fillColors = c;
        }
        public void SetLineColors(Color clr)
        {
            Color[] c = new Color[VertexCount];
            for (int i = 0; i < VertexCount; i++)
                c[i] = clr;
            _edgeColors = c;
        }

        public virtual void OnDeath()
        {

        }

        public virtual void OnCollision(Vector2 p, float scale = 1f)
        {

        }

        public virtual void OnHit(Vector2 p, DamageInformation info)
        {

        }

        public virtual bool CanCollide(Polygon p)
        {
            return true;
        }
    }

    //struct OBB
    //{
    //    Vector2 c;
    //    Vector2 e;
    //    Vector2[] u;
    //    public OBB(Vector2 c, Vector2 e, Vector2[] u)
    //    {
    //        this.c = c;
    //        this.u = u;
    //        this.e = e;
    //    }
    //}
}
