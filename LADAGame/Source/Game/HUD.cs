using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace LADAGame
{
    public sealed class HUD
    {
        private Color StatsColor = Color.Red;
        private string RestartString = "Press Return to restart";

        public void Update()
        {

        }

        public void Draw()
        {
            string scoreFmt = string.Format("Score: {0} x{1}", Game.Current.world.Statistics.Score, 1);
            Engine.Current.Graphics.DrawString(scoreFmt, 15, 15, StatsColor);

            if (Game.Current.world.Player.IsDestroyed)
            {
                int measure = Engine.Current.Graphics.MeasureString(RestartString);
                Engine.Current.Graphics.DrawString("Press Return to restart", Engine.Current.Graphics.ViewWidth / 2 - (measure / 2), Engine.Current.Graphics.ViewHeight / 2, StatsColor);
            }
        }
    }
}
