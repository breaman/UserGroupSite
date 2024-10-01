using Microsoft.EntityFrameworkCore;
using UserGroupSite.Shared.DTOs;

namespace UserGroupSite.Server.Components.Pages;

public partial class TopicSuggestions : ComponentBase
{
    private bool? _isAuthenticated;
    private List<TopicSuggestion> _topicSuggestions = [];
    
    [Inject]
    private IHttpContextAccessor HttpContextAccessor { get; set; } = default!;
    
    [Inject]
    private ApplicationDbContext DbContext { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        _isAuthenticated = HttpContextAccessor?.HttpContext?.User.Identity?.IsAuthenticated;

        // _topicSuggestions = await DbContext.TopicSuggestions.Where(ts => ts.IsApproved).ToListAsync();
    }
}