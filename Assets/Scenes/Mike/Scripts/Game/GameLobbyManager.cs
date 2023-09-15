using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using GameFramework.Core;
using GameFramework.Manager;
using System.Threading.Tasks;
using System;
using GameFramework.Core.Data;
using Unity.Services.Authentication;
using GameFramework.Events;
using Unity.Services.Lobbies.Models;

namespace Game
{
    public class GameLobbyManager : MySingleton<GameLobbyManager>
    {
        private List<LobbyPlayerData> _lobbyPlayerDatas = new List<LobbyPlayerData>();

        private LobbyPlayerData _localLobbyPlayerData;

        private LobbyData _lobbyData;

        public bool IsHost => _localLobbyPlayerData.Id == LobbyManager.Instance.GetHostId();

        private void OnEnable()
        {
            LobbyEvents.OnLobbyUpdated += OnLobbyUpdated;
        }

        private void OnDisable()
        {
            LobbyEvents.OnLobbyUpdated -= OnLobbyUpdated;
        }

        private void OnLobbyUpdated(Lobby lobby)
        {
            List<Dictionary<string, PlayerDataObject>> playerData = LobbyManager.Instance.GetPlayersData();
            _lobbyPlayerDatas.Clear();

            int numberOfPlayerReady = 0;

            foreach (Dictionary<string, PlayerDataObject> data in playerData)
            {
                LobbyPlayerData lobbyPlayerData = gameObject.AddComponent<LobbyPlayerData>();
                lobbyPlayerData.Initialize(data);

                if (lobbyPlayerData.IsReady)
                {
                    numberOfPlayerReady++;
                }

                if (lobbyPlayerData.Id == AuthenticationService.Instance.PlayerId)
                {
                    _localLobbyPlayerData = lobbyPlayerData;
                }

                _lobbyPlayerDatas.Add(lobbyPlayerData);
            }

            _lobbyData = new LobbyData();
            _lobbyData.Initialize(lobby.Data);

            Events.LobbyEvents.OnLobbyUpdated?.Invoke();

            if (numberOfPlayerReady == lobby.Players.Count)
            {
                Events.LobbyEvents.OnLobbyReady?.Invoke();
            }
        }

        public async Task<bool> CreateLobby()
        {
            /*Dictionary<string, string> playerData = new Dictionary<string, string>()
            {
                {"GamerTag", "HostPlayer" }
            };*/
            _localLobbyPlayerData = gameObject.AddComponent<LobbyPlayerData>();
            _localLobbyPlayerData.Initialize(AuthenticationService.Instance.PlayerId, "HostPlayer");
            _lobbyData = new LobbyData();
            _lobbyData.Initialize(0);

            bool succeeded = await LobbyManager.Instance.CreateLobby(4, true, _localLobbyPlayerData.Serialize(),_lobbyData.Serialize());
            return succeeded;
        }

        public string GetLobbyCode()
        {
            return LobbyManager.Instance.GetLobbyCode();
        }

        internal async Task<bool> JoinLobby(string code)
        {
            /*        Dictionary<string, string> playerData = new Dictionary<string, string>()
                    {
                        { "GamerTag", "JoinPlayer"}
                    };*/
            _localLobbyPlayerData = gameObject.AddComponent<LobbyPlayerData>();
            _localLobbyPlayerData.Initialize(AuthenticationService.Instance.PlayerId, "JoinPlayer");
            bool succeeded = await LobbyManager.Instance.JoinLobby(code, _localLobbyPlayerData.Serialize());
            return succeeded;
        }

        public List<LobbyPlayerData> GetPlayers()
        {
            return _lobbyPlayerDatas;
        }

        public async Task<bool> SetPlayerReady()
        {
            Debug.Log(_localLobbyPlayerData);
            _localLobbyPlayerData.IsReady = true;
            return await LobbyManager.Instance.UpdatePlayerData(_localLobbyPlayerData.Id, _localLobbyPlayerData.Serialize());
        }

        public int GetMapIndex()
        {
            return _lobbyData.MapIndex;
        }

        public async Task<bool> SetSelectedMap(int currentMapIndex)
        {
            _lobbyData.MapIndex = currentMapIndex;
            return await LobbyManager.Instance.UpdateLobbyData(_lobbyData.Serialize());
        }
    }
}