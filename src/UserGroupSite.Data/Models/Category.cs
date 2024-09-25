using System.ComponentModel.DataAnnotations;
using UserGroupSite.Shared.DTOs;

namespace UserGroupSite.Data.Models;

public class Category : FingerPrintEntityBase
{
    [MaxLength(50)]
    public string? Name { get; set; }
    [MaxLength(10)]
    public string? CategoryAbbreviation { get; set; }
    [MaxLength(8)]
    public string? BackgroundColor { get; set; }
    
    public CategoryDto ToDto()
    {
        return new CategoryDto
        {
            CategoryId = Id,
            Name = Name,
            BackgroundColor = BackgroundColor,
            CategoryAbbreviation = CategoryAbbreviation
        };
    }

    public void FromDto(CategoryDto dto)
    {
        Name = dto.Name;
        BackgroundColor = dto.BackgroundColor;
        CategoryAbbreviation = dto.CategoryAbbreviation;
    }
}