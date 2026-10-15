using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;

public class StatusEffectIconTests
{
    [Test]
    public void Resolve_FindsAmountTextByChildNameAndSetAmount()
    {
        GameObject root = new GameObject("Icon");
        GameObject textObject = new GameObject(StatusEffectIcon.AmountTextName);
        textObject.transform.SetParent(root.transform);
        TextMeshProUGUI amountText = textObject.AddComponent<TextMeshProUGUI>();

        StatusEffectIcon icon = StatusEffectIcon.Resolve(root);
        icon.SetAmount("1");

        Assert.AreEqual(amountText, icon.AmountText);
        Assert.AreEqual("1", amountText.text);
    }

    [Test]
    public void Create_UsesIconBuffDebuffPrefabAmountText()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(StatusEffectIcon.IconBuffDebuffPrefabPath);
        Assert.IsNotNull(prefab);

        GameObject parent = new GameObject("Status_Bottom", typeof(RectTransform));
        StatusEffectIcon icon = StatusEffectIcon.Create(parent.transform, "Weakness", null, prefab);

        Assert.IsNotNull(icon);
        Assert.IsNotNull(icon.AmountText);
        Assert.AreEqual(StatusEffectIcon.AmountTextName, icon.AmountText.gameObject.name);
        icon.SetAmount("1");
        Assert.AreEqual("1", icon.AmountText.text);
    }

    [Test]
    public void WeaknessExposure_IsPermanentUntilHit()
    {
        Assert.AreEqual(int.MaxValue, StatusEffectDurationUtility.ResolveDurationFromData(null, 1));

        StatusEffectRuntime runtime = new StatusEffectRuntime
        {
            DurationType = (int)StatusEffectDurationType.Permanent,
            RemainingTurns = int.MaxValue,
            ReleaseCondition = (int)StatusEffectReleaseCondition.OnHit
        };
        Assert.IsFalse(StatusEffectDurationUtility.ShouldDecreaseTurn(runtime));
        runtime.Stack = 1;
        Assert.AreEqual("1", StatusEffectDurationUtility.ResolveDisplayAmount(runtime));
    }

    [Test]
    public void DurationType_OneTurnTicks_ValueTurnsUseRequestedValue()
    {
        StatusEffectData oneTurn = ScriptableObject.CreateInstance<StatusEffectData>();
        oneTurn.DurationType = (int)StatusEffectDurationType.OneTurn;
        Assert.AreEqual(1, StatusEffectDurationUtility.ResolveDurationFromData(oneTurn, 5));

        StatusEffectData valueTurns = ScriptableObject.CreateInstance<StatusEffectData>();
        valueTurns.DurationType = (int)StatusEffectDurationType.ValueTurns;
        Assert.AreEqual(5, StatusEffectDurationUtility.ResolveDurationFromData(valueTurns, 5));

        StatusEffectData permanent = ScriptableObject.CreateInstance<StatusEffectData>();
        permanent.DurationType = (int)StatusEffectDurationType.Permanent;
        Assert.AreEqual(int.MaxValue, StatusEffectDurationUtility.ResolveDurationFromData(permanent, 5));
    }
}
