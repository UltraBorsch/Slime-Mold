using System.Collections.Generic;
using UnityEngine;
using static ComputeStructs;

[CreateAssetMenu(fileName = "ComputeData", menuName = "Compute Helper/ComputeData")]
public class ComputeData : ScriptableObject {
    //list of ints, floats, bools, vectors, matrices
    public List<ComputeFormat<int>> ints;
    public List<ComputeFormat<float>> floats;
    public List<ComputeFormat<bool>> bools;
    public List<ComputeFormat<Vector4>> vectors;
    public List<ComputeFormat<Matrix4x4>> matrices;

    //list of a list of ints, floats, vectors, matrices.
    public List<ComputeFormat<List<int>>> intLists;
    public List<ComputeFormat<List<float>>> floatLists;
    public List<ComputeFormat<List<Vector4>>> vectorLists;
    public List<ComputeFormat<List<Matrix4x4>>> matrixLists;
}
