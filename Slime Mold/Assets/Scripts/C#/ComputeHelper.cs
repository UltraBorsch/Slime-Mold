using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ComputeHelper {
    public static Vector3Int GetThreadGroupSizes(ComputeShader compute, int kernelIndex = 0) {
        uint x, y, z;
        compute.GetKernelThreadGroupSizes(kernelIndex, out x, out y, out z);
        return new Vector3Int((int)x, (int)y, (int)z);
    }

    // Convenience method for dispatching a compute slimeSim.
    // It calculates the number of thread groups based on the number of iterations needed.
    public static void Run(ComputeShader cs, int numIterationsX, int numIterationsY = 1, int numIterationsZ = 1, int kernelIndex = 0) {
        Vector3Int threadGroupSizes = GetThreadGroupSizes(cs, kernelIndex);
        int numGroupsX = Mathf.CeilToInt(numIterationsX / (float)threadGroupSizes.x);
        int numGroupsY = Mathf.CeilToInt(numIterationsY / (float)threadGroupSizes.y);
        int numGroupsZ = Mathf.CeilToInt(numIterationsZ / (float)threadGroupSizes.y);
        cs.Dispatch(kernelIndex, numGroupsX, numGroupsY, numGroupsZ);
    }

    public static void CreateStructuredBuffer<T>(ref ComputeBuffer buffer, int count) {
        int stride = System.Runtime.InteropServices.Marshal.SizeOf(typeof(T));
        bool createNewBuffer = buffer == null || !buffer.IsValid() || buffer.count != count || buffer.stride != stride;
        if (createNewBuffer) {
            Release(buffer);
            buffer = new ComputeBuffer(count, stride);
        }
    }

    public static void CreateStructuredBuffer<T>(ref ComputeBuffer buffer, T[] data) {
        CreateStructuredBuffer<T>(ref buffer, data.Length);
        buffer.SetData(data);
    }

    /// Releases supplied buffer/s if not null
    public static void Release(params ComputeBuffer[] buffers) {
        for (int i = 0; i < buffers.Length; i++) {
            if (buffers[i] != null) {
                buffers[i].Release();
            }
        }
    }

    /// Copy the contents of one render texture into another. Assumes textures are the same size.
    public static void CopyRenderTexture(Texture source, RenderTexture target) {
        Graphics.Blit(source, target);
    }
}
