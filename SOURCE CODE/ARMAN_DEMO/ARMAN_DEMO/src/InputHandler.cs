using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;

namespace ARMAN_DEMO.src
{
    public class InputHandler
    {
        KeyboardState _currentKeyboardState, _previousKeyboardState;
        MouseState _currentMouseState, _previousMouseState;



        public InputHandler()
        {
            _currentKeyboardState = Keyboard.GetState();
            _previousKeyboardState = Keyboard.GetState();
            _currentMouseState = Mouse.GetState();
            _previousMouseState = Mouse.GetState();
        }

        public void UpdateStates()
        {
            _previousKeyboardState = _currentKeyboardState;
            _currentKeyboardState = Keyboard.GetState();

            _previousMouseState = _currentMouseState;
            _currentMouseState = Mouse.GetState();
        }

        public bool MouseLeftClick()
        {
            return _previousMouseState.LeftButton == ButtonState.Released &&
                _currentMouseState.LeftButton == ButtonState.Pressed;
        }

        public bool MouseRightClick()
        {
            return _previousMouseState.RightButton == ButtonState.Released &&
                _currentMouseState.RightButton == ButtonState.Pressed;
        }

        public bool KeyPress(Keys k)
        {
            return _previousKeyboardState.IsKeyUp(k) && _currentKeyboardState.IsKeyDown(k);
        }

        public bool KeyHeld(Keys k)
        {
            return _currentKeyboardState.IsKeyDown(k);
        }
    }
}
