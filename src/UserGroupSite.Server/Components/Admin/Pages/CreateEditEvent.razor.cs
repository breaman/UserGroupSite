using System.ComponentModel;
using System.Text.RegularExpressions;
using Blazored.FluentValidation;
using Markdig;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using UserGroupSite.Shared.DTOs;

namespace UserGroupSite.Server.Components.Admin.Pages;

public partial class CreateEditEvent : ComponentBase
{
    private FluentValidationValidator _fluentValidationValidator = default!;
    
    private string _preview => Markdown.ToHtml(Dto?.Description ?? "");

    [SupplyParameterFromForm] private EventDto Dto { get; set; } = new();
    [SupplyParameterFromForm] private string ButtonAction { get; set; } = default!;

    [Parameter] public int? EventId { get; set; }

    private string _messageResult = default!;
    private bool _eventSaved = false;
    private bool _isEditing = false;
    
    private List<Category> _categories = new();
    private List<User> _users = new();

    [Inject] private ApplicationDbContext DbContext { get; set; } = default!;
    [Inject] private ILogger<CreateEditEvent> Logger { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private IHttpContextAccessor HttpContextAccessor { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        _categories = await DbContext.Categories.ToListAsync();
        _users = await DbContext.Users.ToListAsync();
        
        if (EventId.HasValue)
        {
            _isEditing = true;
            if (Dto.EventId == default)
            {
                var speakingEvent = await DbContext.SpeakingEvents.SingleOrDefaultAsync(se => se.Id == EventId);
                if (speakingEvent is not null)
                {
                    Dto = speakingEvent.ToDto();
                }
            }
        }
    }

    private async Task DeleteEvent(int id)
    {
        Console.WriteLine("Trying to delete an event");
    }
    private async Task SaveEvent()
    {
        if (await _fluentValidationValidator.ValidateAsync())
        {
            if (Dto.EventId > 0) // update the event
            {
                Logger.LogInformation("Attempting to update event");
                var speakingEvent = await DbContext.SpeakingEvents.SingleOrDefaultAsync(s => s.Id == Dto.EventId);
                speakingEvent.FromDto(Dto);
                
                var saveResult = await DbContext.SaveChangesAsync();
                if (saveResult > 0)
                {
                    NavigationManager.NavigateTo($"/Admin/ManageEvents/");
                }
                else
                {
                    _messageResult = "There was an error updating the category, please try again.";
                    _eventSaved = false;
                }
            }
            else // need to insert
            {
                Logger.LogInformation("Attempting to create event");
                
                var newEvent = new SpeakingEvent();

                newEvent.FromDto(Dto);
            
                DbContext.SpeakingEvents.Add(newEvent);
            
                var saveResult = await DbContext.SaveChangesAsync();
                if (saveResult > 0)
                {
                    NavigationManager.NavigateTo("/Admin/ManageEvents");
                }
                else
                {
                    _messageResult = "There was an error creating the category, please try again.";
                    _eventSaved = false;
                }
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
}