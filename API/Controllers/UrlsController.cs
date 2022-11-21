using API.Dtos;
using API.Models;
using LiteDB;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UrlsController : ControllerBase
    {
        private readonly ILiteDatabase _db;

        public UrlsController(ILiteDatabase db)
        {
            _db = db;
        }

        [HttpPost]
        public IActionResult CreateUrl(UrlDto request)
        {
            if (!Uri.TryCreate(request.Url, UriKind.Absolute, out var inputUri))
            {
                return BadRequest("URL is invalid.");
            }

            var links = _db.GetCollection<ShortUrl>(BsonAutoId.Int32);
            var entry = new ShortUrl(inputUri);
            links.Insert(entry);
            var result = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/{entry.UrlChunk}";
            
            return Ok(new
            {
                url = result
            });
        }
    }
}
