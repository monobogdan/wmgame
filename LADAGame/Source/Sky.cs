using System;
using System.Collections.Generic;
using System.Text;

namespace LADAGame
{
    public sealed class Sky
    {
        private Model sphere;
        private Material tex;
        private Transform transform;

        public Sky()
        {
            sphere = Model.FromFile("sky.mdl");
            tex.Diffuse = Texture2D.FromFile("sky.tex");
            tex.DepthWrite = true;

            transform = new Transform();
            transform.Position.Y = -0.3f;
        }

        public void Update()
        {
            transform.Rotation.Y += 0.1f;
        }

        public void Draw()
        {
            Engine.Current.Graphics.DrawModel(sphere, transform, tex);
        }
    }
}
