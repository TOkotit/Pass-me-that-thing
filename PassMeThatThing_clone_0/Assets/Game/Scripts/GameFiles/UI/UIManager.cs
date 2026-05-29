using DI;
using VContainer;

namespace Game.UI
{
    /// <summary>
    /// Класс который наследуется в другие UIManager для конкретных сцен
    /// нужен для переключения окон внутри сцены
    /// </summary>
    public abstract class UIManager
    {
        // чтобы вытаскивать барахло, чтобы собирать вьюмодели окошек
        protected readonly IObjectResolver Container; 
        
        protected UIManager(IObjectResolver container)
        {
            Container = container;
        }
    }
}