﻿using LearnOpenTK.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using OpenTK.Graphics.OpenGL4;

namespace GrafkomUAS
{
    class Material
    {
        string name;
        float shininess;
        Vector3 ambient;
        Vector3 diffuse;
        Vector3 specular;
        float alpha;

        Texture map_Kd;

        public Material(string name, float shininess, Vector3 ambient, Vector3 diffuse, Vector3 specular, float alpha, Texture map_Kd)
        {
            this.name = name;
            this.shininess = shininess;
            this.ambient = ambient;
            this.diffuse = diffuse;
            this.specular = specular;
            this.alpha = alpha;
            this.map_Kd = map_Kd;
        }

        public string Name { get => name; set => name = value; }
        public float Shininess { get => shininess; set => shininess = value; }
        public Vector3 Ambient { get => ambient; set => ambient = value; }
        public Vector3 Diffuse { get => diffuse; set => diffuse = value; }
        public Vector3 Specular { get => specular; set => specular = value; }
        public float Alpha { get => alpha; set => alpha = value; }
        public void DisplayAttribute()
        {
            Console.WriteLine("Name: " + name);
            Console.WriteLine("Shininess: " + shininess);
            Console.WriteLine("Ambient " + ambient);
            Console.WriteLine("Diffuse: " + diffuse);
            Console.WriteLine("Specular: " + specular);
            Console.WriteLine("Alpha: " + alpha);
        }
    }
}
