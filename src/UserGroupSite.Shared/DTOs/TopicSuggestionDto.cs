using FluentValidation;

namespace UserGroupSite.Shared.DTOs;

public class TopicSuggestionDto
{
    public string Title { get; set; }
    public string Description { get; set; }
}

public class TopicSuggestionDtoValidator : AbstractValidator<TopicSuggestionDto>
{
    public TopicSuggestionDtoValidator()
    {
        RuleFor(viewModel => viewModel.Title).NotEmpty().WithMessage("Title is required");
        RuleFor(viewModel => viewModel.Description).NotEmpty().WithMessage("Description is required");
    }
}