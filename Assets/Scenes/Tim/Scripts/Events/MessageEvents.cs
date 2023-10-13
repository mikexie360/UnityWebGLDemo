using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Events
{
    public static class MessageEvents
    {

        public delegate void MessageReceived(string id, string message);
        public static MessageReceived OnMessageReceived;
    }
}