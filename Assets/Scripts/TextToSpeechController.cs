using Microsoft.MixedReality.Toolkit.Audio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextToSpeechController : MonoBehaviour
{
    private TextToSpeech TextToSpeech;
    public string TextToSpeak;

    private void Awake()
    {
        TextToSpeech = GetComponent<TextToSpeech>();
        TextToSpeech.StartSpeaking(TextToSpeak);
    }
}
