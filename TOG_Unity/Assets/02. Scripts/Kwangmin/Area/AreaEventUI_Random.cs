using System;
using System.Collections;
using System.Collections.Generic;
using JxDialogueBox;
using UnityEngine;
using UnityEngine.UI;

public sealed class AreaEventUI_Random : MonoBehaviour
{
    [Serializable]
    private sealed class NpcEntry
    {
        public string eventName;
        public RandomEventNpc npc;
        public RandomEventSpecialAction specialAction;
        public RandomEventNpc afterBattleNpc;
    }

    private enum Phase
    {
        None,
        Talking,
        Busy,
        AfterBattle,
        Done
    }

    private const int RewardCardCount = 3;

    [SerializeField] private Button nextButton;
    [SerializeField] private RandomEventCardPanel cardPanel;
    [SerializeField] private List<NpcEntry> npcEntries = new();

    private IRandomAreaEventHost host;
    private NpcEntry currentEntry;
    private RandomEventNpc activeNpc;
    private DialogueRunner dialogueRunner;
    private Phase phase;
    private bool recordingChoice;
    private string choiceNodeID;
    private int choiceIndex = -1;
    private Coroutine dialogueRoutine;

    public RandomEventData CurrentEvent { get; private set; }
    public string CurrentNpcID => currentEntry?.npc != null ? currentEntry.npc.NpcID : null;
    public bool CanLeave => phase == Phase.Done && (host == null || !host.IsDialoguePlaying);

    public bool IsUIOpen()
    {
        return isActiveAndEnabled && CurrentEvent != null;
    }

    public void Show(RandomEventData data, IRandomAreaEventHost eventHost)
    {
        host = eventHost;
        CurrentEvent = data;
        BindNextButton();
        SetPhase(Phase.Talking);
        BindDialogueRunner();
        HideAllNpcs();

        currentEntry = data != null ? npcEntries.Find(entry => entry.eventName == data.Name) : null;
        if (currentEntry == null || currentEntry.npc == null)
        {
            Debug.LogWarning($"[AreaEventUI_Random] '{data?.Name}' 이벤트에 연결된 NPC가 없어 바로 종료할 수 있게 합니다.", this);
            Finish();
            return;
        }

        ShowNpc(currentEntry.npc);
    }

    public void HandleBattleWon()
    {
        RandomEventNpc afterBattleNpc = currentEntry?.afterBattleNpc;
        if (afterBattleNpc == null)
        {
            Finish();
            return;
        }

        ShowNpc(afterBattleNpc);
        if (host.HasRunnableEvent(afterBattleNpc.NpcID))
        {
            SetPhase(Phase.AfterBattle);
            return;
        }

        Debug.Log($"[AreaEventUI_Random] {afterBattleNpc.NpcID}: 출력할 대화가 없는 상태로 등장합니다.", this);
        Finish();
    }

    private void HandleNpcClicked(RandomEventNpc npc)
    {
        if (host == null || npc != activeNpc || host.IsDialoguePlaying || cardPanel.IsOpen)
        {
            return;
        }

        switch (phase)
        {
            case Phase.Talking:
                InteractTalking(npc);
                break;
            case Phase.AfterBattle:
                InteractAfterBattle(npc);
                break;
        }
    }

    private void InteractTalking(RandomEventNpc npc)
    {
        ResetChoice();
        recordingChoice = true;
        if (!host.TryInteract(npc.NpcID, GetSpecialAction()))
        {
            recordingChoice = false;
            Debug.Log($"[AreaEventUI_Random] {npc.NpcID}: 실행할 대화나 고유 행동이 없어 이벤트를 종료합니다.", this);
            Finish();
            return;
        }

        if (!host.IsDialoguePlaying)
        {
            recordingChoice = false;
            return;
        }

        WaitDialogueEnd(ResolveDialogue);
    }

    private void InteractAfterBattle(RandomEventNpc npc)
    {
        if (!host.TryInteract(npc.NpcID, null) || !host.IsDialoguePlaying)
        {
            Finish();
            return;
        }

        WaitDialogueEnd(Finish);
    }

    private void ResolveDialogue()
    {
        recordingChoice = false;
        if (choiceIndex < 0)
        {
            return;
        }

        string nodeID = choiceNodeID;
        int index = choiceIndex;
        ResetChoice();
        ApplyChoice(nodeID, index);
    }

    private void ApplyChoice(string nodeID, int index)
    {
        if (!RandomEventChoiceTable.TryGet(nodeID, index, out RandomEventChoiceResult result))
        {
            Debug.Log($"[AreaEventUI_Random] 결과가 정의되지 않은 선택지입니다: {nodeID} #{index}", this);
            Finish();
            return;
        }

        if (result.HpLoss > 0 && LoseHp(result.HpLoss))
        {
            SetPhase(Phase.Busy);
            host.Fail();
            return;
        }

        if (result.Heal > 0)
        {
            DataCenter.Instance.SetPlayerHP(result.Heal);
        }

        if (result.AttackGain != 0)
        {
            Debug.Log($"[AreaEventUI_Random] 기본 공격력 +{result.AttackGain} (스탯 시스템 없음)", this);
        }

        if (result.MaxHpGain != 0)
        {
            Debug.Log($"[AreaEventUI_Random] 최대 체력 +{result.MaxHpGain} (스탯 시스템 없음)", this);
        }

        if (!string.IsNullOrEmpty(result.MagicID))
        {
            Debug.Log($"[AreaEventUI_Random] 마법 {result.MagicID} 획득 (마법 시스템 없음)", this);
        }

        if (result.CardReward)
        {
            OpenCardReward();
            return;
        }

        if (result.Battle)
        {
            StartBattle();
            return;
        }

        Finish();
    }

    private Action GetSpecialAction()
    {
        return currentEntry.specialAction switch
        {
            RandomEventSpecialAction.ElementalTrade => StartElementalTrade,
            RandomEventSpecialAction.ClownPromotion => StartClownPromotion,
            _ => null
        };
    }

    private void StartElementalTrade()
    {
        SetPhase(Phase.Busy);
        cardPanel.Open("대가로 낼 카드를 선택하세요",
            RandomEventCardUtil.GetDeckCards(card => !RandomEventCardUtil.IsBasic(card)),
            true,
            card =>
            {
                RandomEventCardUtil.RemoveFromDeck(card);
                Debug.Log($"[AreaEventUI_Random] {card.grade}등급 마법 1장 획득 (마법 시스템 없음)", this);
                Finish();
            },
            Finish);
    }

    private void StartClownPromotion()
    {
        SetPhase(Phase.Busy);
        cardPanel.Open("승급할 0성 카드를 선택하세요",
            RandomEventCardUtil.GetDeckCards(RandomEventCardUtil.CanUpgradeStar),
            true,
            card =>
            {
                CardData upgraded = RandomEventCardUtil.UpgradeStar(card);
                if (upgraded == null)
                {
                    Debug.LogWarning($"[AreaEventUI_Random] {card.id} 카드를 승급하지 못했습니다.", this);
                    Finish();
                    return;
                }

                OpenClownPayment(upgraded);
            },
            Finish);
    }

    private void OpenClownPayment(CardData upgraded)
    {
        cardPanel.Open("대가로 지불할 카드를 선택하세요",
            RandomEventCardUtil.GetDeckCards(card => card.grade == upgraded.grade && !ReferenceEquals(card, upgraded)),
            true,
            card =>
            {
                RandomEventCardUtil.RemoveFromDeck(card);
                Finish();
            },
            StartBattle);
    }

    private void OpenCardReward()
    {
        SetPhase(Phase.Busy);
        List<CardData> cards = RandomEventCardUtil.CreateRewardCards(RewardCardCount);
        if (cards.Count == 0)
        {
            Debug.LogWarning("[AreaEventUI_Random] 보상으로 보여줄 카드가 없습니다.", this);
            Finish();
            return;
        }

        cardPanel.Open("카드를 1장 선택하세요", cards, false, card =>
        {
            RandomEventCardUtil.AddToDeck(card);
            Finish();
        }, null);
    }

    private void StartBattle()
    {
        SetPhase(Phase.Busy);
        if (host.TryStartBattle())
        {
            return;
        }

        Debug.LogWarning("[AreaEventUI_Random] 전투를 시작할 수 없어 이벤트를 종료합니다.", this);
        Finish();
    }

    private bool LoseHp(int amount)
    {
        Player player = FindFirstObjectByType<Player>(FindObjectsInactive.Include);
        DataCenter.Instance.SetPlayerHP(-amount);
        if (player == null)
        {
            Debug.LogWarning("[AreaEventUI_Random] Player를 찾을 수 없어 사망 판정을 건너뜁니다.", this);
            return false;
        }

        return !player.IsAlive;
    }

    private void Finish()
    {
        recordingChoice = false;
        SetPhase(Phase.Done);
    }

    private void SetPhase(Phase value)
    {
        phase = value;
        RefreshNextButton();
    }

    private void WaitDialogueEnd(Action onEnded)
    {
        if (dialogueRoutine != null)
        {
            StopCoroutine(dialogueRoutine);
        }

        dialogueRoutine = StartCoroutine(WaitDialogueEndRoutine(onEnded));
    }

    private IEnumerator WaitDialogueEndRoutine(Action onEnded)
    {
        yield return new WaitUntil(() => host == null || !host.IsDialoguePlaying);
        dialogueRoutine = null;
        onEnded?.Invoke();
    }

    private void HandleChoiceSelected(string nodeID, int index)
    {
        if (!recordingChoice)
        {
            return;
        }

        choiceNodeID = nodeID;
        choiceIndex = index;
    }

    private void ResetChoice()
    {
        choiceNodeID = null;
        choiceIndex = -1;
    }

    private void ShowNpc(RandomEventNpc npc)
    {
        if (activeNpc != null)
        {
            activeNpc.Clicked -= HandleNpcClicked;
            activeNpc.gameObject.SetActive(false);
        }

        activeNpc = npc;
        activeNpc.gameObject.SetActive(true);
        activeNpc.Clicked += HandleNpcClicked;
    }

    private void HideAllNpcs()
    {
        foreach (NpcEntry entry in npcEntries)
        {
            if (entry.npc != null) entry.npc.gameObject.SetActive(false);
            if (entry.afterBattleNpc != null) entry.afterBattleNpc.gameObject.SetActive(false);
        }
    }

    private void BindNextButton()
    {
        nextButton.onClick.RemoveListener(Next);
        nextButton.onClick.AddListener(Next);
    }

    private void RefreshNextButton()
    {
        nextButton.gameObject.SetActive(phase == Phase.Done);
    }

    private void Next()
    {
        if (!IsUIOpen() || !CanLeave)
        {
            return;
        }

        host.Complete();
    }

    private void BindDialogueRunner()
    {
        if (dialogueRunner == null)
        {
            dialogueRunner = FindFirstObjectByType<DialogueRunner>(FindObjectsInactive.Exclude);
        }

        if (dialogueRunner == null)
        {
            Debug.LogWarning("[AreaEventUI_Random] DialogueRunner를 찾을 수 없어 선택지 결과를 받을 수 없습니다.", this);
            return;
        }

        dialogueRunner.OnChoiceSelected -= HandleChoiceSelected;
        dialogueRunner.OnChoiceSelected += HandleChoiceSelected;
    }

    private void OnDestroy()
    {
        if (nextButton != null) nextButton.onClick.RemoveListener(Next);
        if (dialogueRunner != null) dialogueRunner.OnChoiceSelected -= HandleChoiceSelected;
        if (activeNpc != null) activeNpc.Clicked -= HandleNpcClicked;
        host = null;
    }
}
