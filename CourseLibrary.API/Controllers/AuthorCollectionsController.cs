using AutoMapper;
using CourseLibrary.API.Entities;
using CourseLibrary.API.Helpers;
using CourseLibrary.API.Models;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CourseLibrary.API.Controllers
{
  [ApiController]
  [Route("api/authorcollections")]
  public class AuthorCollectionsController : ControllerBase
  {
    private readonly ICourseLibraryRepository repository;
    private readonly IMapper mapper;

    public AuthorCollectionsController(ICourseLibraryRepository repository, IMapper mapper)
    {
      this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
      this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    /// <summary>
    /// Retrieves a collection of authors
    /// </summary>
    /// <param name="ids">A list of guids for each author in the collection</param>
    /// <returns>A collection of authors</returns>
    /// <response code="200">Returns the collection of authors</response>
    /// <response code="400">If the ids are null</response>
    /// <response code="404">If any of the authors don't exist</response>
    [HttpGet("({ids})", Name = "GetAuthorCollection")]
    public IActionResult GetAuthorCollection(
      [FromRoute]
      [ModelBinder(BinderType = typeof(ArrayModelBinder))] IEnumerable<Guid> ids)
    {
      if (ids is null)
      {
        return BadRequest();
      }

      var authorEntities = repository.GetAuthors(ids);

      if (ids.Count() != authorEntities.Count())
      {
        return NotFound();
      }

      var authorsDto = mapper.Map<IEnumerable<AuthorDto>>(authorEntities);
      return Ok(authorsDto);
    }

    /// <summary>
    /// Creates a collection of authors
    /// </summary>
    /// <param name="authorCollection">A list of author objects</param>
    /// <returns></returns>
    /// <response code="201">Returns the collection of authors</response>
    /// <response code="400">If the response body, or any of the authors, is invalid</response>
    /// <response code="500">If any of the supplied Guids are not valid Guids</response>
    [HttpPost]
    public ActionResult<IEnumerable<AuthorDto>> CreateAuthorCollection(IEnumerable<AuthorForCreationDto> authorCollection)
    {
      var authorEntities = mapper.Map<IEnumerable<Author>>(authorCollection);
      foreach (var author in authorEntities)
      {
        repository.AddAuthor(author);
      }
      repository.Save();

      var authorsToReturn = mapper.Map<IEnumerable<AuthorDto>>(authorEntities);
      var idsAsString = string.Join(",", authorsToReturn.Select(a => a.Id));
      return CreatedAtRoute(
        "GetAuthorCollection",
        new { ids = idsAsString },
        authorsToReturn
        );
    }

    /// <summary>
    /// Returns the supported HTTP methods of the route
    /// </summary>
    /// <returns></returns>
    /// <response code="200">Returns supported methods in the Allow header</response>
    [HttpOptions]
    public IActionResult GetAuthorCollectionOptions()
    {
      Response.Headers.Add("Allow", "GET,OPTIONS,POST");
      return Ok();
    }
  }
}
