using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utils;

public class BackgroundHandler : MonoBehaviour
{
    // pantagon
    Vector3[] pantagon =
    {
        new Vector3(Mathf.Cos((72 * 0 + 90) * Mathf.Deg2Rad), Mathf.Sin((72 * 0 + 90) * Mathf.Deg2Rad), 0),
        new Vector3(Mathf.Cos((72 * 1 + 90) * Mathf.Deg2Rad), Mathf.Sin((72 * 1 + 90) * Mathf.Deg2Rad), 0),
        new Vector3(Mathf.Cos((72 * 2 + 90) * Mathf.Deg2Rad), Mathf.Sin((72 * 2 + 90) * Mathf.Deg2Rad), 0),
        new Vector3(Mathf.Cos((72 * 3 + 90) * Mathf.Deg2Rad), Mathf.Sin((72 * 3 + 90) * Mathf.Deg2Rad), 0),
        new Vector3(Mathf.Cos((72 * 4 + 90) * Mathf.Deg2Rad), Mathf.Sin((72 * 4 + 90) * Mathf.Deg2Rad), 0)
    };

    LineRenderer render;
    float size = 12.0f;
    float rotation = 0;

    float spawn_timer = 0;

    // Start is called before the first frame update
    void Start()
    {
        render = gameObject.GetComponent<LineRenderer>();

        render.positionCount = 5;
        render.loop = true;
    }

    // Update is called once per frame
    void Update()
    {
        List<Vector3> vertices = new List<Vector3>();
        foreach (var point in pantagon)
        {
            vertices.Add(Matrix.Rotate(point * size, rotation));
        }
        render.SetPositions(vertices.ToArray());
    }
}
