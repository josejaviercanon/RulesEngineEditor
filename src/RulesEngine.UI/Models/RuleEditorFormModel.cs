namespace RulesEngine.UI.Models;

public sealed class RuleEditorFormModel
{
    public string RuleName { get; set; } = string.Empty;

    public string Expression { get; set; } = string.Empty;

    public int ExecuteOrder { get; set; } = 1;

    public bool IsActive { get; set; } = true;
}
