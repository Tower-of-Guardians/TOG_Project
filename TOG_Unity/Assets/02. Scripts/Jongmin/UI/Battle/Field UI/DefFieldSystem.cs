namespace Jongmin
{
    public class DefFieldSystem : FieldSystem, IDEFCardDropTarget
    {
        public override FieldType FieldType => FieldType.Defense;
        
        public override void CreateCard(BattleCardData battleCardData)
        {
            if (!CanAdd)
            {
                return;
            }
            
            var card = Factory.Create();
            card.SetBattleCardData(battleCardData, CardType.DefField);
            card.View.LockAtk();
            Container.Add(card);
            Layout.UpdateLayout(FieldPreviewMode.None, isAnime:false);
            GameData.Instance.defenseField.Add(card.CardData);
            UpdateFieldStatus();
            RequestUpdateActionCountEvent(1);
        }
        
        public override void RemoveCard(Card card, bool unused = true)
        {
            GameData.Instance.defenseField.Remove(card.CardData);
            UpdateFieldStatus();
            base.RemoveCard(card, unused);
        }
        
        public override void UpdateFieldStatus()
        {
            var targetStatus = GameData.Instance.DefenseField();
            View.UpdateStatus(CurrentStatus, targetStatus);
            CurrentStatus = targetStatus;
        }
    }
}