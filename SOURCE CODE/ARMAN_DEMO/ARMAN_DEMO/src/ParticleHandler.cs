using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace ARMAN_DEMO.src
{
    public class ParticleHandler
    {
        Polygon[] particles;

        const uint maxSize = 1024;
        uint active;

        public ParticleHandler()
        {
            particles = new Polygon[maxSize];
            active = 0;
            Initialize();
        }

        private void Initialize()
        {
            for (int i = 0; i < particles.Length; i++)
            {
                //念のため頂点配列に一個だけ入れる
                particles[i] = new Polygon(new Vector2[]{ Vector2.Zero }, Vector2.Zero, null);
                particles[i].drawable = false;
                particles[i].SetKillTime(1);
            }
        }

        public IEnumerable<Polygon> Alive()
        {
            for (int i = 0; i < particles.Length; i++)
            {
                if (particles[i] != null)
                {
                    if (!particles[i].IsDead)
                        yield return particles[i];
                }
            }
        }

        //基本的にチェックすることは、各粒子の寿命をチェックして、
        //排除するかどうかを決めるだけ
        public void Update(float deltaT)
        {
            //liveParticles = new List<Polygon>();
            active = 0;
            for (int i = 0; i < particles.Length; ++i)
            {
                if (particles[i] == null)
                    continue;
                if (!particles[i].IsDead)
                {
                    //liveParticles.Add(particles[i]);
                    particles[i].Update(deltaT);
                    particles[i].ApplyVelocity();
                    if (particles[i].KillTime < 40)
                        particles[i].Scale(0.93f);
                    if ((particles[i].Vertices[0] - particles[i]._center).LengthSquared() < 0.0025f)
                        particles[i].SetKillTime(1);
                    ++active;
                }

                else
                {

                }
            }
        }

        //public void Draw(VectorGraphics graphics)
        //{
        //    foreach(int index in toDraw)
        //    {
        //        if (particles[index] == null)
        //            continue;
        //        if (particles[index].IsDead)
        //            continue;
        //        particles[index].Draw(graphics);
        //    }
        //}

        public bool Empty()
        {
            return active == 0; 
        }

        //使われていない最初のindexをもとめる
        public int FirstInactive()
        {
            for(int i = 0; i < particles.Length; ++i)
            {
                if (particles[i] == null || particles[i].IsDead)
                    return i;
            }

            return 0;
        }

        public Polygon FirstOpenSlot
        {
            get { return particles[FirstInactive()]; }
            set
            {
                particles[FirstInactive()] = value;
                
            }
        }
    }
}
