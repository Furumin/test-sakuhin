using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ARMAN_DEMO.src
{
    public class StarBackground
    {
        Star[] stars;
        RenderTarget2D texture;
        bool isInitialized;

        public bool IsInitialized { get { return isInitialized; } }

        //星空のデーターを生成
        public StarBackground()
        {
            stars = new Star[600];
            Color[] colors = {Color.YellowGreen, Color.MediumVioletRed, Color.LightGreen, Color.LightYellow,
            Color.Cyan};
            Random random = new();
            for (int i = 0; i < stars.Length; i++)
            {
                int x = random.Next(0, 3000);
                x -= 1500;
                int y = random.Next(0, 3000);
                y -= 1500;
                int j = random.Next(0, 5);
                stars[i] = new Star
                {
                    Pos = new Vector3(x, y, -random.Next(10, 100)),
                    Color = colors[j]
                };
            }

            isInitialized = false;
        }

        //星空をレンダーし、それをテクスチャーに保存
        public void Initialize(VectorGraphics graphics)
        {
            graphics.ClearScreen(Color.Transparent);
            GraphicsDevice device = graphics.Device;
            texture = new RenderTarget2D(
                device,
                device.PresentationParameters.BackBufferWidth,
                device.PresentationParameters.BackBufferHeight,
                false,
                device.PresentationParameters.BackBufferFormat,
                DepthFormat.Depth24);

            device.SetRenderTarget(texture);
            graphics.Begin(DrawMethod.FILL);
            for (int i = 0; i < stars.Length; ++i)
            {
                graphics.AddParticle(stars[i].Pos, 2, stars[i].Color);
            }
            graphics.End();
            graphics.DropRenderTarget();

            isInitialized = true;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, new Rectangle(0, 0, texture.Width, texture.Height), null, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 1f);
        }

        public RenderTarget2D GetTexture()
        {
            return texture;
        }
    }
}
