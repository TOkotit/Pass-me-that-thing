using System;
using System.Collections.Generic;
using System.Globalization;
using DG.Tweening;
using Game.Gameplay.View.UI.ScreenMinigame;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class CutWiresMinigameUI : MinigameUI
{
    [SerializeField] private TextMeshProUGUI countText;
    
    [SerializeField] private UIConnectionManager uiConnectionManager;
    [SerializeField] private int totalConnections;

    private void Start()
    {
        ActiveConnectionsCountChanged(0);
        uiConnectionManager.OnActiveConnectionsCountChanged += ActiveConnectionsCountChanged;
    }

    private void OnDestroy()
    {
        uiConnectionManager.OnActiveConnectionsCountChanged -= ActiveConnectionsCountChanged;
    }

    public void ActiveConnectionsCountChanged(int count)
    {
        countText.text = $"{count}/{totalConnections}";
        if (count >= totalConnections)
        {
            mainBinder.CompleteMinigame();
        }
    }
}
