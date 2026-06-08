namespace JxModule
{
    public class HideIfAttribute : ConditionalVisibilityAttribute
    {
        public HideIfAttribute(string conditionName)
            : base(conditionName, true)
        { }
    }
}