namespace ParadoxFramework.General.Managers
{
    public class ParadoxFixedUpdateManager : ParadoxManagerGeneric<IFixedUpdateManaged>
    {
        private void LateUpdate()
        {
            _currentNode = _managedUpdates.First;
            for (int i = 0; i < _managedUpdates.Count; i++)
            {
                _currentNode.Value.ManagedFixedUpdate();
                _currentNode = _currentNode.Next;
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            ParadoxGameManager.Instance.DisposeFixedUpdate();
        }
    }
}