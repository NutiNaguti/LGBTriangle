using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using BufferTarget = OpenTK.Graphics.OpenGL.BufferTarget;
using BufferUsageHint = OpenTK.Graphics.OpenGL.BufferUsageHint;
using ClearBufferMask = OpenTK.Graphics.OpenGL.ClearBufferMask;
using GL = OpenTK.Graphics.OpenGL.GL;
using PrimitiveType = OpenTK.Graphics.OpenGL.PrimitiveType;
using VertexAttribPointerType = OpenTK.Graphics.OpenGL.VertexAttribPointerType;

namespace LGBTriangle
{
    public class Window : GameWindow
    {

        // private int Width { get; set; }
        // private int Height { get; set; }
        private Shader Shader { get; set; }
        private int VertexArrayObject { get; set; }
        private int VertexBufferObject { get; set; }
        private int ElementBufferObject { get; set; }
        
        private float _greenValue = 0.0f;
        private float  _redValue = 0.0f;
        private float  _blueValue = 0.0f;
        private Stopwatch _timer;

        float[] vertices = {
            -0.5f, -0.5f, 0.0f, // Bottom-left vertex
            0.5f, -0.5f, 0.0f, // Bottom-right vertex
            0.0f,  0.5f, 0.0f  // Top vertex
        };
            
        uint[] indices = {  // note that we start from 0!
            0, 1, 3,   // first triangle
            1, 2, 3    // second triangle
        };

        public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(
            gameWindowSettings, nativeWindowSettings)
        {
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            if (KeyboardState.IsKeyDown(Keys.Escape)) Close();
            base.OnUpdateFrame(args);
        }


        protected override void OnLoad()
        {
            GL.ClearColor(1f, 1f, 1f, 1.0f);
            
            VertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
            
            VertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(VertexArrayObject);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexArrayObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            ElementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            
            Shader = new Shader(
                Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"shaders\shader.vert.glsl")),
                Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"shaders\shader.frag.glsl")));

            Shader.Use();
            
            _timer = new Stopwatch();
            _timer.Start();
            
            base.OnLoad();
        }

        protected override void OnUnload()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);

            // Delete all the resources.
            GL.DeleteBuffer(VertexBufferObject);
            GL.DeleteVertexArray(VertexArrayObject);

            GL.DeleteProgram(Shader.Handle);
            base.OnUnload();
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);
            Shader.Use();

            #region Coloring
            
            var timeValue = _timer.Elapsed.TotalSeconds;
            GetColor(ref _greenValue, ref _redValue, ref _blueValue, ref timeValue);
            
            var vertexColorLocation = GL.GetUniformLocation(Shader.Handle, "ourColor");
            GL.Uniform4(vertexColorLocation, _redValue, _greenValue, _blueValue, 1.0f);

            #endregion
            
            GL.BindVertexArray(VertexArrayObject);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);

            SwapBuffers();
            base.OnRenderFrame(args);
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, Size.X, Size.Y);
        }

        private float GetSinRGBValue(double timeValue)
        {
            return Math.Abs((float) Math.Sin(timeValue));
        }
        
        private float GetCosRGBValue(double timeValue)
        {
            return Math.Abs((float) Math.Cos(timeValue));
        }

        private void GetColor(ref float greenValue, ref float redValue, ref float blueValue, ref double timeValue)
        {
            if (blueValue >= 0.01f && redValue == 0.0f && greenValue == 0.0f)
            {
                while (blueValue >= 0.1f)
                {
                    blueValue = Math.Abs(GetSinRGBValue(timeValue) - GetCosRGBValue(timeValue));
                    return;
                }

                greenValue = 0.0f;
                blueValue = 0.0f;
                redValue = 0.0f;
                timeValue = 0.0f;
                _timer = new();
                _timer.Start();
            }

            if (greenValue < 0.99f && redValue == 0.0)
            {
                while (greenValue < 0.95f)
                { 
                    greenValue = GetSinRGBValue(timeValue);
                    return;
                }

                greenValue = 0.99f;
                timeValue = 0.0;

                _timer = new Stopwatch();
                _timer.Start();
                return;
            }
            
            if (redValue < 0.99f && greenValue >= 0.99f)
            {
                while (redValue <= 0.95f)
                {
                    redValue = GetSinRGBValue(timeValue);
                    return;
                }

                redValue = 0.99f;
                timeValue = 0.0f;
                _timer = new();
                _timer.Start();
                return;
            }

            if (greenValue > 0.001f && redValue >= 0.99f)
            {
                while (greenValue >= 0.1f)
                {
                    greenValue = Math.Abs(GetCosRGBValue(timeValue) - GetSinRGBValue(timeValue));
                    return;
                }

                greenValue = 0.0f;
                timeValue = 0.0f;
                _timer = new();
                _timer.Start();
                return;
            }

            if (blueValue < 0.99f && redValue >= 0.1f)
            {
                while (blueValue <= 0.95f)
                {
                    blueValue = GetSinRGBValue(timeValue);
                    return;
                }

                blueValue = 0.99f;
                timeValue = 0.0f;
                _timer = new();
                _timer.Start();
                return;
            }
            if (redValue > 0.001f && blueValue >= 0.99)
            {
                while (redValue >= 0.1f)
                {
                    redValue = Math.Abs(GetCosRGBValue(timeValue) - GetSinRGBValue(timeValue));
                    return;
                }
                redValue = 0.0f;
                timeValue = 0.0f;
                _timer = new();
                _timer.Start();
            }
        }
    }
}