using System.Collections.Generic;
using UnityEngine;

public static class AttackEffectSpawner
{
    private const int LightAttackThreshold = 10;
    private const int NormalAttackThreshold = 20;

    private const string AttackEffect1Id = "Effect_Attack1";
    private const string AttackEffect2Id = "Effect_Attack2";
    private const string AttackEffect3Id = "Effect_Attack3";

    public static void SpawnOnTargets(int attackValue, IEnumerable<IDamageable> targets)
    {
        if (targets == null || !DIContainer.IsRegistered<EffectManager>())
        {
            return;
        }

        EffectManager effectManager = DIContainer.Resolve<EffectManager>();
        string effectId = GetAttackEffectId(attackValue);

        if (!effectManager.HasEffect(effectId))
        {
            Debug.LogWarning($"AttackEffectSpawner: '{effectId}'가 EffectManager에 등록되어 있지 않습니다.");
            return;
        }

        foreach (IDamageable target in targets)
        {
            if (target == null || !target.IsAlive)
            {
                continue;
            }

            Transform anchor = GetEffectAnchor(target);
            if (anchor == null)
            {
                continue;
            }

            GameObject effect = effectManager.SpawnStaticEffect(effectId, anchor.position, anchor);
            RestartEffectAnimation(effect);
        }
    }

    private static void RestartEffectAnimation(GameObject effect)
    {
        if (effect == null)
        {
            return;
        }

        Animator animator = effect.GetComponent<Animator>();
        if (animator != null)
        {
            animator.Play(0, 0, 0f);
        }
    }

    private static Transform GetEffectAnchor(IDamageable target)
    {
        if (target is Monster monster)
        {
            return monster.AttackAnchor;
        }

        if (target is Component component)
        {
            return component.transform;
        }

        return null;
    }

    private static string GetAttackEffectId(int attackValue)
    {
        if (attackValue < LightAttackThreshold)
        {
            return AttackEffect1Id;
        }

        if (attackValue < NormalAttackThreshold)
        {
            return AttackEffect2Id;
        }

        return AttackEffect3Id;
    }
}
