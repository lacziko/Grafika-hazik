using Silk.NET.OpenGL;
using Silk.NET.Vulkan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GrafikaSzeminarium
{
    internal class ModelObjectDescriptor : IDisposable
    {
        private bool disposedValue;

        public uint Vao { get; private set; }
        public uint Vertices { get; private set; }
        public uint Colors { get; private set; }
        public uint Indices { get; private set; }
        public uint IndexArrayLength { get; private set; }

        private GL Gl;

        public const int DeszkaDarab = 18;
        public const float DeszkaSzelesseg = 1f;
        public const float DeszkaMagassag = 2f;

        public unsafe static ModelObjectDescriptor CreatePlank(GL Gl, bool dontottNormal)
        {
            uint vao = Gl.GenVertexArray();
            Gl.BindVertexArray(vao);

            //10 fok: a szomszedos deszkak 20 fokra vannak, ennek a fele
            float felSzog = (float)(Math.PI / DeszkaDarab);
            //meroleges eseten (0,0,1)
            ///dontott eseten a szeleken 10 fokkal kifele
            float nx = dontottNormal ? (float)Math.Sin(felSzog) : 0f;
            float nz = dontottNormal ? (float)Math.Cos(felSzog) : 1f;
            float w = DeszkaSzelesseg / 2;
            float h = DeszkaMagassag / 2;

            // counter clockwise is front facing, pozicio + normalis
            float[] vertexArray = new float[] {
                -w, -h, 0f, -nx, 0f, nz,
                 w, -h, 0f,  nx, 0f, nz,
                 w,  h, 0f,  nx, 0f, nz,
                -w,  h, 0f, -nx, 0f, nz,
            };

            float[] colorArray = new float[] {
                0.6f, 0.4f, 0.2f, 1.0f,
                0.6f, 0.4f, 0.2f, 1.0f,
                0.6f, 0.4f, 0.2f, 1.0f,
                0.6f, 0.4f, 0.2f, 1.0f,
            };

            uint[] indexArray = new uint[] {
                0, 1, 2,
                0, 2, 3
            };

            uint vertices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, vertices);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)vertexArray.AsSpan(), GLEnum.StaticDraw);
            uint offsetPos = 0;
            uint offsetNormals = offsetPos + 3 * sizeof(float);
            uint vertexSize = offsetNormals + 3 * sizeof(float);
            Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, vertexSize, (void*)offsetPos);
            Gl.EnableVertexAttribArray(0);
            Gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, true, vertexSize, (void*)offsetNormals);
            Gl.EnableVertexAttribArray(2);
            Gl.BindBuffer(GLEnum.ArrayBuffer, 0);

            uint colors = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, colors);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)colorArray.AsSpan(), GLEnum.StaticDraw);
            Gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 0, null);
            Gl.EnableVertexAttribArray(1);
            Gl.BindBuffer(GLEnum.ArrayBuffer, 0);

            uint indices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, indices);
            Gl.BufferData(GLEnum.ElementArrayBuffer, (ReadOnlySpan<uint>)indexArray.AsSpan(), GLEnum.StaticDraw);
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, 0);

            return new ModelObjectDescriptor() { Vao = vao, Vertices = vertices, Colors = colors, Indices = indices, IndexArrayLength = (uint)indexArray.Length, Gl = Gl };
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {

                }
                // always unbound the vertex buffer first, so no halfway results are displayed by accident
                Gl.DeleteBuffer(Vertices);
                Gl.DeleteBuffer(Colors);
                Gl.DeleteBuffer(Indices);
                Gl.DeleteVertexArray(Vao);

                disposedValue = true;
            }
        }

        ~ModelObjectDescriptor()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
