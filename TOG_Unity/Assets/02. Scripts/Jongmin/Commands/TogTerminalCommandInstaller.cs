using JxModule.Terminal;
using UnityEngine;

namespace Jongmin
{
    public sealed class TogTerminalCommandInstaller : JxTerminalCommandInstallerBehaviour
    {
        [SerializeField] private RelicDomain relicDomain;
        
        public override void Install(JxTerminal terminal)
        {
            var relicCommands = new RelicCommands(relicDomain);
            JxCommandAttributeRegistrar.RegisterCommands(terminal, relicCommands);
        }
    }
}