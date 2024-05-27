using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using static ARMAN_DEMO.src.Globals;
using Microsoft.Xna.Framework.Input;

namespace ARMAN_DEMO.src
{

    //レンダーを担当するクラス
    //Game1.csからレンダーすべきオブジェクトをインプットし、
    //各オブジェクトのレンダーモード(LINE OR FILL)に分別して
    //レンダーを行う、また、BLOOMエフェクトのものを別としてレンダーする
    public class RenderSystem
    {
        public const int MAX_TARGETS = 2;
        bool bgRendered;
        StarBackground bg;
        RenderTarget2D bgImage;

        //List<Polygon> polygons;
        List<Polygon> polygons_NORMAL;
        List<Polygon> polygons_NORMAL_FILL;
        List<Polygon> polygons_NORMAL_LINE;
        List<Polygon> polygons_BLOOM;
        List<Polygon> polygons_BLOOM_FILL;
        List<Polygon> polygons_BLOOM_LINE;

        RenderTarget2D starTarget;
        RenderTarget2D[] renderTargets;
        RenderTargetBinding[] rTB;
        Effect[] effects;
        RenderTarget2D[] pingPongBuffers;
        VectorGraphics vectorGraphics;

        SpriteFont[] fonts;

        //レンダーターゲットのイニシャライズ、
        public RenderSystem(GraphicsDevice device, VectorGraphics graphics) 
        {
            bgRendered = false;
            vectorGraphics = graphics;

            polygons_NORMAL = new List<Polygon>();
            polygons_NORMAL_FILL = new List<Polygon>();
            polygons_NORMAL_LINE = new List<Polygon>();
            polygons_BLOOM = new List<Polygon>();
            polygons_BLOOM_FILL = new List<Polygon>();
            polygons_BLOOM_LINE = new List<Polygon>();

            renderTargets = new RenderTarget2D[MAX_TARGETS];
            effects = new Effect[MAX_TARGETS];
            fonts = new SpriteFont[16];
            pingPongBuffers = new RenderTarget2D[]
            {
                new RenderTarget2D(
                device,
                device.PresentationParameters.BackBufferWidth,
                device.PresentationParameters.BackBufferHeight,
                false,
                device.PresentationParameters.BackBufferFormat,
                DepthFormat.Depth24),
                new RenderTarget2D(
                device,
                device.PresentationParameters.BackBufferWidth,
                device.PresentationParameters.BackBufferHeight,
                false,
                device.PresentationParameters.BackBufferFormat,
                DepthFormat.Depth24)
            };
            
            for (int j = 0; j < MAX_TARGETS; j++)
            {
                renderTargets[j] = new RenderTarget2D(
                device,
                device.PresentationParameters.BackBufferWidth,
                device.PresentationParameters.BackBufferHeight,
                false,
                device.PresentationParameters.BackBufferFormat,
                DepthFormat.Depth24);
            }
            rTB = new RenderTargetBinding[MAX_TARGETS];
            rTB[0] = renderTargets[0];
            rTB[1] = renderTargets[1];

            starTarget = new RenderTarget2D(
                device,
                device.PresentationParameters.BackBufferWidth,
                device.PresentationParameters.BackBufferHeight,
                false,
                device.PresentationParameters.BackBufferFormat,
                DepthFormat.Depth24);

            bg = new StarBackground();

            bgImage = new RenderTarget2D(
                device,
                device.PresentationParameters.BackBufferWidth,
                device.PresentationParameters.BackBufferHeight,
                false,
                device.PresentationParameters.BackBufferFormat,
                DepthFormat.Depth24);


            if (!bg.IsInitialized)
            {
                bg.Initialize(graphics);
            }
        }

        public void Clear()
        {
            ClearDrawables();
        }

        public void Draw(VectorGraphics graphics, SpriteBatch spriteBatch)
        {
            graphics.SetStates();
            graphics.ClearScreen(Color.Transparent);

            vectorGraphics.SetRenderTarget(null); // reset target
            if (!bgRendered)
            {
                // render background
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null);
                bgImage = (RenderTarget2D)WithBloom(graphics, effects[EFFECT_BLUR], spriteBatch, bg.GetTexture(), pingPongBuffers, 2, 4, 1.4f);
                spriteBatch.End();
            }

            // render all polys to respective target
            DrawRenderEntities(graphics, spriteBatch, RENDERTARGET_NORMAL);
            DrawRenderEntities(graphics, spriteBatch, RENDERTARGET_BLOOM);

            // prepare final render
            graphics.ClearScreen(Color.Transparent);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null);

            // add bloom to the bloom target
            //ｂloomレンダーターゲットをぼやけて見えるようにして、色をも明るくする
            Texture2D r = WithBloom(graphics, effects[EFFECT_BLUR], spriteBatch, renderTargets[RENDERTARGET_BLOOM], pingPongBuffers, 5, 6, 1.0f);

            // シェーダパラメーターを設定
            effects[EFFECT_BLOOM].Parameters["Texture0"].SetValue(renderTargets[RENDERTARGET_NORMAL]);
            effects[EFFECT_BLOOM].Parameters["Texture1"].SetValue(r);

            // final render
            for (int i = 0; i < effects[1].CurrentTechnique.Passes.Count; ++i)
            {
                effects[1].CurrentTechnique.Passes[i].Apply();
                spriteBatch.Draw(renderTargets[RENDERTARGET_NORMAL], new Rectangle(0, 0, vectorGraphics.Width, vectorGraphics.Height), null, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 1f);
            }
            spriteBatch.End();

            // draw text
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, null, null, null);

            spriteBatch.DrawString(fonts[0], "いどう　WASD\nかいてん　MOUSE\nはっしゃ　MOUSECLICK\n", Vector2.Zero + new Vector2(5, 5), Color.White);
            //spriteBatch.DrawString(fonts[1],
            //    MouseHandler.GetTransformedPosition().ToString(),
            //    Vector2.Zero + new Vector2(5, 30),
            //    Color.White);
            spriteBatch.End();
            ClearDrawables();
        }

        
        private void DrawEntitiesByMethod(VectorGraphics graphics, DrawMethod drawMethod, params List<Polygon>[] polygon_lists)
        {
            graphics.Begin(drawMethod);
            for (int a = 0; a < polygon_lists.Length; ++a)
            {
                
                int last = polygon_lists[a].Count;
                for (int i = 0; i < last; i++)
                {
                    polygon_lists[a][i].Draw(graphics);
                }
            }

            graphics.End();
        }

        private void DrawRenderEntities(VectorGraphics graphics, SpriteBatch spriteBatch, int renderTarget)
        {
            bool is_normal = renderTarget == RENDERTARGET_NORMAL;
            SetRenderTarget(graphics, renderTargets[renderTarget]);
            graphics.ClearScreen(Color.Transparent);

            //指定されたレンダーターゲットのIDによって、レンダーするものが異なる
            //bloom付きのものは、通常のレンダーターゲットに一回描かれて、そのあと
            //ｂloomのレンダーターゲットにもレンダーされる
            if (is_normal)
            {
                DrawBG(spriteBatch);
                DrawEntitiesByMethod(graphics, DrawMethod.FILL, polygons_NORMAL_FILL, polygons_BLOOM_FILL);
                DrawEntitiesByMethod(graphics, DrawMethod.LINE, polygons_NORMAL_LINE, polygons_BLOOM_LINE);
            }
            else 
            {
                DrawEntitiesByMethod(graphics, DrawMethod.FILL, polygons_BLOOM_FILL);
                DrawEntitiesByMethod(graphics, DrawMethod.LINE, polygons_BLOOM_LINE);
            }
            graphics.DropRenderTarget();
        }

        private void DrawBG(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null);
            spriteBatch.Draw(bgImage, new Rectangle(0, 0, bgImage.Width, bgImage.Height), null, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 1f);
            spriteBatch.End();
        }

        public void SetEffect(int effectId, Effect ef)
        {
            effects[effectId] = ef;
        }

        private void SetRenderTarget(VectorGraphics graphics, RenderTarget2D target)
        {
            graphics.SetRenderTarget(target);
            graphics.ClearScreen(Color.Transparent);
        }

        /// <summary>
        /// レンダーターゲットを二つ使うことによってシェーダーのぼやけるエフェクトを数回加えて、
        /// bloomのエフェクトを実現させる。たとえば、blurシェーダーを使い元の映像をpingPong1にレンダーして、
        /// そのblurシェーダー加えられたpingPong1をまたpingPong2に、blurシェーダーをかけてレンダーして、
        /// 同じプロセスを、まさに卓球のように何回かしていくと、色も鮮やかになりBloomのエフェクトができます。
        /// </summary>
        /// <returns></returns>
        private Texture2D WithBloom(VectorGraphics vectorGraphics, Effect bloomEffect, SpriteBatch spriteBatch, RenderTarget2D initialRender, 
            RenderTarget2D[] pingPong, int shaderIterationCount = 6, int pPCount = 10, float weightMul = 1.0f)
        {
            CleanPingPong(pingPong, vectorGraphics);
            bool hori = true;
            bloomEffect.Parameters["weightMul"].SetValue(weightMul);
            bloomEffect.Parameters["iterationCount"].SetValue(shaderIterationCount);
            for (int i = 0; i < pPCount; ++i)
            {
                int index = i % 2;
                int index1 = 1 - index;
                vectorGraphics.SetRenderTarget(pingPong[index1]);
                vectorGraphics.ClearScreen(Color.Transparent);
                for(int a = 0; a < bloomEffect.CurrentTechnique.Passes.Count; a++)
                {
                    bloomEffect.CurrentTechnique.Passes[a].Apply();
                    spriteBatch.Draw(i == 0 ? initialRender : pingPong[index], new Rectangle(0, 0, vectorGraphics.Width, vectorGraphics.Height),
                        null, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 1f);
                }

                hori = !hori;
                bloomEffect.Parameters["horizontal"].SetValue(hori);
            }
            vectorGraphics.DropRenderTarget();
            return pingPong[0];
        }

        private void CleanPingPong(RenderTarget2D[] pingPong, VectorGraphics vectorGraphics)
        {
            for (int i = 0; i < pingPong.Length; ++i)
            {
                vectorGraphics.SetRenderTarget(pingPong[i]);
                vectorGraphics.ClearScreen(Color.Transparent);
                vectorGraphics.DropRenderTarget();
            }
        }

        //オブジェクトのリストを受け止めて、そのレンダーデーターをもとに
        //どこのレンダーリストに入れるかを決める
        public void AddDrawable(List<Polygon> polygons)
        {
            for(int i = 0; i < polygons.Count; ++i)
            {
                if (polygons[i] == null) continue;
                if (polygons[i].RenderMode.HasFlag(DrawMethod.FILL))
                {
                    switch (polygons[i].RenderInfo.Effect)
                    {
                        default:
                        case 0:
                            polygons_NORMAL_FILL.Add(polygons[i]);
                            break;
                        case 1:
                            polygons_BLOOM_FILL.Add(polygons[i]);
                            break;

                    }
                }
                if (polygons[i].RenderMode.HasFlag(DrawMethod.LINE))
                {
                    switch (polygons[i].RenderInfo.Effect)
                    {
                        default:
                        case 0:
                            polygons_NORMAL_LINE.Add(polygons[i]);
                            break;
                        case 1:
                            polygons_BLOOM_LINE.Add(polygons[i]);
                            break;

                    }
                }
            }
        }

        private void ClearDrawables()
        {
            polygons_NORMAL_LINE.Clear();
            polygons_BLOOM_LINE.Clear();
            polygons_NORMAL_FILL.Clear();
            polygons_BLOOM_FILL.Clear();
        }

        public void LoadFonts(SpriteFont[] fonts)
        {
            this.fonts = fonts;
        }

    }
}
