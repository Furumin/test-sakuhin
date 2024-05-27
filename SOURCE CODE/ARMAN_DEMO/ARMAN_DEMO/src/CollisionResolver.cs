using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ARMAN_DEMO.src
{
    public class CollisionResolver
    {
        CollisionHandler handler;
        public CollisionResolver() { handler = new CollisionHandler(); }

        //とある二つのポリゴンの衝突判定を行う
        public void CollisionUpdate(Polygon a, Polygon b, TimeSpan dt)
        {
            if (a == null || b == null) return;
            if (!a.CanCollide(b)) return;
            float tA;
            Contact _contact = new();
            handler.CheckPolygonCollision(a, b, dt, _contact, out tA);
            
            if (tA > 0f)
            {
                a.CheckNewDT(tA);
                b.CheckNewDT(tA);
            }
            //共通面積がどうしてもあったのでどれかを爆発させる
            //この処理を衝突判定システムのクラスの中で行うのは良くないけど
            //時間がないのでこのままにしておいてしまった。
            else if (tA == -1)
            {
                if (a._mass > b._mass)
                    b.SetKillTime(1);
                else if (b._mass > a._mass)
                    a.SetKillTime(1);
            }
        }

        //とあるポリゴンと弾丸の衝突判定をおこなう
        public void ProjectileCollisionUpdate(Polygon a, Projectile b)
        {
            DamageInformation info;
            handler.CheckProjectileCollision(a, b, out info);
        }
    }

}
