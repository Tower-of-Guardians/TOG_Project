using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class MonsterAttackHitEventTests
{
    private static readonly string[] AttackClipPaths =
    {
        "Assets/05. Animations/Monster/WhiteDog/WhiteDog_Attack.anim",
        "Assets/05. Animations/Monster/Uranodon/Uranodon_Attack.anim",
        "Assets/05. Animations/Monster/Flightmare/Flightmare_Attack.anim",
        "Assets/05. Animations/Monster/Grounder/Grounder_Attack.anim",
        "Assets/05. Animations/Monster/Clonier/Clonier_Attack.anim",
        "Assets/05. Animations/Monster/ClonierClone/ClonierClone_Attack.anim"
    };

    private static readonly string[] ActionApplyClipPaths =
    {
        "Assets/05. Animations/Monster/WhiteDog/WhiteDog_Defense.anim",
        "Assets/05. Animations/Monster/WhiteDog/WhiteDog_Curse.anim",
        "Assets/05. Animations/Monster/Grounder/Grounder_Debuff.anim",
        "Assets/05. Animations/Monster/Uranodon/Uranodon_Defense.anim",
        "Assets/05. Animations/Monster/Uranodon/Uranodon_Buff.anim",
        "Assets/05. Animations/Monster/Clonier/Clonier_Summon.anim"
    };

    [Test]
    public void AttackClips_ContainOnAttackHitInsideClip()
    {
        AssertClipsContainEvent(AttackClipPaths, MonsterAnimation.AttackHitFunctionName);
    }

    [Test]
    public void NonAttackActionClips_ContainOnActionApplyInsideClip()
    {
        AssertClipsContainEvent(ActionApplyClipPaths, MonsterAnimation.ActionApplyFunctionName);
    }

    private static void AssertClipsContainEvent(string[] clipPaths, string functionName)
    {
        for (int i = 0; i < clipPaths.Length; i++)
        {
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPaths[i]);
            Assert.IsNotNull(clip, clipPaths[i]);

            bool hasEvent = false;
            AnimationEvent[] events = clip.events;
            for (int eventIndex = 0; eventIndex < events.Length; eventIndex++)
            {
                if (events[eventIndex].functionName != functionName)
                {
                    continue;
                }

                hasEvent = true;
                Assert.Greater(events[eventIndex].time, 0f, clipPaths[i]);
                Assert.Less(events[eventIndex].time, clip.length, clipPaths[i]);
                break;
            }

            Assert.IsTrue(hasEvent, clipPaths[i]);
        }
    }
}
