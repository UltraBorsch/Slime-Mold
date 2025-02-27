using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Helper;

[Serializable]
[CreateAssetMenu(menuName = "Simulation/Species Template")]
public class AgentSpecies : ScriptableObject {
    public float moveSpeed, trailWeight, turnSpeed, sensorAngle, sensorDistance;
    public int sensorSize;
    public Color colour;
    public int numberOfAgents;
    public SpawnData spawnData;
    [HideInInspector] public int speciesId;

   
    private Species species;
    public Species SpeciesStruct { get { UpdateSpecies(); return species; } }

    public void Setup(int speciesId) {
        species = new Species(this);
        this.speciesId = speciesId;
        spawnData.Setup(speciesId);
    }

    public void UpdateSpecies() {
        species.turnSpeedPos = CreateRotationMatrix(turnSpeed * Mathf.Deg2Rad * Time.deltaTime);
        species.turnSpeedNeg = CreateRotationMatrix(-turnSpeed * Mathf.Deg2Rad * Time.deltaTime);
    }
}
