public interface IDamageable
{
    int CurrentHealth { get; }
    int MaxHealth { get; }
    bool IsAlive { get; }

    void TakeDamage(int amount);
}

