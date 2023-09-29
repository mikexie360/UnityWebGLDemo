using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game;

public class MicrophoneSetup : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var audio = GetComponent<AudioSource>();
        audio.clip = Microphone.Start(DeviceManager.Instance.GetInput(), true, 10, 44100);
        audio.loop = true;
        while (!(Microphone.GetPosition(null) > 0))
        {

        }
        audio.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
