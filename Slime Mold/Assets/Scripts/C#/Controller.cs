using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using static ComputeHelper;
using static Helper;

public class Controller : MonoBehaviour {
    public SimulationSettings settings;
    public ComputeShader slimeSim;
    private ComputeBuffer agentBuffer = null, speciesBuffer = null;
    public MeshRenderer agentRenderer;
    private AgentSpawner agentSpawner;

    private Species[] speciesStructs;

    public RenderTexture trailMap, processedTrailMap, colourMap;

    private int numOfAgents;

    private RenderTexture TextureFactory() {
        RenderTexture temp = new(settings.width, settings.height, 0) {
            enableRandomWrite = true,
            graphicsFormat = GraphicsFormat.R16G16B16A16_SFloat,
            autoGenerateMips = false,
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };
        temp.Create();

        return temp;
    }

    private void Start() {
        //Camera.main.aspect = width / height;
        float quadHeight = Camera.main.orthographicSize * 2f, quadWidth = quadHeight * settings.width / settings.height;
        agentRenderer.transform.localScale = new(quadWidth, quadHeight, 1);

        trailMap = TextureFactory();
        processedTrailMap = TextureFactory();
        colourMap = TextureFactory();
        agentRenderer.material.mainTexture = colourMap;

        speciesStructs = new Species[settings.species.Length];
        for (int i = 0; i < settings.species.Length; i++) {
            settings.species[i].Setup(i);
            numOfAgents += settings.species[i].numberOfAgents;
        }

        Agent[] agents = new Agent[numOfAgents];
        agentSpawner = new(settings.width, settings.height);

        int startIndex = 0;
        foreach (AgentSpecies species in settings.species) {
            agents = agentSpawner.SpawnAgents(species, startIndex, agents);
            startIndex += species.numberOfAgents;
        }

        CreateStructuredBuffer(ref agentBuffer, agents);

        for (int i = 0; i < settings.species.Length; i++)
            speciesStructs[i] = settings.species[i].SpeciesStruct;

        settings.timeSteps = Mathf.Max(1, settings.timeSteps);

        slimeSim.SetBuffer(0, "agents", agentBuffer);
        slimeSim.SetInt("width", settings.width);
        slimeSim.SetInt("height", settings.height);
        slimeSim.SetInt("numAgents", numOfAgents);
        slimeSim.SetInt("timeSteps", settings.timeSteps);
        slimeSim.SetInt("numOfSpecies", settings.species.Length);
        slimeSim.SetFloat("evaporateSpeed", settings.evaporateSpeed);
        slimeSim.SetFloat("diffuseSpeed", settings.diffuseSpeed);
        slimeSim.SetTexture(0, "trailMap", trailMap);
        slimeSim.SetTexture(1, "trailMap", trailMap);
        slimeSim.SetTexture(1, "processedTrailMap", processedTrailMap);
        slimeSim.SetTexture(2, "processedTrailMap", processedTrailMap);
        slimeSim.SetTexture(2, "colourMap", colourMap);
    }

    private void FixedUpdate() {
        slimeSim.SetFloat("deltaTime", Time.deltaTime);

        for (int i = 0; i < settings.species.Length; i++) 
            speciesStructs[i] = settings.species[i].SpeciesStruct;

        CreateStructuredBuffer(ref speciesBuffer, speciesStructs);
        slimeSim.SetBuffer(0, "speciesIndex", speciesBuffer);
        slimeSim.SetBuffer(2, "speciesIndex", speciesBuffer);

        RunSimulation();
    }

    void OnDestroy() {
        Release(agentBuffer);
        Release(speciesBuffer);
    }

    public void RunSimulation() {
        Run(slimeSim, numOfAgents, kernelIndex: 0);
        Run(slimeSim, settings.width, settings.height, kernelIndex: 1);
        Run(slimeSim, settings.width, settings.height, kernelIndex: 2);
        CopyRenderTexture(processedTrailMap, trailMap);
    }
}