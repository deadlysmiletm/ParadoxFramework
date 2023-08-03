using System.Collections;
using UnityEngine;

namespace ParadoxFramework.General.Managers
{
    public interface IGameState
    {
        void Enter(IGameState enterState);
        
    }
}