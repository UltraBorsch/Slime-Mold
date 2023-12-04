using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class Controller : MonoBehaviour {
    public int width, height;
    public ComputeShader shader;

    public RenderTexture texture;

    private void OnRenderImage(RenderTexture src, RenderTexture dest) {
        if (texture == null) {
            texture = new(width, height, 0) {
                enableRandomWrite = true,
                graphicsFormat = GraphicsFormat.R16G16B16A16_SFloat,
                autoGenerateMips = false,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            texture.Create();
        }

        shader.SetInt("width", width);
        shader.SetInt("height", height);
        shader.SetTexture(0, "Texture", texture);
        ComputeHelper.Run(shader, width, height);

        Graphics.Blit(texture, dest);
    }


}
