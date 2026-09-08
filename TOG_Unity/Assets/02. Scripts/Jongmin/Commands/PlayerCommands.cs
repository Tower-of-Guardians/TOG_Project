using JxModule.Terminal;

namespace Jongmin
{
    public sealed class PlayerCommands
    {
        [JxCommand("Update Gold")]
        private void UpdateGold(int amount)
        {
            DataCenter.Instance.SetMoney(amount);
        }

        [JxCommand("Update Hp")]
        private void UpdateHp(int amount)
        {
            DataCenter.Instance.SetPlayerHP(amount);
        }

        [JxCommand("Update Exp")]
        private void UpdateExp(int amount)
        {
            DataCenter.Instance.SetPlayerLevel(amount);
        }
    }
}