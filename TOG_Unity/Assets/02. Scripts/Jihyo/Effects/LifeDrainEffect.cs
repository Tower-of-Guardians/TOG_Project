using UnityEngine;

/// <summary>
/// 흡혈 이펙트용 컴포넌트
/// ParticleSystem의 파티클을 직접 조작하여 베지에 곡선으로 이동하는 특수한 이동 이펙트입니다.
/// </summary>
public class LifeDrainEffect : ParticleEffectBase
{
    [Header("이동 설정")]
    [SerializeField] private float curveStrength = 3.0f; // 옆으로 퍼지는 정도

    protected override void UpdateParticles()
    {
        if (ps == null || particles == null) return;

        int numParticlesAlive = ps.GetParticles(particles);
        Vector3 targetPos = GetTargetPosition();
        Vector3 startPos = GetStartPosition();

        for (int i = 0; i < numParticlesAlive; i++)
        {
            float t = 1.0f - (particles[i].remainingLifetime / particles[i].startLifetime);

            uint seed = particles[i].randomSeed;
            float randX = (seed % 100 / 50f - 1f) * curveStrength;
            float randY = (seed / 100 % 100 / 50f - 1f) * curveStrength;

            Vector3 centerPos = (startPos + targetPos) * 0.5f;
            centerPos.x += randX;
            centerPos.y += randY;

            Vector3 worldPosition = Vector3.Lerp(Vector3.Lerp(startPos, centerPos, t), Vector3.Lerp(centerPos, targetPos, t), t);
            particles[i].position = ToParticleSpace(worldPosition);
        }

        ps.SetParticles(particles, numParticlesAlive);
    }

    private Vector3 ToParticleSpace(Vector3 worldPosition)
    {
        if (ps == null || ps.main.simulationSpace == ParticleSystemSimulationSpace.World)
        {
            return worldPosition;
        }

        return ps.transform.InverseTransformPoint(worldPosition);
    }
}
