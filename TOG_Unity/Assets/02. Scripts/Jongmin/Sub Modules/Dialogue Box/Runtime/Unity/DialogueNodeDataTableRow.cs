using JxModule.DataTable;

namespace JxDialogueBox
{
    public class DialogueNodeDataTableRow : DataTableRowBase
    {
        public string nodeType;

        public string speaker;
        public string characterID;
        public string text;
        public string portraitKey;

        public string nextID;

        public string prompt;
        public string targetID;
    }
}