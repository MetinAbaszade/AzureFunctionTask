using AzureFuncTask.Entities;
using AzureFuncTask.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using System.Diagnostics;

namespace AzureFuncTask.Controllers
{
    public class HomeController : Controller
    {
        public enum ImageFormat
        {
            bmp,
            jpeg,
            gif,
            tiff,
            png,
            unknown
        }

        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> UploadFile(MyFile myFile)
        {
            try
            {
                var File = myFile.File;
                // Check if file is image: 
                string FileExtension = Path.GetExtension(File.FileName);
                if (Enum.IsDefined(typeof(ImageFormat), FileExtension.Remove(0, 1).ToLower())) { }
                else { throw new Exception("File is not an image"); }

                // Set unique filename to file: 
                string FileName = Path.ChangeExtension(
                        Path.GetRandomFileName(),
                        FileExtension
                );

                // Copy file to local: 
                string FileLocation = "wwwroot/uploads/";
                File.CopyTo(new FileStream(FileLocation + FileName, FileMode.Create));


                var HttpClient = new HttpClient();
                var FileSizeinMB = ((File.Length / 1024f) / 1024f).ToString();
                var queryString = new Dictionary<string, string>()
                {
                    { "filename", File.FileName },
                    { "filesize", FileSizeinMB },
                    { "foldername", FileLocation },
                };
                var requestUri = QueryHelpers.AddQueryString(
                    "https://matinfunction.azurewebsites.net/api/Function1?code=J6acXDICz1XXv5wvXqn5oz9u5aRHipiWSe2UFNNwQlFoAzFukKJTzA==",
                    queryString
                );

                var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
                var response = await HttpClient.SendAsync(request);
                var responseString = await response.Content.ReadAsStringAsync();
                var values = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseString);
                TempData["responseString"] = $"FileName: {values["FileName"]}</br>" +
                                             $"FileSize: {values["FileSize"]} mb</br>" +
                                             $"FolderName: {values["FolderName"]}";
            }
            catch (Exception ex)
            {
                TempData["responseString"] = ex.Message;
                throw;
            }
            return RedirectToAction("index");
        }
    }
}