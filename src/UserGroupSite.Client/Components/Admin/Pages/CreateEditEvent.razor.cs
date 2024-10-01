using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using Blazored.FluentValidation;
using Blazored.Toast.Services;
using Markdig;
using UserGroupSite.Client.Services;
using SharedConstants = UserGroupSite.Shared.Models.Constants;

namespace UserGroupSite.Client.Components.Admin.Pages;

public partial class CreateEditEvent : ComponentBase, IDisposable
{
    class PersistentData
    {
        public List<CategoryDto> Categories { get; set; }
        public List<UserDto> Users { get; set; }
    }
    private FluentValidationValidator _fluentValidationValidator = default!;
    
    private string _preview => Markdown.ToHtml(Dto?.Description ?? "");

    [SupplyParameterFromForm] private EventDto Dto { get; set; } = new();
    [SupplyParameterFromForm] private string ButtonAction { get; set; } = default!;

    [Parameter] public int? EventId { get; set; }

    private string _messageResult = default!;
    private bool _eventSaved = false;
    private bool _isEditing = false;
    
    private List<string> _errorMessages { get; set; } = new();
    
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
        
        // if (HttpClient.BaseAddress is not null)
        // {
        //     _categories = await HttpClient.GetFromJsonAsync<List<CategoryDto>>($"{SharedConstants.CategoryApiUrl}");
        //     _users = await HttpClient.GetFromJsonAsync<List<UserDto>>($"{SharedConstants.UserApiUrl}");
        //     Dto.CategoryId = _categories.FirstOrDefault().CategoryId;
        //     Dto.SpeakerId = _users.FirstOrDefault().UserId;
        //     
        //     if (EventId.HasValue)
        //     {
        //         _isEditing = true;
        //         if (Dto.EventId == default)
        //         {
        //             Dto = await HttpClient.GetFromJsonAsync<EventDto>($"{SharedConstants.EventApiUrl}/{EventId}");
        //         }
        //     }
        // }
        
        // _categories = await DbContext.Categories.ToListAsync();
        // _users = await DbContext.Users.ToListAsync();
        //
        // if (EventId.HasValue)
        // {
        //     _isEditing = true;
        //     if (Dto.EventId == default)
        //     {
        //         var speakingEvent = await DbContext.SpeakingEvents.SingleOrDefaultAsync(se => se.Id == EventId);
        //         if (speakingEvent is not null)
        //         {
        //             Dto = speakingEvent.ToDto();
        //         }
        //     }
        // }
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
            
            var jsonString = await result.Content.ReadAsStringAsync();
            JsonSerializerOptions options = new()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            if (result.IsSuccessStatusCode)
            {
                // Dto = JsonSerializer.Deserialize<EventDto>(jsonString, options);
                NavigationManager.NavigateTo("/admin/manageevents");
            }
            else
            {
                ToastService.ShowError("Unable to save the event, please correct the errors and try again.");
            }
            // if (Dto.EventId > 0) // update the event
            // {
            //     Logger.LogInformation("Attempting to update event");
            //     var speakingEvent = await DbContext.SpeakingEvents.SingleOrDefaultAsync(s => s.Id == Dto.EventId);
            //     speakingEvent.FromDto(Dto);
            //     
            //     var saveResult = await DbContext.SaveChangesAsync();
            //     if (saveResult > 0)
            //     {
            //         NavigationManager.NavigateTo($"/Admin/ManageEvents/");
            //     }
            //     else
            //     {
            //         _messageResult = "There was an error updating the category, please try again.";
            //         _eventSaved = false;
            //     }
            // }
            // else // need to insert
            // {
            //     Logger.LogInformation("Attempting to create event");
            //     
            //     var newEvent = new SpeakingEvent();
            //
            //     newEvent.FromDto(Dto);
            //
            //     DbContext.SpeakingEvents.Add(newEvent);
            //
            //     var saveResult = await DbContext.SaveChangesAsync();
            //     if (saveResult > 0)
            //     {
            //         NavigationManager.NavigateTo("/Admin/ManageEvents");
            //     }
            //     else
            //     {
            //         _messageResult = "There was an error creating the category, please try again.";
            //         _eventSaved = false;
            //     }
            // }
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
        var persistedData = new PersistentData()
        {
            Categories = _categories,
            Users = _users,
        };
        // PersistentComponentState.PersistAsJson("persistedData", persistedData);
        
        // PersistentComponentState.PersistAsJson("someValue", "Just a test");
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