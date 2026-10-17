using Mirror;
using UnityEngine;

namespace Game.Scripts.GameFiles.LevelGeneration
{
    public enum GamePhase
    {
        Mission,
        Hub
    }
    public enum SessionStep
    {
        Ignored,  
        NextMission,
        GoToHub
    }

    
    /// <summary>
    /// Хранит прогресс сессии: какой блок, какая миссия, какой сид.
    /// Обычный класс: не MonoBehaviour и не NetworkBehaviour, живёт только на сервере.
    /// Его создаёт и держит CustomNetworkRoomManager, поэтому он переживает смену сцен.
    /// Сам ничего не грузит, только считает.
    /// </summary>
    public class SessionProgressionManager
    {
        private static readonly int[] MissionsPerBlock = { 1, 2, 3 };
 
        public int SessionSeed { get; }
        public GamePhase Phase { get; private set; } = GamePhase.Mission;
        public int BlockNumber { get; private set; } = 1;
        public int MissionInBlock { get; private set; } = 1;
        public int TotalMissionNumber { get; private set; } = 1;
        public int HubVisits { get; private set; }
 
        public int RequiredMissionsInBlock =>
            MissionsPerBlock[Mathf.Min(BlockNumber, MissionsPerBlock.Length) - 1];
 
        public SessionProgressionManager(int sessionSeed)
        {
            SessionSeed = sessionSeed;
        }
        
        public int GetMissionSeed()
        {
            unchecked
            {
                var h = SessionSeed * 73856093 ^ TotalMissionNumber * 19349663;
                h ^= h >> 13;
                h *= 1274126177;
                h ^= h >> 16;
                return h;
            }
        }
        
        public SessionStep CompleteMission()
        {
            if (Phase != GamePhase.Mission) return SessionStep.Ignored;
 
            if (MissionInBlock >= RequiredMissionsInBlock)
            {
                Phase = GamePhase.Hub;
                HubVisits++;
                return SessionStep.GoToHub;
            }
 
            MissionInBlock++;
            TotalMissionNumber++;
            return SessionStep.NextMission;
        }
        
        public bool LeaveHub()
        {
            if (Phase != GamePhase.Hub) return false;
 
            BlockNumber++;
            MissionInBlock = 1;
            TotalMissionNumber++;
            Phase = GamePhase.Mission;
            return true;
        }
    }

}