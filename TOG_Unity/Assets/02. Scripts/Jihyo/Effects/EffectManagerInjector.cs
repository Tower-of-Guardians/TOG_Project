using UnityEngine;

public class EffectManagerInjector : MonoBehaviour, IInjector
{
    [Header("의존성 목록")]
    [Header("이펙트 매니저")]
    [SerializeField] private EffectManager effectManager;

    public void Inject()
    {
        if (effectManager == null)
        {
            effectManager = GetComponent<EffectManager>();
        }

        if (effectManager == null)
        {
            Debug.LogError("EffectManagerInjector: EffectManager를 찾을 수 없습니다.", this);
            return;
        }

        if (!DIContainer.IsRegistered<EffectManager>())
        {
            DIContainer.Register<EffectManager>(effectManager);
        }
        else
        {
            Debug.LogWarning("EffectManagerInjector: EffectManager가 이미 등록되어 있습니다.", this);
        }
    }
}
