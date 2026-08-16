using System.Collections.Generic;
using Game.Entity.Stats;
using UnityEngine;

namespace Game.Entity
{
    public class ClassManager
    {
        private readonly MainCharacterModel _model;
        private ClassStats _currentClass;
        private Dictionary<string, ClassStats> _classCache = new Dictionary<string, ClassStats>();

        public ClassManager(MainCharacterModel model)
        {
            _model = model;
            LoadAllClasses();
        }

        public ClassStats CurrentClass => _currentClass;

        private void LoadAllClasses()
        {
            _classCache.Clear();
            var loadedClasses = Resources.LoadAll<ClassStats>("Classes");
            foreach (var classStats in loadedClasses)
            {
                if (!_classCache.ContainsKey(classStats.name))
                    _classCache.Add(classStats.name, classStats);
            }
        }

        public bool TryGetClass(string className, out ClassStats classStats)
        {
            return _classCache.TryGetValue(className, out classStats);
        }

        public void SetClass(ClassStats newClass)
        {
            _currentClass = newClass;
            if (newClass)
                _model.ApplyMultipliers(newClass);
            else
                _model.ResetToBase();
        }

        public void SetClass(string className)
        {
            if (TryGetClass(className, out var classStats))
                SetClass(classStats);
            else
                Debug.LogError($"[ClassManager] Class not found: {className}");
        }

        public void ResetToBase()
        {
            _currentClass = null;
            _model.ResetToBase();
        }
    }
}