using CourseLibrary.API.Models;

namespace CourseLibrary.API.Controllers
{
  public class CourseForCreationDto : CourseForManipulationDto //: IValidatableObject
  {
    //public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    //{
    //  if (Title == Description)
    //  {
    //    yield return new ValidationResult(
    //      "The provided description must be different from the title.",
    //      new[] { "CourseForCreationDto" });
    //  }
    //}
  }
}