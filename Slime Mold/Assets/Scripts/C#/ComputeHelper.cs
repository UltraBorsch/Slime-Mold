using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ComputeHelper {

    //run a kernel of a shader, calculates optimal dispatch sizes
    //  thread sizes/dimension in kernel is how many threads the gpu will run during the job (usually multiple of 32 or 64)
    //  group size in dispatch is how many times per dimension we want to call the shader.
    //  optimal to call shader as few times as possible, so x/y/z iterations should be how many things in that dimension i need to do
    public static void Run(ComputeShader shader, string kernel, int xIterations, int yIterations = 1, int zIterations = 1) {
        Vector3Int ThreadGroupSizes = GetThreadGroupSizes(shader, kernel);

        int xGroups = Mathf.CeilToInt(xIterations / (float)ThreadGroupSizes.x);
        int yGroups = Mathf.CeilToInt(yIterations / (float)ThreadGroupSizes.y);
        int zGroups = Mathf.CeilToInt(zIterations / (float)ThreadGroupSizes.z);

        shader.Dispatch(shader.FindKernel(kernel), xGroups, yGroups, zGroups);
    }

    //handles all simple param passing (i.e. not textures and buffers, since those require kernel indices and whatnot)
    public static void SetParam<T>(ComputeShader shader, T value, string valueName) {
        switch (value) {
            case int i: shader.SetInt(valueName, i); break;
            case float f: shader.SetFloat(valueName, f); break;
            case bool b: shader.SetBool(valueName, b); break;
            case Vector2 v2: shader.SetVector(valueName, v2); break;
            case Vector3 v3: shader.SetVector(valueName, v3); break;
            case Vector4 v4: shader.SetVector(valueName, v4); break;
            case Matrix4x4 m: shader.SetMatrix(valueName, m); break;
            case int[] iarr: shader.SetInts(valueName, iarr); break;
            case float[] farr: shader.SetFloats(valueName, farr); break;
            case Vector4[] varr: shader.SetVectorArray(valueName, varr); break;
            case Matrix4x4[] marr: shader.SetMatrixArray(valueName, marr); break;
            default: Debug.LogError($"Error: {typeof(T)} is not an implemented type to pass to shader {shader.name}."); break;
        }
    }

    //get thread group sizes for each dimension of a kernel
    public static Vector3Int GetThreadGroupSizes(ComputeShader shader, string kernel) {
        shader.GetKernelThreadGroupSizes(shader.FindKernel(kernel), out uint x, out uint y, out uint z);
        return new((int)x, (int)y, (int)z);
    }

    //creates and returns a buffer from basic data
    public static ComputeBuffer CreateAndSetBuffer<T>(ComputeShader shader, string kernel, string BufferName, int size) {
        ComputeBuffer buffer = CreateBuffer<T>(size);
        shader.SetBuffer(shader.FindKernel(kernel), BufferName, buffer);
        return buffer;
    }

    //creates and returns a buffer from data. will also fill with values
    public static ComputeBuffer CreateAndSetBuffer<T>(ComputeShader shader, string kernel, string BufferName, T[] arr) {
        ComputeBuffer buffer = CreateBuffer<T>(arr.Length);
        buffer.SetData(arr);
        shader.SetBuffer(shader.FindKernel(kernel), BufferName, buffer);
        return buffer;
    }

    public static ComputeBuffer CreateBuffer<T>(int size) {
        ComputeBuffer buffer = new(size, System.Runtime.InteropServices.Marshal.SizeOf(typeof(T)));
        return buffer;
    }

    public static void GetDataAndRelease<T>(ComputeBuffer buffer, T[] arr) {
        buffer.GetData(arr);
        buffer.Release();
    }

    public static T[] GetDataAndRelease<T>(ComputeBuffer buffer) {
        T[] arr = new T[buffer.count];
        buffer.GetData(arr);
        buffer.Release();
        return arr;
    }

    /// Releases supplied buffer/s if not null
    public static void Release(params ComputeBuffer[] buffers) {
        for (int i = 0; i < buffers.Length; i++) {
            buffers[i]?.Release();
        }
    }

    /// Copy the contents of one render texture into another. Assumes textures are the same size.
    public static void CopyRenderTexture(Texture source, RenderTexture target) {
        Graphics.Blit(source, target);
    }
}
