using Blazored.FluentValidation;
using Microsoft.EntityFrameworkCore;
using UserGroupSite.Shared.DTOs;

namespace UserGroupSite.Server.Components.Admin.Pages;

public partial class ManageCategories : ComponentBase
{
    private List<Category> _categories = [];
    [SupplyParameterFromForm]
    private CategoryDto Dto { get; set; } = new();
    
    private FluentValidationValidator _fluentValidationValidator = default!;
    private string _messageResult = default!;
    private bool _categorySaved = false;
    private string _actionType = "Create";
    
    [Parameter]
    public int? CategoryId { get; set; }
    
    [Inject]
    private ApplicationDbContext DbContext { get; set; } = default!;
    
    [Inject]
    private ILogger<ManageCategories> Logger { get; set; } = default!;
    
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        if (CategoryId.HasValue && Dto.CategoryId == default)
        {
            var category = await DbContext.Categories.SingleOrDefaultAsync(c => c.Id == CategoryId);
            if (category is not null)
            {
                Dto = category.ToDto();
                _actionType = "Update";
            }
        }
        
        _categories = await DbContext.Categories.ToListAsync();
    }

    private async Task SubmitCategory()
    {
        if (await _fluentValidationValidator.ValidateAsync())
        {
            if (Dto.CategoryId > 0)
            {
                // need to update
                Logger.LogInformation("Attempting to update category");
                var category = await DbContext.Categories.SingleOrDefaultAsync(c => c.Id == Dto.CategoryId);
                category.FromDto(Dto);
                // DbContext.Categories.Update(category);
                var saveResult = await DbContext.SaveChangesAsync();
                if (saveResult > 0)
                {
                    NavigationManager.NavigateTo($"/Admin/ManageCategories/");
                }
                else
                {
                    _messageResult = "There was an error updating the category, please try again.";
                    _categorySaved = false;
                }
            }
            else // need to insert
            {
                Logger.LogInformation("Attempting to create category");
                var newCategory = new Category()
                {
                    Name = Dto.Name,
                    CategoryAbbreviation = Dto.CategoryAbbreviation,
                    BackgroundColor = Dto.BackgroundColor
                };
                DbContext.Categories.Add(newCategory);
            
                var saveResult = await DbContext.SaveChangesAsync();
                if (saveResult > 0)
                {
                    _categories.Add(newCategory);
                    Dto = new();
                }
                else
                {
                    _messageResult = "There was an error creating the category, please try again.";
                    _categorySaved = false;
                }
            }
        }
    }

    private async Task DeleteMe(int id)
    {
        Console.WriteLine("I did something");
    }
}