using Game.Scripts.GameFiles.LevelGeneration;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(RoomPlate))]
    public class RoomPlateEditor : UnityEditor.Editor
    {
        private SerializedProperty doorColorProp;
        private SerializedProperty parentGridProp;
        private SerializedProperty parentRoomProp;
        
        private SerializedProperty hasDoorNorthProp;
        private SerializedProperty hasDoorEastProp;
        private SerializedProperty hasDoorSouthProp;
        private SerializedProperty hasDoorWestProp;
        
        private void OnEnable()
        {
            doorColorProp = serializedObject.FindProperty("doorColor");
            parentGridProp = serializedObject.FindProperty("parentGrid");
            parentRoomProp = serializedObject.FindProperty("parentRoom");
            
            hasDoorNorthProp = serializedObject.FindProperty("HasDoorNorth");
            hasDoorEastProp = serializedObject.FindProperty("HasDoorEast");
            hasDoorSouthProp = serializedObject.FindProperty("HasDoorSouth");
            hasDoorWestProp = serializedObject.FindProperty("HasDoorWest");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorGUILayout.PropertyField(doorColorProp);
            EditorGUILayout.PropertyField(parentGridProp);
            EditorGUILayout.PropertyField(parentRoomProp);
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Door Connections", EditorStyles.boldLabel);
            
            DrawAnchorWidget();
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawAnchorWidget()
        {
            const float boxSize = 100f;
            const float buttonSize = 26f;
            
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            var outerRect = GUILayoutUtility.GetRect(boxSize, boxSize, GUILayout.Width(boxSize), GUILayout.Height(boxSize));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            
            var boxBg = EditorGUIUtility.isProSkin ? new Color(0.2f, 0.2f, 0.2f) : new Color(0.8f, 0.8f, 0.8f);
            var borderColor = EditorGUIUtility.isProSkin ? new Color(0.12f, 0.12f, 0.12f) : new Color(0.6f, 0.6f, 0.6f);
            
            
            EditorGUI.DrawRect(outerRect, boxBg);
            Handles.color = borderColor;
            Handles.DrawSolidRectangleWithOutline(outerRect, Color.clear, borderColor);
            
            
            var rectN = new Rect(outerRect.x + (boxSize - buttonSize) / 2f, outerRect.y + 2f, buttonSize, buttonSize);
            var rectS = new Rect(outerRect.x + (boxSize - buttonSize) / 2f, outerRect.yMax - buttonSize - 2f, buttonSize, buttonSize);
            var rectW = new Rect(outerRect.x + 2f, outerRect.y + (boxSize - buttonSize) / 2f, buttonSize, buttonSize);
            var rectE = new Rect(outerRect.xMax - buttonSize - 2f, outerRect.y + (boxSize - buttonSize) / 2f, buttonSize, buttonSize);
            
            var center = outerRect.center;
            Handles.color = Color.gray;
            Handles.DrawLine(new Vector2(rectW.xMax, center.y), new Vector2(rectE.xMin, center.y));
            Handles.DrawLine(new Vector2(center.x, rectN.yMax), new Vector2(center.x, rectS.yMin));
            
            DrawToggleButton(rectN, hasDoorNorthProp, "N");
            DrawToggleButton(rectS, hasDoorSouthProp, "S");
            DrawToggleButton(rectW, hasDoorWestProp, "W");
            DrawToggleButton(rectE, hasDoorEastProp, "E");
        }
        
        private static void DrawToggleButton(Rect rect, SerializedProperty prop, string label)
        {
            
            var originalColor = GUI.backgroundColor;
            
            GUI.backgroundColor = prop.boolValue ? new Color(0.4f, 0.8f, 0.4f) : new Color(0.6f, 0.6f, 0.6f);

            if (GUI.Button(rect, label))
            {
                prop.boolValue = !prop.boolValue;
            }

            GUI.backgroundColor = originalColor;
        }
    }
}