using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Gameplay.View.UI.ScreenMinigame
{
    public class UIConnectionManager : MonoBehaviour
    {
        [SerializeField] private UIConnectionLine linePrefab;
        [SerializeField] private Transform linesContainer;

        [SerializeField] private List<UIConnectionLineNode> inputNodes = new();
        [SerializeField] private List<UIConnectionLineNode> outputNodes = new();
        
        [SerializeField] private List<Color> nodeColors;
        
        private UIConnectionLineNode _selectedSocket;
        private List<NodePair> _activeConnections = new();
        
        public event Action<int> OnActiveConnectionsCountChanged;
        
        public void OnSocketClicked(UIConnectionLineNode socket)
        {
            if (_selectedSocket == null)
            {
                _selectedSocket = socket;
                return;
            }

            if (_selectedSocket == socket)
            {
                _selectedSocket = null;
                return;
            }

            if (ValidateConnection(_selectedSocket, socket))
            {
                CreateConnection(_selectedSocket, socket);
            }

            _selectedSocket = null;
        }

        private bool ValidateConnection(UIConnectionLineNode s1, UIConnectionLineNode s2)
        {
            if (s1.IsInput == s2.IsInput) return false;
            
            if (s1.ParameterGroup != s2.ParameterGroup)
            {
                Debug.LogWarning("Разные типы параметров! Соединение невозможно.");
                return false;
            }

            return true;
        }

        private void CreateConnection(UIConnectionLineNode s1, UIConnectionLineNode s2)
        {
            var output = s1.IsInput ? s2 : s1;
            var input = s1.IsInput ? s1 : s2;

            var newLine = Instantiate(linePrefab, linesContainer);
            newLine.Setup(output.RectTransform, input.RectTransform);

            var modifier = 1.5f;
            
            if (output.ParameterGroup == "Power")
            {
                input.CurrentValue = output.CurrentValue * modifier;
                newLine.SetColor(Color.green);
            }
            else
            {
                input.CurrentValue = output.CurrentValue;
                newLine.SetColor(Color.blue);
            }

            input.UpdateValueVisual();
            output.UpdateValueVisual();

            _activeConnections.Add(new NodePair(output, input, newLine));
            OnActiveConnectionsCountChanged?.Invoke(_activeConnections.Count);
        }
    }

    public class NodePair
    {
        public UIConnectionLineNode Output;
        public UIConnectionLineNode Input;
        public UIConnectionLine Line;

        public NodePair(UIConnectionLineNode outNode, UIConnectionLineNode inNode, UIConnectionLine line)
        {
            Output = outNode;
            Input = inNode;
            Line = line;
        }
    }
}