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
    class Windows : GameWindow
    {
        private Mesh mesh0;

        private Mesh lamp0;
        private Mesh lamp1;
        private Mesh lamp2;
        Dictionary<string, List<Material>> materials_dict = new Dictionary<string, List<Material>>();

        private Camera _camera;
        private Vector3 _objectPos;

        private Vector2 _lastMousePosition;
        private bool _firstMove;

        //Light

        List<Light> lights = new List<Light>();
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

            //Light Position
            lights.Add(new PointLight(new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0.1f, 0.1f, 0.1f),
                new Vector3(0.0f, 0.0f, 1.0f), new Vector3(0.0f, 0.0f, 1.0f)));
            lights.Add(new PointLight(new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0.1f, 0.1f, 0.1f),
                new Vector3(1.0f, 0.0f, 0.0f), new Vector3(1.0f, 0.0f, 0.0f)));
            lights.Add(new PointLight(new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0.1f, 0.1f, 0.1f),
                new Vector3(0.0f, 1.0f, 0.0f), new Vector3(0.0f, 1.0f, 0.0f)));

            mesh0 = LoadObjFile("C:/Users/vince/source/repos/GrafkomUAS/GrafkomUAS/Resources/KleeBomb.obj", false);
            mesh0.setDiffuseMap("C:/Users/vince/source/repos/GrafkomUAS/GrafkomUAS/Resources/white.jpg");
            mesh0.setSpecularMap("C:/Users/vince/source/repos/GrafkomUAS/GrafkomUAS/Resources/white.jpg");
            mesh0.setupObject(1.0f, 1.0f);

            lamp0 = LoadObjFile("C:/Users/vince/source/repos/GrafkomUAS/GrafkomUAS/Resources/TestCubeInverted.obj", false);
            lamp0.setDiffuseMap("C:/Users/vince/source/repos/GrafkomUAS/GrafkomUAS/Resources/white.jpg");
            lamp0.setSpecularMap("C:/Users/vince/source/repos/GrafkomUAS/GrafkomUAS/Resources/white.jpg");
            lamp0.setupObject(1.0f, 1.0f);
            lamp0.translate(new Vector3(0.4f, 0.0f, 0.0f));
            lights[0].Position = lamp0.getTransform().ExtractTranslation();

            lamp1 = LoadObjFile("C:/Users/vince/source/repos/GrafkomUAS/GrafkomUAS/Resources/TestCubeInverted.obj", false);
            lamp1.setDiffuseMap("C:/Users/vince/source/repos/GrafkomUAS/GrafkomUAS/Resources/white.jpg");
            lamp1.setSpecularMap("C:/Users/vince/source/repos/GrafkomUAS/GrafkomUAS/Resources/white.jpg");
            lamp1.setupObject(1.0f, 1.0f);
            lamp1.translate(new Vector3(-0.4f, 0.0f, 0.0f));
            lights[1].Position = lamp1.getTransform().ExtractTranslation();

            lamp2 = LoadObjFile("C:/Users/vince/source/repos/GrafkomUAS/GrafkomUAS/Resources/TestCubeInverted.obj", false);
            lamp2.setDiffuseMap("C:/Users/vince/source/repos/GrafkomUAS/GrafkomUAS/Resources/white.jpg");
            lamp2.setSpecularMap("C:/Users/vince/source/repos/GrafkomUAS/GrafkomUAS/Resources/white.jpg");
            lamp2.setupObject(1.0f, 1.0f);
            lamp2.translate(new Vector3(0.0f, 0.8f, 0.0f));
            lights[2].Position = lamp2.getTransform().ExtractTranslation();


            var _cameraPosInit = new Vector3(0, 0, 0);
            _camera = new Camera(_cameraPosInit, Size.X / (float)Size.Y);
            _camera.Yaw -= 90f;
            CursorGrabbed = true;
            Console.WriteLine(GLFW.GetTime());
            base.OnLoad();
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            if(GLFW.GetTime() > 0.1)
            {
                //LampRevolution();
                GLFW.SetTime(0.0);
            }
            mesh0.render(_camera, lights);
            lamp0.render(_camera, lights);
            lamp1.render(_camera, lights);
            lamp2.render(_camera, lights);
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
            //komputer ngecek, apakah file bisa diopen atau tidak
            if (!File.Exists(path))
            {
                //mengakhiri program dan kita kasih peringatan
                throw new FileNotFoundException("Unable to open \"" + path + "\", does not exist.");
            }
            //lanjut ke sini
            using (StreamReader streamReader = new StreamReader(path))
            {
                while (!streamReader.EndOfStream)
                {
                    //aku ngambil 1 baris tersebut -> dimasukkan ke dalam List string -> dengan di split pakai spasi
                    List<string> words = new List<string>(streamReader.ReadLine().Split(' '));
                    //removeAll(kondisi dimana penghapusan terjadi)
                    words.RemoveAll(s => s == string.Empty);
                    //Melakukan pengecekkan apakah dalam satu list -> ada isinya atau tidak list nya tersebut
                    //kalau ada continue, perintah-perintah yang ada dibawahnya tidak akan dijalankan 
                    //dan dia bakal kembali keatas lagi / melanjutkannya whilenya
                    if (words.Count == 0)
                        continue;
                    string type = words[0];
                    //remove at -> menghapus data dalam suatu indexs dan otomatis data pada indeks
                    //berikutnya itu otomatis mundur kebelakang 1
                    words.RemoveAt(0);



                    switch (type)
                    {
                        //Render tergantung nama dan objek apa sehingga bisa buat hirarki
                        case "o":
                            if(mesh_count > 0)
                            {
                                Mesh mesh_tmp = new Mesh("C:/Users/vince/source/repos/GrafkomUAS/GrafkomUAS/Shaders/shader.vert",
                                "C:/Users/vince/source/repos/GrafkomUAS/GrafkomUAS/Shaders/lighting.frag");
                                
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
                                if(usemtl)
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
                                
                                
                                if(mesh_count == 1)
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
                        // vertex
                        //parse merubah dari string ke tipe variabel yang diinginkan
                        //ada /10 karena saaat ini belum masuk materi camera
                        case "v":
                            temp_vertices.Add(new Vector3(float.Parse(words[0]) / 10, float.Parse(words[1]) / 10, float.Parse(words[2]) / 10));
                            //vertices.Add(new Vector3(float.Parse(words[0]), float.Parse(words[1]), float.Parse(words[2])));
                            break;

                        case "vt":
                            temp_textureVertices.Add(new Vector3(float.Parse(words[0]), float.Parse(words[1]),
                                                            words.Count < 3 ? 0 : float.Parse(words[2])));
                            break;

                        case "vn":
                            temp_normals.Add(new Vector3(float.Parse(words[0]), float.Parse(words[1]), float.Parse(words[2])));
                            break;
                        case "mtllib":
                            if(usemtl)
                            {
                                string resourceName = "C:/Users/vince/source/repos/GrafkomUAS/GrafkomUAS/Resources/" + words[0];
                                string nameWOExt = words[0].Split(".")[0];
                                Console.WriteLine(nameWOExt);
                                materials_dict.Add(nameWOExt, LoadMtlFile(resourceName));
                                material_library = nameWOExt;
                            }
                            
                            break;
                        case "usemtl":
                            if(usemtl)
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
            Console.WriteLine("Mesh Count: " + mesh_count);
            Console.WriteLine("Mesh Created: " + mesh_created);
            if (mesh_created < mesh_count)
            {

                Mesh mesh_tmp = new Mesh("C:/Users/vince/source/repos/GrafkomUAS/GrafkomUAS/Shaders/shader.vert",
                            "C:/Users/vince/source/repos/GrafkomUAS/GrafkomUAS/Shaders/lighting.frag");
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
                if (mesh_count == 1)
                {
                    mesh = mesh_tmp;
                }
                else if (mesh_count > 1)
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
            List <Material> materials = new List<Material>();
            List<string> name = new List<string>();
            List<float> shininess = new List<float>();
            List<Vector3> ambient = new List<Vector3>();
            List<Vector3> diffuse = new List<Vector3>();
            List<Vector3> specular = new List<Vector3>();
            List<float> alpha = new List<float>();

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
                    //aku ngambil 1 baris tersebut -> dimasukkan ke dalam List string -> dengan di split pakai spasi
                    List<string> words = new List<string>(streamReader.ReadLine().Split(' '));
                    //removeAll(kondisi dimana penghapusan terjadi)
                    words.RemoveAll(s => s == string.Empty);
                    //Melakukan pengecekkan apakah dalam satu list -> ada isinya atau tidak list nya tersebut
                    //kalau ada continue, perintah-perintah yang ada dibawahnya tidak akan dijalankan 
                    //dan dia bakal kembali keatas lagi / melanjutkannya whilenya

                    if (words.Count == 0)
                        continue;
                    //Console.WriteLine(words[0]);
                    string type = words[0];
                    //remove at -> menghapus data dalam suatu indexs dan otomatis data pada indeks
                    //berikutnya itu otomatis mundur kebelakang 1
                    words.RemoveAt(0);

                    //for (int i = 0; i < words.Count; i++)
                    //{
                    //    Console.WriteLine(words[i]);
                    //}


                    switch (type)
                    {
                        case "newmtl":
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

                        default:
                            break;
                    }
                }

            }

            for (int i = 0; i < name.Count; i++)
            {
                materials.Add(new Material(name[i], shininess[i], ambient[i], diffuse[i], specular[i], alpha[i]));
            }
            return materials;
        }

        public void LampRevolution()
        {
            //light0.Position = lamp0.getTransform().ExtractTranslation();
            lamp0.rotate(1f);
        }
    }
}