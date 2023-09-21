using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameFramework.Core.Data;
using Unity.Services.Lobbies.Models;
using GameFramework.Events;

public class TextChatManager : MonoBehaviour
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
    }

    private void OnDisable()
    {

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
            Message m = traversal.GetNode();
            ret += m.GetId() + ": " + m.GetMessage() + "\n"; // UPDATE THIS WHEN I FIGURE OUT PLAYER ID STUFF
            traversal = traversal.GetNext();
        }
        return ret;
    }

    private void DisplayTextObjects()
    {
        _messageLogObject.enabled = true;
        _messageField.Select();
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

    // Update is called once per frame
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
            } else
            {
                string message = _messageInput.text;
                Debug.Log(message);
                if (_messageLogHead == null)
                {
                    _messageLogHead = new DoublyLinkedList<Message>(new Message(null, message)); // FILL IN THIS NULL WITH PLAYER INFO
                    _messageLogTail = _messageLogHead;
                    _messageLogCount = 1;
                } else
                {
                    _messageLogTail.SetNext(new DoublyLinkedList<Message>(new Message(null, message), _messageLogTail)); // FILL IN THIS NULL WITH PLAYER INFO);
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

                _messageField.text = "";
                _messageInput.enabled = false;
                _placeholderMessage.SetActive(false);
                _messageBackground.enabled = false;

                // WANT TO CHANGE THIS TO A TIMER
                _activeCoroutine = HideTimer();
                StartCoroutine(_activeCoroutine);
            }
        }
    }
}
