using Assets.Game.Scripts.GameFiles.GlobalStageManager;
using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VContainer;

namespace Game.Scripts.GameFiles.GameRandomEvents.GameTasks.Quota
{
    public class QuotaManager : NetworkBehaviour
    {
        [Inject] private GlobalStageDatabase _globalStageDatabase;

        //квота
        [SyncVar(hook = nameof(OnCurrentQuotaChanged))]
        private int _currentStageQuota;

        [SyncVar(hook = nameof(OnRequiredQuotaChanged))]
        private int _requiredStageQuota;

        public int CurrentStageQuota => _currentStageQuota;
        public int RequiredStageQuota => _requiredStageQuota;

        public event Action<int, int> OnQuotaChanged;

        [Server]
        public void GetQuota(int level)
        {
            //_currentStageQuota = 0;
            _requiredStageQuota = _globalStageDatabase.GetQuotaSumByLevel(level);
        }

        [Server]
        public void AddQuota(int toAdd)
        {
            _currentStageQuota += toAdd;
        }

        [Server]
        public bool CheckQuota()
        {
            return _currentStageQuota >= _requiredStageQuota;
        }

        private void OnCurrentQuotaChanged(int oldQuota, int newQuota)
        {
            OnQuotaChanged?.Invoke(_currentStageQuota, _requiredStageQuota);
        }

        private void OnRequiredQuotaChanged(int oldReqQuota, int newReqQuota)
        {
            OnQuotaChanged?.Invoke(_currentStageQuota, _requiredStageQuota);
        }
    }
}
