using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Config;
using Utils;
using RhythmGameObjects;
using UnityEditor.Experimental.GraphView;
using System;

// Used by note types which are "blocking" (i.e. notes comes first are judged first)
public class Judgement : MonoBehaviour
{
    // constants for easier read
    // hand touch
    const int NONE = 0;
    const int LEFT = 1;
    const int RIGHT = 2;

    // judgement
    const int BAD = 1;
    const int GOOD = 2;
    const int PERFECT = 3;

    GameObject sfx;
    Chart chart;

    public GameObject HitEffect;
    // These stores the candidates of the notes
    public List<JudgeNote> detectedTapNotes, detectedClapNotes, detectedPunchNotes;

    // Start is called before the first frame update
    void Start()
    {
        detectedTapNotes = new List<JudgeNote>();
        detectedClapNotes = new List<JudgeNote>();
        detectedPunchNotes = new List<JudgeNote>();
        sfx = GameObject.Find("SfxPlayer");
    }

    // Update is called once per frame
    void Update()
    {
        chart = GameObject.Find("Parser").GetComponent<ParseChart>().chart;
        TapJudge();
        ClapJudge();
        PunchJudge();
    }

    void TapJudge()
    {
        if (detectedTapNotes.Count > 0)
        {
            List<int> leftHit = new List<int>();
            List<int> rightHit = new List<int>();

            // find the note hit by the left hand (if any)
            for (int i = 0; i < detectedTapNotes.Count; i++)
            {
                if (detectedTapNotes[i].leftDetect)
                {
                    leftHit.Add(i);
                }
            }
            if (leftHit.Count > 0)
            {
                // find first
                int index = 0;
                for (int i = 1; i < leftHit.Count; i++)
                {
                    if (detectedTapNotes[leftHit[i]].beat < detectedTapNotes[leftHit[index]].beat)
                    {
                        index = i;
                    }
                }
                // first touched is marked as hit
                if (detectedTapNotes[leftHit[index]].hitBy == LEFT)
                {
                    Judge(detectedTapNotes[leftHit[index]], true);
                }
            }

            // find the note hit by the right hand (if any)
            for (int i = 0; i < detectedTapNotes.Count; i++)
            {
                if (detectedTapNotes[i].rightDetect)
                {
                    rightHit.Add(i);
                }
            }
            if (rightHit.Count > 0)
            {
                // find first
                int index = 0;
                for (int i = 1; i < rightHit.Count; i++)
                {
                    if (detectedTapNotes[rightHit[i]].beat < detectedTapNotes[rightHit[index]].beat)
                    {
                        index = i;
                    }
                }
                // first touched is marked as hit
                if (detectedTapNotes[rightHit[index]].hitBy == RIGHT)
                {
                    Judge(detectedTapNotes[rightHit[index]], true);
                }
            }
            // empty list
            detectedTapNotes.Clear();
        }
    }

    void ClapJudge()
    {
        if (detectedClapNotes.Count > 0)
        {
            // find first
            detectedClapNotes = detectedClapNotes.OrderBy(note => note.beat).ToList();
            Judge(detectedClapNotes[0], true);
            // empty list
            detectedClapNotes.Clear();
        }
    }

    void PunchJudge()
    {
        if (detectedPunchNotes.Count > 0)
        {
            List<int> leftHit = new List<int>();
            List<int> rightHit = new List<int>();

            // find the note hit by the left hand (if any)
            for (int i = 0; i < detectedPunchNotes.Count; i++)
            {
                if (detectedPunchNotes[i].leftDetect)
                {
                    leftHit.Add(i);
                }
            }
            if (leftHit.Count > 0)
            {
                // find first
                int index = 0;
                for (int i = 1; i < leftHit.Count; i++)
                {
                    if (detectedPunchNotes[leftHit[i]].beat < detectedPunchNotes[leftHit[index]].beat)
                    {
                        index = i;
                    }
                }
                Judge(detectedPunchNotes[leftHit[index]], false);
            }

            // find the note hit by the left hand (if any)
            for (int i = 0; i < detectedPunchNotes.Count; i++)
            {
                if (detectedPunchNotes[i].rightDetect)
                {
                    rightHit.Add(i);
                }
            }
            if (rightHit.Count > 0)
            {
                // find first
                int index = 0;
                for (int i = 1; i < rightHit.Count; i++)
                {
                    if (detectedPunchNotes[rightHit[i]].beat < detectedPunchNotes[rightHit[index]].beat)
                    {
                        index = i;
                    }
                }
                Judge(detectedPunchNotes[rightHit[index]], false);
            }
            // empty list
            detectedPunchNotes.Clear();
        }
    }

    void Judge(JudgeNote note, bool earlyJudge)
    {
        float time = gameObject.GetComponent<LevelHandler>().timer;
        float beat = note.beat;
        float offset = chart.Offset;

        if (note.note != null)
        {
            if (earlyJudge)
            {
                if (time >= chart.Second(beat) - BasicConfig.judgementTiming[2] + offset)
                {
                    int judgement = NONE;
                    if (time < chart.Second(beat) - BasicConfig.judgementTiming[1] + offset)
                    {
                        gameObject.GetComponent<LevelHandler>().performance.bad++;
                        gameObject.GetComponent<LevelHandler>().performance.combo = 0;
                        judgement = BAD;
                    }
                    else if (time < chart.Second(beat) - BasicConfig.judgementTiming[0] + offset)
                    {
                        gameObject.GetComponent<LevelHandler>().performance.good++;
                        gameObject.GetComponent<LevelHandler>().performance.combo++;
                        judgement = GOOD;
                    }
                    else if (time < chart.Second(beat) + BasicConfig.judgementTiming[0] + offset)
                    {
                        gameObject.GetComponent<LevelHandler>().performance.perfect++;
                        gameObject.GetComponent<LevelHandler>().performance.combo++;
                        judgement = PERFECT;
                    }
                    else
                    {
                        gameObject.GetComponent<LevelHandler>().performance.good++;
                        gameObject.GetComponent<LevelHandler>().performance.combo++;
                        judgement = GOOD;
                    }
                    sfx.GetComponent<AudioSource>().Play();
                    detectedTapNotes.Remove(note);

                    GameObject effect = Instantiate(HitEffect);
                    effect.transform.position = new Vector3(note.note.transform.position.x, note.note.transform.position.y, BasicConfig.judgelinePos);
                    effect.GetComponent<NoteHitEffect>().judgement = judgement;
                    effect.GetComponent<NoteHitEffect>().callAnim = true;

                    Destroy(note.note);
                }
            }
            else
            {
                if (time >= chart.Second(beat) + offset)
                {
                    if (time < chart.Second(beat) + BasicConfig.judgementTiming[1] + offset)
                    {
                        gameObject.GetComponent<LevelHandler>().performance.perfect++;
                        gameObject.GetComponent<LevelHandler>().performance.combo++;
                    }
                    sfx.GetComponent<AudioSource>().Play();
                    detectedPunchNotes.Remove(note);

                    GameObject effect = Instantiate(HitEffect);
                    effect.transform.position = new Vector3(note.note.transform.position.x, note.note.transform.position.y, BasicConfig.judgelinePos);
                    effect.GetComponent<NoteHitEffect>().judgement = PERFECT;
                    effect.GetComponent<NoteHitEffect>().callAnim = true;

                    Destroy(note.note);
                }
            }
        }
    }
}
