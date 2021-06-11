//using System;
//using System.Collections.Generic;
//using System.Text;
//using LearnOpenTK.Common;
//using OpenTK.Windowing.Common;
//using OpenTK.Windowing.Desktop;
//using OpenTK.Graphics.OpenGL4;
//using OpenTK.Mathematics;
//using OpenTK.Windowing.GraphicsLibraryFramework;

//namespace CobaCamera
//{
//    class Windows : GameWindow
//    {
//        //private float[] _vertices =
//        //{
//        //    // position          // texture
//        //    // 0.5f,  0.5f,  0.0f, 1.0f, 1.0f, // top right
//        //    // 0.5f, -0.5f,  0.0f, 1.0f, 0.0f, // bottom right
//        //    //-0.5f, -0.5f,  0.0f, 0.0f, 0.0f, // bottom left
//        //    //-0.5f,  0.5f,  0.0f, 0.0f, 1.0f, // top left

//        //     0.5f,  0.5f,  0.5f, 1.0f, 1.0f, // top right
//        //     0.5f, -0.5f,  0.5f, 1.0f, 0.0f, // bottom right
//        //    -0.5f, -0.5f,  0.5f, 0.0f, 0.0f, // bottom left
//        //    -0.5f,  0.5f,  0.5f, 0.0f, 1.0f, // top left

//        //     0.5f,  0.5f, -0.5f, 0.0f, 1.0f, // top right
//        //     0.5f, -0.5f, -0.5f, 0.0f, 0.0f, // bottom right
//        //    -0.5f, -0.5f, -0.5f, 1.0f, 0.0f, // bottom left
//        //    -0.5f,  0.5f, -0.5f, 1.0f, 1.0f  // top left
//        //};

//        //private uint[] _indices =
//        //{
//        //    // depan
//        //    0, 1, 3,
//        //    1, 2, 3,
//        //    // belakang
//        //    4, 5, 7,
//        //    5, 6, 7,
//        //    // atas
//        //    0, 3, 7,
//        //    0, 4, 7,
//        //    // bawah
//        //    1, 2, 6,
//        //    1, 5, 6,
//        //    // kiri
//        //    2, 3, 6,
//        //    3, 6, 7,
//        //    // kanan
//        //    0, 1, 5,
//        //    0, 4, 5
//        //};

//        //private int _vertexBufferObject;
//        //private int _vertexArrayObject;
//        //private Shader _shader;

//        //private int _elementBufferObject;
//        //private Texture _texture;

//        Mesh object1 = new Mesh();                                      //depan
//        Mesh object2 = new Mesh();                                      //belakang
//        Mesh object3 = new Mesh();                                      //kiri
//        Mesh object4 = new Mesh();                                      //kanan
//        Mesh object5 = new Mesh();                                      //atas

//        private Vector3 posObject1 = new Vector3(0.0f, 0.0f, -2.0f);    //depan
//        private Vector3 posObject2 = new Vector3(0.0f, 0.0f, 2.0f);    //belakang
//        private Vector3 posObject3 = new Vector3(-2.0f, 0.0f, 0.0f);    //kiri
//        private Vector3 posObject4 = new Vector3(2.0f, 0.0f, 0.0f);    //kanan
//        private Vector3 posObject5 = new Vector3(0.0f, 2.0f, 0.0f);    //atas

//        // Camera
//        private Camera _camera;
//        private Vector3 _objectPos; //titik rotasi
//        private Vector2 _LastMousePo;
//        private bool firstmove;


//        public Windows(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
//        {
//        }

//        private Matrix4 generateArbRotationMatrix(Vector3 axis, Vector3 center, float degree)
//        {
//            var rads = MathHelper.DegreesToRadians(degree);

//            var secretFormula = new float[4, 4] {
//                { (float)Math.Cos(rads) + (float)Math.Pow(axis.X, 2) * (1 - (float)Math.Cos(rads)), axis.X* axis.Y * (1 - (float)Math.Cos(rads)) - axis.Z * (float)Math.Sin(rads),    axis.X * axis.Z * (1 - (float)Math.Cos(rads)) + axis.Y * (float)Math.Sin(rads),   0 },
//                { axis.Y * axis.X * (1 - (float)Math.Cos(rads)) + axis.Z * (float)Math.Sin(rads),   (float)Math.Cos(rads) + (float)Math.Pow(axis.Y, 2) * (1 - (float)Math.Cos(rads)), axis.Y * axis.Z * (1 - (float)Math.Cos(rads)) - axis.X * (float)Math.Sin(rads),   0 },
//                { axis.Z * axis.X * (1 - (float)Math.Cos(rads)) - axis.Y * (float)Math.Sin(rads),   axis.Z * axis.Y * (1 - (float)Math.Cos(rads)) + axis.X * (float)Math.Sin(rads),   (float)Math.Cos(rads) + (float)Math.Pow(axis.Z, 2) * (1 - (float)Math.Cos(rads)), 0 },
//                { 0, 0, 0, 1}
//            };
//            var secretFormulaMatrix = new Matrix4(
//                new Vector4(secretFormula[0, 0], secretFormula[0, 1], secretFormula[0, 2], secretFormula[0, 3]),
//                new Vector4(secretFormula[1, 0], secretFormula[1, 1], secretFormula[1, 2], secretFormula[1, 3]),
//                new Vector4(secretFormula[2, 0], secretFormula[2, 1], secretFormula[2, 2], secretFormula[2, 3]),
//                new Vector4(secretFormula[3, 0], secretFormula[3, 1], secretFormula[3, 2], secretFormula[3, 3])
//            );

//            return secretFormulaMatrix;
//        }

//        protected override void OnLoad()
//        {
//            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

//            //
//            GL.Enable(EnableCap.DepthTest);
//            GL.DepthFunc(DepthFunction.Lequal); // <=
//            GL.DepthMask(true);
//            GL.DepthRange(0, 1);
//            //
//            //// VBO
//            //_vertexBufferObject = GL.GenBuffer();
//            //GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
//            //GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float),
//            //    _vertices, BufferUsageHint.StaticDraw);

//            //// VAO
//            //_vertexArrayObject = GL.GenVertexArray();
//            //GL.BindVertexArray(_vertexArrayObject);

//            //// Shader
//            //_shader = new Shader("C:/Users/vince/source/repos/Grafkom2/CobaCamera/Shaders/shader.vert",
//            //    "C:/Users/vince/source/repos/Grafkom2/CobaCamera/Shaders/shader.frag");
//            //_shader.Use();

//            //// EBO
//            //_elementBufferObject = GL.GenBuffer();
//            //GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
//            //GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint),
//            //    _indices, BufferUsageHint.StaticDraw);

//            //// Vertex - VAO
//            //var vertexPosition = _shader.GetAttribLocation("aPosition");
//            //GL.EnableVertexAttribArray(vertexPosition);
//            //GL.VertexAttribPointer(vertexPosition, 3,
//            //    VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);

//            //// Texture coordinate
//            //var texCoordLocation = _shader.GetAttribLocation("aTexCoord");
//            //GL.EnableVertexAttribArray(texCoordLocation);
//            //GL.VertexAttribPointer(texCoordLocation, 2,
//            //    VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));

//            //// Texture
//            //_texture = Texture.LoadFromFile("C:/Users/vince/source/repos/Grafkom2/CobaCamera/Resources/wood.jpg");
//            //_texture.Use(TextureUnit.Texture0);

//            object1.createBoxVertices(posObject1.X, posObject1.Y, posObject1.Z);
//            object1.setupObject((float)Size.X, (float)Size.Y);
//            object2.createBoxVertices(posObject2.X, posObject2.Y, posObject2.Z);
//            object2.setupObject((float)Size.X, (float)Size.Y);
//            object3.createBoxVertices(posObject3.X, posObject3.Y, posObject3.Z);
//            object3.setupObject((float)Size.X, (float)Size.Y);
//            object4.createBoxVertices(posObject4.X, posObject4.Y, posObject4.Z);
//            object4.setupObject((float)Size.X, (float)Size.Y);
//            object5.createBoxVertices(posObject5.X, posObject5.Y, posObject5.Z);
//            object5.setupObject((float)Size.X, (float)Size.Y);

//            // Camera
//            var _cameraPosInit = new Vector3(0, 0, 0);
//            _camera = new Camera(_cameraPosInit, Size.X / (float)Size.Y);
//            _objectPos = posObject3;
//            _camera.Yaw = -90f;
//            //CursorGrabbed = true;
//            base.OnLoad();
//        }

//        protected override void OnRenderFrame(FrameEventArgs args)
//        {
//            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

//            //// Texture
//            //GL.BindVertexArray(_vertexArrayObject);
//            //_texture.Use(TextureUnit.Texture0);
//            //_shader.Use();

//            // Camera
//            //var transform = Matrix4.Identity;
//            //_shader.SetMatrix4("transform", transform);
//            //_shader.SetMatrix4("view", _camera.GetViewMatrix());
//            //_shader.SetMatrix4("projection", _camera.GetProjectionMatrix());

//            //GL.DrawElements(PrimitiveType.Triangles, _indices.Length,
//            //    DrawElementsType.UnsignedInt, 0);

//            object1.render(_camera, new Vector3(1, 0, 0));
//            object2.render(_camera, new Vector3(0, 1, 0));
//            object3.render(_camera, new Vector3(0, 0, 1));
//            object4.render(_camera, new Vector3(1, 1, 0));
//            object5.render(_camera, new Vector3(0, 1, 1));

//            SwapBuffers();

//            base.OnRenderFrame(args);
//        }

//        protected override void OnUpdateFrame(FrameEventArgs args)
//        {
//            const float cameraSpeed = 1.5f;
//            // Escape keyboard
//            if (KeyboardState.IsKeyDown(Keys.Escape))
//            {
//                Close();
//            }
//            // Zoom in
//            if (KeyboardState.IsKeyDown(Keys.I))
//            {
//                _camera.Fov -= 0.05f;
//            }
//            // Zoom out
//            if (KeyboardState.IsKeyDown(Keys.O))
//            {
//                _camera.Fov += 0.05f;
//            }


//            if (KeyboardState.IsKeyDown(Keys.T))
//            {
//                _camera.Pitch += 0.03f;
//            }
//            if (KeyboardState.IsKeyDown(Keys.G))
//            {
//                _camera.Pitch -= 0.05f;
//            }
//            if (KeyboardState.IsKeyDown(Keys.F))
//            {
//                _camera.Yaw -= 0.05f;
//            }
//            if (KeyboardState.IsKeyDown(Keys.H))
//            {
//                _camera.Yaw += 0.05f;
//            }



//            if (KeyboardState.IsKeyDown(Keys.W))
//            {
//                _camera.Position += _camera.Front * cameraSpeed * (float)args.Time;
//            }
//            if (KeyboardState.IsKeyDown(Keys.S))
//            {
//                _camera.Position -= _camera.Front * cameraSpeed * (float)args.Time;

//            }
//            if (KeyboardState.IsKeyDown(Keys.A))
//            {
//                _camera.Position -= _camera.Right * cameraSpeed * (float)args.Time;

//            }
//            if (KeyboardState.IsKeyDown(Keys.D))
//            {
//                _camera.Position += _camera.Right * cameraSpeed * (float)args.Time;

//            }



//            if (KeyboardState.IsKeyDown(Keys.Space))
//            {
//                _camera.Position += _camera.Up * cameraSpeed * (float)args.Time;

//            }
//            if (KeyboardState.IsKeyDown(Keys.LeftControl))
//            {
//                _camera.Position -= _camera.Up * cameraSpeed * (float)args.Time;

//            }
//            const float _rotationSpeed = 0.02f;
//            //K (Atas - X)
//            if(KeyboardState.IsKeyDown(Keys.K))
//            {
//                _objectPos *= 2;
//                var axis = new Vector3(1, 0, 0);
//                _camera.Position -= _objectPos;
//                _camera.Pitch -= _rotationSpeed;
//                _camera.Position = Vector3.Transform(_camera.Position,
//                    generateArbRotationMatrix(axis, _objectPos, _rotationSpeed).ExtractRotation());
//                _camera.Position += _objectPos;
//                _camera._front = -Vector3.Normalize(_camera.Position - _objectPos);
//                _objectPos /= 2;
//            }
//            //M (Bawah - X)
//            if (KeyboardState.IsKeyDown(Keys.M))
//            {
//                _objectPos *= 2;
//                var axis = new Vector3(1, 0, 0);
//                _camera.Position -= _objectPos;
//                _camera.Pitch += _rotationSpeed;
//                _camera.Position = Vector3.Transform(_camera.Position,
//                    generateArbRotationMatrix(axis, _objectPos, -_rotationSpeed).ExtractRotation());
//                _camera.Position += _objectPos;
//                _camera._front = -Vector3.Normalize(_camera.Position - _objectPos);
//                _objectPos /= 2;
//            }
//            //N (Kiri - Y)
//            if (KeyboardState.IsKeyDown(Keys.N))
//            {
//                _objectPos *= 2;
//                var axis = new Vector3(0, 1, 0);
//                _camera.Position -= _objectPos;
//                _camera.Yaw += _rotationSpeed;
//                _camera.Position = Vector3.Transform(_camera.Position,
//                    generateArbRotationMatrix(axis, _objectPos, _rotationSpeed).ExtractRotation());
//                _camera.Position += _objectPos;
//                _camera._front = -Vector3.Normalize(_camera.Position - _objectPos);
//                _objectPos /= 2;
//            }
//            //, (Kanan - Y)
//            if (KeyboardState.IsKeyDown(Keys.Comma))
//            {
//                _objectPos *= 2;
//                var axis = new Vector3(0, 1, 0);
//                _camera.Position -= _objectPos;
//                _camera.Yaw -= _rotationSpeed;
//                _camera.Position = Vector3.Transform(_camera.Position,
//                    generateArbRotationMatrix(axis, _objectPos, -_rotationSpeed).ExtractRotation());
//                _camera.Position += _objectPos;
//                _camera._front = -Vector3.Normalize(_camera.Position - _objectPos);
//                _objectPos /= 2;
//            }
//            //J (putar - Z)
//            if (KeyboardState.IsKeyDown(Keys.J))
//            {
//                _objectPos *= 2;
//                var axis = new Vector3(0, 0, 1);
//                _camera.Position -= _objectPos;
//                _camera.Position = Vector3.Transform(_camera.Position,
//                    generateArbRotationMatrix(axis, _objectPos, _rotationSpeed).ExtractRotation());
//                _camera.Position += _objectPos;
//                _camera._front = -Vector3.Normalize(_camera.Position - _objectPos);
//                _objectPos /= 2;
//            }
//            //L (putar - Z)
//            if (KeyboardState.IsKeyDown(Keys.L))
//            {
//                _objectPos *= 2;
//                var axis = new Vector3(0, 0, 1);
//                _camera.Position -= _objectPos;
//                _camera.Position = Vector3.Transform(_camera.Position,
//                    generateArbRotationMatrix(axis, _objectPos, -_rotationSpeed).ExtractRotation());
//                _camera.Position += _objectPos;
//                _camera._front = -Vector3.Normalize(_camera.Position - _objectPos);
//                _objectPos /= 2;
//            }



//            const float sensitivity = 0.2f;
//            //if (firstmove)
//            //{
//            //    _LastMousePo = new Vector2(MouseState.X, MouseState.Y);
//            //    firstmove = false;
//            //}
//            //else
//            //{
//            //    var deltaX = MouseState.X - _LastMousePo.X;
//            //    var deltaY = MouseState.Y - _LastMousePo.Y;
//            //    _LastMousePo = new Vector2(MouseState.X, MouseState.Y);

//            //    _camera.Yaw += deltaX * sensitivity;
//            //    _camera.Pitch -= deltaY * sensitivity;
//            //}
//            base.OnUpdateFrame(args);
//        }
//    }
//}
