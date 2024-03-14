using System;
using System.Collections.Generic;
using System.Text;

namespace LADAGame
{
    public sealed class Player : Entity
    {
        private Model model;
        private Material tex;

        public bool IsDestroyed;

        public BoundingBox Bounds;

        public Player()
        {
            model = Model.FromFile("player.mdl");
            tex.Diffuse = Texture2D.FromFile("player.tex");

            Transform.Rotation.Y = 180;
            Transform.Position.Y = -3;
            Transform.Position.Z = 15;
        }

        public override void Update()
        {
            float hVel = Engine.Current.Input.GetKeyState(GamepadKey.Left) ? -1 : (Engine.Current.Input.GetKeyState(GamepadKey.Right) ? 1 : 0);

            Transform.Position.X += hVel * 0.7f;
            Transform.Rotation.Y = MathUtils.lerp(Transform.Rotation.Y, 180 + (hVel * 35), 0.1f);

            Bounds = model.Bounds;
            Bounds.X += Transform.Position.X;
            Bounds.Y += Transform.Position.Y;
            Bounds.Z += Transform.Position.Z;
        }

        public override void Draw()
        {
            if (!IsDestroyed)
                Engine.Current.Graphics.DrawModel(model, Transform, tex);
        }
    }
}
