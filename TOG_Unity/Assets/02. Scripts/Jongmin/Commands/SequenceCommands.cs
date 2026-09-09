using JxModule.Terminal;
using UnityEngine;

namespace Jongmin
{
    public class SequenceCommands
    {
        private readonly BattleManager _battleManager;
        
        public SequenceCommands(BattleManager battleManager)
        {
            _battleManager = battleManager;
        }

        [JxCommand("Skip Current Stage")]
        private void SkipCurrentStage()
        {
            if (!_battleManager)
            {
                Debug.Log("Battle Manager가 존재하지 않습니다.");
                return;
            }

            if (_battleManager.IsProcessingAttack())
            {
                Debug.Log("공격 중엔 스테이지를 건너뛸 수 없습니다.");
                return;
            }
            
            _battleManager.ForceVictoryForDebug();
            Debug.Log("현재 스테이지를 건너뜁니다.");
        }
    }
}