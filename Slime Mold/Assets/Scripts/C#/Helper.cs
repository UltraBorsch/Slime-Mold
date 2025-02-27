using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Helper {
    public struct Agent {
        public Vector2 position, direction;
        public int speciesIndex;

        public Agent(Vector2 position, Vector2 direction, int speciesIndex) {
            this.position = position;
            this.direction = direction;
            this.speciesIndex = speciesIndex;
        }
    }

    public struct Species {
        public static readonly int Size = 23 * sizeof(float) + sizeof(int);
        public float moveSpeed, trailWeight, sensorDistance;
        public Matrix2x2 turnSpeedPos, turnSpeedNeg, sensorAnglePos, sensorAngleNeg;
        public int sensorSize;
        public Vector4 colour, mask;

        public Species(AgentSpecies specie) {
            moveSpeed = specie.moveSpeed;
            trailWeight = specie.trailWeight;
            sensorDistance = specie.sensorDistance;
            sensorSize = specie.sensorSize;
            colour = specie.colour;

            sensorAnglePos = CreateRotationMatrix(specie.sensorAngle * Mathf.Deg2Rad);
            sensorAngleNeg = CreateRotationMatrix(-specie.sensorAngle * Mathf.Deg2Rad);

            turnSpeedPos = CreateRotationMatrix(specie.turnSpeed * Mathf.Deg2Rad * Time.deltaTime);
            turnSpeedNeg = CreateRotationMatrix(-specie.turnSpeed * Mathf.Deg2Rad * Time.deltaTime);
            mask = new(0, 0, 0, 0);
            mask[specie.speciesId] = 1;
        }
    }

    public struct Matrix2x2 {
        public float _11, _12, _21, _22;

        public Matrix2x2(Vector2 r1, Vector2 r2) {
            _11 = r1[0];
            _12 = r1[1];
            _21 = r2[0];
            _22 = r2[1];
        }
    }

    public static Matrix2x2 CreateRotationMatrix(float angle) {
        Vector2 r1 = new(Mathf.Cos(angle), -Mathf.Sin(angle));
        Vector2 r2 = new(Mathf.Sin(angle), Mathf.Cos(angle));

        return new(r1, r2);
    }

    public static int PosOrNeg() {
        return Random.Range(0f, 1f) < 0.5 ? -1 : 1;
    }
}
