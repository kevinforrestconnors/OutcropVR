using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Windows.Speech;

public class VoiceManager : MonoBehaviour
{

    KeywordRecognizer keywordRecognizer;

    public string[] Keywords_array;

    void Start()
    {
        
        Keywords_array = new string[11];
        Keywords_array[0] = "Speed up";
        Keywords_array[1] = "Increase speed";
        Keywords_array[2] = "Slow down";
        Keywords_array[3] = "Decrease speed";
        Keywords_array[4] = "Make line";
        Keywords_array[5] = "Make plane";
        Keywords_array[6] = "Make surface";
        Keywords_array[7] = "Pause";
        Keywords_array[8] = "Pause drawing";
        Keywords_array[9] = "Resume";
        Keywords_array[10] = "Resume drawing";

        keywordRecognizer = new KeywordRecognizer(Keywords_array);
        keywordRecognizer.OnPhraseRecognized += OnKeywordsRecognized;
        keywordRecognizer.Start();
    }

    void OnKeywordsRecognized(PhraseRecognizedEventArgs args)
    {
        Debug.Log("Keyword: " + args.text + "; Confidence: " + args.confidence + "; Start Time: " + args.phraseStartTime + "; Duration: " + args.phraseDuration);

        if (args.text == "Speed up" || args.text == "Increase speed")
        {
            FlyingMovement.SpeedUp();
        }
        else if (args.text == "Slow down" || args.text == "Decrease speed")
        {
            FlyingMovement.SpeedDown();
        }
        else if (args.text == "Make line")
        {
            LaserPointer.MakingLine();
        }
        else if (args.text == "Make plane")
        {
            LaserPointer.MakingPlane();
        }
        else if (args.text == "Make surface")
        {
            LaserPointer.MakingSurface();
        }
        else if (args.text == "Pause" || args.text == "Pause drawing")
        {
            LaserPointer.Pause();
        }
        else if (args.text == "Resume" || args.text == "Resume drawing")
        {
            LaserPointer.Resume();
        }
    }
}