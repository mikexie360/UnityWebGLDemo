using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Game;
using GameFramework.Core.Data;
using Unity.Services.Lobbies.Models;
using Game.Events;
using Unity.Netcode;
using UnityEngine.SceneManagement;

namespace GameFramework.Core.GameFramework.Manager
{
    public class TextChatManager : Singleton<TextChatManager>
    {
        private TMPro.TextMeshProUGUI _messageLogObject;
        private DoublyLinkedList<Message> _messageLogHead;
        private DoublyLinkedList<Message> _messageLogTail;
        private Scrollbar _messageSrollRect;
        //private int _messageLogCount = 0;
        //private const int MAX_MESSAGE_LOG = 50;
        private int _fillerCount = 11;

        private TMPro.TextMeshProUGUI _messageInput;
        private GameObject _placeholderMessage;
        private TMPro.TMP_InputField _messageField;
        private Image _messageBackground;

        private GameLobbyManager _gameLobby;
        private LobbyPlayerData _player;

        private TextMessageHandler _server;

        private PlayerController _playerControl;


        private IEnumerator _activeCoroutine;
        private void OnEnable()
        {
            _messageLogObject = gameObject.transform.parent.Find("Scroll View").Find("Viewport").GetChild(0).gameObject.GetComponent<TMPro.TextMeshProUGUI>();
            _messageSrollRect = gameObject.transform.parent.Find("Scroll View").Find("Scrollbar Vertical").gameObject.GetComponent<Scrollbar>();

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

        }

        private void OnDisable()
        {
            MessageEvents.OnMessageReceived -= AddMessage;
        }

        private void AddFiller(int count)
        {
            for (int i = 0; i < count; i++)
            {
                _messageLogObject.text += "\n";
            }
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
                //_messageLogCount = 1;
            }
            else
            {
                
                _messageLogTail.SetNext(new DoublyLinkedList<Message>(new Message(player, message), _messageLogTail));
                _messageLogTail = _messageLogTail.GetNext();
                /*
                if (_messageLogCount >= MAX_MESSAGE_LOG)
                {
                    _messageLogHead = _messageLogHead.GetNext();
                }
                else
                {
                    _messageLogCount++;
                }
                */
            }
            _messageLogObject.text = "";
            if (_fillerCount >= 0)
            {
                AddFiller(_fillerCount);
                _fillerCount--;
            }
            _messageLogObject.text += GetMessageLog();
            _messageLogObject.enabled = true;

            if (_activeCoroutine != null)
            {
                StopCoroutine(_activeCoroutine);
            }

            _activeCoroutine = HideTimer();
            StartCoroutine(_activeCoroutine);
        }

        private void LateUpdate()
        {
            if (!_messageInput.enabled)
            {
                _messageSrollRect.value = 0;
            }
        }

        private void DisplayTextObjects()
        {
            _messageLogObject.enabled = true;
            _messageInput.enabled = true;
            _placeholderMessage.SetActive(true);
            _messageBackground.enabled = true;
            _messageField.ActivateInputField();
            _messageField.Select();
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
                if (_playerControl == null)
                {
                    try
                    {
                        _playerControl = (PlayerController)NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject().gameObject.GetComponent(typeof(PlayerController));
                    }
                    catch (System.Exception e) { Debug.Log(e); }
                }
                if (!_messageInput.enabled)
                {
                    if (_playerControl != null)
                    {
                        _playerControl.GetControl().Disable();
                    }

                    DisplayTextObjects();
                    if (_activeCoroutine != null)
                    {
                        StopCoroutine(_activeCoroutine);
                    }
                    Cursor.lockState = CursorLockMode.None;
                }
                else
                {
                    string message = _messageInput.text;
                    var temp = message.Trim();

                    _messageField.text = "";
                    _messageInput.enabled = false;
                    _placeholderMessage.SetActive(false);
                    _messageBackground.enabled = false;

                    if (_playerControl != null)
                    {
                        _playerControl.GetControl().Enable();
                    }
                    Cursor.lockState = CursorLockMode.Locked;

                    if (temp.Equals("") || (temp.Length == 1 && temp.ToCharArray()[0] == 8203))
                    {
                        return;
                    }

                    if (NetworkManager.Singleton.IsServer)
                    {
                        AddMessage(_player.Gamertag, message);
                        _server.SendUnnamedMessage(_player.Gamertag, message);
                    }
                    else
                    {
                        _server.SendUnnamedMessage(_player.Gamertag, message);
                    }
                }
            }
        }
    }
}