using R3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Scripts.GameFiles.GameRoot
{
    public class TutorialModel
    {
        //стадии подсказок
        public ReactiveProperty<bool> lookTimerPrepare = new();

        public ReactiveProperty<bool> lookEvent = new();

        public ReactiveProperty<bool> lookItem = new();

        public ReactiveProperty<bool> lookItemSkills = new();

        public ReactiveProperty<bool> lookRest = new();
    }
}
