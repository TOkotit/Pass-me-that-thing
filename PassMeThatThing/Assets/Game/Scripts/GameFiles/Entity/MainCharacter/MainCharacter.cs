using Entity;
using Game.Scripts.GameFiles.Items;
using UnityEngine;

namespace Game.Scripts.GameFiles.Entity
{
    public class MainCharacter : Damagable
    {
        MainCharacterModel model;
        public override DamagableModel DamagableModel { get => model; }

        public override void OnStartClient()
        {
            var playerInteraction = GetComponent<PlayerInteraction>();
            model.SetPlayerInteraction(playerInteraction);
        }
    }
}