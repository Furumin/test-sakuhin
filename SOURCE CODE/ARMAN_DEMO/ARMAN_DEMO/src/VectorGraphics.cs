using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SharpDX.MediaFoundation;
using System;

namespace ARMAN_DEMO
{
    public enum DrawMethod
    {
        LINE = 2,
        FILL = 1
    };

    //グラフィックデバイスへ頂点データーなどを送ったり、レンダーデーターの扱い
    //を担当するクラス。あらゆるポリゴンは、このクラスのオブジェクトに
    //時分の頂点データーを送って、そのデーターを使ってレンダーするのがVectorGraphics.Draw()

    //基本的な使い方は、、、
    //vectorGraphics.Begin();
    // [オブジェクト].Draw(vectorGraphics);
    // ...
    //vectorGraphics.End();
    public sealed class VectorGraphics : IDisposable
    {

        private GraphicsDevice graphicsDevice;

        private VertexBuffer vertexBuffer;
        private IndexBuffer indexBuffer;
        public BasicEffect effect;
        public BasicEffect effect2;

        private bool isDisposed;
        const int maxIndexBuffer = 4096*8;
        
        //頂点の結び方の情報を含めるarray. 
        short[] indices;    // all the indices for what vertices we want
        short currentIndex; // the current index in indices open to be written to
        int currentIndexData; //the current unique index in indices. next primitive drawn starts with this index
        
        int shapeAmount;
        int vertexAmount;

        short l_currentIndex;
        int l_currentID;

        //頂点の情報を含めるarray
        const int maxVertexBuffer = 4096*8;
        VertexPositionColor[] vertices;
        
        
        Matrix worldTransform;
        Matrix viewTransform;
        Matrix projTransform;
        Texture2D pixelTex;

        public DrawMethod drawMethod;

        public GraphicsDevice Device { get { return graphicsDevice; } }

        // DRAWの流れ
        // プリミティブ追加　->　Begin()　->　DrawBuffers()　->　SetVertices()　->　Draw()  =  レンダー
        public VectorGraphics(Camera camera, MainGame game)
        {

            this.graphicsDevice = game.GraphicsDevice;

            isDisposed = true;
            currentIndex = 0;
            currentIndexData = 0;

            effect = new BasicEffect(graphicsDevice);
            effect2 = new BasicEffect(graphicsDevice);
            shapeAmount = 0;
            vertexAmount = 0;

            pixelTex = new Texture2D(graphicsDevice, 1, 1);
            pixelTex.SetData(new[] { Color.White});

            vertices = new VertexPositionColor[maxVertexBuffer];
            indices = new short[maxIndexBuffer];


            worldTransform = Matrix.CreateScale(1f) * Matrix.CreateRotationZ(0f) * Matrix.CreateTranslation(0f, 0f, 0f);
            viewTransform = Matrix.CreateLookAt(camera.Pos, new Vector3(camera.Pos.X, camera.Pos.Y, camera.Pos.Z-1), Vector3.Up);
            projTransform = Matrix.CreateOrthographicOffCenter(game.ArenaBoundary.Left, game.ArenaBoundary.Right, game.ArenaBoundary.Top, game.ArenaBoundary.Bottom, 10f, 30000f);

            indexBuffer = new IndexBuffer(graphicsDevice, typeof(short), maxIndexBuffer, BufferUsage.WriteOnly);
            vertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionColorNormal), maxVertexBuffer, BufferUsage.WriteOnly);
        }

        public int Width
        {
            get { return graphicsDevice.Viewport.Width; }
        }
        public int Height
        {
            get { return graphicsDevice.Viewport.Height; }
        }

        public Matrix WorldTransform { get { return worldTransform; } }
        public Matrix ViewTransform { get { return viewTransform; } }
        public Matrix ProjectionTransform { get { return projTransform; } }

        public Matrix WorldViewProjection { get { return (worldTransform) * viewTransform * projTransform; } }

        //プリミティブをレンダーする準備をする
        public void Begin(DrawMethod method = DrawMethod.FILL)
        {
            if (!isDisposed)
            {
                throw new ArgumentException("This object has already begun!");
            }

            isDisposed = false;
            Array.Clear(vertices, 0, vertices.Length);
            Array.Clear(indices, 0, indices.Length);

            l_currentIndex = 0;
            l_currentID = 0;

            currentIndex = 0;
            currentIndexData = 0;
            SetStates();


            effect.World = worldTransform;
            effect.Projection = projTransform;
            effect.View = viewTransform;
            effect.VertexColorEnabled = true;

            effect2.World = worldTransform;
            effect2.Projection = projTransform;
            effect2.View = viewTransform;
            effect2.VertexColorEnabled = false;

            drawMethod = method;
        }

        public void Dispose()
        {
            if (isDisposed)
                throw new ArgumentException("already disposed!");

            //indexBuffer?.Dispose();

            isDisposed = true;
        }

        //レンダー後の片づけをする
        public void End()
        {
            Draw();
            Dispose();
            ResetVertices();
            ResetIndices();
        }

        private void ResetVertices()
        {
            vertices = new VertexPositionColor[maxVertexBuffer];
            vertexAmount = 0;
            shapeAmount = 0;
        }

        private void ResetIndices()
        {
            indices = new short[maxIndexBuffer];

            currentIndex = 0;
            l_currentIndex = 0;

            currentIndexData = 0;
            l_currentIndex = 0;
        }


        //バーテックス情報などを各バッファーに転送
        public int SetVertices()
        {
            if (isDisposed)
                throw new Exception("you can't add vertices when this resource is disposed!");

            VertexPositionColor[] active = ExtractActiveVertices(vertexAmount);
            if (active.Length < 1)
                return -1;
            
            VertexPositionColorNormal[] withN = ToVPCN(active);
            vertexBuffer.SetData(withN);

            //indexBuffer = new IndexBuffer(graphicsDevice, typeof(short), currentIndex+1, BufferUsage.WriteOnly);
            indexBuffer.SetData<short>(indices, 0, currentIndex);
            PrepareBuffers();

            return 0;
        }

        //convert to vertexpositioncolornormal
        private VertexPositionColorNormal[] ToVPCN(VertexPositionColor[] input)
        {
            VertexPositionColorNormal[] output = new VertexPositionColorNormal[input.Length];
            for (int i = 0; i < input.Length; i++)
            {
                output[i] = new VertexPositionColorNormal(input[i].Position, input[i].Color, new Vector3(0, 0, -1));
            }
            return output;
        }

        //バーテックス情報などを各バッファーに転送
        public int SetVertices_LINE()
        {
            if (isDisposed)
                throw new Exception("you can't add vertices when this resource is disposed!");

            VertexPositionColor[] active = ExtractActiveVertices(vertexAmount);
            if (active.Length < 1)
                return -1;
            VertexPositionColorNormal[] withN = ToVPCN(active);
            vertexBuffer.SetData(withN);

            //short[] lineI = new short[active.Length];
            //for (short i = 0; i < lineI.Length; i++)
            //    lineI[i] = i;

            //indexBuffer = new IndexBuffer(graphicsDevice, typeof(short), active.Length, BufferUsage.WriteOnly);
            indexBuffer.SetData<short>(indices, 0, l_currentIndex);
            PrepareBuffers();

            return 0;
        }

        private VertexPositionColor[] ExtractActiveVertices(int amount)
        {
            VertexPositionColor[] newV = new VertexPositionColor[amount];
            for (int i = 0; i < amount; i++)
            {
                newV[i] = vertices[i];
            }
            return newV;
        }

        //buffersをグラフィックデバイスに読み込ませる
        private void PrepareBuffers()
        {
            if (vertexBuffer == null || indexBuffer == null)
                throw new ArgumentException("One of the buffers are null!");

            graphicsDevice.SetVertexBuffer(vertexBuffer);
            graphicsDevice.Indices = indexBuffer;
        }


        // Add a shape to be drawn. Only works if the shape is seperate from any other primitive.
        public void AddQuad(VertexPositionColor[] data)
        {
            if (data.Length + vertexAmount > maxIndexBuffer)
                return;
            if (data.Length != 4)
                throw new ArgumentException("not correct amount of vertices!");

            short n = (short)currentIndexData;
            indices[currentIndex] = n; currentIndex++;
            indices[currentIndex] = (short)(n+1); currentIndex++;
            indices[currentIndex] = (short)(n + 2); currentIndex++;
            indices[currentIndex] = (short)(n + 2); currentIndex++;
            indices[currentIndex] = (short)(n + 3); currentIndex++;
            indices[currentIndex] = n; currentIndex++;

            vertices[vertexAmount] = data[0]; vertexAmount++;
            vertices[vertexAmount] = data[1]; vertexAmount++;
            vertices[vertexAmount] = data[2]; vertexAmount++;
            vertices[vertexAmount] = data[3]; vertexAmount++;

            shapeAmount += 2;
            currentIndexData += 4;
        }

        //triangulates given convex shape and adds its vertices
        public void AddShape(VertexPositionColor[] data)
        {
            int length = data.Length;
            if (data.Length + vertexAmount > maxIndexBuffer)
                return;
            if (length < 3)
                throw new ArgumentException("Not a shape!");
            else if (length < 4)
            {
                AddTriangle(data);
                return;
            }

            short n = (short)currentIndexData;

            for (int i = 0; i < length; i++)
            {
                vertices[vertexAmount] = data[i]; vertexAmount++;
            }

            for (int i = 0; i < length-2; i++)
            {
                indices[currentIndex] = n; currentIndex++;
                indices[currentIndex] = (short)(n + i + 1); currentIndex++;
                indices[currentIndex] = (short)(n + i + 2); currentIndex++;
            }

            shapeAmount += length - 2;
            currentIndexData += length;
        }

        public void AddParticle(Vector3 xyz, float size, Color clr)
        {
            VertexPositionColor[] data =
            {
                new VertexPositionColor(xyz, clr),
                new VertexPositionColor(xyz + new Vector3(0,-size,0), clr),
                new VertexPositionColor(xyz+ new Vector3(size,-size,0), clr),
                new VertexPositionColor(xyz + new Vector3(size, 0,0), clr)
            };

            AddQuad(data);
        }

        public void AddTriangle(VertexPositionColor[] data)
        {
            if (data.Length + vertexAmount > maxIndexBuffer)
                return;
            if (data.Length != 3)
                throw new ArgumentException("not correct amount of vertices!");

            short n = (short)currentIndexData;
            for (int i = 0; i < 3; i++)
            {
                vertices[vertexAmount+i] = data[i];
                indices[currentIndex+i] = (short)(n+i);
            }
            shapeAmount++;
            currentIndex += 3;
            currentIndexData += 3;
            vertexAmount += 3;
        }

        public void AddLine(VertexPositionColor[] data)
        {
            if (data.Length + vertexAmount > maxIndexBuffer)
                return;
            if (data.Length != 2)
                throw new ArgumentException("not correct amount of vertices!");

            short n = (short)l_currentID;
            vertices[vertexAmount] = data[0];
            vertices[vertexAmount+1] = data[1];

            indices[l_currentIndex] = n;
            indices[l_currentIndex + 1] = (short)(n + 1);

            l_currentIndex += 2;
            vertexAmount += 2;
            l_currentID += 2;
        }

        public void AddShape_LINE(VertexPositionColor[] data)
        {
            if (data.Length + vertexAmount > maxIndexBuffer)
                return;
            if (data.Length == 0)
                throw new ArgumentException("not correct amount of vertices!");

            for (int i = 0; i < data.Length; i++)
            {
                int j = (i + 1) % data.Length;
                VertexPositionColor[] v = { data[i], data[j] };
                AddLine(v);
            }
        }


        public void SetStates()
        {
            graphicsDevice.BlendState = BlendState.Opaque;
            graphicsDevice.DepthStencilState = DepthStencilState.Default;
            graphicsDevice.RasterizerState = RasterizerState.CullClockwise;
            graphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
        }

        #region DRAW

        private void Draw()
        {
            switch (drawMethod)
            {
                case DrawMethod.LINE:
                    DrawBuffers_LINE();
                    break;
                case DrawMethod.FILL:
                    //RasterizerState rs = new RasterizerState();
                    //rs.FillMode = FillMode.WireFrame;
                    //graphicsDevice.RasterizerState = rs;
                    DrawBuffers();
                    break;
            }
        }
        public void DrawBuffers()
        {
            DrawBuffers(this.effect);
        }
        public void DrawBuffers_LINE()
        {
            DrawBuffers_LINE(this.effect);
        }
        public void DrawBuffers(Effect effect)
        {
            if (SetVertices() != 0)
                return;
            for (int i = 0; i < effect.CurrentTechnique.Passes.Count; ++i)
            {
                effect.CurrentTechnique.Passes[i].Apply();
                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, shapeAmount);
            }

        }

        public void DrawBuffers_LINE(Effect effect)
        {
            if (SetVertices_LINE() != 0)
                return;
            for (int i = 0; i < effect.CurrentTechnique.Passes.Count; ++i)
            {
                effect.CurrentTechnique.Passes[i].Apply();
                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.LineList, 0, 0, vertexAmount / 2);
            }
        }

        public void ClearScreen(Color color)
        {
            graphicsDevice.Clear(color);
        }
        #endregion

        public void SetRenderTarget(RenderTarget2D target)
        {
            graphicsDevice.SetRenderTarget(target);
        }

        public void DropRenderTarget()
        {
            if (graphicsDevice.RenderTargetCount > 1)
                graphicsDevice.SetRenderTargets(null);
            else graphicsDevice.SetRenderTarget(null);
        }
    }
}
