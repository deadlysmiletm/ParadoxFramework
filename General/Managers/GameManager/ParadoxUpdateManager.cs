namespace ParadoxFramework.General.Managers
{
    public sealed class ParadoxUpdateManager : ParadoxManagerGeneric<IUpdateManaged>
    {
        private void Update()
        {
            _currentNode = _managedUpdates.First;
            for (int i = 0; i < _managedUpdates.Count; i++)
            {
                _currentNode.Value.ManagedUpdate();
                _currentNode = _currentNode.Next;
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            ParadoxGameManager.Instance.DisposeUpdate();
        }
    }
}