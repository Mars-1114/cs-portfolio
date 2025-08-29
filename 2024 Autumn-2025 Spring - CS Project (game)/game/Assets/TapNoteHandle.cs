using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using RhythmGameObjects;
using Utils;
using Config;
using Leap;

public class TapNoteHandle : MonoBehaviour
{
    // constants for easier read
    const int NONE = 0;
    const int LEFT = 1;
    const int RIGHT = 2;

    const int MISS = 0;
    const int PERFECT = 3;

    GameObject hands;
    GameObject director;
    GameObject sfx;
    GameObject HitEffect;
    Chart chart;

    public int laneID;
    public float beat;

    public int hitBy;     // 0: no hit, 1: left hand, 2: right hand
    // in leniency range
    public bool lHandInDetect, rHandInDetect;

    // in hit range
    bool lHandInRange, rHandInRange;
    // Start is called before the first frame update
    void Start()
    {
        hands = GameObject.Find("Hand Controller");
        director = GameObject.Find("Director");
        sfx = GameObject.Find("SfxPlayer");
        HitEffect = Resources.Load<GameObject>("GameObjects/HitEffect");
        chart = GameObject.Find("Parser").GetComponent<ParseChart>().chart;
        lHandInRange = false;
        rHandInRange = false;
        lHandInDetect = false;
        rHandInDetect = false;
        hitBy = NONE;
    }

    // Update is called once per frame
    void Update()
    {
        float time = director.GetComponent<LevelHandler>().timer;
        float offset = chart.Offset;

        // judgement
        // send the self reference to the director for hit detection
        if (!PlayerConfig.autoplay)
        {
            Vector2 lHandPos = hands.GetComponent<HandDataProcess>().lHandPos;
            Vector2 rHandPos = hands.GetComponent<HandDataProcess>().rHandPos;
            Vector2 notePos = gameObject.transform.position;
            hitBy = NONE;

            if ((lHandPos - notePos).magnitude <= BasicConfig.noteHitbox + BasicConfig.leniencyDistance ||
                    (rHandPos - notePos).magnitude <= BasicConfig.noteHitbox + BasicConfig.leniencyDistance)
            {
                if ((lHandPos - notePos).magnitude <= BasicConfig.noteHitbox + BasicConfig.leniencyDistance)
                {
                    lHandInDetect = true;
                }
                else
                {
                    rHandInDetect = true;
                }
                if (!lHandInRange && (lHandPos - notePos).magnitude <= BasicConfig.noteHitbox)
                {
                    lHandInRange = true;
                    hitBy = LEFT;
                }
                if (!rHandInRange && (rHandPos - notePos).magnitude <= BasicConfig.noteHitbox)
                {
                    rHandInRange = true;
                    hitBy = RIGHT;
                }
                director.GetComponent<Judgement>().detectedTapNotes.Add(new JudgeNote(gameObject, hitBy, lHandInDetect, rHandInDetect, beat));
            }

            if ((lHandPos - notePos).magnitude > BasicConfig.noteHitbox)
            {
                lHandInRange = false;
            }
            if ((rHandPos - notePos).magnitude > BasicConfig.noteHitbox)
            {
                rHandInRange = false;
            }
            if ((lHandPos - notePos).magnitude > BasicConfig.noteHitbox + BasicConfig.leniencyDistance)
            {
                lHandInDetect = false;
            }
            if ((rHandPos - notePos).magnitude > BasicConfig.noteHitbox + BasicConfig.leniencyDistance)
            {
                rHandInDetect = false;
            }
        }
        else if (chart.Second(beat) <= time - offset)
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

        // move this elsewhere
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
}
