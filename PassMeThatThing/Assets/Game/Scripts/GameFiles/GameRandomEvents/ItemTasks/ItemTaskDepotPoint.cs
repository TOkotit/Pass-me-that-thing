using Assets.Game.Scripts.GameFiles.GameRandomEvents.GameTasks;
using Assets.Game.Scripts.GameFiles.GlobalStageManager;
using Game.Scripts.Enums;
using Game.Scripts.GameFiles.GameRandomEvents;
using Game.Scripts.GameFiles.Items;
using Game.Scripts.GameFiles.Items.ItemPhysics;
using Mirror;

using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using VContainer;


namespace Assets.Game.Scripts.GameFiles.GameRandomEvents.ItemTasks
{
    public class ItemGameTask
    {
        public string ItemId;
        public GameTask GameTask;

        public ItemGameTask(string itemId, GameTask gameTask)
        {
            ItemId = itemId;
            GameTask = gameTask;
        }
    }

    public class ItemTaskDepotPoint : NetworkBehaviour
    {
        [SerializeField] private ItemTaskDepotPointView view;
        [SerializeField] private GameTaskHandler taskHandler;
        [SerializeField] private float taskTimeLimit = 30;
        [SerializeField] private Outline outlineComponent;
        [Inject] private PhysicalItemRegistry registry;
        [Inject] private ItemPoolManager _itemPoolManager;
        [Inject] private GlobalStageDatabase _globalStageDatabase;
        [Inject] private GameRandomEventManager _randomEventManager;

        private int _itemTaskDepotId;

        
        [SyncVar(hook = nameof(OnHasActiveTaskChanged))]
        private bool _hasActiveTask;
        
        //taskId, (itemId, gameTask)
        private Dictionary<int, ItemGameTask> _requiredItems = new();


        [Server]
        public void CreateItemTask(string itemId)
        {

            var p = new GameTaskParameters();

            p.gameTaskType = GameTaskType.Item;
            p.timeLimit = taskTimeLimit;
            p.taskField = itemId;
            p.cost = _globalStageDatabase.BaseQuotaConst;

            var task = taskHandler.CreateGameTask(p);

            task.OnTaskTimerEnd += OnTaskTimerEnd;

            _requiredItems.Add(task.gameTaskId, new ItemGameTask(itemId, task));
            
            UpdateOutlineState();
        }

        private void OnTaskTimerEnd(int taskId)
        {
            var task = _requiredItems[taskId].GameTask;

            task.OnTaskTimerEnd -= OnTaskTimerEnd;

            _requiredItems.Remove(taskId);

            _randomEventManager.DestroyGameTask(taskId);
            
            UpdateOutlineState();
        }

        [Server]
        public void CompleteAndDestroyItemTask(string itemId)
        {
            var itemTask = _requiredItems.Values.FirstOrDefault(x => x.ItemId == itemId);

            if (itemTask == null) return;
            var task = itemTask.GameTask;

            task.OnTaskTimerEnd -= OnTaskTimerEnd;

            _requiredItems.Remove(task.gameTaskId);

            _randomEventManager.CompleteAndDestroyGameTask(task.gameTaskId);
            
            UpdateOutlineState();

        }
        [Server]
        private void UpdateOutlineState()
        {
            _hasActiveTask = _requiredItems.Count > 0;
        }
        
        private void OnHasActiveTaskChanged(bool oldValue, bool newValue)
        {
            if (outlineComponent)
            {
                outlineComponent.enabled = newValue;
            }
        }

        [Server]
        public override void OnStartServer()
        {
            base.OnStartServer();
            RegisterItemTaskDepot();
        }

        [Server]
        public override void OnStopServer()
        {
            base.OnStartServer();
            UnRegisterItemTaskDepot();
        }

        [Server]
        private void RegisterItemTaskDepot()
        {
            if (_randomEventManager != null)
            {
                _itemTaskDepotId = _randomEventManager.RegisterSceneItemTaskDepot(this);
            }
        }

        [Server]
        private void UnRegisterItemTaskDepot()
        {
            if (_randomEventManager != null)
            {
                _randomEventManager.UnregisterSceneItemTaskDepot(_itemTaskDepotId);
            }
        }




        public void OnTriggerEnter(Collider other)
        {
            if (!isServer) return;
            Debug.Log("В приемник что-то попало");

            if (other.CompareTag("Item"))
            {
                if (!registry.TryGetItem(other.gameObject, out var item)) return;
                if (!_requiredItems.Any(x => x.Value.ItemId == item.Network.itemId)) return;


                if (item.Owner)
                {
                    var inv = item.Owner.MainCharacterModel.PlayerInventory;
                    if (inv) inv.ServerRemoveItemFromOwner(item);
                }

                CompleteAndDestroyItemTask(item.Network.itemId);

                item.Owner?.MainCharacterModel.PlayerInteraction
                    .PhysicalItemInteractionController.ReleaseCurrentItem(0f, false);
                _itemPoolManager.DeleteAndDestroyObject(item.Network);

                RpcPlayAnimation();
            }
        }

        [ClientRpc]
        public void RpcPlayAnimation()
        {
            view.PlayRecycleAnimation();
        }
    }
}
