namespace UserGroupSite.Server.Components;

public partial class SuggestedTopic : ComponentBase
{
    [Parameter]
    public bool? IsAuthenticated { get; set; }
    
    [Parameter]
    public TopicSuggestion? Suggestion { get; set; }
}