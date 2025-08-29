using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Config;
using System;

public class NoteHitEffect : MonoBehaviour
{
    // constants for better reading
    const int MISS = 0;
    const int BAD = 1;
    const int GOOD = 2;
    const int PERFECT = 3;

    LineRenderer effect;

    float animTimer = 0;
    float width = 0;
    float size = 0;
    const float opacity = 0.5f;
    const float animTime = 0.3f;
    const float maxSize = 0.5f;

    public int judgement;
    public bool callAnim = true;

    // Start is called before the first frame update
    void Start()
    {
        if (callAnim)
        {
            GameObject.Find("UI").GetComponent<Render>().animTrigger_Judgement = true;
            GameObject.Find("UI").GetComponent<Render>().judgement_latest = judgement;
        }
        effect = gameObject.GetComponent<LineRenderer>();

        // initialize
        effect.useWorldSpace = true;
        effect.sortingOrder = 1;
        effect.material = new Material(Shader.Find("Sprites/Default"));
        effect.loop = true;
        effect.positionCount = 4;

        // set color
        Color color = Color.white;
        switch (judgement)
        {
            case PERFECT:
                color = new Color(0.741f, 0.741f, 0.4f, opacity);
                break;
            case GOOD:
                color = new Color(0.000f, 0.780f, 0.157f, opacity);
                break;
            case BAD:
                color = new Color(0.780f, 0.188f, 0.055f, opacity);
                break;
            default:
                color = Color.white;
                break;
        }
        effect.startColor = color;
        effect.endColor = color;
    }

    // Update is called once per frame
    void Update()
    {
        if (animTimer < animTime)
        {
            // set parameters
            width = (animTimer / animTime <= 0.5f) ? maxSize * 2 * (animTimer / animTime) * (animTimer / animTime) : maxSize * (1 - (-2 * (animTimer / animTime) + 2) * (-2 *  (animTimer / animTime) + 2) / 2);
            size = maxSize * (1 - Mathf.Pow(1 - animTimer / animTime, 4));

            effect.SetPositions(new Vector3[] { });
            // draw line
            Vector3[] points =
            {
                new Vector3(size, 0, 0) + gameObject.transform.position,
                new Vector3(0, size, 0) + gameObject.transform.position,
                new Vector3(-size, 0, 0) + gameObject.transform.position,
                new Vector3(0, -size, 0) + gameObject.transform.position
            };

            effect.startWidth = size - width;
            effect.endWidth = size - width;
            effect.SetPositions(points);

        }
        else
        {
            Destroy(gameObject);
            return;
        }
        animTimer += Time.deltaTime;
    }
}
