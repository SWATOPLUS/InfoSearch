using System;
using Microsoft.AspNetCore.Mvc;

namespace WikiDownloader.ContentLoaderWeb.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StatusController : ControllerBase
    {

        [HttpGet]
        public string Get()
        {
            return DateTime.UtcNow + ". It works!";
        }
    }
}
