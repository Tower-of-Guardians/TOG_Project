using System.Collections.Generic;
using System;

public static class DIContainer
{
    private static Dictionary<Type, object> m_instances = new();

    #region Methods
    public static void Register<T>(object instance) => m_instances[typeof(T)] = instance;

    public static T Resolve<T>()
    {
        if(!IsRegistered<T>())
        {
            throw new Exception($"DI Container에서 관리 중인 {typeof(T)}가 없습니다.");
        }

        return (T)m_instances[typeof(T)];
    }

    public static bool IsRegistered<T>() => m_instances.ContainsKey(typeof(T));

    public static void Clear() => m_instances.Clear();
    #endregion Methods
}