using System.Collections.Generic;
using UnityEngine;

namespace JxModule
{
    public static class RandomUtility
    {
        public static bool Chance(float probability)
        {
            return Random.value < Mathf.Clamp01(probability);
        }

        public static int Sign()
        {
            return Random.value < 0.5f ? -1 : 1;
        }

        public static T GetRandom<T>(IList<T> list)
        {
            if (list == null || list.Count == 0)
            {
                return default;
            }

            return list[Random.Range(0, list.Count)];
        }

        public static void Shuffle<T>(IList<T> list)
        {
            for (var i = 0; i < list.Count; i++)
            {
                var randomIndex = Random.Range(i, list.Count);
                (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
            }
        }
    }
}