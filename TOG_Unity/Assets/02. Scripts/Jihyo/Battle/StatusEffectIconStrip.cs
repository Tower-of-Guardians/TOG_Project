using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Status_Bottom 아래에 활성 상태효과 아이콘을 상태 ID당 하나씩 맞춥니다.
/// </summary>
public static class StatusEffectIconStrip
{
    public const string NodePrefix = "StatusEffect_";

    public static void Refresh(
        Transform parent,
        IReadOnlyCollection<StatusEffectRuntime> statuses,
        GameObject prefab,
        Func<string, Sprite> resolveIcon)
    {
        if (parent == null)
        {
            return;
        }

        List<StatusEffectRuntime> active = new List<StatusEffectRuntime>();
        if (statuses != null)
        {
            foreach (StatusEffectRuntime runtime in statuses)
            {
                if (runtime == null || runtime.IsExpired || string.IsNullOrEmpty(runtime.StatusEffectId))
                {
                    continue;
                }

                active.Add(runtime);
            }
        }

        active.Sort((left, right) => string.CompareOrdinal(left.StatusEffectId, right.StatusEffectId));

        HashSet<string> activeIds = new HashSet<string>();
        for (int i = 0; i < active.Count; i++)
        {
            StatusEffectRuntime runtime = active[i];
            activeIds.Add(runtime.StatusEffectId);

            string nodeName = NodePrefix + runtime.StatusEffectId;
            Transform existing = parent.Find(nodeName);
            Sprite iconSprite = resolveIcon != null ? resolveIcon(runtime.StatusEffectId) : null;
            StatusEffectIcon icon = existing != null
                ? StatusEffectIcon.Resolve(existing.gameObject)
                : StatusEffectIcon.Create(parent, nodeName, iconSprite, prefab);
            if (icon == null)
            {
                continue;
            }

            icon.SetIcon(iconSprite);
            icon.SetAmount(StatusEffectDurationUtility.ResolveDisplayAmount(runtime));
            icon.gameObject.SetActive(true);
            icon.transform.SetSiblingIndex(i);
        }

        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Transform child = parent.GetChild(i);
            if (child == null || !child.name.StartsWith(NodePrefix))
            {
                continue;
            }

            string statusEffectId = child.name.Substring(NodePrefix.Length);
            if (!activeIds.Contains(statusEffectId))
            {
                child.gameObject.SetActive(false);
            }
        }
    }
}
