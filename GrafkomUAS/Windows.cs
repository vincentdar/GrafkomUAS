using System;
using System.Collections.Generic;
using System.Text;
using LearnOpenTK.Common;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
//using Pertemuan7;

namespace GrafkomUAS
{
    class Windows : GameWindow
    {
        private Mesh mesh0;

        private Camera _camera;
        private Vector3 _objectPos;

        private Vector2 _lastMousePosition;
        private bool _firstMove;

        private Vector3 lightPos = new Vector3(1.0f, 0.0f, 0.0f);
        public Windows(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
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
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
            GL.Enable(EnableCap.DepthTest);

            mesh0 = new Mesh("C:/Users/vince/source/repos/GrafkomUAS/GrafkomUAS/Shaders/shader.vert",
                "C:/Users/vince/source/repos/GrafkomUAS/GrafkomUAS/Shaders/lighting.frag");
            mesh0.LoadObjFile("C:/Users/vince/source/repos/GrafkomUAS/GrafkomUAS/Resources/AmongUs3.obj");
            mesh0.setupObject(2.0f, 2.0f);





            var _cameraPosInit = new Vector3(0, 0, 0);
            _camera = new Camera(_cameraPosInit, Size.X / (float)Size.Y);
            _camera.Yaw -= 90f;
            CursorGrabbed = true;

            //Light Position
            lightPos = new Vector3(1.0f, 1.0f, 1.0f);

            base.OnLoad();
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            mesh0.render(_camera, lightPos);
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
    }
}