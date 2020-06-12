using CourseLibrary.API.Models;
using System.ComponentModel.DataAnnotations;

namespace CourseLibrary.API.ValidationAttributes
{
  public class CourseTitleCannotBeTitle : ValidationAttribute
  {
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
      var course = (CourseForManipulationDto)validationContext.ObjectInstance;
      if (course.Title.ToLower() == "title")
      {
        return new ValidationResult(
          "The title can't just be \"Title\".",
          new[] { nameof(CourseForManipulationDto) });
      }

      return ValidationResult.Success;
    }
  }
}
