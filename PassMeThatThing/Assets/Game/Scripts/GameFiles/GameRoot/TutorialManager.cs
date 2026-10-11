using Game.Scripts.Systems;
using R3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VContainer;
using static UnityEngine.Rendering.DebugUI;

namespace Game.Scripts.GameFiles.GameRoot
{
    public class TutorialManager
    {
        [Inject] private OptionsManager _optionsmanager;
        
        //стадии подсказок

        //часть 1  фазы, квота, задания
        public ReactiveProperty<bool> lookTimerPrepare = new();
        public ReactiveProperty<bool> lookQuota = new();
        public ReactiveProperty<bool> lookEvent = new();
        public ReactiveProperty<bool> lookItemSkills = new();
        //public ReactiveProperty<bool> lookAllEventsDone = new();
        public ReactiveProperty<bool> lookRest = new();

        public void SetLookTimerPrepare(bool value)
        {
            if (!_optionsmanager.IsTutorShow) return;
            lookTimerPrepare.Value = value;
        }

        public void SetLookQuota(bool value)
        {
            if (!_optionsmanager.IsTutorShow) return;
            lookQuota.Value = value;
        }

        public void SetLookEvent(bool value)
        {
            if (!_optionsmanager.IsTutorShow) return;
            lookEvent.Value = value;
        }

        public void SetLookItemSkills(bool value)
        {
            if (!_optionsmanager.IsTutorShow) return;
            lookItemSkills.Value = value;
        }

        //public void SetLookAllEventsDone(bool value)
        //{
        //    if (!_optionsmanager.IsTutorShow) return;
        //    lookAllEventsDone.Value = value;
        //}

        public void SetLookRest(bool value)
        {
            if (!_optionsmanager.IsTutorShow) return;
            lookRest.Value = value;
        }

        public void DisableAll()
        {
            lookTimerPrepare.Value = false;
            lookQuota.Value = false;
            lookEvent.Value = false;
            lookItemSkills.Value = false;
            //lookAllEventsDone.Value = false;
            lookRest.Value = false;
        }

        public void EndTutor()
        {
            if (!_optionsmanager.IsTutorShow) return;

            DisableAll();

            _optionsmanager.SetTutor(false);
            _optionsmanager.SaveSettings();
        }
    }
}
