using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = System.Random;

namespace Game.Gameplay.View.UI.ScreenMinigame
{
    public class UIConnectionManager : MonoBehaviour
    {
        [SerializeField] private UIConnectionLine linePrefab;
        [SerializeField] private Transform linesContainer;
        
        [SerializeField] private List<Color> nodeColors;
        
        [SerializeField] private GameObject inputNodesLeftContainer;
        [SerializeField] private GameObject inputNodesRightContainer;
        [SerializeField] private GameObject outputNodesContainer;
        
        private List<UIConnectionLineNode> inputNodesLeft = new();
        private List<UIConnectionLineNode> inputNodesRight = new();
        private List<UIConnectionLineNode> outputNodes = new();
        
        private UIConnectionLineNode _selectedSocket;
        private List<NodePair> _activeConnections = new();
        
        public event Action<int> OnActiveConnectionsCountChanged;

        private void Start()
        {
            inputNodesLeft = inputNodesLeftContainer.GetComponentsInChildren<UIConnectionLineNode>().ToList();
            inputNodesRight = inputNodesRightContainer.GetComponentsInChildren<UIConnectionLineNode>().ToList();
            outputNodes = outputNodesContainer.GetComponentsInChildren<UIConnectionLineNode>().ToList();

            var rand = new Random();
            var shuffledColors = nodeColors.OrderBy(x => rand.Next()).ToList();
            
            for (var i = 0; i < shuffledColors.Count && i < inputNodesLeft.Count; i++)
            {
                inputNodesLeft[i].OnConnectionNodeClicked += OnSocketClicked;
                inputNodesLeft[i].SetupNode(shuffledColors[i]);
            }
            
            shuffledColors = nodeColors.OrderBy(x => rand.Next()).ToList();
            
            for (var i = 0; i < shuffledColors.Count && i < inputNodesRight.Count; i++)
            {
                inputNodesRight[i].OnConnectionNodeClicked += OnSocketClicked;
                inputNodesRight[i].SetupNode(shuffledColors[i]);
            }
            
            shuffledColors = nodeColors.OrderBy(x => rand.Next()).ToList();
            
            for (var i = 0; i < shuffledColors.Count && i < outputNodes.Count; i++)
            {
                outputNodes[i].OnConnectionNodeClicked += OnSocketClicked;
                outputNodes[i].SetupNode(shuffledColors[i]);
            }
        }
        
        private void OnDestroy()
        {
            foreach (var node in inputNodesLeft)
            {
                node.OnConnectionNodeClicked -= OnSocketClicked;
            }
            
            foreach (var node in inputNodesRight)
            {
                node.OnConnectionNodeClicked -= OnSocketClicked;
            }
            
            foreach (var node in outputNodes)
            {
                node.OnConnectionNodeClicked -= OnSocketClicked;
            }
        }

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
                Debug.LogWarning("[UI] разные типы параметров нод");
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
            
            newLine.SetColor(s1.NodeColor);

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