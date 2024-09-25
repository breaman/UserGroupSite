using FluentValidation;

namespace UserGroupSite.Shared.DTOs;

public class CategoryDto
{
    public int CategoryId { get; set; }
    public string? Name { get; set; }
    public string? CategoryAbbreviation { get; set; }
    public string? BackgroundColor { get; set; }
}

public class CategoryDtoValidator : AbstractValidator<CategoryDto>
{
    public CategoryDtoValidator()
    {
        RuleFor(viewModel => viewModel.Name).NotEmpty();
        RuleFor(viewModel => viewModel.CategoryAbbreviation).NotEmpty();
        RuleFor(viewModel => viewModel.BackgroundColor).NotEmpty();
    }
}