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
        public IActionResult Play(string filename)
        {
            return View("Play", filename);
        }
        [HttpGet]
        public IActionResult Download(string url)
        {
            WebClient webClient = new WebClient();

            string filename = Guid.NewGuid() + ".mp4";
            var path = _config.GetValue<string>("AppSetting:Storage");

            webClient.DownloadFile(url, path + filename);

            return RedirectToAction("Play", new { filename });
        }
    }
}