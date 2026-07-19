using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Scripts.Utils
{
    public static class RandomUtilities
    {
        public static int RandomWeightedIndex(float[] weights, float totalWeight)
        {
            float value = Random.Range(0f, totalWeight);
            for (int i = 0; i < weights.Length; i++)
            {
                value -= weights[i];
                if (value <= 0f) return i;
            }
            return weights.Length - 1;
        }

        public static T RandomWeighted<T>(IDictionary<T, float> weightedItems)
        {
            float total = weightedItems.Values.Sum();
            float value = Random.Range(0f, total);
            foreach (var kvp in weightedItems)
            {
                value -= kvp.Value;
                if (value <= 0f) return kvp.Key;
            }
            return weightedItems.Keys.Last();
        }

        public static void Shuffle<T>(IList<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
        
        public static int RandomWeightedByParameter(int parameter, int maxParameter, int maxIndex, float baseWeight = 1f, float falloff = 0.1f)
        {
            var targetIndex = 0;

            var t = Mathf.Clamp01((float)parameter / maxParameter);
            targetIndex = Mathf.RoundToInt(Mathf.Lerp(0, maxIndex, t));

            var weights = new float[maxIndex + 1];
            for (int i = 0; i <= maxIndex; i++)
            {
                if (i <= targetIndex)
                    weights[i] = (targetIndex - i + 1) * baseWeight;
                else
                    weights[i] = baseWeight * Mathf.Pow(falloff, i - targetIndex);
            }

            var totalWeight = weights.Sum();
            return RandomWeightedIndex(weights, totalWeight);
        }
    }
}