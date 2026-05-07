using System;
using UnityEngine;

public class InteractionZone : MonoBehaviour
{
    public LayerMask interactableLayer;
    
    public event Action<Collider> OnInteractionZoneEnter;
    public event Action<Collider> OnInteractionZoneExit;
    
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Interactable"))
        {
            OnInteractionZoneEnter?.Invoke(other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Interactable"))
        {
            OnInteractionZoneExit?.Invoke(other);
        }
    }
}
