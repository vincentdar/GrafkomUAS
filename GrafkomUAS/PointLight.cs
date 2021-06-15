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
    class PointLight : Light
    {
        public PointLight(Vector3 position, Vector3 ambient, Vector3 diffuse, Vector3 specular) 
            : base(position, ambient, diffuse, specular)
        {
        }
    }
}
