using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace ARMAN_DEMO.src
{
    public class ProjectileHandler : PolygonHandler
    {
        Projectile[] projectiles;
        new int MAX_SIZE = 256;

        public Projectile[] Projectiles { get { return projectiles; } }

        public ProjectileHandler(Projectile[] p) : base(p)
        {
            projectiles = new Projectile[MAX_SIZE];
            Initialize();
        }

        private void Initialize()
        {
            for(int i = 0; i < MAX_SIZE; ++i)
            {
                projectiles[i] = new Projectile(new Vector2[] { Vector2.Zero }, 0.0f, Vector2.Zero, Shape.Triangle, null);
                projectiles[i].drawable = false;
                projectiles[i].SetKillTime(1);
            }
        }

        public IEnumerable<Polygon> Alive()
        {
            for (int i = 0; i < MAX_SIZE; ++i)
            {
                if (projectiles[i] != null)
                {
                    if (!projectiles[i].IsDead)
                        yield return projectiles[i];
                }
            }
        }

        public void AddProjectile(Projectile p)
        {
            for (int i = 0; i < projectiles.Length; i++)
            {
                if (!projectiles[i].IsDead)
                    continue;
                projectiles[i] = p;
                return;
            }
        }

        public void Tick(Action<Polygon, Projectile> action, float deltaT, params Polygon[] p)
        {
            for (int i = 0; i < projectiles.Length; ++i)
            {
                if (projectiles[i].IsDead)
                    continue;
                projectiles[i].Update(deltaT);
                if (projectiles[i].Active)
                {
                    for (int j = 0; j < p.Length; ++j)
                    {
                        action(p[j], projectiles[i]);
                    }
                    projectiles[i].ApplyVelocity(deltaT);

                }
                //else if (projectiles[i].IsDead)
                //    projectiles[i] = null;
            }
        }

        //public void Tick(Action<Polygon, Projectile> action, float deltaT, List<Polygon> p)
        //{
        //    Tick(action, deltaT, p.ToArray());
        //}

    }
}
