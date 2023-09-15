using GameFramework.Core;
using GameFramework.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace GameFramework.Manager
{
    public class LobbyManager : MySingleton<LobbyManager>
    {
        private Lobby _lobby;
        private Coroutine _heatbeatCoroutine;
        private Coroutine _refreshLobbyCoroutine;


        public async Task<bool> CreateLobby(int maxPlayers,bool isPrivate, Dictionary<string, string> data, Dictionary<string,string> lobbyData)
        {
            Dictionary<string, PlayerDataObject> playerData = SerializePlayerData(data);
            Player player = new Player(AuthenticationService.Instance.PlayerId, null, playerData);

            CreateLobbyOptions options = new CreateLobbyOptions()
            {
                Data = SerializeLobbyData(lobbyData),
                IsPrivate = isPrivate,
                Player = player
            };

            try
            {
                _lobby = await LobbyService.Instance.CreateLobbyAsync("Lobby", maxPlayers, options);

            }catch (Exception ex)
            {
                Debug.Log("Failed to create lobby");
                return false;
            }

            Debug.Log($"Lobby created with lobby id: {_lobby.Id}");

            _heatbeatCoroutine = StartCoroutine(HeatbeatLobbyCoroutine(_lobby.Id,6f));
            _refreshLobbyCoroutine = StartCoroutine(RefreshLobbyCoroutine(_lobby.Id, 1f));

            return true;
        }

        private IEnumerator HeatbeatLobbyCoroutine(string lobbyId, float waitTimeSeconds)
        {
            while (true)
            {
                Debug.Log("Heartbeat");
                LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
                yield return new WaitForSeconds(waitTimeSeconds);
            }
        }

        private IEnumerator RefreshLobbyCoroutine(string lobbyId, float waitTimeSeconds)
        {
            while (true)
            {
                Debug.Log("Refresh");
                Task<Lobby> task = LobbyService.Instance.GetLobbyAsync(lobbyId);
                yield return new WaitUntil(() => task.IsCompleted);
                Lobby newLobby = task.Result;
                if(newLobby.LastUpdated > _lobby.LastUpdated)
                {
                    _lobby = newLobby;
                    LobbyEvents.OnLobbyUpdated?.Invoke(_lobby);
                }
                yield return new WaitForSeconds(waitTimeSeconds);
            }
        }

        private Dictionary<string,DataObject> SerializeLobbyData(Dictionary<string,string> data)
        {
            Dictionary<string, DataObject> lobbyData = new Dictionary<string, DataObject>();
            foreach (var ( key, value) in data)
            {
                lobbyData.Add(key, new DataObject(
                    visibility: DataObject.VisibilityOptions.Member, // visibile only to players of the lobby
                    value: value));
            }
            return lobbyData;
        }

        private Dictionary<string, PlayerDataObject> SerializePlayerData(Dictionary<string, string> data)
        {
            Dictionary<string, PlayerDataObject> playerData = new Dictionary<string, PlayerDataObject>();
            foreach(var(key,value)in data)
            {
                playerData.Add(key, new PlayerDataObject(
                    visibility: PlayerDataObject.VisibilityOptions.Member, // visible only to players of the lobby
                    value: value)
                );
            }
            return playerData;
        }
        public void OnApplicationQuite()
        {
            if (_lobby != null && _lobby.HostId == AuthenticationService.Instance.PlayerId)
            {
                LobbyService.Instance.DeleteLobbyAsync( _lobby.Id );
            }
        }

        public string GetLobbyCode()
        {
            return _lobby?.LobbyCode;
        }

        public async Task<bool> JoinLobby(string code, Dictionary<string, string> playerData)
        {
            JoinLobbyByCodeOptions options = new JoinLobbyByCodeOptions();
            Player player = new Player(AuthenticationService.Instance.PlayerId, null, SerializePlayerData(playerData));
            options.Player = player;        
            try
            {
                _lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(code, options);  
            }
            catch(System.Exception)
            {
                return false;
            }

            _refreshLobbyCoroutine = StartCoroutine(RefreshLobbyCoroutine(_lobby.Id, 1f));
            return true;
        }

        public List<Dictionary<string, PlayerDataObject>> GetPlayersData()
        {
            List<Dictionary<string, PlayerDataObject>> data = new List<Dictionary<string, PlayerDataObject>>();
            foreach (Player player in _lobby.Players)
            {
                data.Add(player.Data);
            }
            return data;
        }

        public async Task<bool> UpdatePlayerData(string playerId, Dictionary<string, string> data)
        {
            Dictionary<string,PlayerDataObject> playerData = SerializePlayerData(data);
            UpdatePlayerOptions options = new UpdatePlayerOptions()
            {
                Data = playerData

            };
            try{
                _lobby = await LobbyService.Instance.UpdatePlayerAsync(_lobby.Id, playerId, options);
            } catch (System.Exception)
            {
                return false;
            }

            LobbyEvents.OnLobbyUpdated(_lobby);

            return true;
        }

        public async Task<bool> UpdateLobbyData(Dictionary<string, string> data)
        {
            Dictionary<string, DataObject> lobbyData = SerializeLobbyData(data);
            UpdateLobbyOptions options = new UpdateLobbyOptions() 
            { 
                Data = lobbyData 
            };

            try
            {
                _lobby = await LobbyService.Instance.UpdateLobbyAsync(_lobby.Id, options);
            } catch (System.Exception)
            {
                return false;
            }

            LobbyEvents.OnLobbyUpdated(_lobby); 
            return true;
        }

        public string GetHostId()
        {
            return _lobby.HostId;
        }
    }


}

