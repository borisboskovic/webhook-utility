using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace WebhookUtility.Web.Controllers;

[ApiController]
[Route("webhook")]
public class WebhookController : ControllerBase
{
    private const string DirName = "data";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };

    private static string GetFilePath(string filename) => $"{DirName}{Path.DirectorySeparatorChar}{filename}.json";

    [HttpGet("list")]
    public IActionResult List()
    {
        CreateDirectoryIfDoesNotExists();
        var files = Directory.GetFiles(DirName).Select(x => x.Replace("data\\", ""));
        return Ok(files);
    }

    [HttpGet("{file}")]
    public async Task<IActionResult> Get(string file)
    {
        var path = GetFilePath(file);

        CreateDirectoryIfDoesNotExists();
        await CreateFileIfDoesNotExists(path);

        var content = await System.IO.File.ReadAllTextAsync(path, Encoding.UTF8);

        return Ok(content);
    }

    [HttpPost("{file}")]
    public async Task<IActionResult> Post(string file, [FromBody] dynamic payload)
    {
        var path = GetFilePath(file);

        CreateDirectoryIfDoesNotExists();
        await CreateFileIfDoesNotExists(path);

        var contentStr = await System.IO.File.ReadAllTextAsync(path, Encoding.UTF8);
        var contentObj = JsonSerializer.Deserialize<List<dynamic>>(contentStr);
        if (contentObj is null)
        {
            throw new NullReferenceException("Deserialized content is null");
        }

        contentObj.Add(new { CreatedAt = DateTime.UtcNow, payload });
        var output = JsonSerializer.Serialize(contentObj, SerializerOptions);
        await System.IO.File.WriteAllTextAsync(path, output, Encoding.UTF8);

        return Ok();
    }

    [HttpDelete("{file}")]
    public ActionResult Delete(string file)
    {
        var path = GetFilePath(file);
        CreateDirectoryIfDoesNotExists();
        DeleteFileIfExists(path);
        return Ok();
    }

    private static void CreateDirectoryIfDoesNotExists()
    {
        if (!Directory.Exists(DirName))
        {
            Directory.CreateDirectory(DirName);
        }
    }

    private static async Task CreateFileIfDoesNotExists(string path)
    {
        if (!System.IO.File.Exists(path))
        {
            await System.IO.File.WriteAllTextAsync(path, "[]");
        }
    }

    private static void DeleteFileIfExists(string path)
    {
        if (System.IO.File.Exists(path))
        {
            System.IO.File.Delete(path);
        }
    }
}