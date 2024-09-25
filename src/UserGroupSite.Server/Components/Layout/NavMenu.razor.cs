using Microsoft.AspNetCore.Components.Routing;

namespace UserGroupSite.Server.Components.Layout;

public partial class NavMenu : ComponentBase
{
    private string? _currentUrl;

    private string LoginPath => $"/account/login?returnUrl={_currentUrl}";

    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    protected override void OnInitialized()
    {
        _currentUrl = NavigationManager.ToBaseRelativePath(NavigationManager.Uri);
        NavigationManager.LocationChanged += OnLocationChanged;
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        _currentUrl = NavigationManager.ToBaseRelativePath(e.Location);
        StateHasChanged();
    }

    public void Dispose()
    {
        NavigationManager.LocationChanged -= OnLocationChanged;
    }
}