using Microsoft.AspNetCore.Mvc;

namespace GearShop.Controllers.AdminArea
{
    public class LoadFilesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadFilesToStorage(List<IFormFile> files)
        {
            foreach (IFormFile file in files)
            {
                bool result = await WriteFile(file);
                if (!result)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError);
                }
            }

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> SavePriceFilesToDb(List<string> fileNames)
        {
            return Ok();
        }
        
        private async Task<bool> WriteFile(IFormFile file)
        {
            try
            {
                var filepath = Path.Combine(Directory.GetCurrentDirectory(), "Upload\\Files");

                if (!Directory.Exists(filepath))
                {
                    Directory.CreateDirectory(filepath);
                }

                var exactpath = Path.Combine(Directory.GetCurrentDirectory(), "Upload\\Files", file.FileName);
                using (var stream = new FileStream(exactpath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }
    }
}
