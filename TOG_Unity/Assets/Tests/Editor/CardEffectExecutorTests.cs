using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class CardEffectExecutorTests
{
    private const string DaggerCardPath = "Assets/Datas/CardData/11010004.asset";
    private const string WeaknessEffectPath = "Assets/Datas/EffectData/1110001.asset";

    [Test]
    public void Execute_AfterAttackDagger_AppliesWeaknessToHitTarget()
    {
        RecordingApplier applier = new RecordingApplier();
        EffectData weakness = CreateStatusEffectData("1110001", CardEffectTarget.AttackHitTarget, StatusEffectController.WeaknessExposureStatusId);
        CardData dagger = CreateCard(CardWhen.AfterAttack, CardTrigger.None, "1110001", "1");

        CardEffectExecutor.Execute(dagger, CreateAttackContext(weakness), applier);

        Assert.AreEqual(1, applier.Requests.Count);
        Assert.AreEqual(StatusEffectController.WeaknessExposureStatusId, applier.Requests[0].StatusEffectId);
        Assert.AreEqual(CardEffectTarget.AttackHitTarget, applier.Requests[0].Target);
        Assert.AreEqual(1, applier.Requests[0].Stack);
    }

    [Test]
    public void Execute_BeforeThenAfter_RunsEffect1BeforeAttackAndEffect2AfterAttack()
    {
        RecordingApplier applier = new RecordingApplier();
        EffectData extraDamage = CreateStatusEffectData("1110012", CardEffectTarget.Self, "51002026");
        EffectData curse = CreateStatusEffectData("1110057", CardEffectTarget.AttackHitTarget, StatusEffectController.CurseStatusId);
        Dictionary<string, EffectData> effects = new Dictionary<string, EffectData>
        {
            { extraDamage.Id, extraDamage },
            { curse.Id, curse }
        };
        CardData card = CreateCard(CardWhen.BeforeThenAfter, CardTrigger.None, "1110012", "30", "1110057", "1");
        CardEffectContext context = new CardEffectContext
        {
            When = CardWhen.BeforeAttack,
            GetEffectData = id => effects.TryGetValue(id, out EffectData data) ? data : null
        };

        CardEffectExecutor.Execute(card, context, applier);

        Assert.AreEqual(1, applier.Requests.Count);
        Assert.AreEqual("51002026", applier.Requests[0].StatusEffectId);

        context.When = CardWhen.AfterAttack;
        CardEffectExecutor.Execute(card, context, applier);

        Assert.AreEqual(2, applier.Requests.Count);
        Assert.AreEqual(StatusEffectController.CurseStatusId, applier.Requests[1].StatusEffectId);
    }

    [Test]
    public void Execute_SkipsWhenTimingDoesNotMatch()
    {
        RecordingApplier applier = new RecordingApplier();
        EffectData weakness = CreateStatusEffectData("1110001", CardEffectTarget.AttackHitTarget, StatusEffectController.WeaknessExposureStatusId);
        CardData dagger = CreateCard(CardWhen.AfterAttack, CardTrigger.None, "1110001", "1");
        CardEffectContext context = CreateAttackContext(weakness);
        context.When = CardWhen.BeforeAttack;

        CardEffectExecutor.Execute(dagger, context, applier);

        Assert.IsEmpty(applier.Requests);
    }

    [Test]
    public void Execute_SkipsWhenTriggerAttackPlaceOnDefenseField()
    {
        RecordingApplier applier = new RecordingApplier();
        EffectData thorns = CreateStatusEffectData("1110013", CardEffectTarget.Self, StatusEffectController.CurseThornsStatusId);
        CardData card = CreateCard(CardWhen.BeforeAttack, CardTrigger.AttackPlace, "1110013", "2");
        CardEffectContext context = CreateBeforeAttackContext(thorns, FieldType.Defense);

        CardEffectExecutor.Execute(card, context, applier);

        Assert.IsEmpty(applier.Requests);
    }

    [Test]
    public void Execute_BeforeAttackOnDefenseField_AppliesSelfStatus()
    {
        RecordingApplier applier = new RecordingApplier();
        EffectData thorns = CreateStatusEffectData("1110013", CardEffectTarget.Self, StatusEffectController.CurseThornsStatusId);
        CardData card = CreateCard(CardWhen.BeforeAttack, CardTrigger.DefensePlace, "1110013", "2");

        CardEffectExecutor.Execute(card, CreateBeforeAttackContext(thorns, FieldType.Defense), applier);

        Assert.AreEqual(1, applier.Requests.Count);
        Assert.AreEqual(StatusEffectController.CurseThornsStatusId, applier.Requests[0].StatusEffectId);
        Assert.AreEqual(CardEffectTarget.Self, applier.Requests[0].Target);
        Assert.AreEqual(2, applier.Requests[0].Stack);
        Assert.AreEqual(2, applier.Requests[0].Value);
    }

    [Test]
    public void Execute_AttackStatPreviewOnly_QueuesNamedBonusAndSkipsStatus()
    {
        RecordingApplier applier = new RecordingApplier();
        EffectData boxingGloveEffect = ScriptableObject.CreateInstance<EffectData>();
        boxingGloveEffect.Id = "1110005";
        boxingGloveEffect.Target = (int)CardEffectTarget.NamedCards;
        boxingGloveEffect.StatusEffect = string.Empty;
        boxingGloveEffect.NumType = "0";
        EffectData thorns = CreateStatusEffectData("1110013", CardEffectTarget.Self, StatusEffectController.CurseThornsStatusId);
        Dictionary<string, EffectData> effects = new Dictionary<string, EffectData>
        {
            { boxingGloveEffect.Id, boxingGloveEffect },
            { thorns.Id, thorns }
        };
        CardData card = CreateCard(CardWhen.BeforeAttack, CardTrigger.AttackPlace, "1110005", "2", "1110013", "2");
        card.itemName = "권법장갑";
        CardEffectContext context = CreateBeforeAttackContext(boxingGloveEffect, FieldType.Attack);
        context.SourceCard = card;
        context.AttackStatPreviewOnly = true;
        context.GetEffectData = id => effects.TryGetValue(id, out EffectData data) ? data : null;

        CardEffectExecutor.Execute(card, context, applier);

        Assert.AreEqual(1, applier.QueuedNameAttackBonuses.Count);
        Assert.AreEqual(2, applier.QueuedNameAttackBonuses[0].amount);
        Assert.IsEmpty(applier.Requests);
    }

    [Test]
    public void Execute_BoxingGloveBeforeAttackOnAttackField_QueuesNamedAttackBonus()
    {
        RecordingApplier applier = new RecordingApplier();
        EffectData boxingGloveEffect = ScriptableObject.CreateInstance<EffectData>();
        boxingGloveEffect.Id = "1110005";
        boxingGloveEffect.Target = (int)CardEffectTarget.NamedCards;
        boxingGloveEffect.StatusEffect = string.Empty;
        boxingGloveEffect.NumType = "0";
        CardData boxingGlove = CreateCard(CardWhen.BeforeAttack, CardTrigger.AttackPlace, "1110005", "2");
        boxingGlove.itemName = "권법장갑";
        CardEffectContext context = CreateBeforeAttackContext(boxingGloveEffect, FieldType.Attack);
        context.SourceCard = boxingGlove;

        CardEffectExecutor.Execute(boxingGlove, context, applier);

        Assert.AreEqual(1, applier.QueuedNameAttackBonuses.Count);
        Assert.AreEqual(boxingGlove, applier.QueuedNameAttackBonuses[0].sourceCard);
        Assert.AreEqual(2, applier.QueuedNameAttackBonuses[0].amount);
        Assert.IsEmpty(applier.Requests);
        Assert.IsEmpty(applier.ThisTurnAttackBonuses);
    }

    [Test]
    public void Execute_ThrowingBladeBeforeAttackOnFirstTurn_AddsThisTurnAttackBonus()
    {
        RecordingApplier applier = new RecordingApplier();
        EffectData firstTurnBuff = ScriptableObject.CreateInstance<EffectData>();
        firstTurnBuff.Id = CardEffectExecutor.ThisTurnThisCardAttackEffectId;
        firstTurnBuff.Target = (int)CardEffectTarget.ThisCard;
        firstTurnBuff.StatusEffect = string.Empty;
        firstTurnBuff.NumType = "0";
        CardData throwingBlade = CreateCard(CardWhen.BeforeAttack, CardTrigger.FirstTurn, "1110002", "10");
        CardEffectContext context = CreateBeforeAttackContext(firstTurnBuff, FieldType.Attack);
        context.SourceCard = throwingBlade;
        context.IsFirstTurn = true;

        CardEffectExecutor.Execute(throwingBlade, context, applier);

        Assert.AreEqual(1, applier.ThisTurnAttackBonuses.Count);
        Assert.AreEqual(throwingBlade, applier.ThisTurnAttackBonuses[0].sourceCard);
        Assert.AreEqual(10, applier.ThisTurnAttackBonuses[0].amount);
        Assert.IsEmpty(applier.Requests);
    }

    [Test]
    public void Execute_ThrowingBladeAfterFirstTurn_DoesNotApply()
    {
        RecordingApplier applier = new RecordingApplier();
        EffectData firstTurnBuff = ScriptableObject.CreateInstance<EffectData>();
        firstTurnBuff.Id = CardEffectExecutor.ThisTurnThisCardAttackEffectId;
        firstTurnBuff.Target = (int)CardEffectTarget.ThisCard;
        firstTurnBuff.StatusEffect = string.Empty;
        firstTurnBuff.NumType = "0";
        CardData throwingBlade = CreateCard(CardWhen.BeforeAttack, CardTrigger.FirstTurn, "1110002", "10");
        CardEffectContext context = CreateBeforeAttackContext(firstTurnBuff, FieldType.Attack);
        context.SourceCard = throwingBlade;
        context.IsFirstTurn = false;

        CardEffectExecutor.Execute(throwingBlade, context, applier);

        Assert.IsEmpty(applier.ThisTurnAttackBonuses);
        Assert.IsEmpty(applier.Requests);
    }

    [Test]
    public void GetFormattedEffectDescription_ReplacesPlaceholderWithEffectValue()
    {
        CardData boxingGlove = CreateCard(CardWhen.BeforeAttack, CardTrigger.AttackPlace, "1110005", "2");
        boxingGlove.effectDescription = "이번 전투동안 \"권법장갑\"의 공격력이 {0} 증가합니다.";

        Assert.AreEqual("이번 전투동안 \"권법장갑\"의 공격력이 2 증가합니다.", boxingGlove.GetFormattedEffectDescription());
    }

    [Test]
    public void Execute_DoesNotApplyWhenEffectHasNoStatusId()
    {
        RecordingApplier applier = new RecordingApplier();
        EffectData attackBuff = ScriptableObject.CreateInstance<EffectData>();
        attackBuff.Id = "1110003";
        attackBuff.Target = (int)CardEffectTarget.ThisCard;
        attackBuff.StatusEffect = string.Empty;
        attackBuff.NumType = "Integer";
        CardData card = CreateCard(CardWhen.AfterAttack, CardTrigger.None, "1110003", "5");

        CardEffectExecutor.Execute(card, CreateAttackContext(attackBuff), applier);

        Assert.IsEmpty(applier.Requests);
    }

    [Test]
    public void Execute_RunsEffect2AfterEffect1()
    {
        RecordingApplier applier = new RecordingApplier();
        EffectData extraDamage = CreateStatusEffectData("1110012", CardEffectTarget.Self, "51002026");
        EffectData curse = CreateStatusEffectData("1110057", CardEffectTarget.AttackHitTarget, StatusEffectController.CurseStatusId);
        Dictionary<string, EffectData> effects = new Dictionary<string, EffectData>
        {
            { extraDamage.Id, extraDamage },
            { curse.Id, curse }
        };
        CardData card = CreateCard(CardWhen.AfterAttack, CardTrigger.None, "1110012", "30", "1110057", "1");
        CardEffectContext context = CreateAttackContext(extraDamage);
        context.GetEffectData = id => effects.TryGetValue(id, out EffectData data) ? data : null;

        CardEffectExecutor.Execute(card, context, applier);

        Assert.AreEqual(2, applier.Requests.Count);
        Assert.AreEqual("51002026", applier.Requests[0].StatusEffectId);
        Assert.AreEqual(StatusEffectController.CurseStatusId, applier.Requests[1].StatusEffectId);
        Assert.AreEqual(1, applier.Requests[1].Stack);
    }

    [Test]
    public void Execute_TargetCursed_SkipsWhenNoHitTargetHasCurse()
    {
        RecordingApplier applier = new RecordingApplier();
        EffectData buff = CreateStatusEffectData("1110003", CardEffectTarget.ThisCard, StatusEffectController.WeaknessExposureStatusId);
        CardData card = CreateCard(CardWhen.AfterAttack, CardTrigger.TargetCursed, "1110003", "5");
        CardEffectContext context = CreateAttackContext(buff);
        context.HasStatus = (unit, statusId) => false;

        CardEffectExecutor.Execute(card, context, applier);

        Assert.IsEmpty(applier.Requests);
    }

    [Test]
    public void TryGetStatusEffectId_UsesNumTypeWhenStatusEffectIsEmpty()
    {
        EffectData data = ScriptableObject.CreateInstance<EffectData>();
        data.StatusEffect = string.Empty;
        data.NumType = StatusEffectController.WeaknessExposureStatusId;

        Assert.IsTrue(data.TryGetStatusEffectId(out string statusEffectId));
        Assert.AreEqual(StatusEffectController.WeaknessExposureStatusId, statusEffectId);
    }

    [Test]
    public void ImportedDaggerAssets_MatchAfterAttackWeaknessApply()
    {
        CardData dagger = AssetDatabase.LoadAssetAtPath<CardData>(DaggerCardPath);
        EffectData weakness = AssetDatabase.LoadAssetAtPath<EffectData>(WeaknessEffectPath);

        Assert.IsNotNull(dagger);
        Assert.IsNotNull(weakness);
        Assert.AreEqual((int)CardWhen.AfterAttack, dagger.when);
        Assert.AreEqual((int)CardTrigger.None, dagger.trigger);
        Assert.AreEqual("1110001", dagger.effect1ID);
        Assert.AreEqual((int)CardEffectTarget.AttackHitTarget, weakness.Target);
        Assert.IsTrue(weakness.TryGetStatusEffectId(out string statusEffectId));
        Assert.AreEqual(StatusEffectController.WeaknessExposureStatusId, statusEffectId);

        RecordingApplier applier = new RecordingApplier();
        CardEffectContext context = CreateAttackContext(weakness);

        CardEffectExecutor.Execute(dagger, context, applier);

        Assert.AreEqual(1, applier.Requests.Count);
        Assert.AreEqual(StatusEffectController.WeaknessExposureStatusId, applier.Requests[0].StatusEffectId);
        Assert.AreEqual(CardEffectTarget.AttackHitTarget, applier.Requests[0].Target);
    }

    private static CardEffectContext CreateAttackContext(EffectData effectData)
    {
        return new CardEffectContext
        {
            When = CardWhen.AfterAttack,
            PlaceField = FieldType.Attack,
            GetEffectData = id => id == effectData.Id ? effectData : null
        };
    }

    private static CardEffectContext CreateBeforeAttackContext(EffectData effectData, FieldType placeField)
    {
        return new CardEffectContext
        {
            When = CardWhen.BeforeAttack,
            PlaceField = placeField,
            GetEffectData = id => id == effectData.Id ? effectData : null
        };
    }

    private static CardData CreateCard(
        CardWhen when,
        CardTrigger trigger,
        string effect1Id,
        string effect1Value,
        string effect2Id = "",
        string effect2Value = "")
    {
        CardData card = ScriptableObject.CreateInstance<CardData>();
        card.when = (int)when;
        card.trigger = (int)trigger;
        card.effect1ID = effect1Id;
        card.effect1Value = effect1Value;
        card.effect2ID = effect2Id;
        card.effect2Value = effect2Value;
        return card;
    }

    private static EffectData CreateStatusEffectData(string id, CardEffectTarget target, string statusEffectId)
    {
        EffectData data = ScriptableObject.CreateInstance<EffectData>();
        data.Id = id;
        data.Target = (int)target;
        data.StatusEffect = statusEffectId;
        data.NumType = "Integer";
        return data;
    }

    private sealed class RecordingApplier : ICardEffectApplier
    {
        public readonly List<CardEffectApplyRequest> Requests = new List<CardEffectApplyRequest>();
        public readonly List<(CardData sourceCard, int amount)> QueuedNameAttackBonuses = new List<(CardData, int)>();
        public readonly List<(CardData sourceCard, int amount)> ThisTurnAttackBonuses = new List<(CardData, int)>();

        public void ApplyStatus(CardEffectApplyRequest request, CardEffectContext context)
        {
            Requests.Add(request);
        }

        public void QueueNamedCardAttackBonus(CardData sourceCard, int amount, CardEffectContext context)
        {
            QueuedNameAttackBonuses.Add((sourceCard, amount));
        }

        public void AddThisTurnCardAttackBonus(CardData sourceCard, int amount, CardEffectContext context)
        {
            ThisTurnAttackBonuses.Add((sourceCard, amount));
        }
    }
}
