using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace ARMAN_DEMO
{

    //３Ｄの三角形のクラス
    //レンダーに必要なデータのみを含めている
    public class Triangle
    {
        Vector3 center;
        Vector3[] vertices;
        Color[] fillColors, edgeColors;
        Vector3 normal;
        DrawMethod drawMethod;

        public Triangle(Vector3[] verts)
        {
            if (verts.Length != 3)
                throw new ArgumentException("Wrong amount of vertices!");
            vertices = verts;

            UpdateCenter();
            CalculateNormal();
            drawMethod = DrawMethod.FILL | DrawMethod.LINE;
        }

        public Triangle(Vector3 vert1, Vector3 vert2, Vector3 vert3) : this(new Vector3[] {vert1, vert2, vert3})
        {
            
        }

        public Vector3 Center
        {
            get { return center; }
        }

        private void UpdateCenter()
        {
            center = (vertices[0] + vertices[1] + vertices[2]) / 3;
        }

        private void CalculateNormal()
        {
            if (vertices.Length != 3)
                throw new ArgumentException("Wrong amount of vertices!");

            Vector3 ab = vertices[1] - vertices[0];
            Vector3 ac = vertices[2] - vertices[0];

            normal = new Vector3(
                ab.Y * ac.Z - ab.Z * ac.Y,
                ab.Z * ac.X - ab.X * ac.Z,
                ab.X * ac.Y - ab.Y * ac.X
                );
        }

        public void Translate(Vector3 distance)
        {
            for(int i = 0; i < 3; i++)
            {
                vertices[i].X += distance.X;
                vertices[i].Y += distance.Y;
                vertices[i].Z += distance.Z;
            }
            UpdateCenter();
        }

        public void RotateX(float radians, Vector3 center)
        {
            for (int i = 0; i < 3; i++)
            {
                Vector3 step1 = Vector3.Transform(vertices[i], Matrix.CreateTranslation(-center.X, -center.Y, -center.Z));
                Vector3 step2 = Vector3.Transform(step1, Matrix.CreateRotationX(radians));
                Vector3 step3 = Vector3.Transform(step2, Matrix.CreateTranslation(center.X, center.Y, center.Z));
                vertices[i] = step3;
            }
            CalculateNormal();
            UpdateCenter();
        }
        public void RotateX(float radians)
        {
            RotateX(radians, center);
        }

        public void RotateY(float radians, Vector3 center)
        {
            for (int i = 0; i < 3; i++)
            {
                Vector3 step1 = Vector3.Transform(vertices[i], Matrix.CreateTranslation(-center.X, -center.Y, -center.Z));
                Vector3 step2 = Vector3.Transform(step1, Matrix.CreateRotationY(radians));
                Vector3 step3 = Vector3.Transform(step2, Matrix.CreateTranslation(center.X, center.Y, center.Z));
                vertices[i] = step3;
            }
            CalculateNormal();
            UpdateCenter();
        }
        public void RotateY(float radians)
        {
            RotateY(radians, center);
        }

        public void RotateZ(float radians, Vector3 center)
        {
            for (int i = 0; i < 3; i++)
            {
                Vector3 step1 = Vector3.Transform(vertices[i], Matrix.CreateTranslation(-center.X, -center.Y, -center.Z));
                Vector3 step2 = Vector3.Transform(step1, Matrix.CreateRotationZ(radians));
                Vector3 step3 = Vector3.Transform(step2, Matrix.CreateTranslation(center.X, center.Y, center.Z));
                vertices[i] = step3;
            }
            CalculateNormal();
            UpdateCenter();
        }
        public void RotateZ(float radians)
        {
            RotateZ(radians, center);
        }


        public void Scale(float factor)
        {
            for (int i = 0; i < 3; i++)
                vertices[i] *= factor;
        }

        public void SetFillColors(Color[] clrs)
        {
            fillColors = clrs;
        }
        public void SetFillColors(Color clr)
        {
            Color[] clrs = new Color[] { clr, clr, clr };
            fillColors = clrs;
        }
        public void SetEdgeColors(Color[] clrs)
        {
            edgeColors = clrs;
        }
        public void SetEdgeColors(Color clr)
        {
            Color[] clrs = new Color[] { clr, clr, clr };
            edgeColors = clrs;
        }

        //Vector2をVPCに変換する関数
        private VertexPositionColor[] ToVPC(Color[] clrs)
        {
            if (clrs.Length != 3)
                throw new ArgumentException("Wrong amount of clrs!");
            VertexPositionColor[] v = new VertexPositionColor[vertices.Length];
            for (int i = 0; i < 3; i++)
            {
                v[i] = new VertexPositionColor(vertices[i], clrs[i]);
            }

            return v;
        }

        public void Draw(VectorGraphics vectorGraphics)
        {
            switch (vectorGraphics.drawMethod)
            {
                case DrawMethod.LINE:
                    vectorGraphics.AddShape_LINE(ToVPC(edgeColors));
                    break;
                case DrawMethod.FILL:
                    vectorGraphics.AddTriangle(ToVPC(fillColors));
                    break;
            }
        }

        public void ToVPC()
        {

        }
    }

    public class Cube
    {
        Triangle[] triangles;
        Vector3[] vertices;
        Vector3 center;
        float sideLength;

        public Vector3 Center
        {
            get { return center; }
        }

        public Cube(Vector3 center, float halfside)
        {
            this.center = center;
            sideLength = halfside * 2f;
            triangles = new Triangle[12]; 
            vertices = new Vector3[8];

            vertices[0] = center + new Vector3(-halfside, halfside, halfside);
            vertices[1] = center + new Vector3(-halfside, -halfside, halfside);
            vertices[2] = center + new Vector3(halfside, -halfside, halfside);
            vertices[3] = center + new Vector3(halfside, halfside, halfside);

            vertices[4] = center + new Vector3(-halfside, halfside, -halfside);
            vertices[5] = center + new Vector3(halfside, halfside, -halfside);
            vertices[6] = center + new Vector3(halfside, -halfside, -halfside);
            vertices[7] = center + new Vector3(-halfside, -halfside, -halfside);


            // FRONT
            triangles[0] = new Triangle(vertices[0], vertices[1], vertices[2]);
            triangles[1] = new Triangle(vertices[0], vertices[2], vertices[3]);

            // BOTTOM
            triangles[2] = new Triangle(vertices[1], vertices[7], vertices[6]);
            triangles[3] = new Triangle(vertices[1], vertices[6], vertices[2]);

            // RIGHT
            triangles[4] = new Triangle(vertices[3], vertices[2], vertices[6]);
            triangles[5] = new Triangle(vertices[3], vertices[6], vertices[5]);

            // TOP
            triangles[6] = new Triangle(vertices[0], vertices[3], vertices[5]);
            triangles[7] = new Triangle(vertices[0], vertices[5], vertices[4]);

            // LEFT
            triangles[8] = new Triangle(vertices[0], vertices[4], vertices[7]);
            triangles[9] = new Triangle(vertices[0], vertices[7], vertices[1]);

            // BACK
            triangles[10] = new Triangle(vertices[4], vertices[5], vertices[6]);
            triangles[11] = new Triangle(vertices[4], vertices[6], vertices[7]);

            for (int i = 0; i < triangles.Length; i++)
            {
                triangles[i].SetEdgeColors(Color.White);
                triangles[i].SetFillColors(Color.White);
            }
        }

        public void Translate(Vector3 dist)
        {
            for (int i = 0; i < triangles.Length; i++)
            {
                triangles[i].Translate(dist);
            }
            center += dist;
        }

        public void Scale(float factor)
        {
            for (int i = 0; i < triangles.Length; i++)
            {
                triangles[i].Scale(factor);
            }
        }

        public void RotateX(float radians)
        {
            for(int i = 0; i < triangles.Length; i++)
            {
                triangles[i].RotateX(radians, center);
            }
        }
        public void RotateY(float radians)
        {
            for (int i = 0; i < triangles.Length; i++)
            {
                triangles[i].RotateY(radians, center);
            }
        }
        public void RotateZ(float radians)
        {
            for (int i = 0; i < triangles.Length; i++)
            {
                triangles[i].RotateZ(radians, center);
            }
        }

        public void SetEdgeColor(Color clr)
        {
            for (int i = 0; i < triangles.Length; ++i)
            {
                triangles[i].SetEdgeColors(clr);
            }
        }
        public void SetFillColor(Color clr)
        {
            for (int i = 0; i < triangles.Length; ++i)
            {
                triangles[i].SetFillColors(clr);
            }
        }

        public void Draw(VectorGraphics vectorGraphics)
        {
            foreach (Triangle t in triangles)
                t.Draw(vectorGraphics);
        }
    }
}
