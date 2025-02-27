using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Helper;

public enum SpawnShape {
    Circle,
    Box,
    Quadrants
}

public enum SpawnAngle {
    Inward = -1,
    Random = 0,
    Outward = 1
}

[System.Serializable]
public struct SpawnData {
    public float mainAxisLow, mainAxisHigh;
    public float secondAxisLow, secondAxisHigh;
    public SpawnShape shape;
    public SpawnAngle angle;
    [HideInInspector] public int speciesID;

    public void Setup(int speciesID) {
        mainAxisLow = Mathf.Max(mainAxisLow, 0);
        mainAxisHigh = Mathf.Max(Mathf.Min(mainAxisHigh, 1), mainAxisLow);
        secondAxisLow = Mathf.Max(secondAxisLow, 0);
        secondAxisHigh = Mathf.Max(Mathf.Min(secondAxisHigh, 1), secondAxisLow);
        this.speciesID = speciesID;
    }
}

public class AgentSpawner {
    private readonly float width, height;

    public AgentSpawner(float width, float height) {
        this.width = width;
        this.height = height;
    }

    public Agent[] SpawnAgents(AgentSpecies species, int startIndex, Agent[] agents) {
        SpawnData data = species.spawnData;
        int endIndex = startIndex + species.numberOfAgents;

        if (data.shape == SpawnShape.Circle)
            return CircleSpawn(startIndex, endIndex, agents, data);
        else if (data.shape == SpawnShape.Box)
            return BoxSpawn(startIndex, endIndex, agents, data);
        else if (data.shape == SpawnShape.Quadrants)
            return QuadrantSpawn(startIndex, endIndex, agents, data);

        //should not happen, but good error prevention
        return agents;
    }

    //direction: -1 = inward, 1 = outward, 0 = random
    private Agent[] CircleSpawn(int startIndex, int endIndex, Agent[] agents, SpawnData data) {
        float radius = Mathf.Min(width, height) / 2 - 1, angle, rad;
        Vector2 center = new(width / 2, height / 2), dir, position;
        
        for (int i = startIndex; i < endIndex; i++) {
            angle = Random.Range(0f, 2f * Mathf.PI);
            dir = new(Mathf.Cos(angle), Mathf.Sin(angle));
            rad = radius * Random.Range(data.mainAxisLow, data.mainAxisHigh); ;
            position = center + rad * dir;

            if (data.angle == 0) {
                angle = Random.Range(0f, 2f * Mathf.PI);
                dir = new(Mathf.Cos(angle), Mathf.Sin(angle));
            } else
                dir *= (int)data.angle;

            agents[i] = new(position, dir, data.speciesID);
        }

        return agents;
    }

    private Agent[] BoxSpawn(int startIndex, int endIndex, Agent[] agents, SpawnData data) {
        Vector2 center = new(width / 2, height / 2), dir, position;
        float angle;

        for (int i = startIndex; i < endIndex; i++) {
            angle = Random.Range(0f, 2f * Mathf.PI);
            dir = new(Mathf.Cos(angle), Mathf.Sin(angle));

            float width = this.width * Random.Range(data.mainAxisLow, data.mainAxisHigh);
            float height = this.height * Random.Range(data.secondAxisLow, data.secondAxisHigh);

            //sets dir to be the outward normal at first
            //ceiling/floor collision
            if (Mathf.Abs(dir.y * width) > Mathf.Abs(dir.x * height)) {
                position = center + new Vector2(height * dir.x / (2 * Mathf.Abs(dir.y)), height * Mathf.Sign(dir.y) * 0.5f);
                dir = new(0, Mathf.Sign(dir.y));
            } else { //wall collision
                position = center + new Vector2(width * Mathf.Sign(dir.x) * 0.5f, width * dir.y / (2 * Mathf.Abs(dir.x)));
                dir = new(Mathf.Sign(dir.x), 0);
            }

            if (data.angle == SpawnAngle.Random) {
                angle = Random.Range(0f, 2f * Mathf.PI);
                dir = new(Mathf.Cos(angle), Mathf.Sin(angle));
            } else
                dir *= (int)data.angle;

            agents[i] = new(position, dir, data.speciesID);
        }

        return agents;
    }

    //for sake of simplicity, only uses random angles. there is no intuative inward/outward
    private Agent[] QuadrantSpawn(int startIndex, int endIndex, Agent[] agents, SpawnData data) {
        Vector2 center = new(width / 2, height / 2), dir, position;
        float halfWidth = center.x, halfHeight = center.y, xOffset, yOffset, angle;

        for (int i = startIndex; i < endIndex; i++) {
            angle = Random.Range(0f, 2f * Mathf.PI);
            dir = new(Mathf.Cos(angle), Mathf.Sin(angle));

            xOffset = Random.Range(data.mainAxisLow, data.mainAxisHigh) * PosOrNeg() * halfWidth;
            yOffset = Random.Range(data.secondAxisLow, data.secondAxisHigh) * PosOrNeg() * halfHeight;
            position = center + new Vector2(xOffset, yOffset);

            agents[i] = new(position, dir, data.speciesID);
        }

        return agents;
    }
}
