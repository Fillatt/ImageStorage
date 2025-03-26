using DataBaseService;
using DataBaseService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebAPI.Controllers;

[Controller]
public class ImagesController : Controller
{
    #region Fields
    private FilesContext _context;
    #endregion

    #region Constructors
    public ImagesController(FilesContext context)
    {
        _context = context;
    }
    #endregion

    #region Public Methods
    [HttpGet]
    [Route("api/images/all")]
    public IActionResult GetImages()
    {
        var filesDictionary = new Dictionary<string, byte[]>();
        var imagesArray = _context.Images.ToArray();

        foreach (var image in imagesArray)
        {
            if (image.Name != null) filesDictionary.Add(image.Name, image.ImageBytes);
        }

        return Ok(filesDictionary);
    }

    [HttpPost]
    [Route("api/images/add")]
    public async Task<IActionResult> SaveImageAsync()
    {
        var file = HttpContext.Request.Form.Files[0];
        byte[] imageBytes = [];

        var binaryReader = new BinaryReader(file.OpenReadStream());
        imageBytes = binaryReader.ReadBytes((int)file.Length);
        int id = _context.Images.Count() + 1;

        Image image = new Image
        {
            Id = id,
            Name = file.FileName,
            ImageBytes = imageBytes
        };

        await _context.Images.AddAsync(image);
        await _context.SaveChangesAsync();

        return Ok();
    }

    [HttpPut]
    [Route("api/images/update/{id:int}")]
    public async Task<ActionResult> UpdateImageAsync(int id)
    {
        var file = HttpContext.Request.Form.Files[0];
        byte[] imageBytes = [];

        var binaryReader = new BinaryReader(file.OpenReadStream());
        imageBytes = binaryReader.ReadBytes((int)file.Length);

        var item = _context.Images.FirstOrDefault(x => x.Id == id);
        if (item != null)
        {
            item.Name = file.FileName;
            item.ImageBytes = imageBytes;
        }

        await _context.SaveChangesAsync();

        return Ok();
    }

    [HttpDelete]
    [Route("api/images/delete/{id:int}")]
    public async Task<ActionResult> DeleteImageAsync(int id)
    {
        var image = await _context.Images.FirstOrDefaultAsync(x => x.Id == id);
        if (image != null) _context.Images.Remove(image);
        await _context.SaveChangesAsync();

        var items = _context.Images.Where(x => x.Id > id).ToList();
        foreach (var item in items) _context.Images.Remove(item);
        await _context.SaveChangesAsync();

        foreach (var item in items)
        {
            _context.Add(new Image
            {
                Id = id,
                Name = item.Name,
                ImageBytes = item.ImageBytes,
            });
            id++;
        }
        await _context.SaveChangesAsync();

        return Ok();
    }
    #endregion
}

