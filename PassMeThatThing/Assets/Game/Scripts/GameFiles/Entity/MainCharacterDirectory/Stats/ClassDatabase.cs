using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Assets.Game.Scripts.GameFiles.Entity.MainCharacterDirectory.Stats
{
    /// <summary>
    /// Визуальная инфа о классе, основная инфа остается в ClassStats
    /// id должен совпадать c ClassStats.name
    /// </summary>
    [CreateAssetMenu(fileName = "ClassDatabase", menuName = "Scriptable Objects/ClassDatabase")]
    public class ClassDatabase : ScriptableObject
    {
        public List<ClassData> allClasses;
        


        public ClassData GetClass(string id)
        {
            return allClasses.Find(c => c.Id == id);
        }
    }
}