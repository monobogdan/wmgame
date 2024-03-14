using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;

namespace LADAGame
{
    public sealed class Engine
    {
        public static Engine Current;

        public static void Initialize()
        {
            Current = new Engine();
            Current.PostInitialize();
        }

        public Graphics Graphics;
        public Input Input;

        private Form form;

        private Sky sky;

        public static void Log(string fmt, params object[] args)
        {
            DateTime now = DateTime.Now;
            string time = string.Format("{0}:{1}", now.Minute, now.Second);
            string logFmt = string.Format("[{0}]: {1}", now, string.Format(fmt, args));

            Debug.WriteLine(logFmt);
        }

        private Engine()
        {
            form = new Form();
            form.Text = "Game";
            form.Show();

            Game.Current = new Game();
        }

        Model model;
        Texture2D tex;

        private void PostInitialize()
        {
            Log("Initializing subsystems");

            Graphics = new Graphics(form);
            Input = new Input(form);

            model = Model.FromFile("player.mdl");
            tex = Texture2D.FromFile("player.tex");

            Game.Current.Start();
        }

        public void Run()
        {
            while (!form.IsDisposed)
            {
                Application.DoEvents();
                Game.Current.Update();

                Graphics.BeginScene();
                Game.Current.Draw();
                Graphics.EndScene();
            }
        }
    }

    public static class EntryPoint
    {
        public static void Main(string[] args)
        {
            Engine.Initialize();

            Engine.Current.Run();
        }
    }
}
