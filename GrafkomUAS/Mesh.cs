using LearnOpenTK.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using OpenTK.Graphics.OpenGL4;

namespace GrafkomUAS
{
    class Mesh
    {
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector3> textureVertices = new List<Vector3>();

        Material material;
        Light light;

        int _ebo;
        List<uint> vertexIndices = new List<uint>();

        string name;
        int _vbo;
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
            if(normals.Count < vertices.Count)
            {
                GL.BufferData(BufferTarget.ArrayBuffer, vertices.Count * Vector3.SizeInBytes,
               vertices.ToArray(), BufferUsageHint.StaticDraw);
            }
            else
            {
                GL.BufferData(BufferTarget.ArrayBuffer, normals.Count * Vector3.SizeInBytes,
                normals.ToArray(), BufferUsageHint.StaticDraw);
            }
            

            var normalLocation = _shader.GetAttribLocation("aNormal");
            GL.EnableVertexAttribArray(normalLocation);
            GL.VertexAttribPointer(normalLocation, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);

            //Textures
            //Inisialiasi VBO
            _vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            if (textureVertices.Count < vertices.Count)
            {
                GL.BufferData(BufferTarget.ArrayBuffer, vertices.Count * Vector3.SizeInBytes,
               vertices.ToArray(), BufferUsageHint.StaticDraw);
            }
            else
            {
                GL.BufferData(BufferTarget.ArrayBuffer, textureVertices.Count * Vector3.SizeInBytes,
                textureVertices.ToArray(), BufferUsageHint.StaticDraw);
            }
            var texCoordLocation = _shader.GetAttribLocation("aTexCoords");
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);

            //Elements
            _ebo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, vertexIndices.Count * sizeof(uint),
                vertexIndices.ToArray(), BufferUsageHint.StaticDraw);

            //setting disini
            //                               x = 0 y = 0 z = 
            view = Matrix4.CreateTranslation(0.0f, 0.0f, -3.0f);
            projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f), sizeX / sizeY, 0.1f, 100.0f);

            //Materials
            Console.WriteLine("================================================");
            Console.WriteLine("Object Name: " + name);
            Console.WriteLine("Vertices: " + vertices.Count);
            Console.WriteLine("Normals: " + normals.Count);
            Console.WriteLine("TextureVertices: " + textureVertices.Count);
            if(material != null)
            {
                material.DisplayAttribute();
            }
            Console.WriteLine("================================================");
            

            foreach(var meshobj in child)
            {
                meshobj.setupObject(sizeX, sizeY);
            }
        }
        public void render(Camera _camera, List<Light> light)
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
            if(material != null)
            {
                _shader.SetInt("material.diffuse_sampler", 0);
                _shader.SetInt("material.specular_sampler", 1);
                _shader.SetVector3("material.ambient", material.Ambient);
                _shader.SetVector3("material.diffuse", material.Diffuse);
                _shader.SetVector3("material.specular", material.Specular);
                _shader.SetFloat("material.shininess", material.Shininess);
            }
            else
            {
                _shader.SetInt("material.diffuse_sampler", 0);
                _shader.SetInt("material.specular_sampler", 1);
                _shader.SetVector3("material.ambient", new Vector3(0.1f));
                _shader.SetVector3("material.diffuse", new Vector3(1.0f));
                _shader.SetVector3("material.specular", new Vector3(1.0f));
                _shader.SetFloat("material.shininess", 128.0f);
            }

            for(int i = 0; i < light.Count; i++)
            {

                _shader.SetVector3("lights[" + i + "].position", light[i].Position);
                //_shader.SetVector3("lights[" + i + "].direction", new Vector3(-0.2f, -1.0f, -0.3f));
                _shader.SetVector3("lights[" + i + "].ambient", light[i].Ambient);
                _shader.SetVector3("lights[" + i + "].diffuse", light[i].Diffuse);
                _shader.SetVector3("lights[" + i + "].specular", light[i].Specular);
            }
            

            //perlu diganti di parameter 2
            GL.DrawArrays(PrimitiveType.Triangles, 0, vertices.Count);

            //ada disini
            foreach (var meshobj in child)
            {
                meshobj.render(_camera, light);
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
       
        //TRANSFORMASI
        public Matrix4 getTransform()
        {
            return transform;
        }
        public void rotate(float angle)
        {
            //rotate parentnya
            //sumbu Y
            transform = transform * Matrix4.CreateRotationY(MathHelper.DegreesToRadians(angle));
            //rotate childnya
            foreach (var meshobj in child)
            {
                meshobj.rotate(angle);
            }
        }
        public void scale()
        {
            transform = transform * Matrix4.CreateScale(1.9f);
        }
        public void translate(Vector3 translation)
        {
            transform = transform * Matrix4.CreateTranslation(translation);
            foreach (var meshobj in child)
            {
                meshobj.translate(translation);
            }
        }

        //SETTER GETTER
        public List<Vector3> getVertices()
        {
            return vertices;
        }
        public void setVertices(List<Vector3> vertices)
        {
            this.vertices = vertices;
        }
        public List<Vector3> getNormals()
        {
            return normals;
        }
        public void setNormals(List<Vector3> normals)
        {
            this.normals = normals;
        }
        public List<Vector3> getTextureVertices()
        {
            return textureVertices;
        }
        public void setTextureVertices(List<Vector3> textureVertices)
        {
            this.textureVertices = textureVertices;
        }
        public void setMaterial(Material material)
        {
            this.material = material;
        }
        public Material getMaterial()
        {
            return material;
        }
        public void setName(string name)
        {
            this.name = name;
        }
        public string getName()
        {
            return name;
        }
        public void AddVertices(Vector3 vec)
        {
            vertices.Add(vec);
        }
        public void AddTextureVertices(Vector3 vec)
        {
            textureVertices.Add(vec);
        }
        public void AddNormals(Vector3 vec)
        {
            normals.Add(vec);
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

        public void setShader(Shader shader)
        {
            this._shader = shader;
        }
        public Shader getShader()
        {
            return _shader;
        }

        public void setDiffuseMap(string filepath)
        {
            _diffuseMap = Texture.LoadFromFile(filepath);
            //Give all the diffuse map
            foreach (var meshobj in child)
            {
                meshobj.setDiffuseMap(filepath);
            }
        }
        public void setSpecularMap(string filepath)
        {
            _specularMap = Texture.LoadFromFile(filepath);
            //Give all the specular map
            foreach (var meshobj in child)
            {
                meshobj.setSpecularMap(filepath);
            }
        }
    }
}
