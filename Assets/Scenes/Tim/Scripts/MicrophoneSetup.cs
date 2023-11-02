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
    private float _threshold = 0.01f;

    void Start()
    {
        _audio = GetComponent<AudioSource>();
        _server = (AudioHandler)gameObject.GetComponent(typeof(AudioHandler));
        toggle.onClick.AddListener(delegate
        {
            buttonStatus = !buttonStatus;
            //if (buttonStatus)
            //{
            //    ChangeMic();
            //} else
            //{
            //    _audio.Stop();
            //}
        });
        SettingEvents.OnInputUpdated += OnInputUpdated;
        SettingEvents.OnOutputUpdated += OnOutputUpdated;
        ChangeMic();
    }

    private void OnInputUpdated(string mic)
    {
        ChangeMic(mic);
    }

    private void OnEnable()
    {
        AudioEvents.OnAudioReceived += PlayAudio;
    }

    private void OnDisable()
    {
        AudioEvents.OnAudioReceived += PlayAudio;
    }

    void ChangeMic(string mic = "")
    {
        string input = mic;
        if (mic == "")
        {
            input = DeviceManager.Instance.GetInput();
        }
        _mic = Microphone.Start(input, true, 20, AudioSettings.outputSampleRate);
    }

    private void PlayAudio(int samples, int channels, int frequency, float[] data)
    {
        AudioClip result = AudioClip.Create("temp", samples, channels, frequency, false);
        result.SetData(data, 0);
        _audio.PlayOneShot(result);
    }

    private float GetAverage(float[] data)
    {
        float sum = 0.0f;
        foreach (float f in data)
        {
            sum += Mathf.Abs(f);
        }
        return sum / data.Length;
    }

    private IEnumerator SendData()
    {
        if (buttonStatus)
        {
            yield return new WaitForSeconds(0.1f);
            int length = AudioSettings.outputSampleRate / 10 * _mic.channels;
            float[] data = new float[length];
            int startPosition = Microphone.GetPosition(DeviceManager.Instance.GetInput());
            try
            {
                //Debug.Log(startPosition + " " + length);
                _mic.GetData(data, Mathf.Max(startPosition - length, 0));
                _audio.Stop();
            }
            catch (System.Exception)
            {
                Debug.Log("error");
                Debug.Log(startPosition);
                Debug.Log(length);
            }
            if (GetAverage(data) > _threshold)
            {
                Debug.Log("average:" + GetAverage(data));
                _server.SendUnnamedMessage(_mic.samples, _mic.channels, _mic.frequency, data);
            }
        }
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
