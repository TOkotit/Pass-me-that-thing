using System.Collections;
using UnityEngine;

namespace Assets.Game.Scripts.GameFiles.Entity.Buildings.WireSystem
{
    public class WireLineView : MonoBehaviour
    {
        [SerializeField] public GameObject plug1;
        [SerializeField] public GameObject plug2;

        [SerializeField] public Transform wirePoint1;
        [SerializeField] public Transform wirePoint2;

        [SerializeField] public LineRenderer lineRenderer;
    }
}