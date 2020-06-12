using CourseLibrary.API.ValidationAttributes;
using System.ComponentModel.DataAnnotations;

namespace CourseLibrary.API.Models
{
  [CourseTitleMustBeDifferentFromDescription
  (ErrorMessage = "Title must be different from description.")]
  [CourseTitleCannotBeTitle]
  public abstract class CourseForManipulationDto
  {
    [Required(ErrorMessage = "Title cannot be blank or null.")]
    [MaxLength(100, ErrorMessage = "Title cannot be more than 100 characters.")]
    public string Title { get; set; }

    [MaxLength(1500, ErrorMessage = "Description cannot be more than 1500 characters.")]
    public virtual string Description { get; set; }
  }
}
