using JxModule.Terminal;
using UnityEngine;

namespace Jongmin
{
    public sealed class TogTerminalCommandInstaller : JxTerminalCommandInstallerBehaviour
    {
        [SerializeField] private RelicDomain relicDomain;
        [SerializeField] private HandDomain handDomain;
        [SerializeField] private FieldDomain fieldDomain;
        
        public override void Install(JxTerminal terminal)
        {
            var relicCommands = new RelicCommands(relicDomain);
            JxCommandAttributeRegistrar.RegisterCommands(terminal, relicCommands);
            
            var cardCommands = new CardCommands(handDomain, fieldDomain);
            JxCommandAttributeRegistrar.RegisterCommands(terminal, cardCommands);

            var playCommands = new PlayerCommands();
            JxCommandAttributeRegistrar.RegisterCommands(terminal, playCommands);
        }
    }
}