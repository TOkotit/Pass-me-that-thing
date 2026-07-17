using System.Collections;
using System.Collections.Generic;
using Game.Entity;
using Mirror;
using Systems;
using Unity.VisualScripting;
using UnityEngine;
using Utils;
using VContainer;

namespace Game.Scripts.Systems
{
    public class GameoverHandler
    {
        [Inject] GameManager gameManager;
        [Inject] private ICoroutineRunner coroutineRunner;
        private int deathTimer = 10;
        private List<MainCharacter> players = new List<MainCharacter>();

        public void RegisterPlayer(MainCharacter player)
        {
            if (!players.Contains(player))
                players.Add(player);
        }

        public void CheckForGameOver()
        {
            foreach (var player in players)
            {
                if (player.IsAlive)
                    return; 
            }
            coroutineRunner.StartRoutine(Gameover(deathTimer));
        }

        private IEnumerator Gameover(int falloutTimer)
        {
            Debug.Log($"Game over in {falloutTimer} seconds!");
            yield return new WaitForSeconds(1f);
            foreach (var player in players)
            {
                if (player.IsAlive) yield break;
            }
            if (falloutTimer > 1)
            {
                coroutineRunner.StartRoutine(Gameover(falloutTimer - 1));
            }
            else
            {
                Debug.Log("Game over!");
                // конец игры
                gameManager.SetState(GameState.GameOver);
                //сделать ui или выход со сцены
            }
        }
    }
}