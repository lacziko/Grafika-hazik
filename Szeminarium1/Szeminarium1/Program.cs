using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace Szeminarium1
{
    internal static class Program
    {
        private static IWindow graphicWindow;

        private static GL Gl;

        private static uint program;

        private static readonly string VertexShaderSource = @"
        #version 330 core
        layout (location = 0) in vec3 vPos;
		layout (location = 1) in vec4 vCol;

		out vec4 outCol;
        
        void main()
        {
			outCol = vCol;
            gl_Position = vec4(vPos.x, vPos.y, vPos.z, 1.0);
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
            windowOptions.Title = "1. szeminárium - háromszög";
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

        private static void GraphicWindow_Load()
        {
            // egszeri beallitasokat
            //Console.WriteLine("Loaded");

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
        }

        private static void GraphicWindow_Update(double deltaTime)
        {
            // NO GL
            // make it threadsave
            //Console.WriteLine($"Update after {deltaTime} [s]");
        }

        private static unsafe void GraphicWindow_Render(double deltaTime)
        {
            //Console.WriteLine($"Render after {deltaTime} [s]");

            Gl.Clear(ClearBufferMask.ColorBufferBit);
            CheckError("Clear");

            uint vao = Gl.GenVertexArray();
            Gl.BindVertexArray(vao);
            CheckError("vao letrehozas");

            //ha atteszem a floatot double a kod nem fordul le
            //Cannot convert type 'System.Span<double>' to 'System.ReadOnlySpan<float>'

            //ha kitorlok egy csucsot a kod lefordul, hibat nem kapok de a haromszog ofc eltorzul
            //mert az osszes utana jovo csucs elcsuszik
            float[] vertexArray = new float[] {
                -0.5f, -0.5f, 0.0f,
                +0.5f, -0.5f, 0.0f,
                 0.0f, +0.5f, 0.0f,
                 1f, 1f, 0f
            };

            //ha kihagyok egy sort/oszlopot, azon a helyen fekete lesz,
            //definialatlan viselkedes, nem fix mindig fekete
            //ha 1.0fre teszek egy full sort, azon a helyen feher lesz
            float[] colorArray = new float[] {
                1.0f, 1.0f, 0.5f, 1.0f,
                1.0f, 0.5f, 1.0f, 1.0f,
                1.0f, 1.0f, 1.0f, 1.0f,
                0.5f, 1.0f, 1.0f, 1.0f
            };

            //ha egy indexet kicserelek egy olyanra ami nem letezik akkor torzul a haromszog,
            //mert egy masik pontba huzza
            //pl 5 index ami nem oszthato 3mal -> a csonka utolso haromszog elmarad
            uint[] indexArray = new uint[] { 
                0, 1, 2,
                2, 1, 3
            };


            uint vertices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, vertices); //ha ezt torlom a kod lefut, de full feher kepernyo, nincs rajzolas
            //ha felcserelem az elso kettot akkor is lefut de akkor is full feher kepernyo
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)vertexArray.AsSpan(), GLEnum.StaticDraw);
            //ha torlom a bufferdatat, nem rajzol semmit mert ures a buffer
            Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, null);
            //ha 3 helyett 2ot irok a GPU kettesevel olvassa a koordinatakat vagyis az alakzat szetesik
            Gl.EnableVertexAttribArray(0); //ha  felcserlem a vertexAttribPointerrel nem tortenik semmi
            //mert az egyik egy flaget allit, a masik a bufferbol olvas es ezek fuggetlenek egymastol
            //ha torlom teljesen a poziciok nem jutnak el a shaderhez es nem latszik semmi
            //es ha 0 helyett 100at irok ami invalid value lefut de nem ad hibat es az abra is szetesik
            CheckError("pozi buffer (location 0)");

            uint colors = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, colors);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)colorArray.AsSpan(), GLEnum.StaticDraw);
            Gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 0, null);
            Gl.EnableVertexAttribArray(1);
            //ha EnableVertexAttribArray(2) teszek az 1 helyett akkor a szin attributum kikapcsolva marad
            //es fekete lesz az alakzat
            //ha torlom teljesen akkor is fekete lesz mert nem kapcsolodik be
            CheckError("Szin buffer (location 1)");


            uint indices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, indices);
            Gl.BufferData(GLEnum.ElementArrayBuffer, (ReadOnlySpan<uint>)indexArray.AsSpan(), GLEnum.StaticDraw);

            Gl.BindBuffer(GLEnum.ArrayBuffer, 0);

            Gl.UseProgram(program);
            //ha ezt torlom nem rajzolodik ki semmi
            CheckError("Useprogram");
            
            Gl.DrawElements(GLEnum.Triangles, (uint)indexArray.Length, GLEnum.UnsignedInt, null); // we used element buffer
            //h kicserleem a BindBufferrel akkor nem lesz index buffer es vagy osszeomlik a kep vagy torzul
            CheckError("Draw elements");
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, 0);
            Gl.BindVertexArray(vao);

            // always unbound the vertex buffer first, so no halfway results are displayed by accident
            Gl.DeleteBuffer(vertices);
            Gl.DeleteBuffer(colors);
            Gl.DeleteBuffer(indices);
            Gl.DeleteVertexArray(vao);
            CheckError("takaritas");
        }
    }
}
