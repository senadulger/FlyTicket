using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using prgmlab3.Models;

namespace prgmlab3.Controllers;

[ApiController]
[Route("/test")]
public class TestController : Controller
{

    [HttpGet]
   public IActionResult Index()
    {
        return Ok(new
        {
            a = 1,
            sa = "true"
        });
    }

}