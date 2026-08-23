#if UNITY_EDITOR
using Assets.Game.Scripts.GameFiles.GameRoot;
using UnityEditor;
using Mirror;
using UnityEngine;

public class AutoRegisterPrefabs
{
    [MenuItem("Tools/Mirror/Auto Register Prefabs")]
    public static void RegisterPrefabs()
    {
        var selectedObject = Selection.activeGameObject;

        if (selectedObject == null)
        {
            Debug.LogError("Ошибка: Выделите объект с компонентом NetworkManager в окне Hierarchy или Project.");
            return;
        }

        var manager = selectedObject.GetComponent<CustomNetworkRoomManager>();

        if (manager == null)
        {
            Debug.LogError($"Ошибка: На объекте {selectedObject.name} нет компонента NetworkManager.");
            return;
        }
        
        var searchFolders = new[] { "Assets/Game/Prefabs/Rooms/Rooms Prefabs" };
        var guids = AssetDatabase.FindAssets("t:GameObject", searchFolders);
        
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            
            if (prefab != null && prefab.GetComponent<NetworkIdentity>() != null)
            {
                if (!manager.spawnPrefabs.Contains(prefab))
                {
                    manager.spawnPrefabs.Add(prefab);
                }
            }
        }
        
        EditorUtility.SetDirty(manager);
        AssetDatabase.SaveAssets();
        
        Debug.Log($"Готово. Префабы зарегистрированы в NetworkManager на объекте {selectedObject.name}.");
    }
}
#endif