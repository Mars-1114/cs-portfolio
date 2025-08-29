using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utils;
using Config;
using RhythmGameObjects;

public class ClapNoteHandle : MonoBehaviour
{
    // constants for easier read
    const int MISS = 0;
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
            Vector2 clapPos = hands.GetComponent<HandDataProcess>().clapPos;
            Vector2 notePos = gameObject.transform.position;

            if ((clapPos - notePos).magnitude <= BasicConfig.noteHitbox)
            {
                director.GetComponent<Judgement>().detectedClapNotes.Add(new JudgeNote(gameObject, beat));
            }
            // miss
            if (time >= chart.Second(beat) + BasicConfig.judgementTiming[1] + offset)
            {
                Destroy(gameObject);
                director.GetComponent<LevelHandler>().performance.miss++;
                director.GetComponent<LevelHandler>().performance.combo = 0;

                GameObject.Find("UI").GetComponent<Render>().animTrigger_Judgement = true;
                GameObject.Find("UI").GetComponent<Render>().judgement_latest = MISS;

                return;
            }
        }
        else if (chart.Second(beat) < time - offset)
        {
            Destroy(gameObject);
            sfx.GetComponent<AudioSource>().Play();
            director.GetComponent<LevelHandler>().performance.perfect++;
            director.GetComponent<LevelHandler>().performance.combo++;

            GameObject effect = Instantiate(HitEffect);
            effect.transform.position = new Vector3(transform.position.x, transform.position.y, BasicConfig.judgelinePos);
            effect.GetComponent<NoteHitEffect>().judgement = PERFECT;
            effect.GetComponent<NoteHitEffect>().callAnim = false;

            GameObject.Find("UI").GetComponent<Render>().animTrigger_Judgement = true;
            GameObject.Find("UI").GetComponent<Render>().judgement_latest = PERFECT;

            return;
        }
    }
}
