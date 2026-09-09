using JxModule.Terminal;
using UnityEngine;

namespace Jongmin
{
    public sealed class TogTerminalCommandInstaller : JxTerminalCommandInstallerBehaviour
    {
        [SerializeField] private RelicDomain relicDomain;
        [SerializeField] private HandDomain handDomain;
        [SerializeField] private FieldDomain fieldDomain;
        [SerializeField] private EventDomain eventDomain;
        [SerializeField] private BattleManager battleManager;
        
        public override void Install(JxTerminal terminal)
        {
            var relicCommands = new RelicCommands(relicDomain);
            JxCommandAttributeRegistrar.RegisterCommands(terminal, relicCommands);
            
            var cardCommands = new CardCommands(handDomain, fieldDomain, eventDomain);
            JxCommandAttributeRegistrar.RegisterCommands(terminal, cardCommands);

            var playCommands = new PlayerCommands();
            JxCommandAttributeRegistrar.RegisterCommands(terminal, playCommands);
            
            var eventCommands = new EventCommands(eventDomain);
            JxCommandAttributeRegistrar.RegisterCommands(terminal, eventCommands);
            
            var sequenceCommands = new SequenceCommands(battleManager);
            JxCommandAttributeRegistrar.RegisterCommands(terminal, sequenceCommands);
        }
    }
}