using Ardin.ProxyPlayer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Ardin.ProxyPlayer.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _config;
        public HomeController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet]
        public IActionResult Index(string filename)
        {
            var path = _config.GetValue<string>("AppSetting:Storage");

            List<Movie> movies = new List<Movie>();
            foreach (var moviePath in Directory.GetFiles(path, "*.mp4").Where(a => !a.Contains("_original.mp4")))
            {
                FileInfo fileInfo = new FileInfo(moviePath);
                Movie movie = new Movie()
                {
                    Name = Path.GetFileNameWithoutExtension(moviePath),
                    DateAdded = fileInfo.CreationTime.ToString(),
                    Link = Path.GetFileName(moviePath),
                    Size = (fileInfo.Length / 1048576).ToString()
                };
                movies.Add(movie);
            }
            ViewBag.VideoList = movies;

            return View("Index", filename);
        }
        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Add(string link, string secret)
        {
            if (secret.Contains("aria"))
            {
                var path = _config.GetValue<string>("AppSetting:EnginePath");
                System.IO.File.WriteAllText(Path.Combine(path, "DownloadList.txt"), link);
                return Content("okey , added " + link);
            }
            else
                return Content("wrong secret" + link);
        }
    }
}