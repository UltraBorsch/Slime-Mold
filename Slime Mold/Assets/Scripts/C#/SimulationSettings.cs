using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Simulation/Simulation Template")]
public class SimulationSettings : ScriptableObject {
    public int width, height, timeSteps;
    public float evaporateSpeed, diffuseSpeed;
    public AgentSpecies[] species;

    public void OnEnable() {
        timeSteps = Mathf.Max(1, timeSteps);
    }
}
