using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ARMAN_DEMO
{
    public class Camera
    {
        private const float zoomUpperLimit = 1.5f;
        private const float zoomLowerLimit = 0.3f;

        public Matrix _transform;
        private Vector3 _pos;
        private int _viewportWidth;
        private int _viewportHeight;

        private Rectangle _border;

        public float rotationY;

        public Camera(Viewport viewport, int worldWidth, int worldHeight, float initialZoom, Rectangle border)
        {
            _pos = Vector3.Zero;
            _viewportWidth = viewport.Width;
            _viewportHeight = viewport.Height;

            _border = border;
            rotationY = 0f;
        }

        public int Width
        {
            get { return _viewportWidth; }
            set
            {
                _viewportWidth = value;
            }
        }

        public int Height
        {
            get { return _viewportHeight; }
            set
            {
                _viewportHeight = value;
            }
        }

        public Vector3 Pos
        {
            get { return _pos; }
            set
            {
                _pos = value;

                if (_border != Rectangle.Empty)
                {
                    float _cameraLeftBorder = _pos.X - _viewportWidth / 2;
                    float _cameraRightBorder = _pos.X + _viewportWidth / 2;
                    float _cameraTopBorder = _pos.Y + _viewportHeight / 2;
                    float _cameraBottomBorder = _pos.Y / _viewportHeight / 2;


                    if (_cameraLeftBorder < _border.Left)
                        _pos.X = _border.Left + _viewportWidth / 2;
                    if (_cameraRightBorder > _border.Right)
                        _pos.X = _border.Right - _viewportWidth / 2;
                    if (_cameraTopBorder < _border.Top)
                        _pos.Y = _border.Top + _viewportHeight / 2;
                    if (_cameraBottomBorder > _border.Bottom)
                        _pos.Y = _border.Bottom - _viewportHeight / 2;
                }
            }
        }
    }
}
