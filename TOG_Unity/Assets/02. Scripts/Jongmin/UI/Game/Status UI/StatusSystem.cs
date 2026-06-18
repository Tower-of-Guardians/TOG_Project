using UnityEngine;

namespace Jongmin
{
    public class StatusSystem : MonoBehaviour
    {
        private StatusView _view;

        public void Construct(StatusView view)
        {
            _view = view;
        }

        public void Initialize(PlayerState playerState)
        {
            UpdateLevel(playerState.level, playerState.experience);
            UpdateGold(playerState.money);
        }

        public void UpdateLevel(int level, int exp)
        {
            var expRatio = (float)exp / DataCenter.Instance.playerstate.maxexperience;
            _view.UpdateLevel(level, expRatio);
        }

        public void UpdateGold(int gold)
        {
            _view.UpdateGold(gold);
        }
    }
}

