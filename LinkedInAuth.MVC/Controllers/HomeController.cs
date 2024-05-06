using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using LinkedInAuth.MVC.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Newtonsoft.Json.Linq;

namespace LinkedInAuth.MVC.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IMapper _mapper;
    private readonly IWebHostEnvironment _hostingEnvironment;

    public HomeController(ILogger<HomeController> logger, IMapper mapper, IWebHostEnvironment hostingEnvironment)
    {
        _logger = logger;
        _mapper = mapper;
        _hostingEnvironment = hostingEnvironment;
    }

    public IActionResult Index()
    {
        return View();
    }
    public IActionResult Login()
    {
        return Challenge(new AuthenticationProperties { RedirectUri = "/" }, "LinkedIn");
    }

    public async Task<IActionResult> Profile([FromServices] HttpClient httpClient)
    {
        var accessToken = await HttpContext.GetTokenAsync("access_token");
        
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        var response = await httpClient.GetAsync("https://api.linkedin.com/v2/userinfo");

        if (response.IsSuccessStatusCode)
        {
            var user = JObject.Parse(await response.Content.ReadAsStringAsync());
            var userProfile = _mapper.Map<UserProfileViewModel>(user);
            var imageUrl = userProfile.Picture;
            await DownloadImage(imageUrl);
            return View(userProfile);
            
            // var user = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
            // var userProfile = _mapper.Map<UserProfileViewModel>(user);
            // return View(userProfile);
        }

        return RedirectToAction("Login");

    }

    public async Task<IActionResult> Callback()
    {
        var authResult = await HttpContext.AuthenticateAsync("LinkedIn");
        
        if (!authResult.Succeeded)
        {
            return RedirectToAction("Error");
        }
        
        var accessToken = authResult.Properties.GetTokenValue("access_token");
        
        if (string.IsNullOrEmpty(accessToken))
        {
            return RedirectToAction("Error");
        }
        
        return RedirectToAction("Profile");
    }

    public async Task DownloadImage(string url)
    {
        await Task.Run(() =>
        {
            using (WebClient client = new WebClient()) 
            {
                client.DownloadFile(new Uri(url), @"Images\image.png");
            }
        });
    }
    
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        const string url = "https://linkedin.com/m/logout";
        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        return RedirectToAction("Index");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}