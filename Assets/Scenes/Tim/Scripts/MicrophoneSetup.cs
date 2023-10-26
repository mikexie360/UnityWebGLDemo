using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Game;
using Game.Events;
using Unity.Netcode;

public class MicrophoneSetup : MonoBehaviour
{
    private AudioSource _audio;
    public Button toggle;
    private bool buttonStatus = true;
    private IEnumerator _activeCoroutine;
    private AudioClip _mic;
    private AudioHandler _server;

    void Start()
    {
        _audio = GetComponent<AudioSource>();
        _server = (AudioHandler)gameObject.GetComponent(typeof(AudioHandler));
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
        SettingEvents.OnInputUpdated += OnInputUpdated;
        SettingEvents.OnOutputUpdated += OnOutputUpdated;
        ChangeMic();
    }

    private void OnInputUpdated(string _)
    {
        ChangeMic();
    }

    private void OnEnable()
    {
        AudioEvents.OnAudioReceived += PlayAudio;
    }

    private void OnDisable()
    {
        AudioEvents.OnAudioReceived += PlayAudio;
    }

    void ChangeMic()
    {
        _mic = Microphone.Start(DeviceManager.Instance.GetInput(), true, 20, AudioSettings.outputSampleRate);
    }

    private void PlayAudio(int samples, int channels, int frequency, float[] data)
    {
        AudioClip result = AudioClip.Create("temp", samples, channels, frequency, false);
        result.SetData(data, 0);
        _audio.PlayOneShot(result);
    }

    private IEnumerator SendData()
    {
        yield return new WaitForSeconds(1f);
        //_audio.Stop();
        int length = AudioSettings.outputSampleRate * _mic.channels;
        float[] data = new float[length];
        int startPosition = Microphone.GetPosition(DeviceManager.Instance.GetInput());
        try
        {
            _mic.GetData(data, startPosition - length);
        } catch (System.Exception e)
        {
            Debug.Log("error");
            Debug.Log(startPosition);
            Debug.Log(length);
        }
        _server.SendUnnamedMessage(_mic.samples, _mic.channels, _mic.frequency, data);
        _activeCoroutine = null;
    }

    private void OnOutputUpdated(string output)
    {
        //outputDevice = output;
    }

    void LateUpdate()
    {
        if (_activeCoroutine == null)
        {
            _activeCoroutine = SendData();
            StartCoroutine(_activeCoroutine);
        }
    }
}
