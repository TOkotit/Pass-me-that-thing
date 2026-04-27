using System.Collections;
using UnityEngine;

namespace Utils
{
    
    
    /// <summary>
    /// Сервис для запуска Unity-корутин из не-MonoBehaviour классов.
    /// </summary>
    public interface ICoroutineRunner
    {
        Coroutine StartRoutine(IEnumerator routine);
        void StopRoutine(Coroutine routine);
    }
}