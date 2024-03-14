using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.WindowsMobile.DirectX;

namespace LADAGame
{

    public abstract class Entity
    {
        public Transform Transform;

        public abstract void Update();
        public abstract void Draw();
    }

    public sealed class SectorRenderer
    {
        public const float SectorSize = 96;
        public const float ScrollingSpeed = 1;

        private Model road;
        private Material roadMaterial;
        private Model terrain;
        private Material terrainMaterial;

        public float time;
        public Transform sector1, sector2;

        public SectorRenderer()
        {
            road = Model.FromFile("road.mdl");
            roadMaterial.Diffuse = Texture2D.FromFile("road.tex");

            terrain = Model.FromFile("terrain.mdl");
            terrainMaterial.Diffuse = Texture2D.FromFile("grass.tex");

            sector1.Position.Y = -4;
            sector2.Position.Y = -4;
            sector2.Position.Z = SectorSize;
        }

        public void Update()
        {
            sector1.Position.Z -= ScrollingSpeed;
            sector2.Position.Z -= ScrollingSpeed;

            if (sector1.Position.Z + SectorSize < 0)
                sector1.Position.Z = SectorSize;

            if (sector2.Position.Z + SectorSize < 0)
                sector2.Position.Z = SectorSize;
        }

        public void Draw()
        {
            Engine.Current.Graphics.DrawModel(road, sector1, roadMaterial);
            Engine.Current.Graphics.DrawModel(terrain, sector1, terrainMaterial);
            Engine.Current.Graphics.DrawModel(road, sector2, roadMaterial);
            Engine.Current.Graphics.DrawModel(terrain, sector2, terrainMaterial);
        }
    }

    public sealed class Statistics
    {
        public int Score;
        
    }

    public sealed class TrafficSpawner
    {
        public const float SpawnTime = 1;
        public const int MaxCarsInScene = 5;
        
        private float nextSpawn;

        public int CurrentCarCount;

        public void Reset()
        {
            CurrentCarCount = 0;
            nextSpawn = 0;
        }

        public void Update()
        {
            nextSpawn -= 0.1f;

            if (nextSpawn < 0 && CurrentCarCount < MaxCarsInScene)
            {
                TrafficCar traffic = new TrafficCar();
                Game.Current.world.Spawn(traffic);

                CurrentCarCount++;
                nextSpawn = SpawnTime;
            }

            foreach (Entity ent in Game.Current.world.Entities)
            {
                if (ent is TrafficCar && ent.Transform.Position.Z < -10)
                {
                    Game.Current.world.Remove(ent);

                    CurrentCarCount--;
                }
            }
        }
    }

    public sealed class World
    {
        public List<Entity> Entities;
        private List<Entity> entityRemovalList;

        private Sky sky;
        private SectorRenderer renderer;
        private TrafficSpawner spawner;

        public Player Player;
        public Statistics Statistics;

        public World()
        {
            Entities = new List<Entity>();

            entityRemovalList = new List<Entity>();
            sky = new Sky();
            renderer = new SectorRenderer();
            spawner = new TrafficSpawner();

            SetupCamera();
        }

        public void Start()
        {
            Entities.Clear();

            Player = new Player();
            Spawn(Player);

            Statistics = new Statistics();
            spawner.Reset();
        }

        private void SetupCamera()
        {
            Engine.Current.Graphics.Camera.Position.Y = 15;
            Engine.Current.Graphics.Camera.Rotation.X = -50;
        }

        // Returns offset by X, up to 4 lanes
        public float PickLane(int laneNum)
        {
            switch (laneNum)
            {
                case 0:
                    return -5;
                case 1:
                    return -2;
                case 2:
                    return 2;
                case 3:
                    return 5;
            }

            return 0;
        }

        public void Spawn(Entity ent)
        {
            if(ent != null)
                Entities.Add(ent);
        }

        public void Remove(Entity ent)
        {
            entityRemovalList.Add(ent);
        }

        public void Update()
        {
            sky.Update();
            renderer.Update();
            spawner.Update();

            foreach (Entity ent in Entities)
                ent.Update();

            foreach (Entity ent in Game.Current.world.Entities)
            {
                if (ent is TrafficCar)
                {
                    if (Player.Bounds.Intersects(((TrafficCar)ent).Bounds))
                    {
                        // TODO: Damage logic
                        Player.IsDestroyed = true;
                    }
                }
            }

            if(!Player.IsDestroyed)
                Statistics.Score++;

            foreach (Entity ent in entityRemovalList)
                Entities.Remove(ent);

            entityRemovalList.Clear();
        }

        public void Draw()
        {
            sky.Draw();

            renderer.Draw();

            foreach (Entity ent in Entities)
                ent.Draw();
        }

    }
}
