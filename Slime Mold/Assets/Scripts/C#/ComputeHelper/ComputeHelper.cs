using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using static ComputeStructs;

public static class ComputeHelper {
    private static readonly List<ComputeBuffer> createdBuffers = new();

    //static constructor. mostly just subscribes to the application quitting event.
    static ComputeHelper() {
        Application.quitting += OnQuit;
    }

    //on quit, release all the buffers.
    private static void OnQuit() {
        Release(createdBuffers.ToArray());
    }

    /// <summary> Dispatch a kernel of a shader. Also calculates optimal dispatch sizes. </summary>
    public static void Run(ComputeShader shader, int kernel, int xIterations, int yIterations = 1, int zIterations = 1) {
        Vector3Int ThreadGroupSizes = GetThreadGroupSizes(shader, kernel);

        int xGroups = Mathf.CeilToInt(xIterations / (float)ThreadGroupSizes.x);
        int yGroups = Mathf.CeilToInt(yIterations / (float)ThreadGroupSizes.y);
        int zGroups = Mathf.CeilToInt(zIterations / (float)ThreadGroupSizes.z);

        shader.Dispatch(kernel, xGroups, yGroups, zGroups);
    }

    /// <summary> Dispatch a kernel of a shader. Also calculates optimal dispatch sizes. </summary>
    //  thread sizes/dimension in kernel is how many threads the gpu will run during the job (usually multiple of 32 or 64)
    //  group size in dispatch is how many times per dimension we want to call the shader.
    //  optimal to call shader as few times as possible, so x/y/z iterations should be how many things in that dimension i need to do
    public static void Run(ComputeShader shader, string kernel, int xIterations, int yIterations = 1, int zIterations = 1) {
        Run(shader, shader.FindKernel(kernel), xIterations, yIterations, zIterations);
    }

    /// <summary> Handles all simple parameter passing to the given ComputeShader. </summary>
    public static void SetParam<T>(ComputeShader shader, T value, string valueName) {
        switch (value) {
            case int i: shader.SetInt(valueName, i); break;
            case float f: shader.SetFloat(valueName, f); break;
            case double d: shader.SetFloat(valueName, (float)d); break;
            case bool b: shader.SetBool(valueName, b); break;
            case Vector2 v2: shader.SetVector(valueName, v2); break;
            case Vector3 v3: shader.SetVector(valueName, v3); break;
            case Vector4 v4: shader.SetVector(valueName, v4); break;
            case Color c: shader.SetVector(valueName, c); break;
            case Matrix4x4 m: shader.SetMatrix(valueName, m); break;
            case int[] iarr: shader.SetInts(valueName, iarr); break;
            case float[] farr: shader.SetFloats(valueName, farr); break;
            case double[] darr: shader.SetFloats(valueName, darr.Select(d => (float)d).ToArray()); break;
            case Vector4[] varr: shader.SetVectorArray(valueName, varr); break;
            case Matrix4x4[] marr: shader.SetMatrixArray(valueName, marr); break;
            case Vector2Int iv2: shader.SetInts(valueName, new[] {iv2.x, iv2.y }); break;
            case Vector3Int iv3: shader.SetInts(valueName, new[] { iv3.x, iv3.y, iv3.z }); break;
            default: Debug.LogError($"Error: {typeof(T)} is not an implemented type to pass to shader {shader.name}."); break;
        }
    }

    public static void SetParam(ComputeShader shader, ComputeData data) {
        foreach (ComputeFormat<int> cf in data.ints) SetParam(shader, cf.value, cf.name);
        foreach (ComputeFormat<float> cf in data.floats) SetParam(shader, cf.value, cf.name);
        foreach (ComputeFormat<bool> cf in data.bools) SetParam(shader, cf.value, cf.name);
        foreach (ComputeFormat<Vector4> cf in data.vectors) SetParam(shader, cf.value, cf.name);
        foreach (ComputeFormat<Matrix4x4> cf in data.matrices) SetParam(shader, cf.value, cf.name);

        foreach (ComputeFormat<List<int>> cf in data.intLists) SetParam(shader, cf.value.ToArray(), cf.name);
        foreach (ComputeFormat<List<float>> cf in data.floatLists) SetParam(shader, cf.value.ToArray(), cf.name);
        foreach (ComputeFormat<List<Vector4>> cf in data.vectorLists) SetParam(shader, cf.value.ToArray(), cf.name);
        foreach (ComputeFormat<List<Matrix4x4>> cf in data.matrixLists) SetParam(shader, cf.value.ToArray(), cf.name);
    }

    /// <summary> Get thread group sizes for each dimension of a kernel.</summary>
    public static Vector3Int GetThreadGroupSizes(ComputeShader shader, string kernel) {
        shader.GetKernelThreadGroupSizes(shader.FindKernel(kernel), out uint x, out uint y, out uint z);
        return new((int)x, (int)y, (int)z);
    }

    /// <summary> Get thread group sizes for each dimension of a kernel.</summary>
    public static Vector3Int GetThreadGroupSizes(ComputeShader shader, int kernel) {
        shader.GetKernelThreadGroupSizes(kernel, out uint x, out uint y, out uint z);
        return new((int)x, (int)y, (int)z);
    }

    public static void SetBuffer(ComputeShader shader, string kernel, string BufferName, ComputeBuffer buffer) {
        shader.SetBuffer(shader.FindKernel(kernel), BufferName, buffer);
        if (!createdBuffers.Contains(buffer))
            createdBuffers.Add(buffer);
    }

    /// <summary> Creates and returns a buffer from basic data. </summary>
    public static ComputeBuffer CreateAndSetBuffer<T>(ComputeShader shader, string kernel, string BufferName, int size) {
        ComputeBuffer buffer = CreateBuffer<T>(size);
        shader.SetBuffer(shader.FindKernel(kernel), BufferName, buffer);
        return buffer;
    }

    /// <summary> Creates and returns a buffer from data. will also fill with the buffer with provided values. </summary>
    public static ComputeBuffer CreateAndSetBuffer<T>(ComputeShader shader, string kernel, string BufferName, T[] arr) where T : struct {
        ComputeBuffer buffer = CreateBuffer<T>(arr.Length);
        buffer.SetData(arr);
        shader.SetBuffer(shader.FindKernel(kernel), BufferName, buffer);
        return buffer;
    }

    /// <summary> Creates and returns a buffer from data. will also fill with the buffer with provided values. Does not set to a shader kernel.</summary>
    public static ComputeBuffer CreateAndSetBuffer<T>(T[] arr) {
        ComputeBuffer buffer = CreateBuffer<T>(arr.Length);
        buffer.SetData(arr);
        return buffer;
    }

    /// <summary> Creates buffer from an array of IStructable objects. </summary>
    public static ComputeBuffer CreateAndSetBuffer<T>(ComputeShader shader, string kernel, string BufferName, IStructable<T>[] arr) where T : struct {
        int count = arr.Length;

        T[] structs = new T[count];
        for (int i = 0; i < count; i++)
            structs[i] = arr[i].GetStruct();

        ComputeBuffer buffer = CreateBuffer<T>(count);
        buffer.SetData(structs);
        shader.SetBuffer(shader.FindKernel(kernel), BufferName, buffer);
        return buffer;
    }

    /// <summary> Create a buffer of a given size of type T. </summary>
    public static ComputeBuffer CreateBuffer<T>(int size) {
        ComputeBuffer buffer = new(size, System.Runtime.InteropServices.Marshal.SizeOf(typeof(T)));
        if (!createdBuffers.Contains(buffer))
            createdBuffers.Add(buffer);
        return buffer;
    }

    /// <summary> Fills provided array with data contained within the buffer. Then released the buffer. </summary>
    public static void GetDataAndRelease<T>(ComputeBuffer buffer, T[] arr) {
        buffer.GetData(arr);
        Release(buffer);
    }

    /// <summary> Creates an array and fills it with the data contained within the provided Buffer. Then releases the buffer. </summary>
    public static T[] GetDataAndRelease<T>(ComputeBuffer buffer) {
        T[] arr = new T[buffer.count];
        buffer.GetData(arr);
        Release(buffer);
        return arr;
    }

    /// <summary> Releases supplied buffer(s) if not null </summary>
    public static void Release(params ComputeBuffer[] buffers) {
        for (int i = 0; i < buffers.Length; i++) {
            buffers[i]?.Release();
            createdBuffers.Remove(buffers[i]);
        }
    }

    /// <summary> Copy the contents of one render texture into another. Assumes textures are the same size. </summary>
    public static void CopyRenderTexture(Texture source, RenderTexture target) {
        Graphics.Blit(source, target);
    }

    /// <summary> Creates a RenderTexture of given dimensions. Convenience method for shaders. </summary>
    public static RenderTexture TextureFactory(int width, int height) {
        RenderTexture temp = new(width, height, 0) {
            enableRandomWrite = true,
            graphicsFormat = GraphicsFormat.R16G16B16A16_SFloat,
            autoGenerateMips = false,
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };
        temp.Create();

        return temp;
    }

    /// <summary> Creates a RenderTexture of given resolution. Convenience method for shaders. </summary>
    public static RenderTexture TextureFactory(Vector2Int resolution) {
        return TextureFactory(resolution.x, resolution.y);
    }

    /// <summary> Creates a RenderTexture of given resolution. Convenience method for shaders. </summary>
    public static RenderTexture TextureFactory(Resolution resolution) {
        return TextureFactory(resolution.width, resolution.height);
    }
}