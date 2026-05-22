using Entity;
using Game.Scripts.GameFiles.Items;
using VContainer;

namespace Game.Scripts.GameFiles.Entity
{
    public class MainCharacterModel : DamagableModel
    {
        private PlayerInteraction _playerInteraction;
        public PlayerInteraction PlayerInteraction => _playerInteraction;

        public void SetPlayerInteraction(PlayerInteraction playerInteraction)
        {
            _playerInteraction = playerInteraction;
        }
        
        //Тут будут уникальные для игрока относительно других сущностей свойства
    }
}