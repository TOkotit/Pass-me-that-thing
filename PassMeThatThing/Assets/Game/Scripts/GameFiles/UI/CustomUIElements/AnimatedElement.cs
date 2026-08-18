using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Game.Scripts.GameFiles.UI.CustomUIElements
{
    [UxmlElement("AnimatedElement")]
    public partial class AnimatedElement : VisualElement
    {
        [UxmlAttribute]
        public List<Sprite> frames { get; set; }

        [UxmlAttribute]
        public float intervalMs { get; set; } = 100f;

        private int _currentFrame;

        public AnimatedElement()
        {
            schedule.Execute(UpdateFrame).Every((long)intervalMs);
        }

        private void UpdateFrame()
        {
            if (frames == null || frames.Count == 0) return;

            style.backgroundImage = Background.FromSprite(frames[_currentFrame]);
            _currentFrame = (_currentFrame + 1) % frames.Count;
        }
    }
}