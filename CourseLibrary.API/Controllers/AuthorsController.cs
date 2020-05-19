using AutoMapper;
using CourseLibrary.API.Models;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace CourseLibrary.API.Controllers
{
  [ApiController]
  [Route("api/authors")]
  public class AuthorsController : ControllerBase
  {
    private readonly ICourseLibraryRepository repository;
    private readonly IMapper mapper;

    public AuthorsController(ICourseLibraryRepository repository, IMapper mapper)
    {
      this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
      this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    /// <summary>
    /// Returns all authors, or an empty collection.
    /// </summary>
    /// <returns>Author[]</returns>
    /// <response code="200">Returns all authors or an empty collection.</response>
    [HttpGet]
    [HttpHead]
    public ActionResult<IEnumerable<AuthorDto>> GetAuthors(
      [FromQuery] string mainCategory, 
      [FromQuery] string searchQuery)
    {
      var authorsFromRepo = repository.GetAuthors(mainCategory, searchQuery);
      return Ok(mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepo));
    }

    /// <summary>
    /// Returns a single author by id
    /// </summary>
    /// <param name="authorId">The author's Guid</param>
    /// <returns>Author</returns>
    /// <response code="200">Returns the author.</response>
    /// <response code="404">If no author matches the supplied authorId.</response>
    [HttpGet("{authorId:guid}")]
    public ActionResult<AuthorDto> GetAuthor(Guid authorId)
    {
      var author = repository.GetAuthor(authorId);
      if (author == null)
      {
        return NotFound();
      }

      return Ok(mapper.Map<AuthorDto>(author));
    }
  }
}
