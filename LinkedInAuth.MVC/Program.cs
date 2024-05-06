using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddHttpClient();

builder.Services.AddHttpsRedirection(options => 
    options.HttpsPort = 443);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.Cookie.Name = "auth-token";
    });

builder.Services.AddMvc();

builder.Services.AddAuthentication().AddOAuth("LinkedIn", options =>
{
    options.ClientId = builder.Configuration["LinkedIn:ClientId"];
    options.ClientSecret = builder.Configuration["LinkedIn:ClientSecret"];
    options.CallbackPath = "/signin-linkedin";
    options.AuthorizationEndpoint = "https://www.linkedin.com/oauth/v2/authorization";
    options.TokenEndpoint = "https://www.linkedin.com/oauth/v2/accessToken";
    options.UserInformationEndpoint = "https://api.linkedin.com/v2/userinfo";
    options.Scope.Add("openid");
    options.Scope.Add("email");
    options.Scope.Add("profile");
    options.SaveTokens = true;

    options.Events = new OAuthEvents
    {
        OnCreatingTicket = async context =>
        {
            var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
            request.Headers.Add("x-li-format", "json");
            request.Headers.Add("Authorization", $"Bearer {context.AccessToken}");

            var response = await context.Backchannel.SendAsync(request, context.HttpContext.RequestAborted);
            response.EnsureSuccessStatusCode();

            var user = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
            var email = user.GetProperty("email").GetString();
            var identity = (ClaimsIdentity)context.Principal.Identity;
            identity.AddClaim(new Claim(ClaimTypes.Name, email));
            
            context.RunClaimActions(user);
        }
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.UseEndpoints(endpoints => 
    endpoints.MapControllerRoute(
        name: "signin-linkedin",
        pattern: "signin-linkedin",
        defaults: new { controller = "Home", action = "Callback" }));

app.Run();