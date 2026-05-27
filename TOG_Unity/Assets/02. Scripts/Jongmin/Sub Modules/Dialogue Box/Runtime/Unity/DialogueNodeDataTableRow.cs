using JxModule.DataTable;

namespace JxDialogueBox
{
    public class DialogueNodeDataTableRow : DataTableRowBase
    {
        public string NodeType;

        public string Speaker;
        public string CharacterID;
        public string Text;
        public string PortraitKey;

        public string NextID;

        public string Prompt;
        public string TargetID;
    }
}