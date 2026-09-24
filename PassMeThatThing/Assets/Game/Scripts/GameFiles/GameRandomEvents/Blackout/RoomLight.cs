using System;
using System.Collections;
using System.Collections.Generic;
using Game.Scripts.GameFiles.LevelGeneration.Room_Envieroments;
using Mirror;
using UnityEngine;
using UnityEngine.Serialization;


public enum RoomLightState
{
    Common,
    Warning,
    Off
}

public class RoomLight : MonoBehaviour
{
    
    [SerializeField] private float rotationSpeed = 30f;
    
    [Tooltip("Ссылки на компоненты Light")]
    [SerializeField] private Light commonLightComponent;
    [SerializeField] private GameObject warningLighContainerPrefab;
    
    [Tooltip("Префабы ламп для разных состояний")]
    [SerializeField] private GameObject lampOnCommonPrefab;
    [SerializeField] private GameObject lampOffPrefab;
    [SerializeField] private GameObject lampWarningPrefab;

    public RoomLightState CurrentState { get; private set; } = RoomLightState.Common;

    private void Start()
    {
        if (!commonLightComponent)
        {
            Debug.LogError($"[RoomLight] Light component for commonLightComponent  not assigned");
            return;
        };
        if (!warningLighContainerPrefab)
        {
            Debug.LogError($"[RoomLight] Light container prefab not assigned");
            return;
        }
        

        if (NetworkVisionManager.Instance)
        {
            NetworkVisionManager.Instance.RegisterRoomLight(this);
        }
        else
        {
            Debug.LogWarning($"[RoomLight] Can't find NetworkVisionManager for {{gameObject.name}}");
        }
    }

    private void OnDestroy()
    {
        if (NetworkVisionManager.Instance)
        {
            NetworkVisionManager.Instance.UnregisterRoomLight(this);
        }
    }

    public void SetLampState(RoomLightState state)
    {
        Debug.Log($"[RoomLight] SetLampState was called on state {state}");
        CurrentState = state;

        switch (state)
        {
            case RoomLightState.Common:
                lampOnCommonPrefab.SetActive(true);
                commonLightComponent.enabled = true;

                
                lampOffPrefab.SetActive(false);
                lampWarningPrefab.SetActive(false);
                warningLighContainerPrefab.SetActive(false);

                Debug.Log($"[RoomLight] Common is activate");
                break;
            
            case RoomLightState.Off:
                lampOffPrefab.SetActive(true);

                lampOnCommonPrefab.SetActive(false);
                lampWarningPrefab.SetActive(false);
                commonLightComponent.enabled = false;
                warningLighContainerPrefab.SetActive(false);
                Debug.Log($"[RoomLight] Off is activate");

                break;
            
            case RoomLightState.Warning:
                lampWarningPrefab.SetActive(true);
                warningLighContainerPrefab.SetActive(true);
                
                lampOnCommonPrefab.SetActive(false);
                lampOffPrefab.SetActive(false);
                commonLightComponent.enabled = false;
                Debug.Log($"[RoomLight] Warning is activate");

                break;
            
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }
    }

    public void Update()
    {
        if (CurrentState == RoomLightState.Warning)
        {
            warningLighContainerPrefab.transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
        }
    }
}