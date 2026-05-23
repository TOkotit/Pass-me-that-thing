using Mirror;
using UnityEngine;
using System.Linq;

namespace Systems
{
    //Уже не нужен----------------
    // public class NetworkPrefabLoader : MonoBehaviour
    // {
    //     void Awake()
    //     {
    //         var manager = NetworkManager.singleton;
    //         
    //         var prefabs = Resources.LoadAll<GameObject>("Prefabs/Items");
    //
    //         foreach (var prefab in prefabs)
    //         {
    //             if (prefab.GetComponent<NetworkIdentity>() != null)
    //             {
    //                 if (!manager.spawnPrefabs.Contains(prefab))
    //                 {
    //                     manager.spawnPrefabs.Add(prefab);
    //                 }
    //             }
    //         }
    //     }
    // }
}
