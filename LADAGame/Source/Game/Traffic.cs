using System;
using System.Collections.Generic;
using System.Text;

namespace LADAGame
{
    public sealed class TrafficCar : Entity
    {
        public const float BaseSpeed = 1.9f;
        public const int ZOffset = 75;
        public const int ZOffsetMax = 95;

        private static Model[] PreloadedCars;
        private static Material[] PreloadedMaterials;
        private static float[] SpeedBias = { 1.0f, 1.3f, 1.7f, 1.9f };

        private Model model;
        private Material material;
        private int selectedBias;

        public BoundingBox Bounds;

        private static void LoadTrafficModel(int idx, string name)
        {
            PreloadedCars[idx] = Model.FromFile(name + ".mdl");
            PreloadedMaterials[idx].Diffuse = Texture2D.FromFile(name + ".tex");
        }

        public static void Preload()
        {
            PreloadedCars = new Model[1];
            PreloadedMaterials = new Material[1];

            LoadTrafficModel(0, "traffic1");
        }

        public TrafficCar()
        {
            Transform.Position.X = Game.Current.world.PickLane(new Random().Next(0, 4));
            Transform.Position.Y = Game.Current.world.Player.Transform.Position.Y;
            Transform.Position.Z = new Random().Next(ZOffset, ZOffsetMax);

            selectedBias = new Random().Next(0, SpeedBias.Length - 1);

            int carModel = new Random().Next(0, PreloadedCars.Length - 1);
            model = PreloadedCars[carModel];
            material = PreloadedMaterials[carModel];

        }

        public override void Update()
        {
            Transform.Position.Z -= BaseSpeed * SpeedBias[selectedBias];
        
            Bounds = model.Bounds;
            Bounds.X += Transform.Position.X;
            Bounds.Y += Transform.Position.Y;
            Bounds.Z += Transform.Position.Z;
        }

        public override void Draw()
        {
            Engine.Current.Graphics.DrawModel(model, Transform, material);
        }
    }
}
