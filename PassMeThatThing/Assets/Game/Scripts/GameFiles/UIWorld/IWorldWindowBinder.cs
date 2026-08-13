using Game.UI;
using System.Collections;
using UnityEngine;

namespace Assets.Game.Scripts.GameFiles.UIWorld
{
    public interface IWorldWindowBinder 
    {
        void Bind(WorldWindowViewModel viewModel);
        void Close();
    }
}