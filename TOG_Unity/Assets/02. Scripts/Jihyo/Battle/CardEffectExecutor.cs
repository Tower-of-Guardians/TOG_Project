using System;
using System.Collections.Generic;
using Jongmin;
using UnityEngine;

public sealed class CardEffectApplyRequest
{
    public CardEffectTarget Target;
    public string StatusEffectId;
    public int Stack;
    public int Value;
}

public sealed class CardEffectContext
{
    public CardWhen When;
    public FieldType PlaceField;
    public Player Source;
    public CardData SourceCard;
    public IReadOnlyList<BaseUnit> HitTargets;
    public IReadOnlyList<BaseUnit> AllEnemies;
    public bool IsFirstTurn;
    public bool HasKilled;
    public bool UsedMagicThisTurn;
    public bool AttackStatPreviewOnly;
    public Func<string, EffectData> GetEffectData;
    public Func<BaseUnit, string, bool> HasStatus;
}

public interface ICardEffectApplier
{
    void ApplyStatus(CardEffectApplyRequest request, CardEffectContext context);
    void QueueNamedCardAttackBonus(CardData sourceCard, int amount, CardEffectContext context);
    void AddThisTurnCardAttackBonus(CardData sourceCard, int amount, CardEffectContext context);
}

/// <summary>
/// CardData의 When/Trigger/EffectID를 읽어 효과가 실제 적용될 수 있게 만듭니다.
/// 상태 부여는 StatusEffectController에 맡깁니다.
/// </summary>
public static class CardEffectExecutor
{
    public const string ThisTurnThisCardAttackEffectId = "1110002";
    private static readonly BaseUnit[] EmptyTargets = Array.Empty<BaseUnit>();

    public static void PreviewAttackBonusesOnPlace(CardData card, FieldType placeField)
    {
        Execute(card, new CardEffectContext
        {
            When = CardWhen.BeforeAttack,
            PlaceField = placeField,
            Source = ResolveBattlePlayer(),
            SourceCard = card,
            IsFirstTurn = IsBattleFirstTurn(),
            AttackStatPreviewOnly = true
        });
    }

    public static void ExecuteFieldCards(CardWhen phase, CardEffectContext context, ICardEffectApplier applier = null)
    {
        if (GameData.Instance == null || context == null)
        {
            return;
        }

        context.When = phase;
        ExecuteCardsOnField(GameData.Instance.attackField, FieldType.Attack, context, applier);
        ExecuteCardsOnField(GameData.Instance.defenseField, FieldType.Defense, context, applier);
    }

    public static void Execute(CardData card, CardEffectContext context, ICardEffectApplier applier = null)
    {
        if (card == null || context == null)
        {
            return;
        }

        if (!TryGetEffectsForPhase((CardWhen)card.when, context.When, out bool runEffect1, out bool runEffect2))
        {
            return;
        }

        if (!IsTriggerSatisfied((CardTrigger)card.trigger, context))
        {
            return;
        }

        ICardEffectApplier resolvedApplier = applier ?? StatusEffectCardEffectApplier.Instance;
        if (runEffect1)
        {
            TryExecuteEffect(card.effect1ID, card.effect1Value, card, context, resolvedApplier);
        }

        if (runEffect2)
        {
            TryExecuteEffect(card.effect2ID, card.effect2Value, card, context, resolvedApplier);
        }
    }

    public static bool TryGetEffectsForPhase(CardWhen cardWhen, CardWhen phase, out bool runEffect1, out bool runEffect2)
    {
        runEffect1 = false;
        runEffect2 = false;
        if (phase != CardWhen.BeforeAttack && phase != CardWhen.AfterAttack)
        {
            return false;
        }

        switch (cardWhen)
        {
            case CardWhen.BeforeAttack:
                if (phase != CardWhen.BeforeAttack)
                {
                    return false;
                }

                runEffect1 = true;
                runEffect2 = true;
                return true;
            case CardWhen.AfterAttack:
                if (phase != CardWhen.AfterAttack)
                {
                    return false;
                }

                runEffect1 = true;
                runEffect2 = true;
                return true;
            case CardWhen.BeforeThenAfter:
                if (phase == CardWhen.BeforeAttack)
                {
                    runEffect1 = true;
                    return true;
                }

                runEffect2 = true;
                return true;
            default:
                return false;
        }
    }

    public static bool IsTriggerSatisfied(CardTrigger trigger, CardEffectContext context)
    {
        if (context == null)
        {
            return false;
        }

        switch (trigger)
        {
            case CardTrigger.None:
                return true;
            case CardTrigger.FirstTurn:
                return context.IsFirstTurn;
            case CardTrigger.AttackPlace:
                return context.PlaceField == FieldType.Attack;
            case CardTrigger.DefensePlace:
                return context.PlaceField == FieldType.Defense;
            case CardTrigger.OnKill:
                return context.HasKilled;
            case CardTrigger.TargetCursed:
                return AnyHitTargetHasStatus(context, StatusEffectController.CurseStatusId);
            case CardTrigger.Frenzy:
                return HasStatus(context, context.Source, StatusEffectController.FrenzyStatusId);
            case CardTrigger.UsedMagicThisTurn:
                return context.UsedMagicThisTurn;
            case CardTrigger.TargetNotCursed:
                return AnyHitTargetMissingStatus(context, StatusEffectController.CurseStatusId);
            default:
                return false;
        }
    }

    public static IReadOnlyList<BaseUnit> ResolveTargets(CardEffectTarget target, CardEffectContext context)
    {
        if (context == null)
        {
            return EmptyTargets;
        }

        switch (target)
        {
            case CardEffectTarget.Self:
                return context.Source != null ? new BaseUnit[] { context.Source } : EmptyTargets;
            case CardEffectTarget.AttackHitTarget:
                return context.HitTargets ?? EmptyTargets;
            case CardEffectTarget.AllEnemies:
                return context.AllEnemies ?? EmptyTargets;
            default:
                return EmptyTargets;
        }
    }

    private static void ExecuteCardsOnField(
        IEnumerable<CardData> cards,
        FieldType fieldType,
        CardEffectContext context,
        ICardEffectApplier applier)
    {
        if (cards == null)
        {
            return;
        }

        context.PlaceField = fieldType;
        foreach (CardData card in cards)
        {
            Execute(card, context, applier);
        }
    }

    private static Player ResolveBattlePlayer()
    {
        if (!DIContainer.IsRegistered<BattleManager>())
        {
            return null;
        }

        return DIContainer.Resolve<BattleManager>()?.GetSetupController()?.GetPlayer();
    }

    public static bool IsBattleFirstTurn()
    {
        return GameData.Instance != null && GameData.Instance.IsCombatFirstTurn;
    }

    public static void RefreshVisibleCardAttackLabels()
    {
        if (GameData.Instance == null)
        {
            return;
        }

        if (DIContainer.IsRegistered<HandDomain>())
        {
            RefreshContainerAttackLabels(DIContainer.Resolve<HandDomain>()?.Container);
        }

        if (DIContainer.IsRegistered<FieldDomain>())
        {
            FieldDomain fieldDomain = DIContainer.Resolve<FieldDomain>();
            RefreshContainerAttackLabels(fieldDomain?.AtkContainer);
            RefreshContainerAttackLabels(fieldDomain?.DefContainer);
        }
    }

    private static void RefreshContainerAttackLabels(CardContainer container)
    {
        if (container == null)
        {
            return;
        }

        IReadOnlyList<Card> cards = container.Cards;
        for (int i = 0; i < cards.Count; i++)
        {
            Card card = cards[i];
            if (card?.CardData == null || card.View?.AtkLabel?.Label == null)
            {
                continue;
            }

            card.View.AtkLabel.Label.text = $"{GameData.Instance.GetCardAttack(card.CardData)}";
        }
    }

    private static void TryExecuteEffect(string effectId, string effectValue, CardData sourceCard, CardEffectContext context, ICardEffectApplier applier)
    {
        if (string.IsNullOrWhiteSpace(effectId) || effectId == "0")
        {
            return;
        }

        EffectData effectData = LookupEffectData(effectId.Trim(), context);
        if (effectData == null)
        {
            return;
        }

        int parsedValue = ParseEffectValue(effectValue);
        if (effectData.TryGetStatusEffectId(out string statusEffectId))
        {
            if (context.AttackStatPreviewOnly)
            {
                return;
            }

            applier.ApplyStatus(new CardEffectApplyRequest
            {
                Target = (CardEffectTarget)effectData.Target,
                StatusEffectId = statusEffectId,
                Stack = Mathf.Max(1, parsedValue),
                Value = parsedValue
            }, context);
            return;
        }

        CardData resolvedSource = context.SourceCard != null ? context.SourceCard : sourceCard;
        if (resolvedSource == null)
        {
            return;
        }

        if (effectData.Id == ThisTurnThisCardAttackEffectId)
        {
            if (context.IsFirstTurn)
            {
                applier.AddThisTurnCardAttackBonus(resolvedSource, parsedValue, context);
            }
            return;
        }

        if ((CardEffectTarget)effectData.Target == CardEffectTarget.NamedCards)
        {
            applier.QueueNamedCardAttackBonus(resolvedSource, parsedValue, context);
        }
    }

    private static EffectData LookupEffectData(string effectId, CardEffectContext context)
    {
        if (context.GetEffectData != null)
        {
            return context.GetEffectData(effectId);
        }

        if (DataCenter.Instance == null)
        {
            return null;
        }

        EffectData resolved = null;
        DataCenter.Instance.GetEffectData(effectId, data => resolved = data);
        return resolved;
    }

    private static int ParseEffectValue(string raw)
    {
        return int.TryParse(raw, out int value) ? Mathf.Max(0, value) : 0;
    }

    private static bool AnyHitTargetHasStatus(CardEffectContext context, string statusEffectId)
    {
        IReadOnlyList<BaseUnit> hitTargets = context.HitTargets;
        if (hitTargets == null)
        {
            return false;
        }

        for (int i = 0; i < hitTargets.Count; i++)
        {
            if (HasStatus(context, hitTargets[i], statusEffectId))
            {
                return true;
            }
        }

        return false;
    }

    private static bool AnyHitTargetMissingStatus(CardEffectContext context, string statusEffectId)
    {
        IReadOnlyList<BaseUnit> hitTargets = context.HitTargets;
        if (hitTargets == null)
        {
            return false;
        }

        bool hasLivingTarget = false;
        for (int i = 0; i < hitTargets.Count; i++)
        {
            if (hitTargets[i] == null)
            {
                continue;
            }

            hasLivingTarget = true;
            if (!HasStatus(context, hitTargets[i], statusEffectId))
            {
                return true;
            }
        }

        return hasLivingTarget;
    }

    private static bool HasStatus(CardEffectContext context, BaseUnit unit, string statusEffectId)
    {
        if (unit == null || string.IsNullOrEmpty(statusEffectId))
        {
            return false;
        }

        if (context.HasStatus != null)
        {
            return context.HasStatus(unit, statusEffectId);
        }

        StatusEffectController controller = unit.GetComponent<StatusEffectController>();
        return controller != null && controller.TryGetStatusStack(statusEffectId, out int stack) && stack > 0;
    }
}

public sealed class StatusEffectCardEffectApplier : ICardEffectApplier
{
    public static readonly StatusEffectCardEffectApplier Instance = new StatusEffectCardEffectApplier();

    public void ApplyStatus(CardEffectApplyRequest request, CardEffectContext context)
    {
        if (request == null || string.IsNullOrEmpty(request.StatusEffectId))
        {
            return;
        }

        IReadOnlyList<BaseUnit> targets = CardEffectExecutor.ResolveTargets(request.Target, context);
        for (int i = 0; i < targets.Count; i++)
        {
            BaseUnit unit = targets[i];
            if (unit == null)
            {
                continue;
            }

            StatusEffectController controller = StatusEffectController.Resolve(unit);
            if (controller != null)
            {
                controller.TryApplyStatus(request.StatusEffectId, request.Stack, request.Value);
            }
        }
    }

    public void QueueNamedCardAttackBonus(CardData sourceCard, int amount, CardEffectContext context)
    {
        if (GameData.Instance == null)
        {
            return;
        }

        GameData.Instance.QueueNamedCardAttackBonus(sourceCard, amount);
        CardEffectExecutor.RefreshVisibleCardAttackLabels();
    }

    public void AddThisTurnCardAttackBonus(CardData sourceCard, int amount, CardEffectContext context)
    {
        if (GameData.Instance == null)
        {
            return;
        }

        GameData.Instance.AddThisTurnCardAttackBonus(sourceCard, amount);
        CardEffectExecutor.RefreshVisibleCardAttackLabels();
    }
}
