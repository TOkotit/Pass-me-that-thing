using DG.Tweening;
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
        //задается при создании элемента из темплейта
        public ProgressBar timeBar; 

        public void UpdateTimeProgress(float time, float limit)
        {
            var newValue = (limit - time) / limit * 100;
            DOTween.To(
                () => timeBar.value,
                x => timeBar.value = x,
                newValue, 0.5f).SetEase(Ease.OutQuad);
        }
    }
}
