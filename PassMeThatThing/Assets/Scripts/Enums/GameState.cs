namespace Systems
{
    
    /// <summary>
    /// Различные состояния игры: запуск, геймплей, пауза, меню...
    /// </summary>
    public enum GameState
    {
        Booting,    
        Gameplay,   
        Paused,    
        GameOver,
        Menu
    }
}