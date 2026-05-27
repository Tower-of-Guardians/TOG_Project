using System;
using System.Collections.Generic;
using JxModule.DataTable;

namespace JxDialogueBox
{
    public class DialogueTableDataSource : IDialogueDataSource
    {
        private readonly DataTable _entryTable;
        private readonly DataTable _nodeTable;
        private readonly DataTable _choiceTable;
        
        private readonly Dictionary<string, DialogueNode> _nodeDict = new(StringComparer.Ordinal);
        private readonly Dictionary<string, string> _entryDict = new(StringComparer.Ordinal);

        public DialogueTableDataSource()
        {
            _entryTable = DataTableManager.FindTable<DialogueEntryDataTableRow>("DT_DialogueEntry");
            _nodeTable = DataTableManager.FindTable<DialogueNodeDataTableRow>("DT_DialogueNode");
            _choiceTable = DataTableManager.FindTable<DialogueChoiceDataTableRow>("DT_DialogueChoice");

            BuildCache();
        }
        
        public bool TryGetNode(string nodeID, out DialogueNode node)
        {
            return _nodeDict.TryGetValue(nodeID, out node);
        }

        public string GetEntryNodeID(string dialogueID)
        {
            return _entryDict.TryGetValue(dialogueID, out var entryNodeID) ?  entryNodeID : string.Empty;
        }

        private void BuildCache()
        {
            BuildEntryDict();
            BuildNodeDict();
        }

        private void BuildEntryDict()
        {
            foreach (var row in _entryTable.FindAll<DialogueEntryDataTableRow>())
            {
                if (string.IsNullOrEmpty(row.DialogueID))
                {
                    continue;
                }
                
                _entryDict[row.DialogueID] = row.EntryNodeID;
            }
        }

        private void BuildNodeDict()
        {
            foreach (var row in _nodeTable.FindAll<DialogueNodeDataTableRow>())
            {
                if (string.IsNullOrEmpty(row.rowID))
                {
                    continue;
                }

                var nodeType = ParseNodeType(row.NodeType);
                DialogueNode node = nodeType switch
                {
                    NodeType.Line => new LineNode(row.rowID,
                                                  new SpeakerRef(ParseSpeaker(row.Speaker), row.CharacterID),
                                                  row.Text ?? string.Empty,
                                                  row.PortraitKey ?? string.Empty,
                                                  row.NextID ?? string.Empty),

                    NodeType.Choice => new ChoiceNode(row.rowID,
                                                      row.Prompt ?? string.Empty,
                                                      BuildOptions(row.rowID)),

                    NodeType.Jump => new JumpNode(row.rowID, row.TargetID ?? string.Empty),
                    
                    _ => new EndNode(row.rowID)
                };

                _nodeDict[row.rowID] = node;
            }
        }
        
        private List<ChoiceOption> BuildOptions(string nodeID)
        {
            var options = new List<DialogueChoiceDataTableRow>();
            foreach (var row in _choiceTable.FindAll<DialogueChoiceDataTableRow>())
            {
                if (row.NodeID == nodeID)
                {
                    options.Add(row);
                }
            }

            options.Sort((a, b) => a.OptionIndex.CompareTo(b.OptionIndex));

            var result = new List<ChoiceOption>(options.Count);
            foreach (var option in options)
            {
                result.Add(new ChoiceOption(option.Text ?? string.Empty, option.NextID ?? string.Empty));
            }

            return result;
        }

        private static NodeType ParseNodeType(string value)
        {
            return Enum.TryParse<NodeType>(value, true, out var result) ? result : NodeType.End;
        }

        private static Speaker ParseSpeaker(string value)
        {
            return Enum.TryParse<Speaker>(value, true, out var result) ? result : Speaker.Npc;
        }
    }
}