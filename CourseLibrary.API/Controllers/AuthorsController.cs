using AutoMapper;
using CourseLibrary.API.Entities;
using CourseLibrary.API.Models;
using CourseLibrary.API.ResourceParameters;
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
      [FromQuery] AuthorsResourceParameters authorsResourceParameters)
    {
      var authorsFromRepo = repository.GetAuthors(authorsResourceParameters);
      return Ok(mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepo));
    }

    /// <summary>
    /// Returns a single author by id
    /// </summary>
    /// <param name="authorId">The author's Guid</param>
    /// <returns>Author</returns>
    /// <response code="200">Returns the author.</response>
    /// <response code="404">If no author matches the supplied authorId.</response>
    [HttpGet("{authorId:guid}", Name = "GetAuthor")]
    public ActionResult<AuthorDto> GetAuthor(Guid authorId)
    {
      var author = repository.GetAuthor(authorId);
      if (author == null)
      {
        return NotFound();
      }

      return Ok(mapper.Map<AuthorDto>(author));
    }

    /// <summary>
    /// Creates a new author, with or without a list of courses
    /// </summary>
    /// <param name="author"></param>
    /// <returns>The newly created author</returns>
    /// <response code="201">Returns the newly created author</response>
    /// <response code="400">If the request body is invalid</response>
    [HttpPost]
    public ActionResult<AuthorDto> CreateAuthor(AuthorForCreationDto author)
    {
      var authorEntity = mapper.Map<Author>(author);
      repository.AddAuthor(authorEntity);
      repository.Save();
      var authorDto = mapper.Map<AuthorDto>(authorEntity);
      return CreatedAtRoute(
        "GetAuthor",
        new { authorId = authorDto.Id },
        authorDto);
    }

    /// <summary>
    /// Deletes an author.
    /// </summary>
    /// <param name="authorId">The author's Guid</param>
    /// <returns>
    /// <response code="204">Author successfully deleted</response>
    /// <response code="404">Author not found</response>
    /// </returns>
    [HttpDelete("{authorId}")]
    public ActionResult DeleteAuthor(Guid authorId)
    {
      var authorFromRepo = repository.GetAuthor(authorId);

      if (authorFromRepo == null) return NotFound();

      repository.DeleteAuthor(authorFromRepo);
      repository.Save();

      return NoContent();
    }

    /// <summary>
    /// Returns the supported HTTP methods of the route
    /// </summary>
    /// <returns></returns>
    /// <response code="200">Returns supported methods in the Allow header</response>
    [HttpOptions]
    public IActionResult GetAuthorsOptions()
    {
      Response.Headers.Add("Allow", "GET,OPTIONS,POST,DELETE");
      return Ok();
    }
  }
}
