using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using OpenTK.Graphics.OpenGL;

namespace LGBTriangle
{
    public class Shader : IDisposable
    {
        private bool _disposedValue;
        public readonly int Handle;
        private readonly Dictionary<string, int> _uniformLocations;
        
        public Shader(string vertexPath, string fragmentPath)
        {
            string vertexShaderSource;

            using (var reader = new StreamReader(vertexPath, Encoding.UTF8))
            {
                vertexShaderSource = reader.ReadToEnd();
            }

            string fragmentShaderSource;

            using (var reader = new StreamReader(fragmentPath, Encoding.UTF8))
            {
                fragmentShaderSource = reader.ReadToEnd();
            }

            var vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vertexShaderSource);

            var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragmentShaderSource);

            GL.CompileShader(vertexShader);
            GL.GetShaderInfoLog(vertexShader);
            //CheckCompilation(vertexShader);

            GL.CompileShader(fragmentShader);
            GL.GetShaderInfoLog(fragmentShader);
            //CheckCompilation(fragmentShader);            

            Handle = GL.CreateProgram();

            GL.AttachShader(Handle, vertexShader);
            GL.AttachShader(Handle, fragmentShader);

            GL.LinkProgram(Handle);

            GL.DetachShader(Handle, vertexShader);
            GL.DetachShader(Handle, fragmentShader);
            GL.DeleteShader(fragmentShader);
            GL.DeleteShader(vertexShader);
        }

        public void Dispose()
        {
            if (!_disposedValue)
            {
                GL.DeleteProgram(Handle);
                _disposedValue = true;
            }
        }

        public void Use()
        {
            GL.UseProgram(Handle);
        }

        ~Shader()
        {
            GL.DeleteProgram(Handle);
        }

        public int GetAttribLocation(string attribName)
        {
            return GL.GetAttribLocation(Handle, attribName);
        }
    }
}