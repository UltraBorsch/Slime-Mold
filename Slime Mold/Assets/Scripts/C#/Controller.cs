using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class Controller : MonoBehaviour {
    public int width, height, numOfAgents;
    public float agentSpeed;
    public ComputeShader slimeSim, agentDisplay;
    private ComputeBuffer agentBuffer = null;
    public MeshRenderer agentRenderer;

    public RenderTexture trailMap;

    public struct Agent {
        public Vector2 position;
        public Vector2 direction;

        public Agent(Vector2 position, Vector2 direction) {
            this.position = position;
            this.direction = direction;
        }
    }

    private void CreateAgents(Agent[] arr) {
        for (int i = 0; i < arr.Length; i++) {
            Vector2 position = new(width / 2, height / 2);
            float angle = Random.Range(0f, 1f) * 2f * Mathf.PI;
            Vector2 direction = new(Mathf.Cos(angle), Mathf.Sin(angle));
            arr[i] = new(position, direction);
        }
    }

    private void Start() {
        trailMap = new(width, height, 0) {
            enableRandomWrite = true,
            graphicsFormat = GraphicsFormat.R16G16B16A16_SFloat,
            autoGenerateMips = false,
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };
        trailMap.Create();
        agentRenderer.material.mainTexture = trailMap;

        Agent[] agents = new Agent[numOfAgents];
        CreateAgents(agents);
        ComputeHelper.CreateStructuredBuffer(ref agentBuffer, agents);

        slimeSim.SetBuffer(0, "agents", agentBuffer);
        slimeSim.SetInt("width", width);
        slimeSim.SetInt("height", height);
        slimeSim.SetInt("numAgents", numOfAgents);
        slimeSim.SetFloat("PI", Mathf.PI);
        slimeSim.SetFloat("moveSpeed", agentSpeed);
        slimeSim.SetTexture(0, "TrailMap", trailMap);
    }

    private void FixedUpdate() {
        slimeSim.SetFloat("deltaTime", Time.deltaTime);
        RunSimulation();
    }

    void OnDestroy() {
        ComputeHelper.Release(agentBuffer);
    }

    public void RunSimulation() {
        ComputeHelper.Run(slimeSim, numOfAgents);
    }
}
