using System.Net.Http.Json;
using System.Text.RegularExpressions;
using Blazored.FluentValidation;
using Blazored.Toast.Services;
using UserGroupSite.Client.Services;
using SharedConstants = UserGroupSite.Shared.Models.Constants;

namespace UserGroupSite.Client.Components.Admin.Pages;

public partial class CreateEditEvent : ComponentBase, IDisposable
{
    private FluentValidationValidator _fluentValidationValidator = default!;

    [SupplyParameterFromForm] private EventDto Dto { get; set; } = new();
    [SupplyParameterFromForm] private string ButtonAction { get; set; } = default!;

    [Parameter] public int? EventId { get; set; }

    private string _messageResult = default!;
    private bool _eventSaved = false;
    private bool _isEditing = false;
    
    private List<CategoryDto> _categories = new();
    private List<UserDto> _users = new();
    
    private PersistingComponentStateSubscription _persistingSubscription;
    
    [Inject] private ILogger<CreateEditEvent> Logger { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private HttpClient HttpClient { get; set; } = default!;
    [Inject] private ICategoryService CategoryService { get; set; } = default!;
    [Inject] private IEventService EventService { get; set; } = default!;
    [Inject] private IApplicationUserService UserService { get; set; } = default!;
    [Inject] private IToastService ToastService { get; set; }
    [Inject] private PersistentComponentState PersistentComponentState { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        _persistingSubscription = PersistentComponentState.RegisterOnPersisting(PersistData);

        var foundInState =
            PersistentComponentState.TryTakeFromJson<List<CategoryDto>>(nameof(_categories),
                out var restoredCategoryData);
        _categories = foundInState ? restoredCategoryData! : await CategoryService.GetAllCategoriesAsync();

        foundInState =
            PersistentComponentState.TryTakeFromJson<List<UserDto>>(nameof(_users), out var restoredUserData);
        _users = foundInState ? restoredUserData! : await UserService.GetAllUsersAsync();

        if (EventId.HasValue)
        {
            _isEditing = true;
            if (Dto.EventId == default)
            {
                foundInState =
                    PersistentComponentState.TryTakeFromJson<EventDto>(nameof(Dto), out var restoredEventData);
                Dto = foundInState ? restoredEventData! : await EventService.GetEventByIdAsync(EventId.Value);
            }
        }
    }

    private async Task DeleteEvent(int id)
    {
        Console.WriteLine("Trying to delete an event");
        
        var result = await HttpClient.PostAsync($"{SharedConstants.EventApiUrl}/delete/{id}", null);

        if (result.IsSuccessStatusCode)
        {
            NavigationManager.NavigateTo("/admin/manageevents");
        }
    }
    
    private async Task SaveEvent()
    {
        HttpResponseMessage? result = null;
        
        if (await _fluentValidationValidator.ValidateAsync())
        {
            if (Dto.EventId > 0)
            {
                result = await HttpClient.PutAsJsonAsync($"{SharedConstants.EventApiUrl}/update/{EventId}", Dto);
            }
            else
            {
                result = await HttpClient.PostAsJsonAsync($"{SharedConstants.EventApiUrl}/create", Dto);
            }

            if (result.IsSuccessStatusCode)
            {
                // ToastService.ShowSuccess("The Event has been saved off correctly.");
                NavigationManager.NavigateTo("/admin/manageevents");
            }
            else
            {
                ToastService.ShowError("Unable to save the event, please correct the errors and try again.");
            }
        }
    }
    
    private async Task UpdateSlug(string title)
    {
        Dto.Title = title;
        if (string.IsNullOrWhiteSpace(Dto.Slug))
        {
            title = title.ToLower();
            title = Regex.Replace(title, "[^a-zA-Z0-9 -]", "");
            title = title.Trim().Replace(' ', '-');
            Dto.Slug = title;
        }
    }

    private void ReplaceSlug()
    {
        var title = Dto.Title.ToLower();
        title = Regex.Replace(title, "[^a-zA-Z0-9 -]", "");
        title = title.Trim().Replace(' ', '-');
        Dto.Slug = title;
    }

    private Task PersistData()
    {
        PersistentComponentState.PersistAsJson(nameof(_categories), _categories);
        PersistentComponentState.PersistAsJson(nameof(_users), _users);
        if (_isEditing)
        {
            PersistentComponentState.PersistAsJson(nameof(Dto), Dto);
        }

        return Task.CompletedTask;
    }
    
    void IDisposable.Dispose() => _persistingSubscription.Dispose();
}