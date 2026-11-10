using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using System.Numerics;

namespace Szeminarium1
{
    internal static class Program
    {
        private static IWindow graphicWindow;

        private static GL Gl;

        private static uint program;

        private static uint[] vaos = new uint[27];
        private static Matrix4x4[] models = new Matrix4x4[27];
        private static uint indexCount;

        private static readonly string VertexShaderSource = @"
        #version 330 core
        layout (location = 0) in vec3 vPos;
		layout (location = 1) in vec4 vCol;
        
        uniform mat4 uModel;
        uniform mat4 uView;
        uniform mat4 uProjection;

		out vec4 outCol;
        
        void main()
        {
			outCol = vCol;
            gl_Position = uProjection * uView * uModel * vec4(vPos.x, vPos.y, vPos.z, 1.0);
        }
        ";


        private static readonly string FragmentShaderSource = @"
        #version 330 core
        out vec4 FragColor;
		
		in vec4 outCol;

        void main()
        {
            FragColor = outCol;
        }
        ";

        static void Main(string[] args)
        {
            WindowOptions windowOptions = WindowOptions.Default;
            windowOptions.Title = "2-1 - 3D kocka";
            windowOptions.Size = new Silk.NET.Maths.Vector2D<int>(500, 500);

            graphicWindow = Window.Create(windowOptions);

            graphicWindow.Load += GraphicWindow_Load;
            graphicWindow.Update += GraphicWindow_Update;
            graphicWindow.Render += GraphicWindow_Render;

            graphicWindow.Run();
        }

        private static void CheckError(string where)
        {
            bool hadError = false;
            GLEnum error;
            while ((error = Gl.GetError()) != GLEnum.NoError)
            {
                hadError = true;
                Console.WriteLine($"hiba {where}: {error}");
            }
            if (hadError)
                throw new Exception($"hiba itt: {where}");
        }

        private static unsafe void GraphicWindow_Load()
        {
            Gl = graphicWindow.CreateOpenGL();

            Gl.ClearColor(System.Drawing.Color.White);
            CheckError("ClearColor");

            uint vshader = Gl.CreateShader(ShaderType.VertexShader);
            uint fshader = Gl.CreateShader(ShaderType.FragmentShader);
            CheckError("Shaderek letrehozasa");

            Gl.ShaderSource(vshader, VertexShaderSource);
            Gl.CompileShader(vshader);
            Gl.GetShader(vshader, ShaderParameterName.CompileStatus, out int vStatus);
            if (vStatus != (int)GLEnum.True)
                throw new Exception("Vertex shader failed to compile: " + Gl.GetShaderInfoLog(vshader));
            CheckError("Vertex shader forsitas");
            Gl.ShaderSource(fshader, FragmentShaderSource);
            Gl.CompileShader(fshader);
            CheckError("Fragment shader forditas");

            program = Gl.CreateProgram();
            Gl.AttachShader(program, vshader);
            Gl.AttachShader(program, fshader);
            Gl.LinkProgram(program);
            CheckError("Program attach es link");
            Gl.GetProgram(program, GLEnum.LinkStatus, out var status);
            if (status == 0)
            {
                Console.WriteLine($"Error linking shader {Gl.GetProgramInfoLog(program)}");
            }
            Gl.DetachShader(program, vshader);
            Gl.DetachShader(program, fshader);
            Gl.DeleteShader(vshader);
            Gl.DeleteShader(fshader);
            CheckError("Shaderek torlese");

            //melyseg teszt ami a kozelebbi lap takarja a tavolabbit
            Gl.Enable(EnableCap.DepthTest);

            //kamera es projekcio fix ertekkel
            Gl.UseProgram(program);
            Vector3 kameraHely = new Vector3(8f, 5f, 4f);
            Matrix4x4 view = Matrix4x4.CreateLookAt(kameraHely, Vector3.Zero, Vector3.UnitY);
            Matrix4x4 projection = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 4f, 1f, 0.1f, 100f);
            Gl.UniformMatrix4(Gl.GetUniformLocation(program, "uView"), 1, false, (float*)&view);
            Gl.UniformMatrix4(Gl.GetUniformLocation(program, "uProjection"), 1, false, (float*)&projection);
            CheckError("kamera es projekcio");
            //egy kis kocka modellje 6 lap x 4 csucs azaz mind a 27 kocka ezt hasznalja
            float[] vertexArray = new float[] {
                //felso
                -0.5f,  0.5f,  0.5f,
                 0.5f,  0.5f,  0.5f,
                 0.5f,  0.5f, -0.5f,
                -0.5f,  0.5f, -0.5f,

                //elulso
                -0.5f,  0.5f,  0.5f,
                -0.5f, -0.5f,  0.5f,
                 0.5f, -0.5f,  0.5f,
                 0.5f,  0.5f,  0.5f,

                //bal
                -0.5f,  0.5f,  0.5f,
                -0.5f,  0.5f, -0.5f,
                -0.5f, -0.5f, -0.5f,
                -0.5f, -0.5f,  0.5f,

                //also
                -0.5f, -0.5f,  0.5f,
                 0.5f, -0.5f,  0.5f,
                 0.5f, -0.5f, -0.5f,
                -0.5f, -0.5f, -0.5f,

                //hatso
                 0.5f,  0.5f, -0.5f,
                -0.5f,  0.5f, -0.5f,
                -0.5f, -0.5f, -0.5f,
                 0.5f, -0.5f, -0.5f,

                //jobb
                 0.5f,  0.5f,  0.5f,
                 0.5f,  0.5f, -0.5f,
                 0.5f, -0.5f, -0.5f,
                 0.5f, -0.5f,  0.5f,
            };

            float[][] faceColors = new float[][] {
                new float[] { 0.5f, 0.7f, 0.0f, 1.0f}, //felso
                new float[] { 1.0f, 0.5f, 0.0f, 1.0f }, // elulso
                new float[] {1.0f, 1.0f, 1.0f, 1.0f  }, // bal
                new float[] { 1.0f, 1.0f, 0.5f, 1.0f }, // also
                new float[] { 1.0f, 0.5f, 0.5f, 1.0f }, // hatso
                new float[] { 0.0f, 0.5f, 1.0f, 1.0f }, // jobb
            };
            float[] insideColor = new float[] { 0.15f, 0.15f, 0.15f, 1.0f }; //belso lapok

            uint[] indexArray = new uint[] {
                0, 1, 2,
                0, 2, 3,

                4, 5, 6,
                4, 6, 7,

                8, 9, 10,
                8, 10, 11,

                12, 13, 14,
                12, 14, 15,

                16, 17, 18,
                16, 18, 19,

                20, 21, 22,
                20, 22, 23
            };
            indexCount = (uint)indexArray.Length;

            // kozos pozicio buffer: egyszer toltjuk fel, mind a 27 vao ezt hasznalja
            uint vertices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, vertices);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)vertexArray.AsSpan(), GLEnum.StaticDraw);

            uint indices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, indices);
            Gl.BufferData(GLEnum.ElementArrayBuffer, (ReadOnlySpan<uint>)indexArray.AsSpan(), GLEnum.StaticDraw);
            CheckError("kozos bufferek");

            //27 kis kocka ahol x, y, z = -1, 0, 1
            int i = 0;
            for (int x = -1; x <= 1; x++)
                for (int y = -1; y <= 1; y++)
                    for (int z = -1; z <= 1; z++)
                    {
                        vaos[i] = Gl.GenVertexArray();
                        Gl.BindVertexArray(vaos[i]);
                        CheckError("vao letrehozas");

                        Gl.BindBuffer(GLEnum.ArrayBuffer, vertices);
                        Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, null);
                        Gl.EnableVertexAttribArray(0);
                        CheckError("pozi buffer (location 0)");

                        bool[] kivulVan = new bool[] { y == 1, z == 1, x == -1, y == -1, z == -1, x == 1 };
                        float[] colorArray = new float[6 * 4 * 4];
                        for (int face = 0; face < 6; face++)
                        {
                            for (int vertex = 0; vertex < 4; vertex++)
                            {
                                int hely = (face * 4 + vertex) * 4;
                                for (int c = 0; c < 4; c++)
                                {
                                    if (kivulVan[face])
                                        colorArray[hely + c] = faceColors[face][c];
                                    else
                                        colorArray[hely + c] = insideColor[c];
                                }
                            }
                        }

                        uint colors = Gl.GenBuffer();
                        Gl.BindBuffer(GLEnum.ArrayBuffer, colors);
                        Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)colorArray.AsSpan(), GLEnum.StaticDraw);
                        Gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 0, null);
                        Gl.EnableVertexAttribArray(1);

                        CheckError("Szin buffer (location 1)");

                        //a kozos index buffert kotom be ami a vao allapotanak resze
                        Gl.BindBuffer(GLEnum.ElementArrayBuffer, indices);

                        float lepes = 1.1f; //kocka el 1 szoval 0.1 res marad
                        models[i] = Matrix4x4.CreateTranslation(x * lepes, y * lepes, z * lepes);
                        i++;
                    }
            Gl.BindVertexArray(0);
            CheckError("27 vao letrehozasa");

        }

        private static void GraphicWindow_Update(double deltaTime)
        {
        }

        private static unsafe void GraphicWindow_Render(double deltaTime)
        {
            Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            CheckError("Clear");

            Gl.UseProgram(program);
            CheckError("Useprogram");

            for (int i = 0; i < 27; i++)
            {
                Matrix4x4 model = models[i];
                Gl.UniformMatrix4(Gl.GetUniformLocation(program, "uModel"), 1, false, (float*)&model);

                Gl.BindVertexArray(vaos[i]);
                Gl.DrawElements(GLEnum.Triangles, indexCount, GLEnum.UnsignedInt, null);
            }
            Gl.BindVertexArray(0);
            CheckError("Draw elements");
        }

    }
}
