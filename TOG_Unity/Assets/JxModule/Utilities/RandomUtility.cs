using System;
using System.Collections.Generic;
using UnityEngine;

using Random = UnityEngine.Random;

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

        public static T GetWeightedRandom<T>(IList<T> list, Func<T, float> weightSelector)
        {
            return TryGetWeightedRandom(list, weightSelector, out var result) ? result : default;
        }

        public static bool TryGetWeightedRandom<T>(IList<T> list, Func<T, float> weightSelector, out T result)
        {
            result = default;

            if (list == null || list.Count == 0 || weightSelector == null)
            {
                return false;
            }

            var totalWeight = 0f;
            foreach (var item in list)
            {
                totalWeight += Mathf.Max(0f, weightSelector(item));
            }

            if (totalWeight <= 0f)
            {
                return false;
            }

            var randomValue = Random.Range(0f, totalWeight);
            var currentWeight = 0f;

            foreach (var item in list)
            {
                currentWeight += Mathf.Max(0f, weightSelector(item));

                if (randomValue > currentWeight)
                {
                    continue;
                }
                
                result = item;
                return true;
            }

            result = list[^1];
            return true;
        }

        public static T GetSequentialRandom<T>(IList<T> list, Func<T, float> probabilitySelector, T fallback = default)
        {
            return TryGetSequentialRandom(list, probabilitySelector, out var result) ? result : fallback;
        }

        public static bool TryGetSequentialRandom<T>(IList<T> list, Func<T, float> probabilitySelector, out T result)
        {
            result = default;

            if (list == null || list.Count == 0 || probabilitySelector == null)
            {
                return false;
            }

            foreach (var item in list)
            {
                var probability = Mathf.Clamp01(probabilitySelector(item));

                if (Chance(probability))
                {
                    result = item;
                    return true;
                }
            }

            return false;
        }
    }
}