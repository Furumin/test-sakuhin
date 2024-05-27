using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ARMAN_DEMO.src
{
    public class PolygonHandler
    {
        protected Polygon[] _polygons;
        protected int MAX_SIZE = 1024;
        public PolygonHandler(Polygon[] polygons)
        {
            _polygons = new Polygon[polygons.Length];
            for (int i = 0; i < polygons.Length; i++)
            {
                _polygons[i] = polygons[i];
            }
        }

        public PolygonHandler()
        {
            _polygons = new Polygon[MAX_SIZE];
        }

        public Polygon[] Polygons
        { get { return _polygons; } }

        //各ポリゴンをアップデートさせ, パラメーターとしてもらった関数を行ってから速度を更新
        //パラメーターの関数が衝突判定関数を前提にしているから、一般的なTick関数としては相応しくないところもある
        public void Tick(Action<Polygon, Polygon, TimeSpan> action, TimeSpan dt, float deltaT, params Polygon[] p)
        {
            for (int i = 0; i < _polygons.Length; ++i)
            {
                if (_polygons[i] == null)
                    continue;

                _polygons[i].Update(deltaT);

                if (_polygons[i].IsDead)
                    continue;

                for (int j = 0; j < p.Length; ++j)
                {
                    if (p[j] == _polygons[i])
                        continue;
                    action(p[j], _polygons[i], dt);
                }

                _polygons[i].ApplyVelocity(deltaT);

            }
        }

        public void Tick(Action<Polygon, Polygon, TimeSpan> action, TimeSpan dt, float deltaT, List <Polygon> p)
        {
            for (int i = 0; i < _polygons.Length; ++i)
            {
                if (_polygons[i] == null)
                    continue;

                _polygons[i].Update(deltaT);

                if (_polygons[i].IsDead)
                    continue;

                for (int j = 0; j < p.Count; ++j)
                {
                    if (p[j] == _polygons[i])
                        continue;
                    action(p[j], _polygons[i], dt);
                }

                _polygons[i].ApplyVelocity(deltaT);

            }
        }

        //public void Tick(Action<Polygon, Polygon, TimeSpan> action, TimeSpan dt, float deltaT, List<Polygon> p)
        //{
        //    Tick(action, dt, deltaT, p.ToArray());
        //}

        public void AddPolygon(Polygon p)
        {
            for (int i = 0; i < _polygons.Length; i++)
            {
                if (_polygons[i] != null)
                    continue;
                _polygons[i] = p;
                return;
            }
        }
    }
}
