using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game;
using Game.Events;

public class Devices : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var dropdown = GetComponent<TMPro.TMP_Dropdown>();
        foreach (var d in Microphone.devices)
        {
            dropdown.options.Add(new TMPro.TMP_Dropdown.OptionData(d));
        }
        SettingEvents.OnInputUpdated.Invoke(dropdown.options[0].text);
        dropdown.onValueChanged.AddListener(delegate {
            //Debug.Log(dropdown.options[dropdown.value].text);
            SettingEvents.OnInputUpdated(dropdown.options[dropdown.value].text);
            Debug.Log(DeviceManager.Instance.GetInput());
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
