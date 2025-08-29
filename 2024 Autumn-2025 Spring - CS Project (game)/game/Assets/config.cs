using System;
using System.Collections.Generic;
using UnityEngine;

// Configurations For Game Tweaking
//   ## Should Contain Constants Only ##
namespace Config
{
    // Developer settings
    public class BasicConfig
    {
        // Gameplay
        public static float judgelinePos = -6.5f;
        public static float renderDistance = 20.0f;
        public static Vector3 trackerPos = new Vector3(0, -0.35f, -9.5f);
        public static float countdown = 2;    // in seconds

        // Judgement
        public static float[] judgementTiming =
        {
            0.08f,  //PERFECT
            0.16f,  //GOOD
            0.18f   //BAD
        };
        public static float leniencyDistance = 0.5f;
        public static float noteHitbox = 0.5f;

        // Visual
        public static int sampleRate = 20;
        public static float trackSampleDistance = 1.5f;

        // song list
        public static List<string> songs = new List<string>
        {
            "Creo - Flow",
            "Fujiyori - Fly Again"
        };
    }

    // Player settings
    public class PlayerConfig
    {
        public static float offset = 0;
        public static int musicVolume = 100;
        public static int sfxVolume = 100;

        public static bool autoplay = false;
    }

    // Song list
    public class SongConfig
    {

    }
}