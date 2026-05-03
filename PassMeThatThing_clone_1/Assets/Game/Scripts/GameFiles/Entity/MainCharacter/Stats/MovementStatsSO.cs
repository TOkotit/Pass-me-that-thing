using UnityEngine;

namespace MainCharacter
{
    
    // Пока просто создал заранее чтобы было. Конкретно этот файл брать не надо
    // Данные нужно брать из отдельного конфига в ресурсах через ResourcesLoad. Это делать будем в контейнере
    // Там автоматически сохраняются данные если их менять и эти данные подтягиваются в runTime
    // то есть можно балансить прям во время запуска без проблем
    // Для создания конфига нужно указать названия вот эти файлов и в Create будет доступно создание конфига
    
    // RE: Bruh
    
    [CreateAssetMenu(fileName = "MovementStats", menuName = "MainCharacter/MovementStatsSO")]
    public class MovementStatsSO : ScriptableObject
    {
        [SerializeField] private float _speed = 10;
        [SerializeField] private float _jumpHeight = 7;
        [SerializeField] private int _wallJumpCost = 10;
        [SerializeField] private int _dashCost = 20;
        [SerializeField] private float _dashSpeed = 50;
        [SerializeField] private float _dashTime = 0.1f;
        [SerializeField] private float _dashCooldown = 1f;
        [SerializeField] private int _wallJumpCount = 1;
        
        public float Speed => _speed;
        public float JumpHeight => _jumpHeight;
        public int WallJumpCost => _wallJumpCost;
        public int DashCost => _dashCost;
        public float DashSpeed => _dashSpeed;
        public float DashTime => _dashTime;
        public float DashCooldown => _dashCooldown;
        public int WallJumpCount => _wallJumpCount;
    }
}