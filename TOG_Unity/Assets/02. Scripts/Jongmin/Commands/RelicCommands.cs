using System.Collections.Generic;
using System.Linq;
using JxModule.DataTable;
using JxModule.Terminal;

namespace Jongmin
{
    public sealed class RelicCommands
    {
        private readonly RelicDomain _relicDomain;

        public RelicCommands(RelicDomain relicDomain)
        {
            _relicDomain = relicDomain;
        }

        [JxCommand("Relic List")]
        private IEnumerable<string> ListRelics()
        {
            return DataTableManager
                .FindAllRows<RelicDataTableRow>()
                .Where(x => x.isEnable)
                .Select(x => x.rowID);
        }

        [JxCommand("Add Relic")]
        private void AddRelic([JxOptionValue("Relic List")] string relicID)
        {
            _relicDomain.TryAddRelic(relicID);
        }

        [JxCommand("Remove Relic")]
        private void RemoveRelic([JxOptionValue("Relic List")] string relicID)
        {
            _relicDomain.TryRemoveRelic(relicID);
        }
    }
}