using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Runtime.InteropServices;
using Smd2Bmd;

namespace AssetTool
{
    public sealed class TextureConverter
    {
        public const int Header = 0x1234;

        public static unsafe void Convert(string fileName, Stream stream)
        {
            Console.WriteLine("Converting texture " + fileName);
            
            Bitmap bitmap = (Bitmap)Image.FromStream(stream);
            byte[] pixels = new byte[bitmap.Width * bitmap.Height * 2];

            System.Drawing.Imaging.BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), 
                System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format16bppRgb565);
            Marshal.Copy(data.Scan0, pixels, 0, pixels.Length);

            bitmap.UnlockBits(data);

            using (Stream outStrm = File.Create(Path.GetFileNameWithoutExtension(fileName) + ".tex"))
            {
                BinaryWriter writer = new BinaryWriter(outStrm);

                writer.Write(Header);
                writer.Write(0); // 0 - 565, 1 - RGBA
                writer.Write(bitmap.Width);
                writer.Write(bitmap.Height);

                writer.Write(pixels);
            }
        }
    }

    public struct BoundingBox
    {
        public float MinX, MinY, MinZ;
        public float MaxX, MaxY, MaxZ;
    }

    public sealed class ModelConverter
    {
        public const int Header = 0x1234;

        static BoundingBox CalculateBBox(SmdMesh mesh)
        {
            BoundingBox ret = new BoundingBox();
            ret.MinX = float.PositiveInfinity;
            ret.MinY = float.PositiveInfinity;
            ret.MinZ = float.PositiveInfinity;
            ret.MaxX = float.NegativeInfinity;
            ret.MaxY = float.NegativeInfinity;
            ret.MaxZ = float.NegativeInfinity;

            foreach (SmdTriangle triangle in mesh.Triangles)
            {
                for (int i = 0; i < 3; i++)
                {
                    ret.MinX = Math.Min(ret.MinX, triangle.Verts[i].Position.X);
                    ret.MinY = Math.Min(ret.MinY, triangle.Verts[i].Position.Y);
                    ret.MinZ = Math.Min(ret.MinZ, triangle.Verts[i].Position.Z);

                    ret.MaxX = Math.Max(ret.MaxX, triangle.Verts[i].Position.X);
                    ret.MaxY = Math.Max(ret.MaxY, triangle.Verts[i].Position.Y);
                    ret.MaxZ = Math.Max(ret.MaxZ, triangle.Verts[i].Position.Z);
                }
            }

            return ret;
        }

        private static int FloatToFixedPoint(float x)
        {
            return ((int)((x) * 65536.0f));
        }

        public static void Convert(string fileName, Stream stream)
        {
            Console.WriteLine("Converting mesh " + fileName);

            Smd2Bmd.SmdMesh mesh = new Smd2Bmd.SmdMesh(stream);
            BoundingBox bb = CalculateBBox(mesh);

            using(Stream outStrm = File.Create(Path.GetFileNameWithoutExtension(fileName) + ".mdl"))
            {
                BinaryWriter writer = new BinaryWriter(outStrm);

                writer.Write(Header);
                writer.Write(mesh.Triangles.Count * 3); // Verts count
                
                // BBox
                writer.Write(bb.MinX);
                writer.Write(bb.MinY);
                writer.Write(bb.MinZ);
                writer.Write(bb.MaxX);
                writer.Write(bb.MaxY);
                writer.Write(bb.MaxZ);

                foreach (SmdTriangle triangle in mesh.Triangles)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        writer.Write(FloatToFixedPoint(triangle.Verts[i].Position.X));
                        writer.Write(FloatToFixedPoint(triangle.Verts[i].Position.Y));
                        writer.Write(FloatToFixedPoint(triangle.Verts[i].Position.Z));

                        writer.Write(triangle.Verts[i].UV.X);
                        writer.Write(triangle.Verts[i].UV.Y);
                    }
                }
            }
        }
    }

    class Program
    {

        static void Main(string[] args)
        {
            if(args.Length < 1)
            {
                Console.WriteLine("Usage: AssetTool input file");

                return;
            }

            string fileName = args[0];


            using (Stream strm = File.OpenRead(fileName))
            {
                if(fileName.EndsWith(".png"))
                {
                    TextureConverter.Convert(fileName, strm);
                }

                if(fileName.EndsWith(".smd"))
                {
                    ModelConverter.Convert(fileName, strm);
                }
            }
        }
    }
}
