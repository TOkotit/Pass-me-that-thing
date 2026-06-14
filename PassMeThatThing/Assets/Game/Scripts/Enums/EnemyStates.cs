namespace Game.Scripts.Enums
{
    /// <summary>
    /// Обобщенные фазы поведения для мобов
    /// </summary>
    public enum EnemyStates
    {
        Walk, //моб идет в сторону бункера
        Chase, 
        Attack,
        Death,
        Knockout
    }
}