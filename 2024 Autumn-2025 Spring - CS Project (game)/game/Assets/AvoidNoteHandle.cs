using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utils;
using Config;
using RhythmGameObjects;

public class AvoidNoteHandle : MonoBehaviour
{
    // constants for better reading
    const int NONE = 0;
    const int BAD = 1;
    const int PERFECT = 3;

    GameObject director;
    GameObject hands;
    GameObject sfx;
    GameObject HitEffect;
    Chart chart;

    public int laneID;
    public float beat;

    // Start is called before the first frame update
    void Start()
    {
        hands = GameObject.Find("Hand Controller");
        director = GameObject.Find("Director");
        sfx = GameObject.Find("SfxPlayer");
        HitEffect = Resources.Load<GameObject>("GameObjects/HitEffect");
        chart = GameObject.Find("Parser").GetComponent<ParseChart>().chart;
    }

    // Update is called once per frame
    void Update()
    {
        float time = director.GetComponent<LevelHandler>().timer;
        float offset = chart.Offset;

        if (!PlayerConfig.autoplay)
        {
            Vector2 lHandPos = hands.GetComponent<HandDataProcess>().lHandPos;
            Vector2 rHandPos = hands.GetComponent<HandDataProcess>().rHandPos;
            Vector2 notePos = gameObject.transform.position;
            if (chart.Second(beat) + offset < time)
            {
                if ((lHandPos - notePos).magnitude <= BasicConfig.noteHitbox || (rHandPos - notePos).magnitude <= BasicConfig.noteHitbox)
                {
                    Destroy(gameObject);
                    director.GetComponent<LevelHandler>().performance.bad++;
                    director.GetComponent<LevelHandler>().performance.combo = 0;

                    GameObject effect = Instantiate(HitEffect);
                    effect.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, BasicConfig.judgelinePos);
                    effect.GetComponent<NoteHitEffect>().judgement = BAD;
                    effect.GetComponent<NoteHitEffect>().callAnim = true;

                    return;
                }
                else
                {
                    Destroy(gameObject);
                    sfx.GetComponent<AudioSource>().Play();

                    director.GetComponent<LevelHandler>().performance.perfect++;
                    director.GetComponent<LevelHandler>().performance.combo++;

                    GameObject effect = Instantiate(HitEffect);
                    effect.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, BasicConfig.judgelinePos);
                    effect.GetComponent<NoteHitEffect>().judgement = PERFECT;
                    effect.GetComponent<NoteHitEffect>().callAnim = true;

                    return;
                }
            }
        }
        else
        {
            if (chart.Second(beat) + offset < time)
            {
                Destroy(gameObject);
                director.GetComponent<LevelHandler>().performance.perfect++;
                director.GetComponent<LevelHandler>().performance.combo++;
                sfx.GetComponent<AudioSource>().Play();

                GameObject effect = Instantiate(HitEffect);
                effect.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, BasicConfig.judgelinePos);
                effect.GetComponent<NoteHitEffect>().judgement = PERFECT;
                effect.GetComponent<NoteHitEffect>().callAnim = true;

                return;
            }
        }
    }
}
