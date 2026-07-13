namespace Game.Scripts.Enums
{
    public enum GameEventsType
    {
        None = 0, // заглушка если комната не ивентовая
        
        
        //Наводнение
        FloodBrokenPump, //сломанный насос
        FloodPipeBreak, //прорыв трубы
        
        //Отключение света
        BlackoutBlowFuse, //вышибло пробки
        BlackoutCutWires, //перерезанные провода
    }
}