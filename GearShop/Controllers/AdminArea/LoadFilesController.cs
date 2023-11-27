using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GearShop.Controllers.AdminArea
{
    //Контроллер не доделан! вычистить!
    public class LoadFilesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
		[HttpPost]
        public async Task<IActionResult> UploadFilesToStorage(List<IFormFile> files)
        {
            foreach (IFormFile file in files)
            {
                bool result = await WriteFile(file, "Upload\\Files");
                if (!result)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError);
                }
            }

            return Ok();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> UploadProductImages(List<IFormFile> files)
        {
	        foreach (IFormFile file in files)
	        {
		        bool result = await WriteFile(file, Path.Combine("wwwroot", "productImages"));
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
        
        private async Task<bool> WriteFile(IFormFile file, string uploadFolder)
        {
            try
            {
                var filepath = Path.Combine(Directory.GetCurrentDirectory(), uploadFolder);

                if (!Directory.Exists(filepath))
                {
                    Directory.CreateDirectory(filepath);
                }

                var exactpath = Path.Combine(Directory.GetCurrentDirectory(), uploadFolder, file.FileName);
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
