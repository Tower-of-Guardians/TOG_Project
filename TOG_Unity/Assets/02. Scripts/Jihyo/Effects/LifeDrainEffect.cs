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

            // 입자의 고유 Seed를 이용해 각자 다른 휘어짐 값을 가짐
            uint seed = particles[i].randomSeed;
            // -1에서 1 사이의 랜덤값을 생성하여 입자마다 고유한 경로 부여
            float randX = (seed % 100 / 50f - 1f) * curveStrength;
            float randY = (seed / 100 % 100 / 50f - 1f) * curveStrength;

            // 중간 지점을 입자마다 다르게 설정 (랜덤 곡선)
            Vector3 centerPos = (startPos + targetPos) * 0.5f;
            centerPos.x += randX;
            centerPos.y += randY;

            // 베지에 곡선으로 위치 업데이트
            particles[i].position = Vector3.Lerp(Vector3.Lerp(startPos, centerPos, t), Vector3.Lerp(centerPos, targetPos, t), t);
        }

        ps.SetParticles(particles, numParticlesAlive);
    }
}
