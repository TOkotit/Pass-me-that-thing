using Game.Scripts.GameFiles.LevelGeneration.Editor_Grid;
using UnityEngine;

namespace Game.Scripts.GameFiles.LevelGeneration
{
    
    /// <summary>
    /// <para>
    /// Проверяет доступность выбранного места для создания объекта<br/>
    /// Гарантирует, что повернутая комната при размещении в заданных координатах не пересечется с уже занятыми ячейками глобальной сетки<br/>
    /// </para>
    /// Применяется в <see cref="LevelOrchestrator"/> перед подтверждением координат комнаты или туннеля
    /// </summary>
    public static class RoomCollisionValidator
    {
        public static bool IsPlacementValid(LevelGrid grid, RoomDataEntry entry, RoomRotation rotation, Vector3Int origin)
        {
            if (!grid || !entry.PrefabGameObject || !entry.RoomComponent) return false;

            var rotatedPlates = RoomRotationHelper.GetRotatedPlates(entry, rotation);

            for (var i = 0; i < rotatedPlates.Length; i++)
            {
                var globalCellPos = origin + rotatedPlates[i].LocalPosition;

                if (grid.IsCellOccupied(globalCellPos))
                {
                    return false; 
                }
            }

            return true;
        }
    }
}