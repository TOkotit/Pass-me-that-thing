using System;
using System.Collections.Generic;
using Game.UI;
using UnityEngine;

namespace Game.Gameplay.View.UI.ScreenBuild
{
    public class ScreenBuildBinder : WindowBinder<ScreenBuildViewModel>
    {
        [SerializeField] private GameObject buildPreviewContainer;
    }
}