using UnityEngine;
using System;

public static class ComputeStructs {

    [Serializable]
    public struct ComputeFormat<T> {
        public T value;
        public string name;
    };

    public struct Matrix2x3 : IEquatable<Matrix2x3>, IFormattable {
        public float m00;
        public float m10;
        public float m01;
        public float m11;
        public float m02;
        public float m12;


        private static readonly Matrix2x3 zeroMatrix = new Matrix2x3(new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f));

        public Matrix2x3(Vector3 r1, Vector3 r2) {
            m00 = r1[0];
            m01 = r1[1];
            m02 = r1[2];
            m10 = r2[0];
            m11 = r2[1];
            m12 = r2[2];
        }

        public bool Equals(Matrix2x3 other) {
            if (other is Matrix2x3 other2) {
                return GetRow(0) == other2.GetRow(0) && GetRow(1) == other2.GetRow(1);
            }

            return false;
        }

        public string ToString(string format, IFormatProvider formatProvider) {
            return GetRow(0).ToString() + "\n" + GetRow(1).ToString();
        }

        public static Matrix2x3 Zero => zeroMatrix;

        public Vector3 GetRow(int index) {
            if (index < 0 || index > 1)
                throw new IndexOutOfRangeException("Invalid matrix index!");
            if (index == 0)
                return new(m00, m01, m02);
            return new(m10, m11, m12);
        }
    }
}
