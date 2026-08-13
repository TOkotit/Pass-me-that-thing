using System;
using R3;

namespace Game.UI
{
    /// <summary>
    /// Абстрактный класс описывающий определенное окно ui
    /// </summary>
    public abstract class WindowViewModel : IDisposable
    {
        private readonly Subject<WindowViewModel> _closeRequested = new();
        public Observable<WindowViewModel> CloseRequested => _closeRequested;
        public abstract string Id { get; }

        public void RequestClose()
        {
            _closeRequested.OnNext(this);
        }

        public virtual void Dispose()
        {
            
        }
    }
}