using LearnOpenTK.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using OpenTK.Graphics.OpenGL4;

namespace GrafkomUAS
{
    class Mesh
    {
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector3> textureVertices = new List<Vector3>();
        List<uint> vertexIndices = new List<uint>();

        List<float> floatData = new List<float>();
        int _vbo;
        int _ebo;
        int _vao;
        Shader _shader;
        Matrix4 transform;
        Matrix4 view;
        Matrix4 projection;

        //// The texture containing information for the diffuse map, this would more commonly
        //// just be called the color/texture of the object.
        private Texture _diffuseMap;

        //// The specular map is a black/white representation of how specular each part of the texture is.
        private Texture _specularMap;


        //Hirarki pada parent
        public List<Mesh> child = new List<Mesh>();
        public Mesh(string vertPath, string fragPath)
        {
            _shader = new Shader(vertPath, fragPath);
        }
        public void setupObject(float sizeX, float sizeY)
        {
            //Inisialisasi Transformasi
            transform = Matrix4.Identity;

            Console.WriteLine("Vertices: " + vertices.Count);
            Console.WriteLine(vertices[0].X + " " + vertices[0].Y + " " + vertices[0].Z);
            Console.WriteLine("Normals: " + normals.Count);
            Console.WriteLine("TextureVertices: " + textureVertices.Count);

            //Vertices
            //Inisialiasi VBO
            _vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Count * Vector3.SizeInBytes,
                vertices.ToArray(), BufferUsageHint.StaticDraw);

            //Inisialisasi VAO
            _vao = GL.GenVertexArray();
            GL.BindVertexArray(_vao);
            var vertexLocation = _shader.GetAttribLocation("aPosition");
            GL.EnableVertexAttribArray(vertexLocation);
            GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);

            //Normals
            //Inisialiasi VBO
            _vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, normals.Count * Vector3.SizeInBytes,
                normals.ToArray(), BufferUsageHint.StaticDraw);

            var normalLocation = _shader.GetAttribLocation("aNormal");
            GL.EnableVertexAttribArray(normalLocation);
            GL.VertexAttribPointer(normalLocation, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);

            //Textures
            //Inisialiasi VBO
            _vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, textureVertices.Count * Vector3.SizeInBytes,
                textureVertices.ToArray(), BufferUsageHint.StaticDraw);
            var texCoordLocation = _shader.GetAttribLocation("aTexCoords");
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);

            //Elements
            _ebo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, vertexIndices.Count * sizeof(uint),
                vertexIndices.ToArray(), BufferUsageHint.StaticDraw);

            _diffuseMap = Texture.LoadFromFile("C:/Users/vince/source/repos/GrafkomUAS/GrafkomUAS/Resources/badLogic.jpg");
            _specularMap = Texture.LoadFromFile("C:/Users/vince/source/repos/GrafkomUAS/GrafkomUAS/Resources/container2_specular.png");

            //setting disini
            //                               x = 0 y = 0 z = 
            view = Matrix4.CreateTranslation(0.0f, 0.0f, -3.0f);
            projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f), sizeX / sizeY, 0.1f, 100.0f);
        }
        public void render(Camera _camera, Vector3 _lightPos)
        {
            //render itu akan selalu terpanggil setiap frame
            GL.BindVertexArray(_vao);
            _diffuseMap.Use(TextureUnit.Texture0);
            _specularMap.Use(TextureUnit.Texture1);
            _shader.Use();

            
            _shader.SetMatrix4("transform", transform);
            _shader.SetMatrix4("view", _camera.GetViewMatrix());
            _shader.SetMatrix4("projection", _camera.GetProjectionMatrix());

            //_shader.SetVector3("objectColor", new Vector3(1.0f, 0.5f, 0.31f));
            //_shader.SetVector3("lightColor", new Vector3(1.0f, 1.0f, 1.0f));
            //_shader.SetVector3("lightPos", _lightPos);
            _shader.SetVector3("viewPos", _camera.Position);
            ////material settings
            _shader.SetInt("material.diffuse", 0);
            _shader.SetInt("material.specular", 1);
            //_shader.SetVector3("material.ambient", new Vector3(1.0f, 0.5f, 0.31f));
            //_shader.SetVector3("material.diffuse", new Vector3(1.0f, 0.5f, 0.31f));
            //_shader.SetVector3("material.specular", new Vector3(0.5f, 0.5f, 0.5f));
            _shader.SetFloat("material.shininess", 32.0f);
            // This is where we change the lights color over time using the sin function
            //Vector3 lightColor;
            //float time = DateTime.Now.Second + DateTime.Now.Millisecond / 1000f;
            //lightColor.X = (MathF.Sin(time * 2.0f) + 1) / 2f;
            //lightColor.Y = (MathF.Sin(time * 0.7f) + 1) / 2f;
            //lightColor.Z = (MathF.Sin(time * 1.3f) + 1) / 2f;

            //// The ambient light is less intensive than the diffuse light in order to make it less dominant
            //Vector3 ambientColor = lightColor * new Vector3(0.2f);
            //Vector3 diffuseColor = lightColor * new Vector3(0.5f);

            _shader.SetVector3("light.position", _lightPos);
            //_shader.SetVector3("light.direction", new Vector3(-0.2f, -1.0f, -0.3f));
            //_shader.SetVector3("light.ambient", ambientColor);
            //_shader.SetVector3("light.diffuse", diffuseColor);
            _shader.SetVector3("light.ambient", new Vector3(0.2f));
            _shader.SetVector3("light.diffuse", new Vector3(0.5f));
            _shader.SetVector3("light.specular", new Vector3(1.0f, 1.0f, 1.0f));

            //perlu diganti di parameter 2
            GL.DrawArrays(PrimitiveType.Triangles, 0, vertices.Count);

            //ada disini
            foreach (var meshobj in child)
            {
                meshobj.render(_camera, _lightPos);
            }
        }
        
        public void LoadObjFile(string path)
        {
            List<Vector3> temp_vertices = new List<Vector3>();
            List<Vector3> temp_normals = new List<Vector3>();
            List<Vector3> temp_textureVertices = new List<Vector3>();
            List<uint> temp_vertexIndices = new List<uint>();
            List<uint> temp_normalsIndices = new List<uint>();
            List<uint> temp_textureIndices = new List<uint>();
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
                    List<string> words = new List<string>(streamReader.ReadLine().ToLower().Split(' '));
                    //removeAll(kondisi dimana penghapusan terjadi)
                    words.RemoveAll(s => s == string.Empty);
                    //Melakukan pengecekkan apakah dalam satu list -> ada isinya atau tidak list nya tersebut
                    //kalau ada continue, perintah-perintah yang ada dibawahnya tidak akan dijalankan 
                    //dan dia bakal kembali keatas lagi / melanjutkannya whilenya
                    if (words.Count == 0)
                        continue;

                    //System.Console.WriteLine("New While");
                    //foreach (string x in words)
                    //               {
                    //	System.Console.WriteLine("tes");
                    //	System.Console.WriteLine(x);
                    //               }
                    //System.Console.WriteLine(words[0]);
                    string type = words[0];
                    //remove at -> menghapus data dalam suatu indexs dan otomatis data pada indeks
                    //berikutnya itu otomatis mundur kebelakang 1
                    words.RemoveAt(0);
                    

                    
                    switch (type)
                    {
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
                        // face
                        case "f":                            
                            foreach (string w in words)
                            {
                                if (w.Length == 0)
                                    continue;

                                string[] comps = w.Split('/');
                                temp_vertexIndices.Add(uint.Parse(comps[0]));
                                temp_textureIndices.Add(uint.Parse(comps[1]));
                                temp_normalsIndices.Add(uint.Parse(comps[2]));
                                

                            }
                            break;

                        default:
                            break;
                    }
                }
            }
            for(int i = 0; i < temp_vertexIndices.Count; i++)
            {
                uint vertexIndex = temp_vertexIndices[i];
                vertices.Add(temp_vertices[(int)vertexIndex - 1]);
            }
            for(int i = 0; i < temp_normalsIndices.Count; i++)
            {
                uint normalIndex = temp_normalsIndices[i];
                Vector3 vec = temp_normals[(int)normalIndex - 1];
                normals.Add(vec);
            }
            for (int i = 0; i < temp_textureIndices.Count; i++)
            {
                uint textureIndex = temp_textureIndices[i];
                textureVertices.Add(temp_textureVertices[(int)textureIndex - 1]);
            }
        }
        public void createBoxVertices(float x, float y, float z)
        {
            //biar lebih fleksibel jangan inisialiasi posisi dan 
            //panjang kotak didalam tapi ditaruh ke parameter
            float _positionX = x;
            float _positionY = y;
            float _positionZ = z;

            float _boxLength = 2.0f;

            //Buat temporary vector
            Vector3 temp_vector;
            //1. Inisialisasi vertex
            // Titik 1
            temp_vector.X = _positionX - _boxLength / 2.0f; // x 
            temp_vector.Y = _positionY + _boxLength / 2.0f; // y
            temp_vector.Z = _positionZ - _boxLength / 2.0f; // z

            vertices.Add(temp_vector);

            // Titik 2
            temp_vector.X = _positionX + _boxLength / 2.0f; // x
            temp_vector.Y = _positionY + _boxLength / 2.0f; // y
            temp_vector.Z = _positionZ - _boxLength / 2.0f; // z

            vertices.Add(temp_vector);
            // Titik 3
            temp_vector.X = _positionX - _boxLength / 2.0f; // x
            temp_vector.Y = _positionY - _boxLength / 2.0f; // y
            temp_vector.Z = _positionZ - _boxLength / 2.0f; // z
            vertices.Add(temp_vector);

            // Titik 4
            temp_vector.X = _positionX + _boxLength / 2.0f; // x
            temp_vector.Y = _positionY - _boxLength / 2.0f; // y
            temp_vector.Z = _positionZ - _boxLength / 2.0f; // z

            vertices.Add(temp_vector);

            // Titik 5
            temp_vector.X = _positionX - _boxLength / 2.0f; // x
            temp_vector.Y = _positionY + _boxLength / 2.0f; // y
            temp_vector.Z = _positionZ + _boxLength / 2.0f; // z

            vertices.Add(temp_vector);

            // Titik 6
            temp_vector.X = _positionX + _boxLength / 2.0f; // x
            temp_vector.Y = _positionY + _boxLength / 2.0f; // y
            temp_vector.Z = _positionZ + _boxLength / 2.0f; // z

            vertices.Add(temp_vector);

            // Titik 7
            temp_vector.X = _positionX - _boxLength / 2.0f; // x
            temp_vector.Y = _positionY - _boxLength / 2.0f; // y
            temp_vector.Z = _positionZ + _boxLength / 2.0f; // z

            vertices.Add(temp_vector);

            // Titik 8
            temp_vector.X = _positionX + _boxLength / 2.0f; // x
            temp_vector.Y = _positionY - _boxLength / 2.0f; // y
            temp_vector.Z = _positionZ + _boxLength / 2.0f; // z

            vertices.Add(temp_vector);
            //2. Inisialisasi index vertex
            vertexIndices = new List<uint> {
                // Segitiga Depan 1
                0, 1, 2,
                // Segitiga Depan 2
                1, 2, 3,
                // Segitiga Atas 1
                0, 4, 5,
                // Segitiga Atas 2
                0, 1, 5,
                // Segitiga Kanan 1
                1, 3, 5,
                // Segitiga Kanan 2
                3, 5, 7,
                // Segitiga Kiri 1
                0, 2, 4,
                // Segitiga Kiri 2
                2, 4, 6,
                // Segitiga Belakang 1
                4, 5, 6,
                // Segitiga Belakang 2
                5, 6, 7,
                // Segitiga Bawah 1
                2, 3, 6,
                // Segitiga Bawah 2
                3, 6, 7
            };
            


            textureVertices.Add(new Vector3(0.0f, 1.0f, 0.0f));
            textureVertices.Add(new Vector3(1.0f, 1.0f, 0.0f));
            textureVertices.Add(new Vector3(0.0f, 0.0f, 0.0f));
            textureVertices.Add(new Vector3(1.0f, 0.0f, 0.0f));
            textureVertices.Add(new Vector3(0.0f, 1.0f, 0.0f));
            textureVertices.Add(new Vector3(1.0f, 1.0f, 0.0f));
            textureVertices.Add(new Vector3(0.0f, 0.0f, 0.0f));
            textureVertices.Add(new Vector3(1.0f, 0.0f, 0.0f));

        }
        public void hardcodeBoxVertices()
        {
            vertices.Add(new Vector3(-0.5f, 0.5f, -0.5f));
            vertices.Add(new Vector3(0.5f, 0.5f, -0.5f));
            vertices.Add(new Vector3(-0.5f, -0.5f, -0.5f));
            vertices.Add(new Vector3(0.5f, -0.5f, -0.5f));
            vertices.Add(new Vector3(-0.5f, 0.5f, 0.5f));
            vertices.Add(new Vector3(0.5f, 0.5f, 0.5f));
            vertices.Add(new Vector3(-0.5f, -0.5f, 0.5f));
            vertices.Add(new Vector3(0.5f, 0.5f, 0.5f));
        }

        //TRANSFORMASI
        public Matrix4 getTransform()
        {
            return transform;
        }
        public void rotate()
        {
            //rotate parentnya
            //sumbu Y
            transform = transform * Matrix4.CreateRotationY(MathHelper.DegreesToRadians(20f));
            //rotate childnya
            foreach (var meshobj in child)
            {
                meshobj.rotate();
            }
        }
        public void scale()
        {
            transform = transform * Matrix4.CreateScale(1.9f);
        }
        public void translate()
        {
            //perpindahan sebanyak 0.1 ke x, 0.1 ke y, dan 0 ke z
            transform = transform * Matrix4.CreateTranslation(0.1f, 0.1f, 0.0f);
        }

        //SETTER GETTER
        public List<Vector3> getVertices()
        {
            return vertices;
        }
        public List<uint> getVertexIndices()
        {
            return vertexIndices;
        }

        public void setVertexIndices(List<uint> temp)
        {
            vertexIndices = temp;
        }
        public int getVertexBufferObject()
        {
            return _vbo;
        }

        public int getElementBufferObject()
        {
            return _ebo;
        }

        public int getVertexArrayObject()
        {
            return _vao;
        }

        public Shader getShader()
        {
            return _shader;
        }
    }
}
