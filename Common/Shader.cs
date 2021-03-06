﻿using System;
using System.IO;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL4;


namespace LearnOpenGL_TK.Common
{
    //A simple class meant to help create shaders.
    public class Shader: System.IDisposable
    {
        int Handle;

        
        //This is how you create a simple shader.
        //Shaders are written in GLSL, which is a language very similar to C in its semantics.
        //The GLSL source is compiled *at runtime*, so it can optimize itself for the graphics card it's currently being used on.
        //A commented example of GLSL can be found in shader.vert
        public Shader(string vertPath, string fragPath)
        {
            //There are several different types of shaders, but the only two you need for basic rendering are the vertex and fragment shaders.
            //The vertex shader is responsible for moving around vertices, and uploading that data to the fragment shader.
            //  The vertex shader won't be too important here, but they'll be more important later.
            //The fragment shader is responsible for then converting the vertices to "fragments", which represent all the data OpenGL needs to draw a pixel.
            //  The fragment shader is what we'll be using the most here.

            //Create handles for both the vertex and fragment shaders.
            int VertexShader;
            int FragmentShader;

            //Load vertex shader and compile
            //LoadSource is a simple function that just loads all text from the file whose path is given.
            string VertexShaderSource = LoadSource(vertPath);

            //GL.CreateShader will create an empty shader (obviously). The ShaderType enum denotes which type of shader will be created.
            VertexShader = GL.CreateShader(ShaderType.VertexShader);

            //Now, bind the GLSL source code
            GL.ShaderSource(VertexShader, VertexShaderSource);

            //And then compile
            GL.CompileShader(VertexShader);

            //Check for compile errors
            string infoLogVert = GL.GetShaderInfoLog(VertexShader);
            if (infoLogVert != System.String.Empty)
                System.Console.WriteLine(infoLogVert);


            //Do the same thing for the fragment shader
            string FragmentShaderSource = LoadSource(fragPath);
            FragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(FragmentShader, FragmentShaderSource);
            GL.CompileShader(FragmentShader);

            //Check for compile errors
            string infoLogFrag = GL.GetShaderInfoLog(VertexShader);
            if (infoLogFrag != System.String.Empty)
                System.Console.WriteLine(infoLogFrag);


            //These two shaders must then be merged into a shader program, which can then be used by OpenGL.
            //To do this, create a program...
            Handle = GL.CreateProgram();

            //Attach both shaders...
            GL.AttachShader(Handle, VertexShader);
            GL.AttachShader(Handle, FragmentShader);

            //And then link them together.
            GL.LinkProgram(Handle);

            //Check for linker errors
            string infoLogLink = GL.GetProgramInfoLog(Handle);
            if (infoLogLink != System.String.Empty)
                System.Console.WriteLine(infoLogLink);


            //Now that it's done, clean up.
            //When the shader program is linked, it no longer needs the individual shaders attacked to it; the compiled code is copied into the shader program.
            //Detact them, and then delete them.
            GL.DetachShader(Handle, VertexShader);
            GL.DetachShader(Handle, FragmentShader);
            GL.DeleteShader(FragmentShader);
            GL.DeleteShader(VertexShader);
        }


        //A wrapper function that enables the shader program.
        public void Use()
        {
            GL.UseProgram(Handle);
        }


        //The shader sources provided with this project use hardcoded layout(location)-s. If you want to do it dynamically,
        //you can omit the layout(location=X) lines in the vertex shader, and use this in VertexAttribPointer instead of the hardcoded values.
        public int GetAttribLocation(string attribName)
        {
            return GL.GetAttribLocation(Handle, attribName);
        }


        //Just loads the entire file into a string.
        private string LoadSource(string path)
        {
            string readContents;

            using (StreamReader streamReader = new StreamReader(path, Encoding.UTF8))
            {
                readContents = streamReader.ReadToEnd();
            }

            return readContents;
        }
        
        //Uniform setters
        //Uniforms are variables that can be set by user code, instead of reading them from the VBO.
        //You use VBOs for vertex-related data, and uniforms for almost everything else.
        
        //Setting a uniform is almost always the exact same, so I'll explain it here once, instead of in every method:
        //    1. Bind the program you want to set the uniform on
        //    2. Get a handle to the location of the uniform with GL.GetUniformLocation.
        //    3. Use the appropriate GL.Uniform* function to set the uniform.

        /// <summary>
        /// Set a uniform int on this shader.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        public void SetInt(string name, int data)
        {
            GL.UseProgram(Handle);
            var location = GL.GetUniformLocation(Handle, name);
            GL.Uniform1(location, data);
        }

        /// <summary>
        /// Set a uniform Matrix4 on this shader
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        /// <remarks>
        ///   <para>
        ///   The matrix is transposed before being sent to the shader.
        ///   </para>
        /// </remarks>
        public void SetMatrix4(string name, Matrix4 data)
        {
            GL.UseProgram(Handle);
            var location = GL.GetUniformLocation(Handle, name);
            GL.UniformMatrix4(location, true, ref data);
        }


        //This section is dedicated to cleaning up the shader after it's finished.
        //Doing this solely in a finalizer results in a crash because of the Object-Oriented Language Problem
        //( https://www.khronos.org/opengl/wiki/Common_Mistakes#The_Object_Oriented_Language_Problem )
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                GL.DeleteProgram(Handle);

                disposedValue = true;
            }
        }

        ~Shader()
        {
            Dispose(false);
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
