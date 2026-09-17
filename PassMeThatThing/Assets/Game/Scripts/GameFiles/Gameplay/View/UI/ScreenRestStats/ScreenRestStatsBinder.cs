
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine;
using Game.UI;
using Game.Scripts.Enums;
using UnityEngine.InputSystem;
using Game.Scripts.GameFiles.GlobalStageManager;

namespace Game.Gameplay.View.UI
{
    public class ScreenRestStatsBinder : WindowBinder<ScreenRestStatsViewModel>
    {
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] private VisualTreeAsset statIconPrefab;

        [SerializeField] private InputActionReference closeRef;

        private VisualElement _root;

        private VisualElement _resourceContainer;
        private VisualElement _enemyContainer;
        private VisualElement _eventsContainer;

        private Label _closeLabel;
        private Label _gameGlobalStateText;
        private Label _gameGlobalStateTimerText;

        private void Awake()
        {
            _root = uiDocument.rootVisualElement;
            _resourceContainer = _root.Q<VisualElement>("ResourceContainer");
            _enemyContainer = _root.Q<VisualElement>("EnemyContainer");
            _eventsContainer = _root.Q<VisualElement>("EventsContainer");

            _closeLabel = _root.Q<Label>("CloseLabel");
            _gameGlobalStateText = _root.Q<Label>("PhaseLb");
            _gameGlobalStateTimerText = _root.Q<Label>("RemainingTimeLb");
        }

        private void Start()
        {
            UpdateCloseText();
            ViewModel.RequestInitRecievedRes(UpdateResources);
            ViewModel.RequestInitKilledEnemies(UpdateEnemies);
            ViewModel.RequestInitFixedEvents(UpdateEvents);


            ViewModel.RequestInitGlobalState(UpdateGameGlobalState);
            ViewModel.RequestSubGlobalState(UpdateGameGlobalState);

            ViewModel.RequestSubGlobalStateTimer(UpdateGameGlobalStateTimer);
        }

        private void OnDestroy()
        {
            ViewModel.RequestUnsubGlobalState(UpdateGameGlobalState);

            ViewModel.RequestUnsubGlobalStateTimer(UpdateGameGlobalStateTimer);
        }

        private void UpdateResources(IReadOnlyDictionary<Resource, float> res)
        {
            _resourceContainer.Clear();

            foreach (var r in res)
            {
                var e = statIconPrefab.Instantiate();
                _resourceContainer.Add(e);

                e.Q<VisualElement>("Image").style.backgroundImage
                    = new StyleBackground(ViewModel.resourceDatabase.GetResource(r.Key).resourceImage);
                e.Q<Label>("Label").text = r.Value.ToString();
            }
        }

        private void UpdateEnemies(IReadOnlyDictionary<string, int> enemies)
        {
            _enemyContainer.Clear();

            foreach (var enemy in enemies)
            {
                var e = statIconPrefab.Instantiate();
                _enemyContainer.Add(e);

                e.Q<VisualElement>("Image").style.backgroundImage
                    = new StyleBackground(ViewModel.enemyDatabase.GetEnemy(enemy.Key).EnemyImage);
                e.Q<Label>("Label").text = enemy.Value.ToString();
            }
        }

        private void UpdateEvents(IReadOnlyDictionary<GameEventsType, int> events)
        {
            _eventsContainer.Clear();

            foreach (var ev in events)
            {
                var e = statIconPrefab.Instantiate();
                _eventsContainer.Add(e);

                e.Q<VisualElement>("Image").style.backgroundImage
                    = new StyleBackground(ViewModel.gameEventsDatabase.GetEvent(ev.Key).EventImage);
                e.Q<Label>("Label").text = ev.Value.ToString();
            }
        }

        private void UpdateCloseText()
        {
            var bindString = "[" + InputControlPath.ToHumanReadableString(
                        closeRef.action.bindings[0].effectivePath,
                        InputControlPath.HumanReadableStringOptions.OmitDevice) + "]";
            _closeLabel.text = bindString + "Close";
        }

        private void UpdateGameGlobalState(Stage newValue)
        {
            _gameGlobalStateText.text = newValue.Type switch
            {
                GlobalStagesType.Fight => "Фаза обороны",
                GlobalStagesType.Preparation => "Фаза подготовки",
                GlobalStagesType.Rest => "Отдых",
                _ => "Неизвестная фаза"
            };
        }

        private void UpdateGameGlobalStateTimer(float remainingSeconds)
        {
            var minutes = Mathf.FloorToInt(remainingSeconds / 60f);
            var seconds = Mathf.FloorToInt(remainingSeconds % 60f);

            _gameGlobalStateTimerText.text = $"{minutes:00}:{seconds:00}";
        }
    }
}
