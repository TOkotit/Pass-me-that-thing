using System;
using Game.Scripts.GameFiles.Items;
using UnityEngine;

namespace MainCharacter_old
{
    public class InteractionZoneAlign : MonoBehaviour
    {
        [SerializeField] private PlayerInteraction playerInteraction;
        [SerializeField] private Transform zone;

        private void Start()
        {
            zone.localPosition = new Vector3(0,0, playerInteraction.InteractionDistance/2);
            zone.localScale = new Vector3(0, 0, playerInteraction.InteractionDistance/2);
        }
    }
}