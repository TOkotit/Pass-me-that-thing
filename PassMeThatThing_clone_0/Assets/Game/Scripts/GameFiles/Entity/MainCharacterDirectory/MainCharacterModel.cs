using Entity;
using Game.Scripts.GameFiles.Items;
using VContainer;

namespace Game.Entity
{
    public class MainCharacterModel : DamagableModel
    {
        private PlayerInteraction _playerInteraction;
        private PlayerInventory _playerInventory;
        public PlayerInteraction PlayerInteraction => _playerInteraction;
        public PlayerInventory PlayerInventory => _playerInventory;

        public void SetPlayerInteraction(PlayerInteraction playerInteraction)
        {
            _playerInteraction = playerInteraction;
        }

        public void SetPlayerInventory(PlayerInventory playerInventory)
        {
            _playerInventory = playerInventory;
        }
        
        //Тут будут уникальные для игрока относительно других сущностей свойства
    }
}