using GameFramework.Core.Data;
using System.Collections;
using System.Collections.Generic;

using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Game
{
    public class LobbyPlayer : NetworkBehaviour
    {
        [SerializeField] private TextMeshPro _playerName;
        [SerializeField] private Renderer _isReadyRenderer;
        private LobbyPlayerData _data;
        private MaterialPropertyBlock _propertyBlock;

        public void Start()
        {
            _propertyBlock = new MaterialPropertyBlock();
/*            _propertyBlock.SetColor("_BaseColor", Color.red);
*/        }
        public void SetData(LobbyPlayerData data)
        {
            _data = data;
            _playerName.text = _data.Gamertag;

            Debug.Log(_data.IsReady);
            if (_data.IsReady)
            {
                Debug.Log("_isReadyRenderer "+_isReadyRenderer);
                if (_isReadyRenderer != null)
                {
                    _isReadyRenderer.GetPropertyBlock(_propertyBlock);
                    _propertyBlock.SetColor("_Color", Color.green);
                    _isReadyRenderer.SetPropertyBlock(_propertyBlock);
                }
            }
            
            gameObject.SetActive(true);

        }
    }
}