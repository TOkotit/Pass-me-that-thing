namespace Game.UI
{
    /// <summary>
    /// Интерфейс для монобехов, которые прикрепляются к окнам(объектам) ui
    /// </summary>
    public interface IWindowBinder
    {
        void Bind(WindowViewModel viewModel);
        void Close();
    }
}