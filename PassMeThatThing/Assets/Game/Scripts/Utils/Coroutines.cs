using System.Collections;
using UnityEngine;

namespace Utils
{
    
    
    public class Coroutines : MonoBehaviour, ICoroutineRunner
    { 
        //сообщение  что-то для теста
        public Coroutine StartRoutine(IEnumerator routine)
            => StartCoroutine(routine);
        public void StopRoutine(Coroutine routine)
        {
            if (routine != null)
                StopCoroutine(routine);
        }
        
    }
}