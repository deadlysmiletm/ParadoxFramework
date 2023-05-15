///	© Copyright 2023, Lucas Leonardo Conti - DeadlySmileTM

using System;

namespace ParadoxFramework.General.Pool.ErrorHandler
{
    internal enum EPoolExceptions
    {
        None,
        PoolDontExistException,
        PoolIsEmptyException,
        InstanceCreationException,
    }

    internal static class PoolErrorHandler
    {
        public static void ThrowError(EPoolExceptions exception, string className, string actionName, string poolName)
        {
            throw new Exception(GetExceptionMessage(exception, className, actionName, poolName));
        }

        private static string GetExceptionMessage(EPoolExceptions exception, string className, string actionName, string poolName)
        {
            return exception switch
            {
                EPoolExceptions.PoolDontExistException => $"{className}: You're trying to {actionName} on a pool that don't exist. Pool name: {poolName}",
                EPoolExceptions.InstanceCreationException => $"{className}: There was an error creation an instance for {poolName}. Mesage: {actionName}",
                _ => String.Empty,
            };
        }
    }
}
