using Mirror;
using Game.Entity;
using UnityEngine;
using NUnit.Framework;
using System.Collections.Generic;
using Assets.Game.Scripts.GameFiles.Entity.MainCharacterDirectory.Stats;

namespace Assets.Game.Scripts.GameFiles.InteractableObjects
{
    public class ClassShelf : NetworkBehaviour
    {
        [SerializeField] private ClassData defaultClass;
        [SerializeField] private List<ClassShelfObject> shelfObjects;

        private void Start()
        {
            foreach (var shelfObject in shelfObjects)
            {
                shelfObject.SetEnabled(true);
                shelfObject.OnClassChanged += OnClassSet;
            }
        }

        private void OnDestroy()
        {
            foreach (var shelfObject in shelfObjects)
            {
                shelfObject.OnClassChanged -= OnClassSet;
            }
        }

        [Server]
        public void SetEnabled(string classId, bool enabled)
        {
            var shelfObject = shelfObjects
                .Find(s => s.ClassData.Id == classId
                && s.IsEnabled != enabled);


            if (shelfObject != null)
            {
                shelfObject.SetEnabled(enabled);
            }
            else
            {
                Debug.Log($"SetEnabled shelfObject is null");
            }
        }

        [Command(requiresAuthority = false)]
        private void CmdResetClassForPlayer(MainCharacter targetPlayer)
        {
            if (!targetPlayer) return;

            if (targetPlayer.ClassManager.CurrentClass != null)
                SetEnabled(targetPlayer.ClassManager.CurrentClass.name, true);

            targetPlayer.ResetClass();
        }

        [Command(requiresAuthority = false)]
        private void CmdChangeClassForPlayer(MainCharacter targetPlayer, string classId)
        {
            if (!targetPlayer) return;

            if (targetPlayer.ClassManager.CurrentClass != null)
                SetEnabled(targetPlayer.ClassManager.CurrentClass.name, true);

            targetPlayer.ChangeClass(classId);

            if (classId != "")
                SetEnabled(classId, false);
        }

        public void OnClassSet(string classId)
        {
            var player = NetworkClient.localPlayer?.GetComponent<MainCharacter>();
            if (player)
            {
                if (classId == defaultClass.Id)
                {
                    CmdResetClassForPlayer(player);
                }
                else
                {
                    CmdChangeClassForPlayer(player, classId);
                }
            }
        }
    }
}
