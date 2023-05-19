///	© Copyright 2023, Lucas Leonardo Conti - DeadlySmileTM

namespace ParadoxFramework.General.Pool
{
    public interface IPoolObject
    {
        public IPoolObject FactoryMethod();
        public void OnPoolReturn(IPoolObject obj);
        public void DisposeObject(IPoolObject obj);
    }
}
