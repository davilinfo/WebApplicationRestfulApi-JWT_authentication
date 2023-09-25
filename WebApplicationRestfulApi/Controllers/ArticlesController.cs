using System;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace WebApplicationRestfulApi.Controllers
{

  [ApiController]
  [Route("api/[controller]")]
  public class ArticlesController : ControllerBase
  {
    private IRepository _repository;

    private readonly LoggerProxy _logger;

    public ArticlesController(IRepository repository)
    {      
      _logger = new LoggerProxy(); // use _logger.WriteLine() to write to the console.
      _repository = repository;
    }

    [HttpGet("{id}")]
    public IActionResult Get(Guid id)
    {
      _logger.WriteLine($"Articles: get article: {id}");
      try
      {
        var article = _repository.Get(id);

        if (article != null)
        {
          return Ok(new { id = article.Id, title = article.Title, text = article.Text });
        }
        else
        {
          return NotFound("Article not found.");
        }
      }
      catch (Exception ex)
      {
        _logger.WriteLine($"Date: {DateTime.UtcNow}, {ex.Message}");
        return BadRequest(ModelState);
      }
    }
    [Authorize]
    [HttpPost()]
    public IActionResult CreateArticle([FromBody] ViewModelArticle article)
    {
      _logger.WriteLine($"Articles: create article: {JsonSerializer.Serialize<ViewModelArticle>(article)}");

      try
      {
        if (article == null)
        {
          ModelState.AddModelError("articleNull", "article object is mandatory");
        }

        if (string.IsNullOrEmpty(article.Title))
        {
          ModelState.AddModelError("emptyTitle", "article Title is mandatory");
        }

        if (ModelState.IsValid)
        {
          var newArticle = new Article { Title = article.Title, Text = article.Text };
          var guid = _repository.Create(newArticle);
          newArticle.Id = guid;


          return Created($"api/articles/{guid}", new {id= guid, title= newArticle.Title, text= newArticle.Text});
        }

        foreach (var item in ModelState.Values)
        {
          foreach (var erro in item.Errors)
          {
            _logger.WriteLine($"Date: {DateTime.UtcNow}, {erro.ErrorMessage}");
          }
        }
        return BadRequest(ModelState);
      }catch(Exception ex)
      {
        _logger.WriteLine($"Date: {DateTime.UtcNow}, {ex.Message}");
        return BadRequest(ModelState);
      }

    }
    [Authorize]
    [HttpDelete("{guid}")]
    public IActionResult Delete(string guid)
    {
      _logger.WriteLine($"Articles: delete article: guid");

      try
      {
        if (_repository.Delete(Guid.Parse(guid)))
        {
          return Ok();
        }
        else
        {
          return NotFound("Article not found.");
        }
      }catch(Exception ex)
      {
        _logger.WriteLine($"Date: {DateTime.UtcNow}, {ex.Message}");
        return BadRequest(ModelState);
      }
    }
    [Authorize]
    [HttpPut("{guid}")]
    public IActionResult Put(string guid, ViewModelArticle updateArticle)
    {
      _logger.WriteLine($"Articles: put article: guid article: {JsonSerializer.Serialize<ViewModelArticle>(updateArticle)}");
      try
      {
        if (string.IsNullOrEmpty(updateArticle.Title))
        {
          ModelState.AddModelError("emptyTitle", "title was not provided");
        }

        if (ModelState.IsValid)
        {
          var article = _repository.Get(Guid.Parse(guid));
          if (article != null)
          {
            article.Title = updateArticle.Title;
            article.Text = updateArticle.Text;

            if (_repository.Update(article))
            {
              return Ok();
            }
            return BadRequest();
          }
          else
          {
            return NotFound("Article not found.");
          }
        }
        else
        {
          foreach (var item in ModelState.Values)
          {
            foreach (var erro in item.Errors)
            {
              _logger.WriteLine($"Date: {DateTime.UtcNow}, {erro.ErrorMessage}");
            }
          }
          return BadRequest();
        }
      }catch(Exception ex)
      {
        _logger.WriteLine($"Date: {DateTime.UtcNow}, {ex.Message}");
        return BadRequest(ModelState);
      }

    }    
      
  }


  public class Repository : IRepository
  {
    public Guid Create(Article article)
    {
      return Guid.NewGuid();
    }

    public bool Delete(Guid id)
    {
      return true;
    }

    public Article Get(Guid id)
    {
      return new Article();
    }

    public bool Update(Article articleToUpdate)
    {
      return true;
    }
  }

  public interface IRepository
  {
    // Returns a found article or null.
    Article Get(Guid id);
    // Creates a new article and returns its identifier.
    // Throws an exception if a article is null.
    // Throws an exception if a title is null or empty.
    Guid Create(Article article);
    // Returns true if an article was deleted or false if it was not possible to find it.
    bool Delete(Guid id);
    // Returns true if an article was updated or false if it was not possible to find it.
    // Throws an exception if an articleToUpdate is null.
    // Throws an exception or if a title is null or empty.
    bool Update(Article articleToUpdate);
  }

  public class ViewModelArticle
  {
    public string Title { get; set; }
    public string Text { get; set; }
  }

  public class Article
  {
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Text { get; set; }
  }
}