using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap;

using Utils;
using Config;
using RhythmGameObjects;

public class HandDataProcess : MonoBehaviour
{
    Hand lHand, rHand;
    GameObject lHandIndicator, rHandIndicator, director;

    public Vector3 lHandPos, rHandPos, clapPos, lPunchPos, rPunchPos;
    public List<HandPosRecord> lHandPosTrail, rHandPosTrail, clapPosTrail;
    public bool lPunching = false, rPunching = false;

    bool isClapping = false;

    // Start is called before the first frame update
    void Start()
    {
        gameObject.transform.position = BasicConfig.trackerPos;
        lHandIndicator = GameObject.Find("Left Hand Position");
        rHandIndicator = GameObject.Find("Right Hand Position");
        director = GameObject.Find("Director");
        lHandPosTrail = new List<HandPosRecord>();
        rHandPosTrail = new List<HandPosRecord>();
        clapPosTrail = new List<HandPosRecord>();
    }

    // Update is called once per frame
    void Update()
    {
        float time = director.GetComponent<LevelHandler>().timer;
        lHand = Hands.Provider.GetHand(Chirality.Left);
        rHand = Hands.Provider.GetHand(Chirality.Right);

        lHandPos = new Vector3(-100, -100, BasicConfig.judgelinePos);
        rHandPos = new Vector3(-100, -100, BasicConfig.judgelinePos);
        clapPos = new Vector3(-100, -100, BasicConfig.judgelinePos);
        lPunchPos = new Vector3(-100, -100, BasicConfig.judgelinePos);
        rPunchPos = new Vector3(-100, -100, BasicConfig.judgelinePos);

        if (lHand != null)
        {
            lHandPos = Compute.TransformHandPos(lHand.PalmPosition + lHand.Direction * 0.05f);
        }
        if (rHand != null)
        {
            rHandPos = Compute.TransformHandPos(rHand.PalmPosition + rHand.Direction * 0.05f);
        }
        lHandIndicator.transform.position = lHandPos;
        rHandIndicator.transform.position = rHandPos;

        // create a trail of the hand position in a given time frame
        lHandPosTrail.Add(new HandPosRecord(lHandPos, time));
        rHandPosTrail.Add(new HandPosRecord(rHandPos, time));

        if (lHandPosTrail.Count > 0)
        {
            while (lHandPosTrail[0].time < time - BasicConfig.judgementTiming[1])
            {
                lHandPosTrail.RemoveAt(0);
                if (lHandPosTrail.Count == 0)
                {
                    break;
                }
            }
        }

        if (rHandPosTrail.Count > 0)
        {
            while (rHandPosTrail[0].time < time - BasicConfig.judgementTiming[1])
            {
                rHandPosTrail.RemoveAt(0);
                if (rHandPosTrail.Count == 0)
                {
                    break;
                }
            }
        }

        // Detect clapping, the following 3 must be satisfied:
        // 1. The palm positions of 2 hands needs to be within a distance
        // 2. The normal vectors of 2 hands needs to face each other
        if (lHand != null && rHand != null)
        {
            if (Vector3.Dot(lHand.PalmNormal, rHand.PalmNormal) <= -0.8f)
            {
                float lDot = Vector3.Dot(lHand.PalmNormal, lHand.PalmPosition);
                float rDot = Vector3.Dot(lHand.PalmNormal, rHand.PalmPosition);
                // check the normal direction distance
                if (Mathf.Abs(lDot - rDot) <= 0.08f)
                {
                    Vector3 projRHandPos = rHand.PalmPosition - lHand.PalmNormal * (lDot - rDot);
                    // check the projected-to-plane distance
                    if ((lHand.PalmPosition - projRHandPos).magnitude <= 0.08f)
                    {
                        if (!isClapping)
                        {
                            isClapping = true;
                            clapPos = Compute.TransformHandPos((lHand.PalmPosition + rHand.PalmPosition) / 2);

                            // add to trail
                            clapPosTrail.Add(new HandPosRecord(clapPos, time));

                            // debug
                            //Debug.Log("Clap at " + clapPos + "!");
                        }
                    }
                    else
                    {
                        //Debug.Log("Fail Projected Distance Check");
                        isClapping = false;
                    }
                }
                else
                {
                    //Debug.Log("Fail Normal Distance Check");
                    isClapping = false;
                }
            }
            else
            {
                //Debug.Log("Fail Direction Check");
                isClapping = false;
            }
        }

        if (clapPosTrail.Count > 0)
        {
            while (clapPosTrail[0].time < time - BasicConfig.judgementTiming[1])
            {
                clapPosTrail.RemoveAt(0);
                if (clapPosTrail.Count == 0)
                {
                    break;
                }
            }
        }

        // Detect punching, the following 2 are required:
        // 1. Player needs to hold their fist. This is implemented by checking if four fingers are pointing towards palm and the thumb not extending.
        //      # Although there's a provided fist detection in the package, the stability of it is low.
        // 2. The hand needs to push forward (+z direction)
        //
        // We take the center point of the proximal(first) bone of the middle finger as the punching point
        if (lHand != null)
        {
            if (lHand.PalmVelocity.z >= 0.7f)
            {
                if (checkHoldFist(Chirality.Left) && !lPunching)
                {
                    lPunchPos = Compute.TransformHandPos(lHand.fingers[2].bones[2].Center);
                }
            }
            else
            {
                lPunching = false;
            }
        }
        if (rHand != null)
        {
            if (rHand.PalmVelocity.z >= 0.7f)
            {
                if (checkHoldFist(Chirality.Right) && !rPunching)
                {
                    rPunchPos = Compute.TransformHandPos(rHand.fingers[2].bones[2].Center);
                }
            }
            else
            {
                rPunching = false;
            }
        }
    }
    
    bool checkHoldFist(Chirality chirality)
    {
        Hand hand;
        if (chirality == Chirality.Left)
        {
            hand = lHand;
        }
        else
        {
            hand = rHand;
        }
        if (hand.fingers[0].IsExtended)
        {
            return false;
        }
        for (int i = 1; i < hand.fingers.Length; i++)
        {
            if (Vector3.Dot(hand.fingers[i].Direction, hand.Direction) > -0.65f)
            {
                return false;
            }
        }
        return true;
    }
}
