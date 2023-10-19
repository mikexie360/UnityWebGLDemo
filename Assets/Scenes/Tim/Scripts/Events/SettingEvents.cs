using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Events
{
    public static class SettingEvents
    {

        public delegate void InputUpdated(string input);
        public static InputUpdated OnInputUpdated;

        public delegate void OutputUpdated(string output);
        public static OutputUpdated OnOutputUpdated;
    }
}