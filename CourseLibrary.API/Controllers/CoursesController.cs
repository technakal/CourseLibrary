using AutoMapper;
using CourseLibrary.API.Models;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Mvc;
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
    [HttpGet("{courseId:guid}")]
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
  }
}
