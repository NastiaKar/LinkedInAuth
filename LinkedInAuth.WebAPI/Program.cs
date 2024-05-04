using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.Cookie.Name = "auth-token";
    });


builder.Services.AddAuthentication().AddOAuth("LinkedIn", options =>
{
    options.ClientId = builder.Configuration["LinkedIn:ClientId"];
    options.ClientSecret = builder.Configuration["LinkedIn:ClientSecret"];
    options.CallbackPath = "/signin-linkedin";
    options.AuthorizationEndpoint = "https://www.linkedin.com/oauth/v2/authorization";
    options.TokenEndpoint = "https://www.linkedin.com/oauth/v2/accessToken";
    options.UserInformationEndpoint = "https://api.linkedin.com/v2/me";
    options.Scope.Add("profile");
    options.Scope.Add("email");
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
            context.RunClaimActions(user);
        }
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Map("/login", appBuilder =>
{
    appBuilder.Run(async context =>
    {
        await context.ChallengeAsync("LinkedIn", new AuthenticationProperties() { RedirectUri = "/" });
    });
});

app.Map("/logout", appBuilder =>
{
    appBuilder.Run(async context =>
    {
        await context.SignOutAsync();
        context.Response.Redirect("/");
    });
});

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapDefaultControllerRoute();

app.MapControllers();

app.Run();