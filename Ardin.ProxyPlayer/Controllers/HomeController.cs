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

            ViewBag.VideoList = Directory.GetFiles(path, "*.mp4").Where(a => !a.Contains("_original.mp4")).Select(a => Path.GetFileName(a)).ToArray();

            return View("Index", filename);
        }
    }
}