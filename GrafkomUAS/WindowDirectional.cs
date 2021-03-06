using System;
using System.Collections.Generic;
using System.IO;
using LearnOpenTK.Common;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
//using Pertemuan7;

namespace GrafkomUAS
{
    class WindowDirectional : GameWindow
    {
        private Mesh mesh0;
        private Mesh mesh1;

        private Mesh lamp0;

        Dictionary<string, List<Material>> materials_dict = new Dictionary<string, List<Material>>();

        private Camera _camera;
        private Vector3 _objectPos;

        private Vector2 _lastMousePosition;
        private bool _firstMove;
        private bool postprocessing = false;

        //Light
        List<Light> lights = new List<Light>();

        //Frame Buffers
        int fbo;
        int depthMapfbo;

        //Shader
        Shader shader;
        Shader screenShader;
        Shader depthShader;
        Shader debugShader;

        //Quad Screen
        float[] quadVertices = { // vertex attributes for a quad that fills the entire screen in Normalized Device Coordinates.
        // positions   // texCoords
        -1.0f,  1.0f,  0.0f, 1.0f,
        -1.0f, -1.0f,  0.0f, 0.0f,
         1.0f, -1.0f,  1.0f, 0.0f,

        -1.0f,  1.0f,  0.0f, 1.0f,
         1.0f, -1.0f,  1.0f, 0.0f,
         1.0f,  1.0f,  1.0f, 1.0f
        };
        int _vao;
        int _vbo;
        int texColorBuffer;
        

        float[] quadVerticesDepth = {
            // positions        // texture Coords
            -1.0f,  1.0f, 0.0f,  0.0f, 1.0f,
            -1.0f, -1.0f, 0.0f, 0.0f, 0.0f,
             1.0f, -1.0f, 0.0f, 1.0f, 0.0f,

            -1.0f,  1.0f, 0.0f, 0.0f, 1.0f,
             1.0f, -1.0f, 0.0f, 1.0f, 0.0f,
             1.0f,  1.0f, 0.0f, 1.0f, 1.0f
        };
        int _vao_depth;
        int _vbo_depth;
        int depthMap;
        const int SHADOW_WIDTH = 1024, SHADOW_HEIGHT = 1024;


        public WindowDirectional(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
        {
        }

        private Matrix4 generateArbRotationMatrix(Vector3 axis, Vector3 center, float degree)
        {
            var rads = MathHelper.DegreesToRadians(degree);

            var secretFormula = new float[4, 4] {
                { (float)Math.Cos(rads) + (float)Math.Pow(axis.X, 2) * (1 - (float)Math.Cos(rads)), axis.X* axis.Y * (1 - (float)Math.Cos(rads)) - axis.Z * (float)Math.Sin(rads),    axis.X * axis.Z * (1 - (float)Math.Cos(rads)) + axis.Y * (float)Math.Sin(rads),   0 },
                { axis.Y * axis.X * (1 - (float)Math.Cos(rads)) + axis.Z * (float)Math.Sin(rads),   (float)Math.Cos(rads) + (float)Math.Pow(axis.Y, 2) * (1 - (float)Math.Cos(rads)), axis.Y * axis.Z * (1 - (float)Math.Cos(rads)) - axis.X * (float)Math.Sin(rads),   0 },
                { axis.Z * axis.X * (1 - (float)Math.Cos(rads)) - axis.Y * (float)Math.Sin(rads),   axis.Z * axis.Y * (1 - (float)Math.Cos(rads)) + axis.X * (float)Math.Sin(rads),   (float)Math.Cos(rads) + (float)Math.Pow(axis.Z, 2) * (1 - (float)Math.Cos(rads)), 0 },
                { 0, 0, 0, 1}
            };
            var secretFormulaMatrix = new Matrix4(
                new Vector4(secretFormula[0, 0], secretFormula[0, 1], secretFormula[0, 2], secretFormula[0, 3]),
                new Vector4(secretFormula[1, 0], secretFormula[1, 1], secretFormula[1, 2], secretFormula[1, 3]),
                new Vector4(secretFormula[2, 0], secretFormula[2, 1], secretFormula[2, 2], secretFormula[2, 3]),
                new Vector4(secretFormula[3, 0], secretFormula[3, 1], secretFormula[3, 2], secretFormula[3, 3])
            );

            return secretFormulaMatrix;
        }

        protected override void OnLoad()
        {
            GL.ClearColor(0.2f, 0.2f, 0.5f, 1.0f);
            GL.Enable(EnableCap.DepthTest);

            //Shader
            shader = new Shader("C:/Users/vince/source/repos/GrafkomUAS/GrafkomUAS/Shaders/shader.vert",
                "C:/Users/vince/source/repos/GrafkomUAS/GrafkomUAS/Shaders/lighting.frag");
            shader.Use();

            //Screen Shader
            screenShader = new Shader("C:/Users/vince/source/repos/GrafkomUAS/GrafkomUAS/Shaders/PostProcessing.vert",
                "C:/Users/vince/source/repos/GrafkomUAS/GrafkomUAS/Shaders/PostProcessing.frag");
            screenShader.Use();
            screenShader.SetInt("screenTexture", 0);
            //Frame Buffers
            GL.GenFramebuffers(1, out fbo);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);

            //Add Texture to Frame Buffer
            GL.GenTextures(1, out texColorBuffer);
            Console.WriteLine("Tex Color FBO: " + fbo);
            Console.WriteLine("TexColorBuffer: " + texColorBuffer);
            GL.BindTexture(TextureTarget.Texture2D, texColorBuffer);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, 800, 600, 0, PixelFormat.Rgb, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D
                , texColorBuffer, 0);
            //Render Buffer
            int rbo;
            GL.GenRenderbuffers(1, out rbo);
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, rbo);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, 800, 600);
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, rbo);

            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment,
                RenderbufferTarget.Renderbuffer, rbo);

            if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) == FramebufferErrorCode.FramebufferComplete)
            {
                Console.WriteLine("Screen Frame Buffer Created");
            }
            else
            {
                Console.WriteLine("Screen Frame Buffer NOT complete");
            }
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            //Added Depth Mapping shadow
            depthShader = new Shader("C:/Users/vince/source/repos/GrafkomUAS/GrafkomUAS/Shaders/ShadowMapping.vert",
                "C:/Users/vince/source/repos/GrafkomUAS/GrafkomUAS/Shaders/ShadowMapping.frag");
            depthShader.Use();

            

            //Depth FBO
            GL.GenFramebuffers(1, out depthMapfbo);
            //Create Depth Texture
            GL.GenTextures(1, out depthMap);
            Console.WriteLine("Depth Map FBO: " + depthMapfbo);
            Console.WriteLine("Depth Map: " + depthMap);
            GL.BindTexture(TextureTarget.Texture2D, depthMap);
            GL.TexImage2D(TextureTarget.Texture2D,
                    0,
                    PixelInternalFormat.DepthComponent16,
                    SHADOW_WIDTH,
                    SHADOW_HEIGHT,
                    0,
                    PixelFormat.DepthComponent,
                    PixelType.Float,
                    IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, depthMapfbo);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, depthMap, 0);
            GL.DrawBuffer(DrawBufferMode.None);
            GL.ReadBuffer(ReadBufferMode.None);
            if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) == FramebufferErrorCode.FramebufferComplete)
            {
                Console.WriteLine("Depth Frame Buffer Created");
            }
            else
            {
                Console.WriteLine("Depth Frame Buffer NOT complete");
            }
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            debugShader = new Shader("C:/Users/vince/source/repos/GrafkomUAS/GrafkomUAS/Shaders/debugQuad.vert",
                "C:/Users/vince/source/repos/GrafkomUAS/GrafkomUAS/Shaders/debugQuad.frag");
            debugShader.Use();
            debugShader.SetInt("depthMap", 0);

            //Initialize default material
            InitDefaultMaterial();

            //Screen Quad
            GL.GenVertexArrays(1, out _vao);
            GL.GenBuffers(1, out _vbo);
            GL.BindVertexArray(_vbo);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, quadVertices.Length * sizeof(float), quadVertices, BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));

            //Screen Quad
            GL.GenVertexArrays(1, out _vao_depth);
            GL.GenBuffers(1, out _vbo_depth);
            GL.BindVertexArray(_vbo_depth);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo_depth);
            GL.BufferData(BufferTarget.ArrayBuffer, quadVerticesDepth.Length * sizeof(float), quadVerticesDepth, BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));

            //Light Position
            lights.Add(new DirectionLight(new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0.05f, 0.05f, 0.05f),
                new Vector3(1.0f, 1.0f, 1.0f), new Vector3(1.0f, 1.0f, 1.0f), new Vector3(-0.3f, -0.3f, -0.3f)));

            mesh0 = LoadObjFile("C:/Users/vince/source/repos/GrafkomUAS/GrafkomUAS/Resources/floor.obj");
            mesh0.setupObject(1.0f, 1.0f);
            mesh0.translate(new Vector3(0f, -0.5f, 0f));

            mesh1 = LoadObjFile("C:/Users/vince/source/repos/GrafkomUAS/GrafkomUAS/Resources/oldEdited.obj");
            mesh1.setupObject(1.0f, 1.0f);
            mesh1.translate(new Vector3(0f, 0.0f, 0.0f));

            lamp0 = LoadObjFile("C:/Users/vince/source/repos/GrafkomUAS/GrafkomUAS/Resources/KleeBomb.obj", false);
            lamp0.setupObject(1.0f, 1.0f);
            lamp0.translate(new Vector3(0.3f, 0.3f, 0.3f));
            lights[0].Position = lamp0.getTransform().ExtractTranslation();



            var _cameraPosInit = new Vector3(0.3f, 0.3f, 0.3f);
            _camera = new Camera(_cameraPosInit, Size.X / (float)Size.Y);
            _camera.Yaw -= 90f;
            CursorGrabbed = true;
            base.OnLoad();
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            Console.WriteLine(_camera.Position);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            if (GLFW.GetTime() > 0.02)
            {
                //LampRevolution();
                GLFW.SetTime(0.0);
            }

            //Depth Map Rendering
            
            //Process Depth Shader
            float near_plane = 1.0f, far_plane = 7.5f;
            Matrix4 lightProjection, lightView;
            Matrix4.CreateOrthographic(-10.0f, 10.0f, near_plane, far_plane, out lightProjection);
            lightView = Matrix4.LookAt(lights[0].Position, new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 1.0f, 0.0f));
            Matrix4 lightSpaceMatrix = lightProjection * lightView;

            if (postprocessing)
            {
                //GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);
                //GL.Enable(EnableCap.DepthTest);
                //GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                ////Textured Rendering
                //GL.ActiveTexture(TextureUnit.Texture0);
                //shader.Use();
                //for (int i = 0; i < lights.Count; i++)
                //{
                //    mesh0.calculateTextureRender(_camera, lights[i], i);
                //    mesh1.calculateTextureRender(_camera, lights[i], i);
                //    lamp0.calculateTextureRender(_camera, lights[i], i);
                //}
                //GL.BindVertexArray(0);

                ////Default FrameBuffer
                //GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                //GL.Disable(EnableCap.DepthTest);
                //GL.Clear(ClearBufferMask.ColorBufferBit);

                //screenShader.Use();
                //screenShader.SetInt("screenTexture", texColorBuffer);
                //GL.BindVertexArray(_vao);
                //GL.BindTexture(TextureTarget.Texture2D, texColorBuffer);
                //GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

                GL.Viewport(0, 0, SHADOW_WIDTH, SHADOW_HEIGHT);
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, depthMapfbo);
                GL.Enable(EnableCap.DepthTest);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                //Textured Rendering
                GL.ActiveTexture(TextureUnit.Texture0);
                depthShader.Use();
                depthShader.SetMatrix4("lightSpaceMatrix", lightSpaceMatrix);
                for (int i = 0; i < lights.Count; i++)
                {
                    //mesh0.calculateDepthRender(_camera, lights[i], i);
                    mesh1.calculateDepthRender(_camera, lights[i], i);
                    lamp0.calculateDepthRender(_camera, lights[i], i);
                }
                //Default FrameBuffer
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                GL.Viewport(0, 0, 800, 600);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                debugShader.Use();
                debugShader.SetInt("depthMap", 0);
                GL.BindVertexArray(_vao_depth);
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, depthMap);
                GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

                GL.Viewport(0, 0, 800, 600);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                shader.Use();
                shader.SetMatrix4("lightSpaceMatrix", lightSpaceMatrix);
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                GL.Enable(EnableCap.DepthTest);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                //Textured Rendering
                GL.ActiveTexture(TextureUnit.Texture0);
                for (int i = 0; i < lights.Count; i++)
                {
                    mesh0.calculateTextureRender(_camera, lights[i], i);
                    mesh1.calculateTextureRender(_camera, lights[i], i);
                    lamp0.calculateTextureRender(_camera, lights[i], i);
                }

            }
            else
            {
                GL.Viewport(0, 0, 800, 600);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                shader.Use();
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                GL.Enable(EnableCap.DepthTest);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                //Textured Rendering
                GL.ActiveTexture(TextureUnit.Texture0);
                for (int i = 0; i < lights.Count; i++)
                {
                    mesh0.calculateTextureRender(_camera, lights[i], i);
                    mesh1.calculateTextureRender(_camera, lights[i], i);
                    lamp0.calculateTextureRender(_camera, lights[i], i);
                }
            }


            SwapBuffers();

            base.OnRenderFrame(args);
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            const float cameraSpeed = 1.5f;
            // Escape keyboard
            if (KeyboardState.IsKeyDown(Keys.Escape))
            {
                Close();
            }
            // Zoom in
            if (KeyboardState.IsKeyDown(Keys.I))
            {
                _camera.Fov -= 0.05f;
            }
            // Zoom out
            if (KeyboardState.IsKeyDown(Keys.O))
            {
                _camera.Fov += 0.05f;
            }

            // Rotasi X di pivot Camera
            // Lihat ke atas (T)
            if (KeyboardState.IsKeyDown(Keys.T))
            {
                _camera.Pitch += 0.05f;
            }
            // Lihat ke bawah (G)
            if (KeyboardState.IsKeyDown(Keys.G))
            {
                _camera.Pitch -= 0.05f;
            }
            // Rotasi Y di pivot Camera
            // Lihat ke kiri (F)
            if (KeyboardState.IsKeyDown(Keys.F))
            {
                _camera.Yaw -= 0.05f;
            }
            // Lihat ke kanan (H)
            if (KeyboardState.IsKeyDown(Keys.H))
            {
                _camera.Yaw += 0.05f;
            }

            // Maju (W)
            if (KeyboardState.IsKeyDown(Keys.W))
            {
                _camera.Position += _camera.Front * cameraSpeed * (float)args.Time;
            }
            // Mundur (S)
            if (KeyboardState.IsKeyDown(Keys.S))
            {
                _camera.Position -= _camera.Front * cameraSpeed * (float)args.Time;
            }
            // Kiri (A)
            if (KeyboardState.IsKeyDown(Keys.A))
            {
                _camera.Position -= _camera.Right * cameraSpeed * (float)args.Time;
            }
            // Kanan (D)
            if (KeyboardState.IsKeyDown(Keys.D))
            {
                _camera.Position += _camera.Right * cameraSpeed * (float)args.Time;
            }
            // Naik (Spasi)
            if (KeyboardState.IsKeyDown(Keys.Space))
            {
                _camera.Position += _camera.Up * cameraSpeed * (float)args.Time;
            }
            // Turun (Ctrl)
            if (KeyboardState.IsKeyDown(Keys.LeftControl))
            {
                _camera.Position -= _camera.Up * cameraSpeed * (float)args.Time;
            }
            // Blinn
            if (KeyboardState.IsKeyReleased(Keys.F1))
            {
                mesh0.setBlinn(!mesh0.getBlinn());
                lamp0.setBlinn(!lamp0.getBlinn());

            }
            if (KeyboardState.IsKeyReleased(Keys.F2))
            {
                mesh0.setGamma(!mesh0.getGamma());
                lamp0.setGamma(!lamp0.getGamma());
            }
            if (KeyboardState.IsKeyReleased(Keys.F3))
            {
                postprocessing = !postprocessing;
            }

            const float _rotationSpeed = 0.02f;
            // K (atas -> Rotasi sumbu x)
            if (KeyboardState.IsKeyDown(Keys.K))
            {
                _objectPos *= 2;
                var axis = new Vector3(1, 0, 0);
                _camera.Position -= _objectPos;
                _camera.Pitch -= _rotationSpeed;
                _camera.Position = Vector3.Transform(_camera.Position,
                    generateArbRotationMatrix(axis, _objectPos, _rotationSpeed).ExtractRotation());
                _camera.Position += _objectPos;

                _camera._front = -Vector3.Normalize(_camera.Position - _objectPos);
                _objectPos /= 2;
            }
            // M (bawah -> Rotasi sumbu x)
            if (KeyboardState.IsKeyDown(Keys.M))
            {
                _objectPos *= 2;
                var axis = new Vector3(1, 0, 0);
                _camera.Position -= _objectPos;
                _camera.Pitch += _rotationSpeed;
                _camera.Position = Vector3.Transform(_camera.Position,
                    generateArbRotationMatrix(axis, _objectPos, -_rotationSpeed).ExtractRotation());
                _camera.Position += _objectPos;

                _camera._front = -Vector3.Normalize(_camera.Position - _objectPos);
                _objectPos /= 2;
            }

            // N (kiri -> Rotasi sumbu y)
            if (KeyboardState.IsKeyDown(Keys.N))
            {
                _objectPos *= 2;
                var axis = new Vector3(0, 1, 0);
                _camera.Position -= _objectPos;
                _camera.Yaw += _rotationSpeed;
                _camera.Position = Vector3.Transform(_camera.Position,
                    generateArbRotationMatrix(axis, _objectPos, _rotationSpeed).ExtractRotation());
                _camera.Position += _objectPos;

                _camera._front = -Vector3.Normalize(_camera.Position - _objectPos);
                _objectPos /= 2;
            }
            // , (kanan -> Rotasi sumbu y)
            if (KeyboardState.IsKeyDown(Keys.Comma))
            {
                _objectPos *= 2;
                var axis = new Vector3(0, 1, 0);
                _camera.Position -= _objectPos;
                _camera.Yaw -= _rotationSpeed;
                _camera.Position = Vector3.Transform(_camera.Position,
                    generateArbRotationMatrix(axis, _objectPos, -_rotationSpeed).ExtractRotation());
                _camera.Position += _objectPos;

                _camera._front = -Vector3.Normalize(_camera.Position - _objectPos);
                _objectPos /= 2;
            }

            // J (putar -> Rotasi sumbu z)
            if (KeyboardState.IsKeyDown(Keys.J))
            {
                _objectPos *= 2;
                var axis = new Vector3(0, 0, 1);
                _camera.Position -= _objectPos;
                _camera.Position = Vector3.Transform(_camera.Position,
                    generateArbRotationMatrix(axis, _objectPos, _rotationSpeed).ExtractRotation());
                _camera.Position += _objectPos;

                _camera._front = -Vector3.Normalize(_camera.Position - _objectPos);
                _objectPos /= 2;
            }
            // L (putar -> Rotasi sumbu z)
            if (KeyboardState.IsKeyDown(Keys.L))
            {
                _objectPos *= 2;
                var axis = new Vector3(0, 0, 1);
                _camera.Position -= _objectPos;
                _camera.Position = Vector3.Transform(_camera.Position,
                    generateArbRotationMatrix(axis, _objectPos, -_rotationSpeed).ExtractRotation());
                _camera.Position += _objectPos;

                _camera._front = -Vector3.Normalize(_camera.Position - _objectPos);
                _objectPos /= 2;
            }

            if (!IsFocused)
            {
                return;
            }

            const float sensitivity = 0.2f;
            if (_firstMove)
            {
                _lastMousePosition = new Vector2(MouseState.X, MouseState.Y);
                _firstMove = false;
            }
            else
            {
                // Hitung selisih mouse position
                var deltaX = MouseState.X - _lastMousePosition.X;
                var deltaY = MouseState.Y - _lastMousePosition.Y;
                _lastMousePosition = new Vector2(MouseState.X, MouseState.Y);

                _camera.Yaw += deltaX * sensitivity;
                _camera.Pitch -= deltaY * sensitivity;
            }

            base.OnUpdateFrame(args);
        }

        private void InitDefaultMaterial()
        {
            List<Material> materials = new List<Material>();
            Texture diffuseMap = Texture.LoadFromFile("C:/Users/vince/source/repos/GrafkomUAS/GrafkomUAS/Resources/white.jpg");
            Texture textureMap = Texture.LoadFromFile("C:/Users/vince/source/repos/GrafkomUAS/GrafkomUAS/Resources/white.jpg");
            materials.Add(new Material("Default", 128.0f, new Vector3(0.1f), new Vector3(1f), new Vector3(1f),
                    1.0f, diffuseMap, textureMap));

            materials_dict.Add("Default", materials);

        }

        public Mesh LoadObjFile(string path, bool usemtl = true)
        {
            Mesh mesh = new Mesh("C:/Users/vince/source/repos/GrafkomUAS/GrafkomUAS/Shaders/shader.vert",
                "C:/Users/vince/source/repos/GrafkomUAS/GrafkomUAS/Shaders/lighting.frag");
            List<Vector3> temp_vertices = new List<Vector3>();
            List<Vector3> temp_normals = new List<Vector3>();
            List<Vector3> temp_textureVertices = new List<Vector3>();
            List<uint> temp_vertexIndices = new List<uint>();
            List<uint> temp_normalsIndices = new List<uint>();
            List<uint> temp_textureIndices = new List<uint>();
            List<string> temp_name = new List<string>();
            List<String> temp_materialsName = new List<string>();
            string current_materialsName = "";
            string material_library = "";
            int mesh_count = 0;
            int mesh_created = 0;

            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Unable to open \"" + path + "\", does not exist.");
            }

            using (StreamReader streamReader = new StreamReader(path))
            {
                while (!streamReader.EndOfStream)
                {
                    List<string> words = new List<string>(streamReader.ReadLine().Split(' '));
                    words.RemoveAll(s => s == string.Empty);

                    if (words.Count == 0)
                        continue;
                    string type = words[0];

                    words.RemoveAt(0);

                    switch (type)
                    {
                        //Render tergantung nama dan objek apa sehingga bisa buat hirarki
                        case "o":
                            if (mesh_count > 0)
                            {
                                Mesh mesh_tmp = new Mesh();
                                //Attach Shader
                                mesh_tmp.setShader(shader);
                                mesh_tmp.setDepthShader(depthShader);
                                for (int i = 0; i < temp_vertexIndices.Count; i++)
                                {
                                    uint vertexIndex = temp_vertexIndices[i];
                                    mesh_tmp.AddVertices(temp_vertices[(int)vertexIndex - 1]);
                                }
                                for (int i = 0; i < temp_textureIndices.Count; i++)
                                {
                                    uint textureIndex = temp_textureIndices[i];
                                    mesh_tmp.AddTextureVertices(temp_textureVertices[(int)textureIndex - 1]);
                                }
                                for (int i = 0; i < temp_normalsIndices.Count; i++)
                                {
                                    uint normalIndex = temp_normalsIndices[i];
                                    mesh_tmp.AddNormals(temp_normals[(int)normalIndex - 1]);
                                }
                                mesh_tmp.setName(temp_name[mesh_created]);

                                //Material
                                if (usemtl)
                                {

                                    List<Material> mtl = materials_dict[material_library];
                                    for (int i = 0; i < mtl.Count; i++)
                                    {
                                        if (mtl[i].Name == current_materialsName)
                                        {
                                            mesh_tmp.setMaterial(mtl[i]);
                                        }
                                    }
                                }
                                else
                                {
                                    List<Material> mtl = materials_dict["Default"];
                                    for (int i = 0; i < mtl.Count; i++)
                                    {
                                        if (mtl[i].Name == "Default")
                                        {
                                            mesh_tmp.setMaterial(mtl[i]);
                                        }
                                    }
                                }


                                if (mesh_count == 1)
                                {
                                    mesh = mesh_tmp;
                                }
                                else
                                {
                                    mesh.child.Add(mesh_tmp);
                                }

                                mesh_created++;
                            }
                            temp_name.Add(words[0]);
                            mesh_count++;
                            break;
                        case "v":
                            temp_vertices.Add(new Vector3(float.Parse(words[0]) / 10, float.Parse(words[1]) / 10, float.Parse(words[2]) / 10));
                            break;

                        case "vt":
                            temp_textureVertices.Add(new Vector3(float.Parse(words[0]), float.Parse(words[1]),
                                                            words.Count < 3 ? 0 : float.Parse(words[2])));
                            break;

                        case "vn":
                            temp_normals.Add(new Vector3(float.Parse(words[0]), float.Parse(words[1]), float.Parse(words[2])));
                            break;
                        case "mtllib":
                            if (usemtl)
                            {
                                string resourceName = "C:/Users/vince/source/repos/GrafkomUAS/GrafkomUAS/Resources/" + words[0];
                                string nameWOExt = words[0].Split(".")[0];
                                Console.WriteLine(nameWOExt);
                                materials_dict.Add(nameWOExt, LoadMtlFile(resourceName));
                                material_library = nameWOExt;
                            }

                            break;
                        case "usemtl":
                            if (usemtl)
                            {
                                current_materialsName = words[0];
                            }

                            break;
                        // face
                        case "f":
                            foreach (string w in words)
                            {
                                if (w.Length == 0)
                                    continue;

                                string[] comps = w.Split('/');
                                for (int i = 0; i < comps.Length; i++)
                                {
                                    if (i == 0)
                                    {
                                        if (comps[0].Length > 0)
                                        {
                                            temp_vertexIndices.Add(uint.Parse(comps[0]));
                                        }

                                    }
                                    else if (i == 1)
                                    {
                                        if (comps[1].Length > 0)
                                        {
                                            temp_textureIndices.Add(uint.Parse(comps[1]));
                                        }

                                    }
                                    else if (i == 2)
                                    {
                                        if (comps[2].Length > 0)
                                        {
                                            temp_normalsIndices.Add(uint.Parse(comps[2]));
                                        }

                                    }
                                }

                            }
                            break;

                        default:
                            break;
                    }
                }
            }
            if (mesh_created < mesh_count)
            {

                Mesh mesh_tmp = new Mesh();
                //Attach Shader
                mesh_tmp.setShader(shader);
                mesh_tmp.setDepthShader(depthShader);
                for (int i = 0; i < temp_vertexIndices.Count; i++)
                {
                    uint vertexIndex = temp_vertexIndices[i];
                    mesh_tmp.AddVertices(temp_vertices[(int)vertexIndex - 1]);
                }
                for (int i = 0; i < temp_textureIndices.Count; i++)
                {
                    uint textureIndex = temp_textureIndices[i];
                    mesh_tmp.AddTextureVertices(temp_textureVertices[(int)textureIndex - 1]);
                }
                for (int i = 0; i < temp_normalsIndices.Count; i++)
                {
                    uint normalIndex = temp_normalsIndices[i];
                    mesh_tmp.AddNormals(temp_normals[(int)normalIndex - 1]);
                }
                mesh_tmp.setName(temp_name[mesh_created]);

                //Material
                if (usemtl)
                {

                    List<Material> mtl = materials_dict[material_library];
                    for (int i = 0; i < mtl.Count; i++)
                    {
                        if (mtl[i].Name == current_materialsName)
                        {
                            mesh_tmp.setMaterial(mtl[i]);
                        }
                    }
                }
                else
                {
                    List<Material> mtl = materials_dict["Default"];
                    for (int i = 0; i < mtl.Count; i++)
                    {
                        if (mtl[i].Name == "Default")
                        {
                            mesh_tmp.setMaterial(mtl[i]);
                        }
                    }
                }


                if (mesh_count == 1)
                {
                    mesh = mesh_tmp;
                }
                else
                {
                    mesh.child.Add(mesh_tmp);
                }

                mesh_created++;
            }
            return mesh;
        }
        public List<Material> LoadMtlFile(string path)
        {
            Console.WriteLine("Load MTL file");
            List<Material> materials = new List<Material>();
            List<string> name = new List<string>();
            List<float> shininess = new List<float>();
            List<Vector3> ambient = new List<Vector3>();
            List<Vector3> diffuse = new List<Vector3>();
            List<Vector3> specular = new List<Vector3>();
            List<float> alpha = new List<float>();
            List<string> map_kd = new List<string>();
            List<string> map_ka = new List<string>();

            //komputer ngecek, apakah file bisa diopen atau tidak
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Unable to open \"" + path + "\", does not exist.");
            }
            //lanjut ke sini
            using (StreamReader streamReader = new StreamReader(path))
            {
                while (!streamReader.EndOfStream)
                {
                    List<string> words = new List<string>(streamReader.ReadLine().Split(' '));
                    words.RemoveAll(s => s == string.Empty);

                    if (words.Count == 0)
                        continue;

                    string type = words[0];

                    words.RemoveAt(0);
                    switch (type)
                    {
                        case "newmtl":
                            if (map_kd.Count < name.Count)
                            {
                                map_kd.Add("white.jpg");
                            }
                            if (map_ka.Count < name.Count)
                            {
                                map_ka.Add("white.jpg");
                            }
                            name.Add(words[0]);
                            break;
                        //Shininess
                        case "Ns":
                            shininess.Add(float.Parse(words[0]));
                            break;
                        case "Ka":
                            ambient.Add(new Vector3(float.Parse(words[0]), float.Parse(words[1]), float.Parse(words[2])));
                            break;
                        case "Kd":
                            diffuse.Add(new Vector3(float.Parse(words[0]), float.Parse(words[1]), float.Parse(words[2])));
                            break;
                        case "Ks":
                            specular.Add(new Vector3(float.Parse(words[0]), float.Parse(words[1]), float.Parse(words[2])));
                            break;
                        case "d":
                            alpha.Add(float.Parse(words[0]));
                            break;
                        case "map_Kd":
                            map_kd.Add(words[0]);
                            break;
                        case "map_Ka":
                            map_ka.Add(words[0]);
                            break;
                        default:
                            break;
                    }
                }
            }

            if (map_kd.Count < name.Count)
            {
                map_kd.Add("white.jpg");
            }
            if (map_ka.Count < name.Count)
            {
                map_ka.Add("white.jpg");
            }

            Dictionary<string, Texture> texture_map_Kd = new Dictionary<string, Texture>();
            for (int i = 0; i < map_kd.Count; i++)
            {
                if (!texture_map_Kd.ContainsKey(map_kd[i]))
                {
                    Console.WriteLine("List of map_Kd key: " + map_kd[i]);
                    texture_map_Kd.Add(map_kd[i],
                        Texture.LoadFromFile("C:/Users/vince/source/repos/GrafkomUAS/GrafkomUAS/Resources/" + map_kd[i]));
                }
            }

            Dictionary<string, Texture> texture_map_Ka = new Dictionary<string, Texture>();
            for (int i = 0; i < map_ka.Count; i++)
            {
                if (!texture_map_Ka.ContainsKey(map_ka[i]))
                {
                    texture_map_Ka.Add(map_ka[i],
                        Texture.LoadFromFile("C:/Users/vince/source/repos/GrafkomUAS/GrafkomUAS/Resources/" + map_ka[i]));
                }
            }

            for (int i = 0; i < name.Count; i++)
            {
                materials.Add(new Material(name[i], shininess[i], ambient[i], diffuse[i], specular[i],
                    alpha[i], texture_map_Kd[map_kd[i]], texture_map_Ka[map_ka[i]]));
            }

            return materials;
        }

        //Animation
        public void LampRevolution()
        {
            lights[0].Position = lamp0.getTransform().ExtractTranslation();
            lamp0.rotate(0f, 1.0f, 0.0f);
        }
    }
}