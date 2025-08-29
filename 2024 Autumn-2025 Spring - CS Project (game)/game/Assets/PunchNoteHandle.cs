using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utils;
using Config;
using RhythmGameObjects;

public class PunchNoteHandle : MonoBehaviour
{
    // constants for easier read
    const int NONE = 0;
    const int LEFT = 1;
    const int RIGHT = 2;

    const int MISS = 0;
    const int PERFECT = 3;

    GameObject director;
    GameObject hands;
    GameObject sfx;
    GameObject HitEffect;
    Chart chart;

    public int laneID;
    public float beat;

    int touchBy = NONE;
    // in hit range
    bool lHandInRange = false, rHandInRange = false;

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
            Vector2 lPunchPos = hands.GetComponent<HandDataProcess>().lPunchPos;
            Vector2 rPunchPos = hands.GetComponent<HandDataProcess>().rPunchPos;
            Vector2 notePos = transform.position;

            // hand detection
            if ((lPunchPos - notePos).magnitude <= BasicConfig.noteHitbox && !lHandInRange)
            {
                touchBy = LEFT;
                lHandInRange = true;
            }
            if ((rPunchPos - notePos).magnitude <= BasicConfig.noteHitbox && !rHandInRange)
            {
                touchBy = RIGHT;
                rHandInRange = true;
            }
            if ((lPunchPos - notePos).magnitude > BasicConfig.noteHitbox && lHandInRange)
            {
                if (rHandInRange)
                {
                    touchBy = RIGHT;
                }
                else
                {
                    touchBy = NONE;
                }
                lHandInRange = false;
            }
            if ((rPunchPos - notePos).magnitude > BasicConfig.noteHitbox && rHandInRange)
            {
                if (lHandInRange)
                {
                    touchBy = LEFT;
                }
                else
                {
                    touchBy = NONE;
                }
                rHandInRange = false;
            }

            if (touchBy != NONE)
            {
                director.GetComponent<Judgement>().detectedPunchNotes.Add(new JudgeNote(gameObject, lHandInRange, rHandInRange, beat));
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
