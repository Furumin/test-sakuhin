using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace ARMAN_DEMO.src
{
    public class MouseSprite
    {
        private Projectile _innerFigure, _outerFigure;

        public List<Polygon> Sprite;
        
        public MouseSprite()
        {
            Vector3 cursorPos = MouseHandler.GetTransformedPosition();
            _innerFigure = new Projectile(new Vector2[] { }, 40, new Vector2(cursorPos.X, cursorPos.Y), Shape.Triangle, null);
            _innerFigure.SetDrawMode(DrawMethod.LINE | DrawMethod.FILL);
            _innerFigure.SetFillColors(Color.White);
            _innerFigure.SetLineColors(Color.DarkRed);
            _innerFigure._drawPriority = 90f;

            _outerFigure = new Projectile(new Vector2[] { }, 40, new Vector2(cursorPos.X, cursorPos.Y), Shape.Circle, null);
            _outerFigure.SetDrawMode(DrawMethod.LINE);
            _outerFigure.SetLineColors(Color.DarkRed);
            _outerFigure._drawPriority = 90f;

            Sprite = new List<Polygon>() {_innerFigure, _outerFigure };
        }

        public void Update()
        {
            Vector3 cursorPos = MouseHandler.GetTransformedPosition();
            Vector2 pos = new(cursorPos.X, cursorPos.Y);
            _innerFigure.Push(pos - _innerFigure.Center);
            _outerFigure.Push(pos - _outerFigure.Center);

            if (MouseHandler.LeftHold())
            {
                _innerFigure.Rotate(0.2f);
                _outerFigure.Rotate(-0.2f);
            }
        }
    }
}
