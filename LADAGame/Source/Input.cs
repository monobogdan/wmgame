using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace LADAGame
{
    public enum GamepadKey
    {
        Left,
        Right,
        Up,
        Down,
        OK,
        Count
    }

    public sealed class Input
    {
        private bool[] keyState;

        public Input(Form parentForm)
        {
            keyState = new bool[(int)GamepadKey.Count];

            parentForm.KeyPreview = true;

            parentForm.KeyDown += new KeyEventHandler(OnKeyDown);
            parentForm.KeyUp += new KeyEventHandler(OnKeyUp);
        }

        private GamepadKey ResolveKeyCode(Keys key)
        {
            GamepadKey k = GamepadKey.Count;

            switch (key)
            {
                case Keys.Left:
                    k = GamepadKey.Left;
                    break;
                case Keys.Right:
                    k = GamepadKey.Right;
                    break;
                case Keys.Up:
                    k = GamepadKey.Up;
                    break;
                case Keys.Down:
                    k = GamepadKey.Down;
                    break;
                case Keys.Return:
                    k = GamepadKey.OK;
                    break;
            }

            return k;
        }

        void OnKeyUp(object sender, KeyEventArgs e)
        {
            GamepadKey key = ResolveKeyCode(e.KeyCode);

            if (key != GamepadKey.Count)
                SetKeyState(key, false);
        }

        void OnKeyDown(object sender, KeyEventArgs e)
        {
            GamepadKey key = ResolveKeyCode(e.KeyCode);

            if (key != GamepadKey.Count)
                SetKeyState(key, true);
        }

        public bool GetKeyState(GamepadKey key)
        {
            return keyState[(int)key];
        }

        private void SetKeyState(GamepadKey key, bool state)
        {
            keyState[(int)key] = state;
        }
    }
}
