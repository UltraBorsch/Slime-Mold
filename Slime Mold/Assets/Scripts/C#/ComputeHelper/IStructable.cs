using UnityEngine;

public interface IStructable<T> where T : struct {
    /// <summary> Returns the struct representation of this object. </summary>
    public T GetStruct();
}
