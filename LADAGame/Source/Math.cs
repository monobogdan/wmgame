using System;
using System.Collections.Generic;
using System.Text;

namespace LADAGame
{
    public struct BoundingBox
    {
        public float X, Y, Z;
        public float X2, Y2, Z2;

        public BoundingBox(float x, float y, float z, float x2, float y2, float z2)
        {
            X = x;
            Y = y;
            Z = z;
            X2 = x2;
            Y2 = y2;
            Z2 = z2;
        }

        public bool Intersects(BoundingBox box)
        {
            return (X < box.X + box.X2 && Y < box.Y + box.Y2 && Z < box.Z + box.Z2 && box.X < X + X2 && box.Y < Y + Y2 && box.Z < Z + Z2);
        }
    }
}
