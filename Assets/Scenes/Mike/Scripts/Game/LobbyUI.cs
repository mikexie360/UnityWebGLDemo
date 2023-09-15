using Game.Events;
using GameFramework.Core.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class LobbyUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _lobbyCodeText;
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _readyButton;
        [SerializeField] private Image _mapImage;
        [SerializeField] private Button _leftButton;
        [SerializeField] private Button _rightButton;
        [SerializeField] private TextMeshProUGUI _mapName;
        [SerializeField] private MapSelectionData _mapSelectionData;



        private int _currentMapIndex = 0;

        private void OnEnable()
        {
            if (GameLobbyManager.Instance.IsHost)
            {
                _leftButton.onClick.AddListener(OnLeftButtonClicked);
                _rightButton.onClick.AddListener(OnRightButtonClicked);
                Events.LobbyEvents.OnLobbyReady += OnLobbyReady;
            }
            _readyButton.onClick.AddListener(OnReadyPressed);

            LobbyEvents.OnLobbyUpdated += OnLobbyUpdated;
        }


        private void OnDisable()
        {
            _leftButton.onClick.RemoveAllListeners();
            _rightButton.onClick.RemoveAllListeners();
            
            _readyButton.onClick.RemoveAllListeners();

            Events.LobbyEvents.OnLobbyReady -= OnLobbyReady;

            LobbyEvents.OnLobbyUpdated -= OnLobbyUpdated;

        }
        private async void OnReadyPressed()
        {
            bool succeed = await GameLobbyManager.Instance.SetPlayerReady();
            if (succeed)
            {
                _readyButton.gameObject.SetActive(false);
            }
        }

        private async void OnLeftButtonClicked()
        {
            if(_currentMapIndex - 1 >= 0)
            {
                _currentMapIndex--;
            }
            else
            {
                _currentMapIndex = _mapSelectionData.Maps.Count - 1;
            }
            UpdateMap();
            _ = GameLobbyManager.Instance.SetSelectedMap(_currentMapIndex);
        }

        private async void OnRightButtonClicked()
        {
            if (_currentMapIndex + 1 < _mapSelectionData.Maps.Count)
            {
                _currentMapIndex++;
            }
            else
            {
                _currentMapIndex = 0;
            }
            UpdateMap();
            _ = GameLobbyManager.Instance.SetSelectedMap(_currentMapIndex);

        }

        private void UpdateMap()
        {
            _mapImage.color = _mapSelectionData.Maps[_currentMapIndex].MapThumbnail;
            _mapName.text = _mapSelectionData.Maps[_currentMapIndex].MapName;
        }

        // Start is called before the first frame update
        void Start()
        {
            _lobbyCodeText.text = $"Lobby code: {GameLobbyManager.Instance.GetLobbyCode()}";

            if (!GameLobbyManager.Instance.IsHost)
            {
                _leftButton.gameObject.SetActive(false);
                _rightButton.gameObject.SetActive(false);
            }
        }

        // Update is called once per frame
        void Update()
        {
            _lobbyCodeText.text = $"Lobby code: {GameLobbyManager.Instance.GetLobbyCode()}";

        }

        private void OnLobbyUpdated()
        {
            _currentMapIndex =  GameLobbyManager.Instance.GetMapIndex();
            UpdateMap();
        }

        private void OnLobbyReady()
        {
            _startButton.gameObject.SetActive(true);
        }
    }
}