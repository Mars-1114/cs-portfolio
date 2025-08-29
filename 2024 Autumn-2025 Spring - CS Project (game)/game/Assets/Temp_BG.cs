using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Temp_BG : MonoBehaviour
{
    List<GameObject> children = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        foreach (Transform child in transform)
        {
            children.Add(child.gameObject);
        }
        transform.position = new Vector3(0, 4.5f, -20.0f);
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.z <= -10.0f)
        {
            foreach(var child in children)
            {
                child.SetActive(Random.Range(0, 3) != 0);
            }
            transform.position = new Vector3(0, 4.5f, 92.4f);
        }
        transform.Translate(Vector3.back * 120.0f * Time.deltaTime);
    }
}
