namespace Game.Scripts.Enums
{
    public enum GameEventsType
    {
        //Наводнение
        FloodBrokenPump, //сломанный насос
        FloodPipeBreak, //прорыв трубы
        
        //Отключение света
        BlackoutBlowFuse, //вышибло пробки
        BlackoutCutWires, //перерезанные провода
        
        OtherEvent
    }
}