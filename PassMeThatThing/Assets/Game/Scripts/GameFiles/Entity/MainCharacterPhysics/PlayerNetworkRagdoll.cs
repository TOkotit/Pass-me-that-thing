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
    private Dictionary<string, Rigidbody> playerBoneDict;
    private MainCharacter player;

    public void Setup(Damagable mainCharacter)
    {
        if (mainCharacter is MainCharacter mc) player = mc;
        else { return; }
        var playerBones = player.RagdollHandler.GetRigidbodies();
        playerBoneDict = new Dictionary<string, Rigidbody>();
        foreach (var rb in playerBones)
        {
            if (rb && !playerBoneDict.ContainsKey(rb.name))
                playerBoneDict.Add(rb.name, rb);
        }
    }

    private void SyncBones(bool toRagdoll)
    {
        foreach (var ragdollRb in ragdollBones)
        {
            if (!ragdollRb) continue;
            if (playerBoneDict.TryGetValue(ragdollRb.name, out var playerRb))
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
        if (player.netIdentity.isLocalPlayer)
            ragdollCamera.enabled = true;
        foreach (var bone in ragdollBones)
            bone.gameObject.SetActive(true);
    }

    public void DisableRagdoll()
    {
        SyncBones(false);
        ragdollCamera.enabled = false;
        foreach (var bone in ragdollBones)
            bone.gameObject.SetActive(false);
    }
}
}