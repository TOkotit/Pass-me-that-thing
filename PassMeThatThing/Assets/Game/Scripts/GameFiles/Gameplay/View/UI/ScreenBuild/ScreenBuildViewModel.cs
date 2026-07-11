using System;
using System.Collections.Generic;
using System.Linq;
using Game.UI;
using UnityEngine;
using VContainer;

namespace Game.Gameplay.View.UI.ScreenBuild
{
    public class ScreenBuildViewModel : WindowViewModel
    {
        private BuildingsDatabase _buildingsDatabase;
        public override string Id => "ScreenBuild";


        public ScreenBuildViewModel(GameplayUIManager uiManager, IObjectResolver container)
        {
            _buildingsDatabase =  container.Resolve<BuildingsDatabase>();
        }

        public void RequestSetSprites(Action<List<Sprite>> c)
        {
            c(_buildingsDatabase.allBuildings.Select(b => b.buildingImage).ToList());
        }
    }
}