using System;
using System.Collections.Generic;
using Entity;
using Game.Entity;
using Mirror;
using UnityEngine;

namespace Game.Scripts.GameFiles.Entity.MainCharacterPhysics
{
    public class PlayerNetworkRagdoll : NetworkBehaviour, INetworkRagdoll
    {
        [SerializeField] private List<Rigidbody> ragdollBones;
        [SerializeField] private Camera ragdollCamera;
        [SerializeField] private SkinnedMeshRenderer networkMeshRenderer;
        private Dictionary<string, Rigidbody> _playerBoneDict;
        private MainCharacter _player;
        
        private AudioListener _mainAudioListener;
        private AudioListener _ragdollAudioListener;
        
        private Camera _playerCamera;
        public Camera Camera => _playerCamera;
        public void Setup(Damageable mainCharacter, Dictionary<string, Rigidbody> playerBoneDict)
        {
            if (mainCharacter is MainCharacter mc)
            {
                _player = mc;
                _playerCamera = mc.MCamera.Camera;
            }
            else { return; }
            _playerBoneDict = playerBoneDict;
            
            if (ragdollCamera)
                _ragdollAudioListener = ragdollCamera.GetComponent<AudioListener>();

            if (_ragdollAudioListener)
                _ragdollAudioListener.enabled = false;

            if (!_player.isLocalPlayer)
            {
                if (_mainAudioListener) _mainAudioListener.enabled = false;
                if (_ragdollAudioListener) _ragdollAudioListener.enabled = false;
            }
        }

        private void SyncBones(bool toRagdoll)
        {
            foreach (var ragdollRb in ragdollBones)
            {
                if (!ragdollRb) continue;
                if (_playerBoneDict.TryGetValue(ragdollRb.name, out var playerRb))
                {
                    if (toRagdoll)
                    {
                        ragdollRb.transform.position = playerRb.transform.position;
                        ragdollRb.transform.rotation = playerRb.transform.rotation; 
                    }
                    else
                    {
                        playerRb.transform.position = ragdollRb.transform.position;
                        playerRb.transform.rotation = ragdollRb.transform.rotation;
                    }
                }
            }
        }

        public void EnableRagdoll()
        {
            SyncBones(true);
            if (_player.netIdentity.isLocalPlayer)
            {
                ragdollCamera.transform.rotation = _playerCamera.transform.rotation;
                ragdollCamera.enabled = true;
                
                if (_mainAudioListener) _mainAudioListener.enabled = false;
                if (_ragdollAudioListener) _ragdollAudioListener.enabled = true;
            }
            foreach (var bone in ragdollBones) 
                bone.gameObject.layer = LayerMask.NameToLayer("Ragdoll");
            networkMeshRenderer.enabled = true;
        }

        public void DisableRagdoll()
        {
            SyncBones(false);
            ragdollCamera.enabled = false;
            
            if (_player && _player.isLocalPlayer)
            {
                if (_ragdollAudioListener) _ragdollAudioListener.enabled = false;
                if (_mainAudioListener) _mainAudioListener.enabled = true;
            }
            
            foreach (var bone in ragdollBones) 
                bone.gameObject.layer = LayerMask.NameToLayer("OutOfBounds");
            networkMeshRenderer.enabled = false;
        }
    }
}