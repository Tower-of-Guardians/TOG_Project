using System;
using UnityEngine;

namespace JxModule
{
    [Serializable]
    public class JxObjectPoolConfig
    {
        public GameObject prefab;
        public int initialPoolSize;
        public int maxPoolSize;
        public bool isExpandable = false;
    }
}