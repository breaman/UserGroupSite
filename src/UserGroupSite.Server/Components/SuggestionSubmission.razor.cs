using Blazored.FluentValidation;
using Microsoft.EntityFrameworkCore;
using UserGroupSite.Shared.DTOs;

namespace UserGroupSite.Server.Components;

public partial class SuggestionSubmission : ComponentBase
{
    [Parameter]
    public bool? IsAuthenticated { get; set; }
    
    [SupplyParameterFromForm]
    private TopicSuggestionDto Dto { get; set; } = new();
    
    private FluentValidationValidator _fluentValidationValidator = default!;
    private string _messageResult = default!;
    private bool _suggestionSaved = false;
    
    [Inject]
    private ApplicationDbContext DbContext { get; set; } = default!;
    
    [Inject]
    private ILogger<SuggestionSubmission> Logger { get; set; } = default!;
    
    private async Task SubmitSuggestion()
    {
        // if (await _fluentValidationValidator.ValidateAsync())
        // {
        //     Logger.LogInformation("Attempting to create suggestion");
        //     DbContext.TopicSuggestions.Add(new TopicSuggestion()
        //     {
        //         IsApproved = false,
        //         Title = Dto.Title,
        //         Description = Dto.Description
        //     });
        //     var saveResult = await DbContext.SaveChangesAsync();
        //     if (saveResult > 0)
        //     {
        //         _messageResult = "Your suggestion was successfully created and is just waiting for approval.";
        //         _suggestionSaved = true;
        //     }
        //     else
        //     {
        //         _messageResult = "There was an error creating the suggestion, please try again.";
        //         _suggestionSaved = false;
        //     }
        // }
    }
}