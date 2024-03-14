using System;
using System.Collections.Generic;
using System.Text;

namespace LADAGame
{
    public sealed class Game
    {
        public static Game Current;

        public World world;
        public HUD hud;

        public void Start()
        {
            TrafficCar.Preload();

            hud = new HUD();

            world = new World();
            world.Start();
        }

        public void Update()
        {
            hud.Update();
            world.Update();
        }

        public void Draw()
        {
            world.Draw();
            hud.Draw();

            if (world.Player.IsDestroyed && Engine.Current.Input.GetKeyState(GamepadKey.OK))
                world.Start();
        }
    }
}
