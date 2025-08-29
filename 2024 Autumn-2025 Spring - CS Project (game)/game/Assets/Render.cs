using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Render : MonoBehaviour
{
    // constants for better reading
    const int MISS = 0;
    const int BAD = 1;
    const int GOOD = 2;
    const int PERFECT = 3;

    GameObject director;
    GameObject parser;
    GameObject score;
    GameObject combo;
    GameObject judgement;
    GameObject song;

    float animTimer_LevelBegin = 0;
    float animTimer_Judgement = 2;
    float animTimer_ScoreChange = 2;
    int anim_digit;
    string prev_score;

    public bool animTrigger_Judgement = false;
    public int judgement_latest;

    // Start is called before the first frame update
    void Start()
    {
        director = GameObject.Find("Director");
        score = gameObject.transform.Find("Score").gameObject;
        combo = gameObject.transform.Find("Combo").gameObject;
        judgement = gameObject.transform.Find("Judgement").gameObject;
        song = gameObject.transform.Find("SongName").gameObject;
        parser = GameObject.Find("Parser");

        animTimer_LevelBegin = 0;
        animTimer_Judgement = 2;
        animTimer_ScoreChange = 2;
        anim_digit = 0;
        prev_score = "000000";
    }

    // Update is called once per frame
    void Update()
    {
        var performance = director.GetComponent<LevelHandler>().performance;
        var chart = parser.GetComponent<ParseChart>().chart;
        string score_str_raw = performance.Score().ToString("D6");
        string combo_str_raw = performance.combo.ToString();

        // check score diff
        if (performance.Score() == 1000000)
        {
            anim_digit = 0;
            animTimer_ScoreChange = 0;
        }
        else if (score_str_raw != prev_score)
        {
            for (int i = 0; i < 6; i++)
            {
                if (prev_score[i] != score_str_raw[i])
                {
                    anim_digit = i;
                    animTimer_ScoreChange = 0;
                    break;
                }
            }
        }

        prev_score = score_str_raw;

        // animation
        // level start
        string score_str = "";
        string song_str = "";
        if (animTimer_LevelBegin < 1.2)
        {
            // shorten variable name
            float t = animTimer_LevelBegin;

            float text_opacity = t / 0.3f;
            float text_spacing = 55 * Mathf.Pow(1 - t / 1.2f, 4) + 5;

            float song_margin = 40 * Mathf.Pow(1 - t / 1f, 3);
            float artist_margin = 40 * Mathf.Pow(1 - ((t - 0.2f) / 1f), 3);
            if (t / 1f > 1) song_margin = 0;
            if ((t - 0.2f) / 1f > 1) artist_margin = 0;
            score.GetComponent<TextMeshProUGUI>().color = new Color(1, 1, 1, text_opacity);
            score.GetComponent<TextMeshProUGUI>().characterSpacing = text_spacing;

            // score display
            for (int i = 0; i < 6; i++)
            {
                float char_opacity = text_opacity - i * 0.2f;
                if (char_opacity < 0) char_opacity = 0;
                else if (char_opacity > 1) char_opacity = 1;
                string char_color_hex = ColorUtility.ToHtmlStringRGBA(new Color(1, 1, 1, char_opacity));
                score_str += "<color=#" + char_color_hex + ">" + score_str_raw[i];
            }

            // song name display
            song_str += "<margin-right=" + song_margin + "><color=#" + ColorUtility.ToHtmlStringRGBA(new Color(1, 1, 1, t / 0.7f)) + ">" + chart.Name + "</color></margin>\n<size=60%>";
            song_str += "<margin-right=" + artist_margin + "><color=#" + ColorUtility.ToHtmlStringRGBA(new Color(1, 1, 1, (t - 0.2f) / 0.7f)) + ">" + chart.Artist;

            animTimer_LevelBegin += Time.deltaTime;
        }
        // score change
        else
        {
            song_str = chart.Name + "\n<size=60%>" + chart.Artist;
            if (animTimer_ScoreChange < 0.6f)
            {
                float t = animTimer_ScoreChange;
                float offset = (t / 0.6f >= 1) ? 0 : Mathf.Pow(2, -10 * t / 0.6f);
                offset *= 8;

                for (int i = 0; i < score_str_raw.Length; i++)
                {
                    if (anim_digit == i)
                    {
                        score_str += "<voffset=" + offset + ">" + score_str_raw[i];
                    }
                    else
                    {
                        score_str += score_str_raw[i];
                    }
                }

                animTimer_ScoreChange += Time.deltaTime;
            }
            else
            {
                anim_digit = 0;
                score_str = score_str_raw;
            }
        }

        // judgement text
        if (animTrigger_Judgement)
        {
            animTimer_Judgement = 0;
            animTrigger_Judgement = false;
        }
        float animTime_Judgement = 0.7f;
        var judgement_text = judgement.GetComponent<TextMeshProUGUI>();
        if (animTimer_Judgement < animTime_Judgement)
        {
            // shorten variable name
            float t = animTimer_Judgement;

            float opacity = 0;
            if (t < 0.1f)
            {
                opacity = t / 0.1f;
            }
            else if (t < 0.5f)
            {
                opacity = 1 - (t - 0.1f) / 0.4f;
            }
            float spacing = 70 * (1 - Mathf.Pow(1 - t / animTime_Judgement, 5));
            float offset = (t / animTime_Judgement >= 1) ? 1 : 1 - Mathf.Pow(2, -10 * t / animTime_Judgement);
            offset *= 10;
            string judge_str = "<voffset=" + offset + ">-<size=" + spacing + "%> </size>";
            switch (judgement_latest)
            {
                case PERFECT:
                    judge_str += "PERFECT";
                    judgement_text.color = new Color32(255, 246, 171, 255);
                    judgement_text.fontSharedMaterial.SetColor(ShaderUtilities.ID_GlowColor, new Color32(255, 249, 0, 43));
                    break;
                case GOOD:
                    judge_str += "GOOD";
                    judgement_text.color = new Color32(171, 255, 172, 255);
                    judgement_text.fontSharedMaterial.SetColor(ShaderUtilities.ID_GlowColor, new Color32(0, 220, 37, 43));
                    break;
                case BAD:
                    judge_str += "BAD";
                    judgement_text.color = new Color32(255, 207, 190, 255);
                    judgement_text.fontSharedMaterial.SetColor(ShaderUtilities.ID_GlowColor, new Color32(230, 56, 0, 43));
                    break;
                case MISS:
                    judge_str += "MISS";
                    judgement_text.color = new Color32(255, 92, 110, 255);
                    judgement_text.fontSharedMaterial.SetColor(ShaderUtilities.ID_GlowColor, new Color32(255, 0, 0, 43));
                    break;
                default:
                    judge_str = "";
                    break;
            }
            judge_str += "<size=" + spacing + "%> </size>-";
            judgement_text.alpha = opacity;

            judgement_text.text = judge_str;

            animTimer_Judgement += Time.deltaTime;
        }
        else
        {
            judgement.GetComponent<TextMeshProUGUI>().text = "";
        }

        // render texts
        score.GetComponent<TextMeshProUGUI>().text = score_str;
        song.GetComponent<TextMeshProUGUI>().text = song_str;
        if (performance.combo >= 3)
        {
            combo.GetComponent<TextMeshProUGUI>().text = combo_str_raw + "\n<size=40%><cspace=1>Combo";
        }
        else
        {
            combo.GetComponent<TextMeshProUGUI>().text = "";
        }

    }
}
