using UnityEngine;

public class BaseBootstrapper : MonoBehaviour
{
    private IInjector[] m_injectors;

    protected virtual void Awake()
    {
        m_injectors = transform.GetComponentsInChildren<IInjector>();
    }

    protected virtual void Start()
    {
        foreach(var injector in m_injectors)
        {
            injector.Inject();
        }
    }
}