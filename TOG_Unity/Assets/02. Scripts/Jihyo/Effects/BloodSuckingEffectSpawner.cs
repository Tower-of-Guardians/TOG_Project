using System.Collections.Generic;
using UnityEngine;

public static class BloodSuckingEffectSpawner
{
    public const string EffectId = "Effect_BloodSucking";

    public static int CalculateHeal(int attackPower, int percent)
    {
        if (attackPower <= 0 || percent <= 0)
        {
            return 0;
        }

        return Mathf.Max(1, Mathf.CeilToInt(attackPower * (percent / 100f)));
    }

    public static bool CanApplyHeal(int currentHealth, int maxHealth, int healAmount)
    {
        return healAmount > 0 && currentHealth > 0 && currentHealth < maxHealth;
    }

    public static void SpawnFromTargets(Transform caster, IEnumerable<IDamageable> targets)
    {
        if (caster == null || targets == null || !DIContainer.IsRegistered<EffectManager>())
        {
            return;
        }

        EffectManager effectManager = DIContainer.Resolve<EffectManager>();
        string effectId = ResolveEffectId(effectManager);
        if (string.IsNullOrEmpty(effectId))
        {
            Debug.LogWarning($"BloodSuckingEffectSpawner: '{EffectId}'가 EffectManager에 등록되어 있지 않습니다.");
            return;
        }

        foreach (IDamageable target in targets)
        {
            if (target == null)
            {
                continue;
            }

            Transform from = GetEffectAnchor(target);
            if (from == null)
            {
                continue;
            }

            GameObject effect = effectManager.SpawnParticleEffect(effectId, from, caster);
            if (effect == null)
            {
                continue;
            }

            effect.transform.SetParent(null, true);
            RestartParticle(effect);
        }
    }

    private static string ResolveEffectId(EffectManager effectManager)
    {
        if (effectManager.HasEffect(EffectId))
        {
            return EffectId;
        }

        if (effectManager.HasEffect("Effect_LifeDrain"))
        {
            return "Effect_LifeDrain";
        }

        return string.Empty;
    }

    private static void RestartParticle(GameObject effect)
    {
        if (effect == null)
        {
            return;
        }

        ParticleSystem particleSystem = effect.GetComponentInChildren<ParticleSystem>();
        if (particleSystem == null)
        {
            return;
        }

        particleSystem.Clear(true);
        particleSystem.Play(true);
    }

    private static Transform GetEffectAnchor(IDamageable target)
    {
        if (target is Monster monster)
        {
            return monster.AttackAnchor != null ? monster.AttackAnchor : monster.transform;
        }

        if (target is Component component)
        {
            return component.transform;
        }

        return null;
    }
}
