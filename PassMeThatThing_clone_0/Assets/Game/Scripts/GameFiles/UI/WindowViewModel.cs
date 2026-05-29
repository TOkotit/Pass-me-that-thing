using System;
using R3;

namespace Game.UI
{
    /// <summary>
    /// Абстрактный класс описывающий определенное окно ui
    /// </summary>
    public abstract class WindowViewModel : IDisposable
    {
        public Observable<WindowViewModel> CloseRequested => _closeRequested;
        public abstract string Id { get; }
        
        private readonly Subject<WindowViewModel> _closeRequested = new();

        public void RequestClose()
        {
            _closeRequested.OnNext(this);
        }

        public void Dispose()
        {
            
        }
    }
}