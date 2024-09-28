using System.Net.Http.Json;
using System.Text.Json;
using FluentValidation;
using UserGroupSite.Shared.DTOs;
using UserGroupSite.Shared.Models;
using SharedConstants = UserGroupSite.Shared.Models.Constants;

namespace UserGroupSite.Shared.ViewModels;

public class EventViewModel
{
    public EventDto EventInfo { get; set; }
    
    public List<string> ErrorMessages { get; set; } = new();
    
    public async Task CreateEvent(HttpClient client)
    {
        ErrorMessages.Clear();
        await UpsertEvent(client);
    }
    
    private async Task UpsertEvent(HttpClient client)
    {
        HttpResponseMessage? result = null;

        // if (EventInfo.EventId > 0)
        //     result = await client.PutAsJsonAsync($"{SharedConstants.EventApiUrl}/update/{EventInfo.EventId}", EventInfo);
        // else
        //     result = await client.PostAsJsonAsync($"{SharedConstants.EventApiUrl}/create", EventInfo);

        var jsonString = await result.Content.ReadAsStringAsync();
        JsonSerializerOptions options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        if (result.IsSuccessStatusCode)
        {
            EventInfo = JsonSerializer.Deserialize<EventDto>(jsonString, options);
        }
        else
        {
            var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(jsonString, options);
            foreach (var errors in problemDetails.Errors)
            {
                foreach (var errorMessage in errors.Value)
                {
                    ErrorMessages.Add(errorMessage);
                }
            }
        }
    }
}

public class EventViewModelValidator : AbstractValidator<EventViewModel>
{
    public EventViewModelValidator()
    {
        RuleFor(vm => vm.EventInfo).SetValidator(new EventDtoValidator());
    }
}