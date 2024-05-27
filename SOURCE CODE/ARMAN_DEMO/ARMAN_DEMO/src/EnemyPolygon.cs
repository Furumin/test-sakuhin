using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;


namespace ARMAN_DEMO
{
    //State Machineによって行動がきまる、ごく簡単な敵AI
    public class EnemyPolygon : Polygon
    {
        Color _edgeColor = Color.Purple;
        Color _fillColor = Color.Coral;

        short _shootCooldown = 20;
        float _accelerationSpeed = 30f;

        public enum MovementState
        {
            Spawning,
            Still,
            Moving,
            Stunned
        }
        private MovementState currentMovementState, previousMovementState;

        Cube drawCube;
        Vector2 movementTarget;
        Vector2 movementDirection;
        int stateCounter;
        public EnemyPolygon(Vector2[] dots, Vector2 c, MainGame game) : base(dots, c, game)
        {
            _drawPriority = 5f;
            drawCube = new Cube(new Vector3(c.X, c.Y, _drawPriority), Math.Abs(dots[0].X - c.X));

            stateCounter = 0;

            previousMovementState = MovementState.Spawning;
            currentMovementState = MovementState.Spawning;

            SetDrawMode(DrawMethod.FILL | DrawMethod.LINE);
            SetRenderInfo(0);
            drawCube.SetEdgeColor(_edgeColor);
            drawCube.SetFillColor(_fillColor);
        }

        public override void Update(float deltaT)
        {
            if (IsDead) 
            {
                base.Update(deltaT);
                return;
            }

            previousMovementState = currentMovementState;
            ++stateCounter;
            switch(currentMovementState)
            {
                case MovementState.Spawning:
                    SpawningUpdate();
                    break;
                case MovementState.Still:
                    StillUpdate();
                    break;
                case MovementState.Moving:
                    MovingUpdate();
                    break;
                case MovementState.Stunned:
                    StunnedUpdate();
                    break;
            }


            base.Update(deltaT);

            CapVelocity(600);
            CapAngularVelocity(3);

            if (currentMovementState != previousMovementState)
                stateCounter = 0;
        }

        private void SpawningUpdate()
        {
            if (stateCounter > 120)
                currentMovementState = MovementState.Still;
        }

        private void StillUpdate()
        {
            if (stateCounter % _shootCooldown == 0)
            {
                Shoot(game.Player);
            }
            if (stateCounter >= 120)
            {
                currentMovementState = MovementState.Moving;
                RandomizeMovementTarget(game.ArenaBoundary);
                SetMovementDirection(movementTarget);
            }

            if (_velocity != Vector2.Zero)
            {
                _acceleration = -_velocity * 20f;
            }
        }

        private void MovingUpdate()
        {
            MoveToTarget(_accelerationSpeed);
            drawCube.RotateY(_velocity.Length() / 10000f);
            if (stateCounter % _shootCooldown*2 == 0)
            {
                Shoot(game.Player);
            }
            if (Math.Abs((this._center - movementTarget).LengthSquared()) < 625f)
                currentMovementState = MovementState.Still;
        }

        private void StunnedUpdate()
        {
            if (stateCounter >= 90)
            {
                currentMovementState = MovementState.Still;
                drawCube.RotateX(0.1f);
                drawCube.RotateZ(0.1f);
            }
        }

        //弾丸のデータを設定して打つ
        private void Shoot(ControlPolygon c)
        {
            Vector2[] temp = new Vector2[1];
            Projectile p = new(temp, 5, _center, Shape.Square, game);
            Random r = new();
            Vector2 offset = new(r.Next(0, 40) - 20, r.Next(0, 60) - 30);
            Vector2 vel = (c._center - this._center) + offset;
            vel.Normalize();
            vel *= 800f;
            p.Velocity = vel;
            p.AngularVelocity = 7f;
            p.SetFillColors(Color.Crimson);
            p.SetLineColors(Color.MediumPurple);
            p._mass = 1000000f;
            p._centerInertia = 100000f;
            p.data.knockback = 0.8f;
            p.SetTrailColors(new Color[] { Color.MediumPurple, Color.Crimson });
            p.SetKillTime(200);
            p.SetRenderInfo(1);
            p.data.friendly = false;

            game.AddProjectile(p);
            game.PlaySFX(0);
        }

        public override void UpdatePositions()
        {
            drawCube.Translate(new Vector3(_center.X, _center.Y, _drawPriority) - drawCube.Center);
        }

        public override void Draw(VectorGraphics vectorGraphics)
        {
            if (IsDead) return;
            switch (currentMovementState)
            {
                //case MovementState.Spawning:
                //    DrawSpawnEffect(vectorGraphics, 0);
                //    break;
                default:
                    drawCube.Draw(vectorGraphics);
                    break;

            }
        }

        //未実装
        private void DrawSpawnEffect(VectorGraphics vectorGraphics, int counter)
        {
            int zOffset = (int)_drawPriority + 35;
            Color spawnColor = Color.White;
            for(int i = 0; i < Vertices.Length; ++i)
            {
                Vector2 dot1 = Vertices[i];
                Vector2 dot2 = Vertices[(i + 1) % Vertices.Length];

                Vector3 vert1 = new(dot1, _drawPriority);
                Vector3 vert2 = new(dot2, _drawPriority);
                Vector3 vert3 = new(dot1.X, dot1.Y + 35, zOffset);
                Vector3 vert4 = new(dot2.X, dot2.Y + 35, zOffset);

                vectorGraphics.AddShape(new VertexPositionColor[] {
                    new VertexPositionColor(vert4, spawnColor),
                    new VertexPositionColor(vert2, spawnColor),
                    new VertexPositionColor(vert1, spawnColor),
                    new VertexPositionColor(vert3, spawnColor)});
            }

        }

        public override void OnCollision(Vector2 p, float scale = 1)
        {
            base.OnCollision(p, scale);
            _acceleration = Vector2.Zero;
            currentMovementState = MovementState.Stunned;
        }

        //死ぬと粒子を飛ばす
        public override void OnDeath()
         {
            base.OnDeath();
            Color[] clrs = new Color[] { Color.White, Color.Red, Color.White, Color.Red, Color.White };
            Vector3 pos = new(_center.X, _center.Y, _drawPriority + 1);
            Random random = new();
            int n = 60;
            for (int i = 0; i < n; i++)
            {
                float velo = random.Next(390, 490) * 0.5f;
                float ang = (random.Next(0, 361) / 180f) * (MathHelper.Pi);
                float size = random.Next(15, 25);
                int c = random.Next(4);
                Vector2 vel = new Vector2((float)Math.Cos(ang), (float)Math.Sin(ang)) * velo;
                SpawnParticle(size, pos, vel, -vel * 0.25f, clrs[c], 120, 1, Shape.Triangle);
            }
        }

        private void RandomizeMovementTarget(Rectangle boundary)
        {
            Random r = new();
            movementTarget.X = r.Next(boundary.Left+150, boundary.Right-150);
            movementTarget.Y = r.Next(boundary.Top+100, boundary.Bottom-100);
        }

        public void SetMovementDirection(Vector2 target)
        {
            Vector2 dist = target - _center;
            dist.Normalize();
            movementDirection = dist;
        }

        private void MoveToTarget(float accSpeed)
        {
            //試行錯誤で以下の数字を選んだ
            _acceleration += movementDirection * accSpeed;
            if (Math.Abs((this._center - movementTarget).LengthSquared()) < 1600f)
                _acceleration *= _acceleration / 10f;
        }

        public override void Rotate(float theta)
        {
            base.Rotate(theta);
            drawCube.RotateX(theta/3F);
        }
    }
}
