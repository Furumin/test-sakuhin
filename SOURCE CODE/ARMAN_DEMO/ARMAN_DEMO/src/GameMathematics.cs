using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;
using SharpDX.MediaFoundation;

namespace ARMAN_DEMO
{
    static class GameMathematics
    {
        // Returns 2 times the signed triangle area. The result is positive if
        // abc is ccw, negative if abc is cw, zero if abc is degenerate.
        static float Signed2DTriArea(Vector2 a, Vector2 b, Vector2 c)
        {
            return (a.X - c.X) * (b.Y - c.Y) - (a.Y - c.Y) * (b.X - c.X);
        }
        static int TriangleIsCCW(Vector2 a, Vector2 b, Vector2 c)
        {
            float area = Signed2DTriArea(a, b, c);
            if (area > 0) return 1;
            else if (area < 0) return -1;
            else return 0;
        }

        public static int PointInPolygon(Vector2 p, Vector2[] v)
        {
            //Since I work with CW-Polygons
            //convert into CCW-Polygon
            int n = v.Length;
            Vector2[] cw_v = new Vector2[n];
            for (int i = 0; i < n; i++)
            {
                cw_v[i] = v[n - 1 - i];
            }

            int low = 0, high = v.Length;
            while (low+1<high)
            {
                int mid = (low + high) / 2;
                if (TriangleIsCCW(v[0], v[mid], p) > 0)
                    low = mid;
                else
                    high = mid;
            }

            if (low == 0 || high == n) return -1;

            // p is inside the polygon if it is left of
            // the directed edge from v[low] to v[high]
            return TriangleIsCCW(v[low], v[high], p);
        }
        /// <summary>
        /// Given point c and line segment ab, find point d on ab which is closest to c
        /// </summary>
        /// <param name="c"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="t"></param>
        /// <param name="d"></param>
        public static void ClosestPtPointSegment(Vector2 c, Vector2 a, Vector2 b, ref float t, ref Vector2 d)
        {
            Vector2 ab = b - a;

            t = DotProduct(c - a, ab) / DotProduct(ab, ab);
            if (t < 0.0f) t = 0.0f;
            if (t > 1.0f) t = 1.0f;
            d = a + t * ab;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a">First Point in vector AB</param>
        /// <param name="b">Second point in vector AB</param>
        /// <param name="c">Point whose distance to AB is pursued</param>
        /// <param name="p">Projection of point p onto AB</param>
        /// <returns></returns>
        public static float SqDistPointSegment(Vector2 a, Vector2 b, Vector2 c, ref Vector2 p)
        {
            Vector2 ab = b - a, ac = c - a, bc = c - b;
            p = a + (DotProduct(ac, ab) / DotProduct(ab, ab)) * ab;
            float e = DotProduct(ac, ab);
            if (e <= 0.0f) return DotProduct(ac, ac);
            float f = DotProduct(ab, ab);
            if (e >= f) return DotProduct(bc, bc);


            float sqDist = DotProduct(ac, ac) - e * e / f;

            return sqDist;
        }

        public static float DotProduct(Vector2 a, Vector2 b)
        {
            return a.X * b.X + a.Y * b.Y;
        }



        // Test if segments ab and cd overlap. If they do, compute and return
        // intersection t value along ab and intersection position p
        public static int Test2DSegmentSegment(Vector2 a, Vector2 b, Vector2 c, Vector2 d, ref float t, ref Vector2 p)
        {
            // Sign of areas correspond to which side of ab points c and d are
            float a1 = RoundToThreeD(Signed2DTriArea(a, b, d)); // Compute winding of abd (+ or -)
            float a2 = RoundToThreeD(Signed2DTriArea(a, b, c)); // To intersect, must have sign opposite of a1

            if (Math.Abs(a1) < 25)
                a1 = 0;
            if (Math.Abs(a2) < 25)
                a2 = 0;

            // If c and d are on different sides of ab, areas have different signs
            if (a1 * a2 <= 0.0f)
            {
                // Compute signs for a and b with respect to segment cd
                float a3 = RoundToThreeD(Signed2DTriArea(c, d, a)); // Compute winding of cda (+ or -)
                                                                    // Since area is constant a1 - a2 = a3 - a4, or a4 = a3 + a2 - a1
                float a4 = RoundToThreeD(Signed2DTriArea(c, d, b)); // Must have opposite sign of a3
                if (Math.Abs(a3) < 0.005)
                    a3 = 0;
                if (Math.Abs(a4) < 0.005)
                    a4 = 0;

                // Points a and b on different sides of cd if areas have different signs
                if (a3 * a4 <= 0.0f)
                {
                    // Segments intersect. Find intersection point along L(t) = a + t * (b - a).
                    // Given height h1 of an over cd and height h2 of b over cd,
                    // t = h1 / (h1 - h2) = (b*h1/2) / (b*h1/2 - b*h2/2) = a3 / (a3 - a4),
                    // where b (the base of the triangles cda and cdb, i.e., the length
                    // of cd) cancels out.
                    float divis = a3 - a4;
                    if (divis == 0)
                    {
                        divis = 1;
                    }
                    t = a3 / divis;
                    p = a + t * (b - a);
                    return 1;
                }
            }
            // Segments not intersecting (or collinear)
            return 0;
        }

        public static bool PointOnLine(Vector2 c, Vector2 a, Vector2 b, ref float t)
        {
            Vector2 ab = b - a;
            Vector2 ac = c - a;
            t = DotProduct(ac, ab) / DotProduct(ab, ab);
            ab.Normalize(); ac.Normalize();
            float dot = DotProduct(ac, ab);
            if (dot == 1 && t <= 1.0f && t >= 0.0f)
            {
                return true;
            }
            else return false;
        }

        public static float RoundToThreeD(float a)
        {
            return (float)Math.Round(a, 3);
        }

        public static Vector2 VectorProjection(Vector2 a, Vector2 b)
        {
            return ((DotProduct(a, b) / DotProduct(b, b)) * b);
        }


        /// <summary>
        /// Get true modulo a % b, suited for uses where a < 0
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static int Mod(int a, int b)
        {
            int c = a % b;
            if ((c < 0 && b > 0) || (c > 0 && b < 0))
            {
                c += b;
            }
            return c;
        }

        public static Vector2 RotatedVector(Vector2 v, Vector2 a, float theta)
        {
            float diffX, diffY, cosTheta, sinTheta;
            diffX = v.X - a.X;
            diffY = v.Y - a.Y;

            cosTheta = (float)Math.Cos(theta);
            sinTheta = (float)Math.Sin(theta);

            Vector2 outV = new(
                cosTheta * diffX - sinTheta * diffY + a.X,
                sinTheta * diffX + cosTheta * diffY + a.Y);

            return outV;
        }

        public static float Clamp(float value, float lower, float higher)
        {
            if (value < lower)
                return lower;
            if (value > higher)
                return higher;
            return value;
        }
    }
    }
