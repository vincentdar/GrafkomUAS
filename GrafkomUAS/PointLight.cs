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
        float constant;
        float linear;
        float quadratic;
        public PointLight(Vector3 position, Vector3 ambient, Vector3 diffuse, Vector3 specular) 
            : base(position, ambient, diffuse, specular)
        {
            this.constant = 1.0f;
            this.linear = 1.0f;
            this.quadratic = 1.0f;
        }

        public float Constant { get => constant; set => constant = value; }
        public float Linear { get => linear; set => linear = value; }
        public float Quadratic { get => quadratic; set => quadratic = value; }
    }
}
