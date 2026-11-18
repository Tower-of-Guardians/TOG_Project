using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kwangmin
{
    public sealed class StageProgressView : MonoBehaviour
    {
        [Serializable]
        private struct IconSet
        {
            public AreaEventType type;
            public Sprite normal;
            public Sprite current;
        }

        [SerializeField] private StageProgressNodeView nodeTemplate;
        [SerializeField] private float nodeSpacing = 58f;
        [SerializeField] private IconSet[] iconSets;
        [SerializeField] private Sprite lockedIcon;
        [SerializeField] private Sprite solidPath;
        [SerializeField] private Sprite dottedPath;

        private readonly List<StageProgressNodeView> nodes = new();
        private AreaEventDomain domain;

        private void Start()
        {
            domain = FindFirstObjectByType<AreaEventDomain>(FindObjectsInactive.Include);
            if (domain == null) return;

            domain.OnStageProgressChanged += Refresh;
            Refresh();
        }

        private void OnDestroy()
        {
            if (domain != null) domain.OnStageProgressChanged -= Refresh;
        }

        private void Refresh()
        {
            List<StageProgressNode> states = domain.GetStageProgressNodes();
            while (nodes.Count < states.Count)
            {
                nodes.Add(CreateNode(nodes.Count));
            }

            for (int i = 0; i < nodes.Count; i++)
            {
                bool used = i < states.Count;
                nodes[i].gameObject.SetActive(used);
                if (!used) continue;

                StageProgressNode state = states[i];
                Sprite pathSprite = i == states.Count - 1 ? null : state.IsPathSolid ? solidPath : dottedPath;
                nodes[i].Apply(state, GetIcon(state), pathSprite);
            }
        }

        private StageProgressNodeView CreateNode(int index)
        {
            StageProgressNodeView node = Instantiate(nodeTemplate, transform);
            node.name = $"Stage Node {index + 1}";
            ((RectTransform)node.transform).anchoredPosition = new Vector2(index * nodeSpacing, 0f);
            return node;
        }

        private Sprite GetIcon(StageProgressNode state)
        {
            if (state.State == EStageNodeState.Locked)
            {
                return state.IsBoss ? FindIconSet(AreaEventType.Boss).normal : lockedIcon;
            }
            if (!state.EventType.HasValue)
            {
                return lockedIcon;
            }

            IconSet iconSet = FindIconSet(state.EventType.Value);
            return state.State == EStageNodeState.Current ? iconSet.current : iconSet.normal;
        }

        private IconSet FindIconSet(AreaEventType type)
        {
            return Array.Find(iconSets, iconSet => iconSet.type == type);
        }
    }
}
