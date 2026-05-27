using System.Collections.Generic;
using UnityEngine;

public enum EffectType
{
    Static, // 정적 이펙트
    Moving, // 이동 이펙트 (A에서 B로 이동)
    Tracking, // 추적 이펙트
    Particle // 파티클 이펙트
}

[System.Serializable]
public class EffectPrefabData
{
    public string effectId;
    public GameObject prefab;
    public EffectType effectType;
}

public class EffectManager : MonoBehaviour
{
    [Header("이펙트 프리팹 목록")]
    [SerializeField] private List<EffectPrefabData> effectPrefabs = new List<EffectPrefabData>();

    [Header("풀 설정")]
    [SerializeField] private int defaultInitialPoolSize = 10;
    [SerializeField] private int defaultMaxPoolSize = 50;
    [SerializeField] private bool defaultExpandable = true;

    private Dictionary<string, EffectPrefabData> effectDictionary = new Dictionary<string, EffectPrefabData>();

    private void Awake()
    {
        InitializeEffectDictionary();
        RegisterEffectPools();
    }

    private void InitializeEffectDictionary()
    {
        effectDictionary.Clear();
        foreach (var effectData in effectPrefabs)
        {
            if (effectData.prefab != null && !string.IsNullOrEmpty(effectData.effectId))
            {
                if (effectDictionary.ContainsKey(effectData.effectId))
                {
                    Debug.LogWarning($"EffectManager: 중복된 이펙트 ID '{effectData.effectId}'가 있습니다.", this);
                }
                else
                {
                    effectDictionary[effectData.effectId] = effectData;
                }
            }
        }
    }

    // 타입별로 등록된 이펙트 프리팹을 ObjectPoolManager에 자동으로 등록
    private void RegisterEffectPools()
    {
        if (ObjectPoolManager.Instance == null)
        {
            Debug.LogWarning("EffectManager: ObjectPoolManager를 찾을 수 없습니다. 풀 등록을 건너뜁니다.", this);
            return;
        }

        foreach (var effectData in effectPrefabs)
        {
            if (effectData.prefab != null && !ObjectPoolManager.Instance.HasPool(effectData.prefab))
            {
                // 타입별로 풀 설정
                int initialSize = GetInitialPoolSizeForType(effectData.effectType);
                int maxSize = GetMaxPoolSizeForType(effectData.effectType);

                ObjectPoolManager.Instance.RegisterPool(
                    effectData.prefab,
                    initialSize,
                    maxSize,
                    defaultExpandable
                );
            }
        }
    }

    // 이펙트 타입에 따른 초기 풀 크기를 반환
    private int GetInitialPoolSizeForType(EffectType effectType)
    {
        return effectType switch
        {
            EffectType.Static => defaultInitialPoolSize,
            EffectType.Moving => defaultInitialPoolSize,
            EffectType.Tracking => defaultInitialPoolSize,
            EffectType.Particle => defaultInitialPoolSize,
            _ => defaultInitialPoolSize
        };
    }

    // 이펙트 타입에 따른 최대 풀 크기를 반환
    private int GetMaxPoolSizeForType(EffectType effectType)
    {
        return effectType switch
        {
            EffectType.Static => defaultMaxPoolSize,
            EffectType.Moving => defaultMaxPoolSize,
            EffectType.Tracking => defaultMaxPoolSize,
            EffectType.Particle => defaultMaxPoolSize,
            _ => defaultMaxPoolSize
        };
    }

    /// <summary>
    /// 정적 이펙트를 지정된 위치에 소환합니다.
    /// </summary>
    /// <param name="effectId">이펙트 ID</param>
    /// <param name="position">소환 위치</param>
    /// <param name="parent">부모 Transform (선택사항)</param>
    /// <returns>소환된 이펙트 GameObject</returns>
    public GameObject SpawnStaticEffect(string effectId, Vector3 position, Transform parent = null)
    {
        if (!effectDictionary.TryGetValue(effectId, out EffectPrefabData effectData))
        {
            Debug.LogError($"EffectManager: 이펙트 ID '{effectId}'를 찾을 수 없습니다.", this);
            return null;
        }

        if (effectData.effectType != EffectType.Static)
        {
            Debug.LogWarning($"EffectManager: 이펙트 ID '{effectId}'는 Static 타입이 아닙니다. ({effectData.effectType})", this);
        }

        return SpawnEffect(effectData.prefab, position, parent);
    }

    // 프리팹을 직접 지정하여 정적 이펙트를 소환
    public GameObject SpawnStaticEffect(GameObject prefab, Vector3 position, Transform parent = null)
    {
        return SpawnEffect(prefab, position, parent);
    }

    /// <summary>
    /// 이동 이펙트를 소환합니다. (타겟에서 시전자로 이동)
    /// </summary>
    /// <param name="effectId">이펙트 ID</param>
    /// <param name="fromTarget">시작 위치 (타겟)</param>
    /// <param name="toCaster">도착 위치 (시전자)</param>
    /// <returns>소환된 이펙트 GameObject</returns>
    public GameObject SpawnMovingEffect(string effectId, Transform fromTarget, Transform toCaster)
    {
        if (!effectDictionary.TryGetValue(effectId, out EffectPrefabData effectData))
        {
            Debug.LogError($"EffectManager: 이펙트 ID '{effectId}'를 찾을 수 없습니다.", this);
            return null;
        }

        if (effectData.effectType != EffectType.Moving)
        {
            Debug.LogWarning($"EffectManager: 이펙트 ID '{effectId}'는 Moving 타입이 아닙니다. ({effectData.effectType})", this);
        }

        return SpawnMovingEffect(effectData.prefab, fromTarget, toCaster);
    }

    // 프리팹을 직접 지정하여 이동 이펙트를 소환
    public GameObject SpawnMovingEffect(GameObject prefab, Transform fromTarget, Transform toCaster)
    {
        if (prefab == null)
        {
            Debug.LogError("EffectManager: 프리팹이 null입니다.", this);
            return null;
        }

        if (fromTarget == null || toCaster == null)
        {
            Debug.LogError("EffectManager: fromTarget 또는 toCaster가 null입니다.", this);
            return null;
        }

        GameObject effect = ObjectPoolManager.Instance.Get(prefab);
        if (effect == null)
        {
            return null;
        }

        effect.transform.position = fromTarget.position;

        // MovingEffectBase 컴포넌트가 있는지 확인
        MovingEffectBase movingEffect = effect.GetComponent<MovingEffectBase>();
        if (movingEffect != null)
        {
            movingEffect.Initialize(fromTarget, toCaster);
        }
        // ParticleEffectBase 컴포넌트가 있는지 확인
        else if (effect.GetComponent<ParticleEffectBase>() != null)
        {
            ParticleEffectBase particleEffect = effect.GetComponent<ParticleEffectBase>();
            particleEffect.Initialize(fromTarget, toCaster);
        }
        else
        {
            Debug.LogWarning($"EffectManager: 프리팹 '{prefab.name}'에 MovingEffectBase 또는 ParticleEffectBase 컴포넌트가 없습니다.", this);
        }

        return effect;
    }

    // 위치 기반으로 이동 이펙트를 소환
    public GameObject SpawnMovingEffect(string effectId, Vector3 fromPosition, Vector3 toPosition)
    {
        if (!effectDictionary.TryGetValue(effectId, out EffectPrefabData effectData))
        {
            Debug.LogError($"EffectManager: 이펙트 ID '{effectId}'를 찾을 수 없습니다.", this);
            return null;
        }

        return SpawnMovingEffect(effectData.prefab, fromPosition, toPosition);
    }

    // 프리팹을 직접 지정하여 위치 기반 이동 이펙트를 소환
    public GameObject SpawnMovingEffect(GameObject prefab, Vector3 fromPosition, Vector3 toPosition)
    {
        if (prefab == null)
        {
            Debug.LogError("EffectManager: 프리팩이 null입니다.", this);
            return null;
        }

        GameObject effect = ObjectPoolManager.Instance.Get(prefab);
        if (effect == null)
        {
            return null;
        }

        effect.transform.position = fromPosition;

        MovingEffectBase movingEffect = effect.GetComponent<MovingEffectBase>();
        if (movingEffect != null)
        {
            movingEffect.Initialize(fromPosition, toPosition);
        }
        // ParticleEffectBase 컴포넌트가 있는지 확인
        else if (effect.GetComponent<ParticleEffectBase>() != null)
        {
            ParticleEffectBase particleEffect = effect.GetComponent<ParticleEffectBase>();
            particleEffect.Initialize(fromPosition, toPosition);
        }
        else
        {
            Debug.LogWarning($"EffectManager: 프리팹 '{prefab.name}'에 MovingEffectBase 또는 ParticleEffectBase 컴포넌트가 없습니다.", this);
        }

        return effect;
    }

    /// <summary>
    /// 파티클 이펙트를 소환합니다. (ParticleSystem 기반 이동 이펙트)
    /// </summary>
    /// <param name="effectId">이펙트 ID</param>
    /// <param name="fromTarget">시작 위치 (타겟)</param>
    /// <param name="toCaster">도착 위치 (시전자)</param>
    /// <returns>소환된 이펙트 GameObject</returns>
    public GameObject SpawnParticleEffect(string effectId, Transform fromTarget, Transform toCaster)
    {
        if (!effectDictionary.TryGetValue(effectId, out EffectPrefabData effectData))
        {
            Debug.LogError($"EffectManager: 이펙트 ID '{effectId}'를 찾을 수 없습니다.", this);
            return null;
        }

        if (effectData.effectType != EffectType.Particle)
        {
            Debug.LogWarning($"EffectManager: 이펙트 ID '{effectId}'는 Particle 타입이 아닙니다. ({effectData.effectType})", this);
        }

        return SpawnParticleEffect(effectData.prefab, fromTarget, toCaster);
    }

    // 프리팹을 직접 지정하여 파티클 이펙트를 소환
    public GameObject SpawnParticleEffect(GameObject prefab, Transform fromTarget, Transform toCaster)
    {
        if (prefab == null)
        {
            Debug.LogError("EffectManager: 프리팹이 null입니다.", this);
            return null;
        }

        if (fromTarget == null || toCaster == null)
        {
            Debug.LogError("EffectManager: fromTarget 또는 toCaster가 null입니다.", this);
            return null;
        }

        GameObject effect = ObjectPoolManager.Instance.Get(prefab);
        if (effect == null)
        {
            return null;
        }

        effect.transform.position = fromTarget.position;

        // ParticleEffectBase 컴포넌트가 있는지 확인
        ParticleEffectBase particleEffect = effect.GetComponent<ParticleEffectBase>();
        if (particleEffect != null)
        {
            particleEffect.Initialize(fromTarget, toCaster);
        }
        else
        {
            Debug.LogWarning($"EffectManager: 프리팹 '{prefab.name}'에 ParticleEffectBase 컴포넌트가 없습니다.", this);
        }

        return effect;
    }

    // 위치 기반으로 파티클 이펙트를 소환
    public GameObject SpawnParticleEffect(string effectId, Vector3 fromPosition, Vector3 toPosition)
    {
        if (!effectDictionary.TryGetValue(effectId, out EffectPrefabData effectData))
        {
            Debug.LogError($"EffectManager: 이펙트 ID '{effectId}'를 찾을 수 없습니다.", this);
            return null;
        }

        return SpawnParticleEffect(effectData.prefab, fromPosition, toPosition);
    }

    // 프리팹을 직접 지정하여 위치 기반 파티클 이펙트를 소환
    public GameObject SpawnParticleEffect(GameObject prefab, Vector3 fromPosition, Vector3 toPosition)
    {
        if (prefab == null)
        {
            Debug.LogError("EffectManager: 프리팩이 null입니다.", this);
            return null;
        }

        GameObject effect = ObjectPoolManager.Instance.Get(prefab);
        if (effect == null)
        {
            return null;
        }

        effect.transform.position = fromPosition;

        ParticleEffectBase particleEffect = effect.GetComponent<ParticleEffectBase>();
        if (particleEffect != null)
        {
            particleEffect.Initialize(fromPosition, toPosition);
        }
        else
        {
            Debug.LogWarning($"EffectManager: 프리팹 '{prefab.name}'에 ParticleEffectBase 컴포넌트가 없습니다.", this);
        }

        return effect;
    }

    /// <summary>
    /// 추적 이펙트를 소환합니다. (타겟을 따라다님)
    /// </summary>
    /// <param name="effectId">이펙트 ID</param>
    /// <param name="target">추적할 타겟</param>
    /// <param name="offset">타겟으로부터의 오프셋 (선택사항)</param>
    /// <returns>소환된 이펙트 GameObject</returns>
    public GameObject SpawnTrackingEffect(string effectId, Transform target, Vector3 offset = default)
    {
        if (!effectDictionary.TryGetValue(effectId, out EffectPrefabData effectData))
        {
            Debug.LogError($"EffectManager: 이펙트 ID '{effectId}'를 찾을 수 없습니다.", this);
            return null;
        }

        if (effectData.effectType != EffectType.Tracking)
        {
            Debug.LogWarning($"EffectManager: 이펙트 ID '{effectId}'는 Tracking 타입이 아닙니다. ({effectData.effectType})", this);
        }

        return SpawnTrackingEffect(effectData.prefab, target, offset);
    }

    // 프리팹을 직접 지정하여 추적 이펙트를 소환
    public GameObject SpawnTrackingEffect(GameObject prefab, Transform target, Vector3 offset = default)
    {
        if (prefab == null)
        {
            Debug.LogError("EffectManager: 프리팩이 null입니다.", this);
            return null;
        }

        if (target == null)
        {
            Debug.LogError("EffectManager: target이 null입니다.", this);
            return null;
        }

        GameObject effect = ObjectPoolManager.Instance.Get(prefab);
        if (effect == null)
        {
            return null;
        }

        effect.transform.position = target.position + offset;

        TrackingEffectBase trackingEffect = effect.GetComponent<TrackingEffectBase>();
        if (trackingEffect != null)
        {
            trackingEffect.Initialize(target, offset);
        }
        else
        {
            Debug.LogWarning($"EffectManager: 프리팹 '{prefab.name}'에 TrackingEffectBase 컴포넌트가 없습니다.", this);
        }

        return effect;
    }

    /// 기본 이펙트 소환 메서드 (내부 사용)
    private GameObject SpawnEffect(GameObject prefab, Vector3 position, Transform parent = null)
    {
        if (prefab == null)
        {
            Debug.LogError("EffectManager: 프리팹이 null입니다.", this);
            return null;
        }

        GameObject effect = ObjectPoolManager.Instance.Get(prefab);
        if (effect == null)
        {
            return null;
        }

        effect.transform.position = position;
        if (parent != null)
        {
            effect.transform.SetParent(parent);
        }

        return effect;
    }

    /// 이펙트를 수동으로 반환
    public void ReturnEffect(GameObject effect)
    {
        if (effect != null)
        {
            ObjectPoolManager.Instance.Return(effect);
        }
    }

    /// 이펙트 ID로 프리팹을 가져오기
    public GameObject GetEffectPrefab(string effectId)
    {
        if (effectDictionary.TryGetValue(effectId, out EffectPrefabData effectData))
        {
            return effectData.prefab;
        }
        return null;
    }

    /// 이펙트 ID가 등록되어 있는지 확인
    public bool HasEffect(string effectId)
    {
        return effectDictionary.ContainsKey(effectId);
    }
}
