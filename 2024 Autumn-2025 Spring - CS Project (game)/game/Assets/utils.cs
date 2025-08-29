using System;
using System.Collections.Generic;
using UnityEngine;


using Config;
using RhythmGameObjects;
using UnityEngine.Rendering;

// General Purpose Functions For Computations & Others
namespace Utils
{
    public class Compute
    {
        public static float Speed(float s)
        {
            return s * 20;
        }

        /// <summary>
        /// Transform the position from charting coordinates to world coordinates
        /// </summary>
        /// <param name="x">-20 ~ 20</param>
        /// <param name="y">-10 ~ 10</param>
        /// <returns></returns>
        public static Vector2 TransformCoords(float x, float y)
        {
            return new Vector2(x * 2.4f / 20, y * 1.2f / 10);
        }

        public static Vector3 TransformHandPos(Vector3 handPos)
        {
            Vector3 cameraPos = GameObject.Find("Main Camera").transform.position;
            float judgeZPos = BasicConfig.judgelinePos;
            handPos.z = BasicConfig.trackerPos.z;
            return (handPos - cameraPos) * (judgeZPos - cameraPos.z) / (handPos.z - cameraPos.z) + cameraPos;
        }

        /// <summary>
        /// Compute the integral of the line function
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static float Integral(float yA, float yB, float dx)
        {
            if (yA * yB < 0)
            {
                float lenA = Math.Abs(yA) / (Math.Abs(yA) + Math.Abs(yB)) * dx;
                return (yA * lenA + yB * (dx - lenA)) / 2;
            }
            else
            {
                return (yA + yB) * dx / 2;
            }
        }

        /// <summary>
        /// Find the closest data to a given value. Return the index of the data
        /// </summary>
        /// <param name="data"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static int BinarySearch(List<Vector3> data, float val)
        {
            int a = 0;
            int b = data.Count - 1;
            while (b - a > 1)
            {
                int m = (a + b) / 2;
                if (data[m].z < val)
                {
                    a = m;
                }
                else
                {
                    b = m;
                }
            }
            return a;
        }
    }

    public class Matrix
    {
        public static Vector3 Multiply(List<Vector3> mat, Vector3 vec)
        {
            if (mat.Count != 3)
            {
                throw new Exception("Matrix Size Not Match: mat is size of 3x" + mat.Count);
            }
            else
            {
                Vector3 output = new Vector3();
                for (int i = 0; i < 3; i++)
                {
                    output[i] = mat[0][i] * vec[0] + mat[1][i] * vec[1] + mat[2][i] * vec[2];
                }
                return output;
            }
        }

        public static Vector3 Rotate(Vector3 point, float rad)
        {
            List<Vector3> mat = new List<Vector3>
            {
                new(Mathf.Cos(rad), Mathf.Sin(rad), 0),
                new(-Mathf.Sin(rad), Mathf.Cos(rad), 0),
                new(0, 0, 1)
            };
            return Multiply(mat, point);
        }
    }
}