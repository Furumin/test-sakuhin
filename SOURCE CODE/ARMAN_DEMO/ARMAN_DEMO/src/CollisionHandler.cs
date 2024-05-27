using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using static ARMAN_DEMO.GameMathematics;

namespace ARMAN_DEMO.src
{
    public class CollisionHandler
    {
        //弾丸がつかう衝突判定システム。共通面積が存在してもかまわないので計算がかなりたやすくなる
        public int CheckProjectileCollision(Polygon a, Projectile b, out DamageInformation info)
        {
            info = b.data;
            if (a.IsDead || b.IsDead) return -1;

            if (a is ControlPolygon && b.data.friendly || a is EnemyPolygon && !b.data.friendly)
                return -1;

            for (int i = 0; i < b.VertexCount; i++)
            {
                //弾丸の頂点のいずれかがポリゴンaに入っていればOK
                if (PointInPolygon(b.Vertices[i], a.Vertices) == 1)
                {
                    Contact c = new();
                    c.A = a;
                    c.B = b;
                    c.P = b.Vertices[i];
                    c.N = -b.Velocity;
                    if (c.N == Vector2.Zero)
                        c.N = -a.Velocity;
                    c.N.Normalize();

                    ApplyImpulse(a, b, c);
                    a.OnCollision(c.P);
                    b.OnCollision(c.P);
                    a.OnHit(c.P, b.data);
                    return 1;
                }
            }
            return 0;
        }

        //衝突が起きたかをチェックして、もしおきたら接点の情報を収集
        public void CollisionDetection(Polygon a, Polygon b, ref Contact c, ref float t, TimeSpan dt)
        {
            float deltaT = (float)dt.TotalMilliseconds / 1000f;
            float tContact = deltaT;
            CollisionCheck(a, b, deltaT, ref tContact, ref c);
            t = tContact;
        }

        public void ApplyImpulse(Polygon a, Projectile b, Contact contact)
        {
            ApplyImpulse(a, b, b.data.knockback, contact);
        }
 
        //両ポリゴンに力積の計算を行う
        public void ApplyImpulse(Polygon a, Polygon b, float e, Contact contact)
        {

            Vector2 relVel = a.GetPointVelocity(contact.P) - b.GetPointVelocity(contact.P);
            relVel.Normalize();
            if (!(a is Projectile || b is Projectile))
            {
                if (DotProduct(relVel, contact.N) >= 0.01)
                    return;
            }
            float j = ComputeImpulseJ(e, contact);


            Vector2 r_ap = contact.P - a._center;
            r_ap = new Vector2(-r_ap.Y, r_ap.X);

            Vector2 r_bp = contact.P - b.Center;
            r_bp = new Vector2(-r_bp.Y, r_bp.X);
            a.Velocity += (j / a._mass) * contact.N;
            a.AngularVelocity += (DotProduct(r_ap, j * contact.N) / a._centerInertia);
            if (contact.TYPE is Contact.ContactType.FF)
            {
                a.AngularVelocity = 0f;
                b.AngularVelocity = 0f;
            }

            b.Velocity -= (j / b._mass) * contact.N;
            b.AngularVelocity -= (DotProduct(r_bp, j * contact.N) / b._centerInertia);

        }

        //二つのポリゴンが衝突しているかをチェックする
        //順番・ CollisionDetection() -> CheckIntersection() -> CollisionCheck() ->
        //(共通面積０なるまでに) CollisionCheck()でCheckIntersection()を呼び返す

        public void CheckPolygonCollision(Polygon a, Polygon b, TimeSpan dt, Contact con, out float tCon)
        {
            float deltaT = (float)dt.TotalMilliseconds / 1000f;
            //衝突が起こりえない場合、判定に必要な計算を処理する必要がない
            if ((a == null || b == null) || a.IsDead || b.IsDead)
            {
                tCon = deltaT;
                return;
            }

            Contact c = new();
            float tCandidate = deltaT;

            CollisionDetection(a, b, ref c, ref tCandidate, dt);

            deltaT = tCandidate;
            float smallestT = deltaT;

            //接点発見？
            if (c.A != null)
            {
                float e = 0.4f;
                float velForMaxExplosion = 150000f;

                //ポリゴンの衝突反応関数のパラメーターとして使われるscale
                //これを使って、衝突したポリゴンは粒子の早さなどを調整することができる
                //たとえば、衝突時スピードが早かった場合、粒子の数を増やしたりとか
                float scale = (float)Math.Abs((c.A.Velocity - c.B.Velocity).LengthSquared()) / velForMaxExplosion;
                scale = Clamp(scale, 0.2f, 1.5f);

                ApplyImpulse(a, b, e, c);
                if (a is ControlPolygon) ((ControlPolygon)a).StunCounter = 40;

                a.OnCollision(c.P, scale);
                b.OnCollision(c.P, scale);
            }

            tCon = smallestT;
        }
 
        // 力積を決めるために使われるJの変数を計算
        private float ComputeImpulseJ(float e, Contact c)
        {
            Vector2 v1 = c.A.Velocity + c.A.AngularPointVelocity(c.P); Vector2 v2 = c.B.Velocity + c.B.AngularPointVelocity(c.P);
            Vector2 n = c.N;
            float m1 = c.A._mass, m2 = c.B._mass;
            Vector2 r_ap = c.P - c.A._center, r_bp = c.P - c.B._center;
            r_ap = new Vector2(-r_ap.Y, r_ap.X);
            r_bp = new Vector2(-r_bp.Y, r_bp.X);

            float down2 = (DotProduct(r_ap, n) * DotProduct(r_ap, n)) / c.A._centerInertia;
            float down3 = (DotProduct(r_bp, n) * DotProduct(r_bp, n)) / c.B._centerInertia;

            Vector2 rel_speed = v1 - v2;
            float up = -(1 + e) * DotProduct(rel_speed, n);
            float down = DotProduct(n, n * ((1f / m1) + (1f / m2))) + down2 + down3;
            return up / down;
        }

        //まず共通分があるかを確認してから、衝突しているかをチェックする関数
        public int CheckIntersection(Polygon pol1, Polygon pol2, ref Contact contact)
        {
            int I = -1;
            Polygon p1 = pol1;
            Polygon p2 = pol2;

            //まずp1の頂点がp2に入っているかをチェックしてから、逆のケースもチェック
            for (int shape = 0; shape < 2; shape++)
            {
                if (shape == 1)
                {
                    p1 = pol2;
                    p2 = pol1;
                }
                //check the vertices in p1
                for (int p = 0; p < p1.Vertices.Length; p++)
                {
                    Vector2 v = p1.Vertices[p];

                    int t = PointInPolygon(v, p2.Vertices);
                    if (t > 0)
                    {
                        //共通面積発見
                        contact.A = pol1;
                        contact.B = pol2;
                        contact.P = v;
                        float dist;
                        Vector2 norm = ClosestPointPolygonSide(v, p2, out dist);
                        norm = new Vector2(norm.Y, -norm.X);
                        norm.Normalize();
                        contact.N = norm;
                        //if (dist < 0.5) return 0;
                        return 1;
                    }
                    else if (t == 0)
                    {

                    }
                }
            }

            
            FindCollisionContact(pol1, pol2, ref contact);
            if (contact.A != null)
            {
                //衝突しているが共通面積が０
                return 0;
            }
            return I;
        }

        //ある頂点に一番近い、とあるポリゴンの側面を求める関数
        public Vector2 ClosestPointPolygonSide(Vector2 p, Polygon pol, out float dist)
        {
            dist = float.PositiveInfinity;
            Vector2 closest = Vector2.Zero;
            Vector2 proj = Vector2.Zero;
            for (int i = 0; i < pol.VertexCount; i++)
            {
                int a = i, b = (i + 1) % pol.VertexCount;
                float currentDist = Math.Min(dist, SqDistPointSegment(pol.Vertices[a], pol.Vertices[b], p, ref proj));
                if (currentDist < dist)
                {
                    closest = pol.Vertices[b] - pol.Vertices[a];
                    dist = currentDist;
                }

            }
            if (dist == float.PositiveInfinity)
                throw new ArgumentException("something went wrong!");
            return closest;
        }

        //衝突（共通面積０のケースのみ）を判定する関数
        public void FindCollisionContact(Polygon pol1, Polygon pol2, ref Contact contact)
        {
            List<Contact> contacts = new();
            Polygon p1 = pol1, p2 = pol2;
            //まずp1の頂点がp2に入っているかをチェックしてから、逆のケースもチェック
            for (int a = 0; a < 2; a++)
            {
                int i = a;
                if (i == 1)
                {
                    p1 = pol2;
                    p2 = pol1;
                }

                for (int j = 0; j < p1.Vertices.Length; j++)
                {
                    Vector2 v = p1.Vertices[j];
                    Vector2 n = Vector2.Zero;
                    int DotCollisionIndex = -1;
                    int getSide = GetSide(v, p2, ref n, ref DotCollisionIndex);

                    //共通部分が０の衝突が判定されたらtrueになる
                    if (getSide > 0)
                    {
                        n = new Vector2(n.Y, -n.X);
                        if (i == 1)
                            n = -n;

                        Contact c = new();
                        c.A = pol1;
                        c.B = pol2;
                        c.P = v;
                        c.isVFContact = getSide == 2 ? false : true;

                        c.TYPE = Contact.ContactType.VF;

                        c.N = n;
                        c.N.Normalize();
                        contacts.Add(c);
                        contact = c;
                        //break;
                    }
                }

                if (contacts.Count() > 1)
                {
                    if ((contacts[0].P - contacts[1].P).Length() > 2f)
                    {
                        contact.TYPE = Contact.ContactType.FF;
                    }
                }
            }
        }

        //pTest1, pTest2によるGCの発動がかなり頻繁である
        private void CollisionCheck(Polygon p1, Polygon p2, float t1, ref float tContact, ref Contact contact)
        {
            float t0 = 0;
            tContact = t1;
            int firstI = CheckIntersection(p1, p2, ref contact);
            if (firstI == 0)
            { }
            else
            {
                //各ポリゴンの速度をつかって、次の位置を想定して、
                //衝突が起きるかをチェック
                //より正確な衝突判定になるために、このような
                //予想的な衝突判定システムが必要
                Polygon pTest1 = p1.GetLinearPrediction(t1);
                Polygon pTest2 = p2.GetLinearPrediction(t1);

                int I = CheckIntersection(pTest1, pTest2, ref contact);

                if (I < 0)      //衝突が起こらなかった
                { }
                else
                {
                    if (I == 0) //ちょうど被っている、衝突面積=0
                    {
                        tContact = t1;
                    }
                    else if (I > 0)     //衝突面積があったので、衝突する時点を決めるためにdtを半減してまた衝突判定を行う
                    {
                        int debugCounter = 0;
                        for (debugCounter = 0; debugCounter < 60; debugCounter++)
                        {
                            float tm = (t0 + t1) / 2f;
                            if (tm < 0.00000000001d)
                            {

                            }
                            pTest1 = p1.GetLinearPrediction(tm);
                            pTest2 = p2.GetLinearPrediction(tm);

                            I = CheckIntersection(pTest1, pTest2, ref contact);
                            //Activates when an intersection is unavoidable; explode.    
                            //何回再帰しても共通分が生じたら、当ポリゴンは爆発する
                            if (tm < 0.00000000001d)
                            {
                                tContact = -1;
                                break;
                            }
                            if (I > 0)
                            {
                                t1 = tm;
                            }
                            else if (I == -1)
                                t0 = tm;
                            else if (I == 0)
                            {
                                //contact.A = null;
                                tContact = tm;
                                break;
                            }
                        }
                    }
                }
            }
        }

        //頂点のpはポリゴンのpolのどの側面に位置しているかを求める関数
        // 0: NO COLLISION
        // 1: COLLISION
        // 2: COLLISION ON 頂点
        private int GetSide(Vector2 p, Polygon pol, ref Vector2 side, ref int dotCol)
        {
            int returnValue = 0;
            int n = pol.Vertices.Length;
            for (int i = 0; i < n; i++)
            {
                int a = i, b = (i + 1) % n;
                Vector2 d = Vector2.Zero;
                float dist = SqDistPointSegment(pol.Vertices[a], pol.Vertices[b], p, ref d);
                if (dist < 0.35f)
                {
                    side = pol.Vertices[b] - pol.Vertices[a];
                    returnValue = 1;
                    if (d == Vector2.Zero)
                    {

                    }
                    if (pol.PointOnVertex(p, ref dotCol))
                    {
                        return 2;
                    }
                }
            }
            return returnValue;
        }


    }
}
