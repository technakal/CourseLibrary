using CourseLibrary.API.Models;
using System.ComponentModel.DataAnnotations;

namespace CourseLibrary.API.Controllers
{
  public class CourseForUpdateDto : CourseForManipulationDto
  {
    [Required(ErrorMessage = "Description cannot be null or empty.")]
    public override string Description { get; set; }
  }
}