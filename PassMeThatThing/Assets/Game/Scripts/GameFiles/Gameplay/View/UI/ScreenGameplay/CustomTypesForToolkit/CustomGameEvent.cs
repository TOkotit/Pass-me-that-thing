using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.DebugUI;

namespace Assets.Game.Scripts.GameFiles.Gameplay.View.UI.ScreenGameplay.CustomTypesForToolkit
{
    [UxmlElement("CustomGameEvent")]
    public partial class CustomGameEvent : VisualElement
    {
        public ProgressBar timeBar;

        //public CustomGameEvent()
        //{
        //    timeBar = this.Q<ProgressBar>("EventTimeProgress");
        //}

        public void UpdateTimeProgress(float percent)
        {
            timeBar.value = percent * 100;
        }
    }
}
