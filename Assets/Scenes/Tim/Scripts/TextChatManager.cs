using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Game;
using GameFramework.Core.Data;
using Unity.Services.Lobbies.Models;
using Game.Events;
using Unity.Netcode;

namespace GameFramework.Core.GameFramework.Manager
{
    public class TextChatManager : Singleton<TextChatManager>
    {
        private TMPro.TextMeshProUGUI _messageLogObject;
        private DoublyLinkedList<Message> _messageLogHead;
        private DoublyLinkedList<Message> _messageLogTail;
        private int _messageLogCount = 0;
        private const int MAX_MESSAGE_LOG = 50;

        private TMPro.TextMeshProUGUI _messageInput;
        private GameObject _placeholderMessage;
        private TMPro.TMP_InputField _messageField;
        private Image _messageBackground;

        private GameLobbyManager _gameLobby;
        private LobbyPlayerData _player;

        private TextMessageHandler _server;

        private PlayerControl _playerControl;


        private IEnumerator _activeCoroutine;
        private void OnEnable()
        {
            _messageLogObject = gameObject.transform.parent.Find("Log").gameObject.GetComponent<TMPro.TextMeshProUGUI>();

            _messageInput = gameObject.transform.GetChild(0).gameObject.transform.Find("Text").gameObject.GetComponent<TMPro.TextMeshProUGUI>();
            _placeholderMessage = gameObject.transform.GetChild(0).gameObject.transform.Find("Placeholder").gameObject;
            _messageField = gameObject.GetComponent<TMPro.TMP_InputField>();
            _messageBackground = GetComponent<Image>();

            _messageLogObject.enabled = false;
            _messageInput.enabled = false;
            _placeholderMessage.SetActive(false);
            _messageBackground.enabled = false;

            _gameLobby = GameLobbyManager.Instance;
            _player = _gameLobby.GetLocalPlayer();
            _server = (TextMessageHandler)gameObject.GetComponent(typeof(TextMessageHandler));

            MessageEvents.OnMessageReceived += AddMessage;

            _playerControl = new PlayerControl();
        }

        private void OnDisable()
        {
            MessageEvents.OnMessageReceived -= AddMessage;
        }

        private string GetMessageLog()
        {
            if (_messageLogHead == null)
            {
                return "";
            }
            DoublyLinkedList<Message> traversal = _messageLogHead;
            string ret = "";
            while (traversal != null)
            {
                ret += traversal.GetNode().ToString() + "\n";
                traversal = traversal.GetNext();
            }
            return ret;
        }

        public void AddMessage(string player, string message)
        {
            Debug.Log(player + " " + message);
            if (_messageLogHead == null)
            {
                _messageLogHead = new DoublyLinkedList<Message>(new Message(player, message));
                _messageLogTail = _messageLogHead;
                _messageLogCount = 1;
            }
            else
            {
                _messageLogTail.SetNext(new DoublyLinkedList<Message>(new Message(player, message), _messageLogTail));
                _messageLogTail = _messageLogTail.GetNext();
                if (_messageLogCount >= MAX_MESSAGE_LOG)
                {
                    _messageLogHead = _messageLogHead.GetNext();
                }
                else
                {
                    _messageLogCount++;
                }
            }
            _messageLogObject.text = GetMessageLog();
            _messageLogObject.enabled = true;

            if (_activeCoroutine != null)
            {
                StopCoroutine(_activeCoroutine);
            }

            _messageField.text = "";
            _messageInput.enabled = false;
            _placeholderMessage.SetActive(false);
            _messageBackground.enabled = false;

            _activeCoroutine = HideTimer();
            StartCoroutine(_activeCoroutine);
        }

        private void DisplayTextObjects()
        {
            _messageLogObject.enabled = true;
            _messageField.Select();
            _messageField.ActivateInputField();
            _messageInput.enabled = true;
            _placeholderMessage.SetActive(true);
            _messageBackground.enabled = true;
        }

        private void HideTextObjects()
        {
            _messageLogObject.enabled = false;
        }

        private IEnumerator HideTimer()
        {
            yield return new WaitForSeconds(5f);
            HideTextObjects();
        }

        private void Start()
        {

        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
            {
                if (!_messageInput.enabled)
                {
                    DisplayTextObjects();
                    if (_activeCoroutine != null)
                    {
                        StopCoroutine(_activeCoroutine);
                    }
                    _playerControl.Disable();
                }
                else
                {
                    string message = _messageInput.text;
                    
                    if (NetworkManager.Singleton.IsServer)
                    {
                        AddMessage(_player.Gamertag, message);
                        _server.SendUnnamedMessage(_player.Gamertag, message);
                    }
                    else
                    {
                        _server.SendUnnamedMessage(_player.Gamertag, message);
                    }
                    _playerControl.Enable();
                }
            }
        }
    }
}