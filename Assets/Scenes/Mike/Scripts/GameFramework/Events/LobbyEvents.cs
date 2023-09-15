using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace GameFramework.Events
{
    public class LobbyEvents : MonoBehaviour
    {



        public delegate void LobbyUpdated(Lobby lobby);
        public static LobbyUpdated OnLobbyUpdated;
    }
}