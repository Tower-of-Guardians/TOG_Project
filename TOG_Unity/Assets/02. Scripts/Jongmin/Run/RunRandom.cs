using System;
using System.Collections.Generic;

namespace Jongmin
{
    public static class RunRandom
    {
        public const string CardDraw = "card-draw";
        public const string EnemyAction = "enemy-action";
        public const string Reward = "reward";
        public const string Shop = "shop";
        public const string Event = "event";
        public const string Area = "area";

        private static int _runSeed;
        private static readonly Dictionary<string, int> ConsumedByStream = new();

        public static bool IsInitialized { get; private set; }

        public static void Initialize(int seed, IEnumerable<RandomStreamSaveData> streams = null)
        {
            _runSeed = seed;
            ConsumedByStream.Clear();
            if (streams != null)
            {
                foreach (var stream in streams)
                {
                    if (stream != null && !string.IsNullOrWhiteSpace(stream.streamID))
                    {
                        ConsumedByStream[stream.streamID] = Math.Max(0, stream.consumed);
                    }
                }
            }
            IsInitialized = true;
        }

        public static int Range(string streamID, int minInclusive, int maxExclusive)
        {
            if (maxExclusive <= minInclusive)
            {
                return minInclusive;
            }
            var random = CreateAtCurrentPosition(streamID);
            var result = random.Next(minInclusive, maxExclusive);
            ConsumedByStream[Normalize(streamID)] = GetConsumed(streamID) + 1;
            return result;
        }

        public static float Range(string streamID, float minInclusive, float maxInclusive)
        {
            if (maxInclusive <= minInclusive)
            {
                return minInclusive;
            }
            var random = CreateAtCurrentPosition(streamID);
            var result = minInclusive + (float)random.NextDouble() * (maxInclusive - minInclusive);
            ConsumedByStream[Normalize(streamID)] = GetConsumed(streamID) + 1;
            return result;
        }

        public static List<RandomStreamSaveData> Capture()
        {
            var result = new List<RandomStreamSaveData>(ConsumedByStream.Count);
            foreach (var pair in ConsumedByStream)
            {
                result.Add(new RandomStreamSaveData { streamID = pair.Key, consumed = pair.Value });
            }
            return result;
        }

        private static Random CreateAtCurrentPosition(string streamID)
        {
            if (!IsInitialized)
            {
                Initialize(Environment.TickCount);
            }
            var normalized = Normalize(streamID);
            var random = new Random(CombineSeed(_runSeed, normalized));
            var consumed = GetConsumed(normalized);
            for (var i = 0; i < consumed; i++)
            {
                random.NextDouble();
            }
            return random;
        }

        private static int GetConsumed(string streamID) => ConsumedByStream.GetValueOrDefault(Normalize(streamID), 0);
        private static string Normalize(string streamID) => string.IsNullOrWhiteSpace(streamID) ? "default" : streamID;

        private static int CombineSeed(int seed, string streamID)
        {
            unchecked
            {
                var hash = 2166136261;
                foreach (var character in streamID)
                {
                    hash ^= character;
                    hash *= 16777619;
                }
                return seed ^ (int)hash;
            }
        }
    }
}
