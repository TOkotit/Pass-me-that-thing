using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Game.Scripts.GameFiles.Entity.Buildings.Misc
{
    public class ResourceDepotView : MonoBehaviour
    {
        Sequence s;

        public void PlayRecycleAnimation()
        {
            s = DOTween.Sequence();
            s.Append(transform.DOScaleY(1.3f, 0.2f).From(1f));
            s.Append(transform.DOScaleY(1, 0.2f).From(1.3f));
            s.SetEase(Ease.InBounce);
        }
    }
}
