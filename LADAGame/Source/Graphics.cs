using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;

using Microsoft.WindowsMobile.DirectX;
using Microsoft.WindowsMobile.DirectX.Direct3D;

namespace LADAGame
{
    public struct Vertex
    {
        public const int Size = 32;

        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 UV;
    }

    public struct Transform
    {
        public Vector3 Position;
        public Vector3 Rotation;
    }

    public struct Material
    {
        public Texture2D Diffuse;
        public bool Lit;
        public bool DepthWrite; // False means write to depth
    }

    public sealed class Texture2D
    {
        public string DebugName;
        public Texture Handle;
        public int Width;
        public int Height;


        public static Texture2D FromFile(string fileName)
        {
            string path = "/data/" + fileName;

            if (File.Exists(path))
            {
                using (Stream strm = File.OpenRead(path))
                    return new Texture2D(fileName, strm);
            }

            return null;
        }

        public Texture2D(string debugName, Stream strm)
        {
            BinaryReader reader = new BinaryReader(strm);

            int hdr = reader.ReadInt32();
            int fmt = reader.ReadInt32();

            Width = reader.ReadInt32();
            Height = reader.ReadInt32();

            byte[] data = new byte[Width * Height * 2];
            strm.Read(data, 0, data.Length);

            Handle = new Texture(Engine.Current.Graphics.device, Width, Height, 1, Usage.Lockable, Format.R5G6B5, Pool.VideoMemory);

            int pitch;
            GraphicsStream gs = Handle.LockRectangle(0, LockFlags.None, out pitch);
            gs.Write(data, 0, data.Length);
            Handle.UnlockRectangle(0);

            strm.Close();
        }
    }

    public sealed class Model
    {
        public string DebugName;
        public BoundingBox Bounds;
        public VertexBuffer Buffer;
        public int PrimitiveCount;

        const int Header = 0x1234;

        public static Model FromFile(string fileName)
        {
            string path = "/data/" + fileName;

            if (File.Exists(path))
            {
                using (Stream strm = File.OpenRead(path))
                {
                    byte[] memBuffer = new byte[strm.Length];
                    strm.Read(memBuffer, 0, memBuffer.Length);

                    return new Model(fileName, new MemoryStream(memBuffer));
                }
            }

            return null;
        }

        private static int FloatToFixedPoint(float x)
        {
            return ((int)((x) * 65536.0f));
        }

        public Model(string debugName, Stream strm)
        {
            BinaryReader reader = new BinaryReader(strm);

            int hdr = reader.ReadInt32();
            int numVerts = reader.ReadInt32();
            int vertSize = 20;

            Bounds = new BoundingBox(reader.ReadSingle() * 2, reader.ReadSingle() * 2, reader.ReadSingle() * 2,
                reader.ReadSingle() * 2, reader.ReadSingle() * 2, reader.ReadSingle() * 2);

            PrimitiveCount = numVerts / 3;

            byte[] data = new byte[numVerts * vertSize];
            strm.Read(data, 0, (int)(strm.Length - strm.Position));

            Buffer = new VertexBuffer(Engine.Current.Graphics.device, vertSize * numVerts, Usage.None, VertexFormats.PositionFixed | VertexFormats.Texture1, Pool.SystemMemory);
            GraphicsStream gs = Buffer.Lock(0, Buffer.SizeInBytes, LockFlags.None);
            gs.Write(data, 0, data.Length);
            Buffer.Unlock();

            DebugName = debugName;
        }
    }

    public sealed class MathUtils
    {
        public const float DegToRad = 0.0174533f;

        public static float lerp(float a, float b, float f)
        {
            return a * (1.0f - f) + (b * f);
        }
    }

    public sealed class Camera
    {
        public float FOV;
        public float Near;
        public float Far;

        public Vector3 Position;
        public Vector3 Rotation;

        public Camera()
        {
            FOV = 75.0f;
            Near = 1.0f;
            Far = 350.0f;
        }
    }

    public sealed class Graphics
    {
        internal Device device;
        private Form parentForm;
        private VertexBuffer lineBuffer;

        private float aspectRatio;
        private Sprite spriteBatch;
        private Font primaryFont;

        public int ViewWidth;
        public int ViewHeight;

        public Camera Camera;

        private void CreateDevice()
        {
            PresentParameters pp = new PresentParameters();
            pp.AutoDepthStencilFormat = DepthFormat.D16;
            pp.BackBufferCount = 1;
            pp.BackBufferFormat = Format.R5G6B5;
            pp.BackBufferWidth = 480;
            pp.BackBufferHeight = 640;
            pp.EnableAutoDepthStencil = true;
            pp.FullScreenPresentationInterval = PresentInterval.Default;
            pp.MultiSample = MultiSampleType.None;
            pp.PresentFlag = PresentFlag.None;
            pp.SwapEffect = SwapEffect.Discard;
            pp.Windowed = false;

            ViewWidth = pp.BackBufferWidth;
            ViewHeight = pp.BackBufferHeight;

            device = new Device(0, DeviceType.Default, parentForm.Handle, CreateFlags.None, pp);

            Engine.Log("Created D3DM device");
        }

        private void SetupRenderState()
        {
            device.RenderState.Lighting = false;
            device.RenderState.CullMode = Cull.Clockwise;
        }

        private void AllocateResources()
        {
            lineBuffer = new VertexBuffer(device, 12 * 8, Usage.None, VertexFormats.Position, Pool.SystemMemory);
            Camera = new Camera();

            FontDescription desc = new FontDescription();
            desc.CharSet = CharacterSet.Russian;
            desc.Height = 24;
            desc.OutputPrecision = Precision.Raster;
            desc.PitchAndFamily = PitchAndFamily.DefaultPitch;
            desc.Quality = FontQuality.Default;
            desc.Weight = FontWeight.Bold;

            primaryFont = new Font(device, desc);

            spriteBatch = new Sprite(device);
        }

        public Graphics(Form parentForm)
        {
            this.parentForm = parentForm;

            CreateDevice();
            SetupRenderState();
            AllocateResources();

            aspectRatio = (float)parentForm.ClientSize.Width / (float)parentForm.ClientSize.Height;
        }

        public void BeginScene()
        {
            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, System.Drawing.Color.SkyBlue, 1.0f, 0);
            device.BeginScene();

            device.TextureState[0].MinFilter = TextureFilter.Point;
            device.TextureState[0].MagFilter = TextureFilter.Point;
            device.RenderState.TexturePerspective = true;

            // Prepare projection
            Matrix projMatrix = Matrix.PerspectiveFovLH(Camera.FOV * MathUtils.DegToRad, aspectRatio, Camera.Near, Camera.Far);
            device.SetTransform(TransformType.Projection, projMatrix);
            Matrix viewMatrix = Matrix.Translation(-Camera.Position) * Matrix.RotationX(Camera.Rotation.X * MathUtils.DegToRad);
            device.SetTransform(TransformType.View, viewMatrix);
        }

        public void DrawModel(Model model, Transform transform, Material material)
        {
            Matrix matrix = Matrix.RotationY(transform.Rotation.Y * MathUtils.DegToRad) 
                * Matrix.Translation(transform.Position);
            device.SetTransform(TransformType.World, matrix);
            
            // Setup renderstate
            device.RenderState.ZBufferWriteEnable = !material.DepthWrite;
            device.SetTexture(0, material.Diffuse.Handle);

            device.SetStreamSource(0, model.Buffer, 0);
            device.DrawPrimitives(PrimitiveType.TriangleList, 0, model.PrimitiveCount);
        }

        public int MeasureString(string str)
        {
            System.Drawing.Rectangle rect = new System.Drawing.Rectangle();
            primaryFont.MeasureString(spriteBatch, str, ref rect, DrawTextFormat.None);

            return rect.Width;
        }

        public void DrawString(string str, float x, float y, System.Drawing.Color color)
        {
            spriteBatch.Begin(SpriteFlags.AlphaBlend);
            primaryFont.DrawText(spriteBatch, str, (int)x, (int)y, color);
            spriteBatch.End();
        }

        public void DrawBBox(BoundingBox box)
        {
            GraphicsStream strm = lineBuffer.Lock(0, lineBuffer.SizeInBytes, LockFlags.None);
            strm.Write((float)0); strm.Write((float)-5); strm.Write((float)0);
            strm.Write((float)15); strm.Write((float)-5); strm.Write((float)15);
            lineBuffer.Unlock();

            device.SetStreamSource(0, lineBuffer, 0);
            device.DrawPrimitives(PrimitiveType.LineList, 0, 1);
        }

        public void EndScene()
        {
            device.EndScene();
            device.Present();

            System.Threading.Thread.Sleep(16);
        }
    }
}
