namespace JxModule
{
    public class ShowIfAttribute : ConditionalVisibilityAttribute
    {
        public ShowIfAttribute(string conditionName)
            : base(conditionName, false)
        { }
    }
}