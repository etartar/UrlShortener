
using API.Controllers;
using API.Models;
using LiteDB;
using Microsoft.AspNetCore.WebUtilities;

namespace API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddSingleton<ILiteDatabase, LiteDatabase>(_ => new LiteDatabase("short-links.db"));

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapFallback(RedirectDelegate);

            app.MapControllers();

            app.Run();
        }

        private static async Task RedirectDelegate(HttpContext httpContext)
        {
            var db = httpContext.RequestServices.GetService<ILiteDatabase>();
            var collection = db.GetCollection<ShortUrl>();
            var path = httpContext.Request.Path.ToUriComponent().Trim('/');

            if (path.Equals(string.Empty))
            {
                httpContext.Response.StatusCode = StatusCodes.Status204NoContent;
                return;
            }

            var id = BitConverter.ToInt32(WebEncoders.Base64UrlDecode(path));
            var entry = collection.Find(p => p.Id == id).FirstOrDefault();

            if (entry is null)
            {
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                await httpContext.Response.WriteAsync("Url is invalid.");
                return;
            }

            httpContext.Response.Redirect(entry?.Url ?? "/");

            collection.Delete(id);

            await Task.CompletedTask;
        }
    }
}