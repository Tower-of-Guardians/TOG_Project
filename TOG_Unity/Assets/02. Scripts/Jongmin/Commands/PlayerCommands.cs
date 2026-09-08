using JxModule.Terminal;
using UnityEngine;

namespace Jongmin
{
    public sealed class PlayerCommands
    {
        [JxCommand("Update Gold")]
        private void UpdateGold(int amount)
        {
            DataCenter.Instance.SetMoney(amount);
            Debug.Log($"플레이어의 골드를 {amount}만큼 변경합니다.");
        }

        [JxCommand("Update Hp")]
        private void UpdateHp(int amount)
        {
            DataCenter.Instance.SetPlayerHP(amount);
            Debug.Log($"플레이어의 체력을 {amount}만큼 변경합니다.");
        }

        [JxCommand("Update Exp")]
        private void UpdateExp(int amount)
        {
            DataCenter.Instance.SetPlayerLevel(amount);
            Debug.Log($"플레이어의 경험치를 {amount}만큼 변경합니다.");
        }
    }
}