using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Game;
using Game.Events;

public class MicrophoneSetup : MonoBehaviour
{
    // Start is called before the first frame update
    private AudioSource _audio;
    public Button toggle;
    private bool buttonStatus = true;
    void Start()
    {
        _audio = GetComponent<AudioSource>();
        toggle.onClick.AddListener(delegate
        {
            buttonStatus = !buttonStatus;
            if (buttonStatus)
            {
                ChangeMic();
            } else
            {
                _audio.Stop();
            }
        });
        ChangeMic();
    }

    private void OnInputUpdated(string _)
    {
        ChangeMic();
    }

    void ChangeMic()
    {
        _audio.Stop();
        var mic = new Microphone();
        _audio.clip = Microphone.Start(DeviceManager.Instance.GetInput(), true, 10, 44100);
        _audio.loop = true;
        SettingEvents.OnInputUpdated += OnInputUpdated;
        SettingEvents.OnOutputUpdated += OnOutputUpdated;
        while (!(Microphone.GetPosition(null) > 0))
        {

        }
        Debug.Log("a");
        if (buttonStatus)
        {
            _audio.Play();
        }
    }

    private void OnOutputUpdated(string output)
    {
        //outputDevice = output;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
