using System.Collections.Generic;
using UnityEngine;

namespace Game.Scripts.GameFiles.LevelGeneration
{
    
    /// <summary>
    /// Структура с данными для дверей, хранит локальную и глобальную позицию
    /// </summary>
    public struct VirtualDoor
    {
        public Vector3Int LocalDirection;  
        public Vector3Int GlobalDirection; 
    }
    /// <summary>
    /// <para>Структура для сохранения локальной позиции ячейки, её локального поворота и ссылки на компонент</para>
    /// Используется в <see cref="LevelRoom"/>
    /// </summary>
    public struct VirtualPlateData
    {
        public Vector3Int LocalPosition;
        public List<VirtualDoor> Doors;
    }
    
    /// <summary>
    /// <para>Структура с данными для запланированного сегмента туннеля<br/>
    /// Сохраняет конфигурацию пути между кластерами до этапа физического создания объектов на сцене<br/></para>
    /// Используется в <see cref="LevelRoom"/>
    /// </summary>
    public struct VirtualTunnelData
    {
        public RoomDataEntry Entry;
        public Vector3Int Origin;
        public RoomRotation Rotation;
    }
    
    /// <summary>
    /// <para>Структура с данными для стены.<br/>
    /// Хранит ссылку на GameObject, позицию, поворот и название<br/></para>
    /// Используется в <see cref="LevelOrchestrator"/>
    /// </summary>
    public struct VirtualWallData
    {
        public GameObject Prefab;
        public Vector3 Position;
        public Quaternion Rotation;
        public string Name;
    }
}