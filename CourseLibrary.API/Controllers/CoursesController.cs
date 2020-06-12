using AutoMapper;
using CourseLibrary.API.Entities;
using CourseLibrary.API.Models;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace CourseLibrary.API.Controllers
{
  [ApiController]
  [Route("api/authors/{authorId}/courses")]
  public class CoursesController : ControllerBase
  {
    private readonly ICourseLibraryRepository repository;
    private readonly IMapper mapper;

    public CoursesController(ICourseLibraryRepository repository, IMapper mapper)
    {
      this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
      this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    /// <summary>
    /// Returns all courses for an author
    /// </summary>
    /// <param name="authorId">The guid of the author who owns the course</param>
    /// <returns>A collection of courses</returns>
    /// <response code="200">Returns a collection of courses</response>
    /// <response code="404">If the author does not exist</response>
    [HttpGet]
    public ActionResult<IEnumerable<CourseDto>> GetCoursesForAuthor(Guid authorId)
    {
      if (!repository.AuthorExists(authorId))
      {
        return NotFound();
      }

      var coursesForAuthor = repository.GetCourses(authorId);
      return Ok(mapper.Map<IEnumerable<CourseDto>>(coursesForAuthor));
    }

    /// <summary>
    /// Returns a single course by Guid
    /// </summary>
    /// <param name="authorId">The guid of the author who owns the course</param>
    /// <param name="courseId">The guid of the course</param>
    /// <returns>A single course</returns>
    /// <response code="200">Returns a single course</response>
    /// <response code="404">If either the author or course do not exist</response>
    [HttpGet("{courseId:guid}", Name = "GetCourseForAuthor")]
    public ActionResult<CourseDto> GetCourseforAuthor(Guid authorId, Guid courseId)
    {
      if (!repository.AuthorExists(authorId))
      {
        return NotFound();
      }

      var course = repository.GetCourse(authorId, courseId);
      if (course == null)
      {
        return NotFound();
      }

      return Ok(mapper.Map<CourseDto>(course));
    }

    /// <summary>
    /// Creates a new course for the provided author
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    /// POST /api/authors/{authorId}/courses
    /// {
    ///   "title": "A sample course title",
    ///   "description": "A sample course description."
    /// }
    /// </remarks>
    /// <param name="authorId">The author's Guid</param>
    /// <param name="course">The course details</param>
    /// <returns>The new course</returns>
    /// <response code="201">Returns the newly created course</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="404">If the author does not exist</response>
    /// 
    [HttpPost]
    public ActionResult<CourseDto> CreateCourseForAuthor(
      Guid authorId,
      CourseForCreationDto course)
    {
      if (!repository.AuthorExists(authorId))
      {
        return NotFound();
      }

      var courseEntity = mapper.Map<Course>(course);
      repository.AddCourse(authorId, courseEntity);
      repository.Save();

      var courseDto = mapper.Map<CourseDto>(courseEntity);
      return CreatedAtRoute(
        "GetCourseForAuthor",
        new { authorId, courseId = courseDto.Id },
        courseDto
        );
    }

    /// <summary>
    /// Updates all values of an existing resource.
    /// </summary>
    /// <remarks>
    /// Sample PUT Request: 
    /// {
    ///   "title": "A sample course title",
    ///   "description": "A sample course description."
    /// }
    /// </remarks>
    /// <param name="authorId">The author's Guid</param>
    /// <param name="courseId">The guid of the course</param>
    /// <param name="course">The course details</param>
    /// <returns>
    /// <response code="204">Resource updated successfully</response>
    /// <response code="404">If the author or course does not exist</response>
    /// </returns>
    [HttpPut("{courseId}")]
    public ActionResult UpdateCourseForAuthor(
      Guid authorId,
      Guid courseId,
      CourseForUpdateDto course)
    {
      if (!repository.AuthorExists(authorId))
      {
        return NotFound();
      }

      var courseForAuthorFromRepo = repository.GetCourse(authorId, courseId);

      if (courseForAuthorFromRepo == null)
      {
        var courseToAdd = mapper.Map<Course>(course);
        courseToAdd.Id = courseId;

        repository.AddCourse(authorId, courseToAdd);
        repository.Save();

        var courseToReturn = mapper.Map<CourseDto>(courseToAdd);
        return CreatedAtRoute(
         "GetCourseForAuthor",
          new { authorId, courseId = courseToReturn.Id },
          courseToReturn
          );
      }

      mapper.Map(course, courseForAuthorFromRepo);

      repository.UpdateCourse(courseForAuthorFromRepo);
      repository.Save();

      return NoContent();
    }

    /// <summary>
    /// Applies a patch update to partially update a course.
    /// </summary>
    /// <param name="authorId">The author's Guid</param>
    /// <param name="courseId">The guid of the course</param>
    /// <param name="patchDocument">The patch document describing the updates</param>
    /// <returns>
    /// <response code="204">Resource updated successfully</response>
    /// <response code="404">If the author or course does not exist</response>
    /// </returns>
    [HttpPatch("{courseId}")]
    public ActionResult PartiallyUpdateCourseForAuthor(
      Guid authorId,
      Guid courseId,
      JsonPatchDocument<CourseForUpdateDto> patchDocument)
    {
      if (!repository.AuthorExists(authorId))
      {
        return NotFound();
      }

      var courseFromRepo = repository.GetCourse(authorId, courseId);

      if (courseFromRepo == null)
      {
        var courseDto = new CourseForUpdateDto();
        patchDocument.ApplyTo(courseDto, ModelState);

        if (!TryValidateModel(courseDto))
        {
          return ValidationProblem(ModelState);
        }

        var courseToAdd = mapper.Map<Course>(courseDto);
        courseToAdd.Id = courseId;

        repository.AddCourse(authorId, courseToAdd);
        repository.Save();

        var courseToReturn = mapper.Map<CourseDto>(courseToAdd);

        return CreatedAtRoute(
          "GetCourseForAuthor",
          new { authorId, courseId = courseToReturn.Id },
          courseToReturn
          );
      }

      var courseToPatch = mapper.Map<CourseForUpdateDto>(courseFromRepo);
      // need validation
      patchDocument.ApplyTo(courseToPatch, ModelState);

      if (!TryValidateModel(courseToPatch))
      {
        return ValidationProblem(ModelState);
      }

      mapper.Map(courseToPatch, courseFromRepo);

      repository.UpdateCourse(courseFromRepo);
      repository.Save();

      return NoContent();
    }

    /// <summary>
    /// Deletes a course
    /// </summary>
    /// <param name="authorId">The author's Guid</param>
    /// <param name="courseId">The guid of the course</param>
    /// <returns>
    /// <response code="204">Course successfully deleted</response>
    /// <response code="404">Course not found</response>
    /// </returns>
    [HttpDelete("{courseId}")]
    public ActionResult DeleteCourseForAuthor(Guid authorId, Guid courseId)
    {
      if (!repository.AuthorExists(authorId))
      {
        return NotFound();
      }

      var courseForAuthorFromRepo = repository.GetCourse(authorId, courseId);
      if (courseForAuthorFromRepo == null)
      {
        return NotFound();
      }

      repository.DeleteCourse(courseForAuthorFromRepo);
      repository.Save();

      return NoContent();
    }

    /// <summary>
    /// Returns the supported HTTP methods of the route
    /// </summary>
    /// <returns></returns>
    /// <response code="200">Returns supported methods in the Allow header</response>
    [HttpOptions]
    public IActionResult GetCoursesOptions()
    {
      Response.Headers.Add("Allow", "GET,OPTIONS,POST,PUT,PATCH,DELETE");
      return Ok();
    }

    public override ActionResult ValidationProblem([ActionResultObjectValue] ModelStateDictionary modelStateDictionary)
    {
      var options = HttpContext.RequestServices.GetRequiredService<IOptions<ApiBehaviorOptions>>();
      return (ActionResult)options.Value.InvalidModelStateResponseFactory(ControllerContext);
    }
  }
}
