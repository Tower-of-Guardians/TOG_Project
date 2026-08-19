using JxModule.DataTable;

namespace JxDialogueBox
{
    public class DialogueChoiceDataTableRow : DataTableRowBase
    {
        public string nodeID;
        public int optionIndex;
        public string text;
        public string nextID;
    }
}