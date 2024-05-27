using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ARMAN_DEMO.src
{
    public static class MouseHandler
    {
        private static MouseState _currentState = new(), _previousState = new();
        private static float _viewportWidth = 1f, _viewportHeight = 1f;
        private static GraphicsDevice _device;
        public static VectorGraphics _vectorGraphics;

        #region draw_info
        #endregion

        public static void SetViewport(GraphicsDevice device)
        {
            _viewportWidth = device.Viewport.Width;
            _viewportHeight = device.Viewport.Height;
            _device = device;
        }

        /// <summary>
        /// 画面での座標をリターン
        /// </summary>
        /// <returns></returns>
        public static Point GetPosition()
        {
            return _currentState.Position;
        }

        public static Vector3 GetTransformedPosition()
        {
            Vector3 uprj = _device.Viewport.Unproject(new Vector3(_currentState.Position.X, _currentState.Position.Y, 0f), 
                _vectorGraphics.ProjectionTransform, _vectorGraphics.ViewTransform, _vectorGraphics.WorldTransform);
            return uprj;
        }

        public static void Update()
        {
            _previousState = _currentState;
            _currentState = Mouse.GetState();
        }

        public static bool LeftClick()
        {
            return _currentState.LeftButton == ButtonState.Pressed && _previousState.LeftButton == ButtonState.Released;
        }

        public static bool LeftHold()
        {
            return _currentState.LeftButton == ButtonState.Pressed;
        }


    }
}
