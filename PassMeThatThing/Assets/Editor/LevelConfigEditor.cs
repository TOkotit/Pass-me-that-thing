using Game.Scripts.GameFiles.LevelGeneration.Graph;
using UnityEditor;
using UnityEngine;

namespace Editor
{
   [CustomEditor(typeof(LevelConfig))]
    public class LevelConfigEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawHeader("Настройки генерации уровня", new Color(0.2f, 0.6f, 0.8f));

            // Seed Settings
            BeginBox("Сид и Сложность");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("useRandomSeed"));
            if (!serializedObject.FindProperty("useRandomSeed").boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("customSeed"));
            }
            EditorGUILayout.PropertyField(serializedObject.FindProperty("difficulty"));
            EndBox();

            // Limits
            BeginBox("Ограничения комнат и кластеров");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("minRooms"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("maxRooms"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("absoluteMinRooms"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("maxConnectionsPerRoom"));
            EditorGUILayout.Space(4);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("minClusterSize"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("maxClusterSize"));
            EndBox();

            // Core Rooms
            BeginBox("Базовый кластер (Ядро)");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("coreRooms"), true);
            EndBox();

            // Mandatory Rooms
            BeginBox("Обязательные комнаты (минимум 1 на уровень)");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("mandatoryRooms"), true);
            EndBox();

            // Cluster Pool
            BeginBox("Пул комнат для кластеров");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("clusterRoomSettings"), true);
            EndBox();

            // Level Constraints
            BeginBox("Строгие ограничения на уровень (Операторы)");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("levelConstraints"), true);
            EndBox();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawHeader(string title, Color color)
        {
            GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 13,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };

            Rect rect = EditorGUILayout.GetControlRect(false, 26);
            EditorGUI.DrawRect(rect, color);
            EditorGUI.LabelField(rect, title, headerStyle);
            EditorGUILayout.Space(4);
        }

        private void BeginBox(string label)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.Space(2);
            if (!string.IsNullOrEmpty(label))
            {
                EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
                EditorGUILayout.Space(2);
            }
        }

        private void EndBox()
        {
            EditorGUILayout.Space(2);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(4);
        }
    }
}