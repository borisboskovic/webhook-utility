using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace WebhookUtility.Web.Controllers;

[ApiController]
[Route("/webhook/{file}")]
public class WebhookController : ControllerBase
{
    private const string DirName = "data";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };

    [HttpGet]
    public async Task<IActionResult> Get(string file)
    {
        var path = $"{DirName}\\{file}.json";

        CreateDirectoryIfDoesNotExists();
        await CreateFileIfDoesNotExists(path);

        var content = await System.IO.File.ReadAllTextAsync(path, Encoding.UTF8);

        return Ok(content);
    }

    [HttpPost]
    public async Task<IActionResult> Post(string file, [FromBody] dynamic payload)
    {
        var path = $"{DirName}\\{file}.json";

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

    [HttpDelete]
    public async Task<ActionResult> Delete(string file)
    {
        var path = $"{DirName}\\{file}.json";

        CreateDirectoryIfDoesNotExists();
        await CreateFileIfDoesNotExists(path);

        await System.IO.File.WriteAllTextAsync(path, "[]");

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
}