namespace Game.Scripts.GameFiles.Entity.Buildings.WireSystem
{
    public interface IDependsOnWireNet
    {
        public void OnWireNetWorkingStateChanged(WireType type, bool isNetWorking);
    }
}