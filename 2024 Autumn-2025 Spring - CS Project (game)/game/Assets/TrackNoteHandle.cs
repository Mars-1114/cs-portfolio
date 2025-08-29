using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using RhythmGameObjects;
using Utils;
using Config;
using System;
using UnityEngine.UI;


public class TrackNoteHandle : MonoBehaviour
{
    // constants for easier read
    const int NONE = 0;
    const int LEFT = 1;
    const int RIGHT = 2;
    const bool IN = true;
    const bool OUT = false;

    const int MISS = 0;
    const int PERFECT = 3;

    GameObject director;
    GameObject hands;
    GameObject sfx;
    GameObject HitEffect;
    List<GameObject> children = new List<GameObject>();
    Chart chart;

    public int laneID;
    public float beat;
    public float duration;

    // a pointer for destroying the note
    int targetSampleID = 0;
    // check which hand is within the hit range of the note (can be overwritten by the other hand)
    int touchBy = NONE;
    bool lHandInRange = false;
    bool rHandInRange = false;
    // check which hand is hitting the note (cannot be overwritten by the other hand)
    int hitBy = NONE;
    // check if the note is missed
    bool isMissed = false;

    // change the color when hit by any of the hands
    bool animTrigger_color = false;
    Color default_color = new Color(0.7098526f, 0.735849f, 0);
    Color hand_color;
    float animTimer_color = 0;

    // Start is called before the first frame update
    void Start()
    {
        hands = GameObject.Find("Hand Controller");
        director = GameObject.Find("Director");
        sfx = GameObject.Find("SfxPlayer");
        HitEffect = Resources.Load<GameObject>("GameObjects/HitEffect");
        chart = GameObject.Find("Parser").GetComponent<ParseChart>().chart;
        foreach (Transform child in transform)
        {
            children.Add(child.gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        float time = director.GetComponent<LevelHandler>().timer;
        float offset = chart.Offset;

        if (targetSampleID < children.Count)
        {
            List<HandPosRecord> lHandPosTrail = hands.GetComponent<HandDataProcess>().lHandPosTrail;
            List<HandPosRecord> rHandPosTrail = hands.GetComponent<HandDataProcess>().rHandPosTrail;
            Vector2 notePos = children[targetSampleID].transform.position;

            // display control
            foreach (GameObject child in children)
            {
                if (child != null)
                {
                    if (child.transform.position.z <= BasicConfig.renderDistance)
                    {
                        child.SetActive(true);
                    }
                }
            }

            // hand detection
            // left / right hand moves into the hit range of the note
            if (CheckTrailCollision(lHandPosTrail, notePos, IN) && !lHandInRange && hitBy == NONE)
            {
                touchBy = LEFT;
                lHandInRange = true;
            }
            if (CheckTrailCollision(rHandPosTrail, notePos, IN) && !rHandInRange && hitBy == NONE)
            {
                touchBy = RIGHT;
                rHandInRange = true;
            }
            // left / right hand moves out of the hit range of the note
            if (CheckTrailCollision(lHandPosTrail, notePos, OUT) && lHandInRange)
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
            if (CheckTrailCollision(rHandPosTrail, notePos, OUT) && rHandInRange)
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
        }

        // hit detect
        // if note reaches the judgeline
        if (chart.Second(beat) <= time - offset)
        {
            if (!PlayerConfig.autoplay)
            {
                // assign the note to hand
                if (hitBy == NONE)
                {
                    hitBy = touchBy;

                    // change the note color to the corresponded hand color
                    sfx.GetComponent<AudioSource>().Play();
                    foreach (var child in children)
                    {
                        if (child != null)
                        {
                            if (touchBy == RIGHT)
                            {
                                child.GetComponent<MeshRenderer>().material.mainTexture = Resources.Load<Texture2D>("Meshes/textures/TrackTexture_Right");
                                hand_color = new Color(209.0f / 255, 13.0f / 255, 88.0f / 255);
                            }
                            else if (touchBy == LEFT)
                            {
                                child.GetComponent<MeshRenderer>().material.mainTexture = Resources.Load<Texture2D>("Meshes/textures/TrackTexture_Left");
                                hand_color = new Color(14.0f / 255, 106.0f / 255, 181.0f / 255);
                            }
                        }
                    }
                    animTrigger_color = true;
                }
                if (!isMissed)
                {
                    // if touch by one of the hands and the assigned hand is within the hit range of the note
                    // PERFECT
                    if (touchBy != NONE && hitBy == touchBy)
                    {
                        // distroy child note
                        if (targetSampleID < children.Count && children[targetSampleID].transform.position.z <= BasicConfig.judgelinePos)
                        {
                            // display effect
                            if (targetSampleID % 3 == 0)
                            {
                                GameObject effect = Instantiate(HitEffect);
                                effect.transform.position = new Vector3(children[targetSampleID].transform.position.x, children[targetSampleID].transform.position.y, BasicConfig.judgelinePos);
                                effect.GetComponent<NoteHitEffect>().judgement = PERFECT;
                                effect.GetComponent<NoteHitEffect>().callAnim = false;
                            }

                            Destroy(children[targetSampleID]);
                            targetSampleID++;
                        }
                    }
                    // if the player does not hit the note for over the timing of GOOD
                    // MISS
                    else if (chart.Second(beat) <= time - offset + BasicConfig.judgementTiming[1])
                    {
                        isMissed = true;

                        // change the note color to gray
                        foreach (var child in children)
                        {
                            if (child != null)
                            {
                                child.GetComponent<MeshRenderer>().material.mainTexture = Resources.Load<Texture2D>("Meshes/textures/TrackTexture_Miss");
                                hand_color = new Color(209.0f / 255, 13.0f / 255, 88.0f / 255);
                            }
                        }
                        animTrigger_color = true;

                        // display MISS
                        director.GetComponent<LevelHandler>().performance.miss++;
                        director.GetComponent<LevelHandler>().performance.combo = 0;

                        GameObject.Find("UI").GetComponent<Render>().animTrigger_Judgement = true;
                        GameObject.Find("UI").GetComponent<Render>().judgement_latest = MISS;
                    }
                }
                else
                {
                    // distroy child note
                    if (targetSampleID < children.Count && children[targetSampleID].transform.position.z <= BasicConfig.judgelinePos)
                    {
                        // display effect
                        if (targetSampleID % 3 == 0)
                        {
                            if (targetSampleID == 0)
                            {
                                sfx.GetComponent<AudioSource>().Play();
                            }
                            GameObject effect = Instantiate(HitEffect);
                            effect.transform.position = new Vector3(children[targetSampleID].transform.position.x, children[targetSampleID].transform.position.y, BasicConfig.judgelinePos);
                            effect.GetComponent<NoteHitEffect>().judgement = MISS;
                            effect.GetComponent<NoteHitEffect>().callAnim = false;
                        }

                        Destroy(children[targetSampleID]);
                        targetSampleID++;
                    }
                }
            }
            else
            {
                // distroy child note
                if (targetSampleID < children.Count && children[targetSampleID].transform.position.z <= BasicConfig.judgelinePos)
                {
                    // display effect
                    if (targetSampleID % 3 == 0)
                    {
                        if (targetSampleID == 0)
                        {
                            sfx.GetComponent<AudioSource>().Play();
                        }
                        GameObject effect = Instantiate(HitEffect);
                        effect.transform.position = new Vector3(children[targetSampleID].transform.position.x, children[targetSampleID].transform.position.y, BasicConfig.judgelinePos);
                        effect.GetComponent<NoteHitEffect>().judgement = PERFECT;
                        effect.GetComponent<NoteHitEffect>().callAnim = false;
                    }

                    Destroy(children[targetSampleID]);
                    targetSampleID++;
                }
            }
        }
        // if the timer passed the duration of the note
        if (chart.Second(beat + duration) < time - offset)
        {
            Destroy(gameObject);

            // display PERFECT
            if (!isMissed)
            {
                sfx.GetComponent<AudioSource>().Play();

                director.GetComponent<LevelHandler>().performance.perfect++;
                director.GetComponent<LevelHandler>().performance.combo++;

                GameObject.Find("UI").GetComponent<Render>().animTrigger_Judgement = true;
                GameObject.Find("UI").GetComponent<Render>().judgement_latest = PERFECT;
            }
            return;
        }

        if (animTrigger_color)
        {
            foreach (var child in children)
            {
                if (animTimer_color < 0.2f && child != null)
                {
                    float t = animTimer_color;
                    child.GetComponent<MeshRenderer>().material.EnableKeyword("_EMISSION");
                    child.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", Color.Lerp(default_color, hand_color, t / 0.2f));
                }
            }
            if (animTimer_color < 0.2f)
            {
                animTimer_color += Time.deltaTime;
            }
        }
    }

    bool CheckTrailCollision(List<HandPosRecord> trail, Vector2 notePos, bool cmp)
    {
        // check from the most recent hand position
        for (int i = trail.Count - 1; i >= 0; i--)
        {
            if ((trail[i].Position - notePos).magnitude <= BasicConfig.noteHitbox)
            {
                return cmp;
            }
        }
        return !cmp;
    }
}