using BlogApp;
using BlogApp.InterfaceServices;
using BlogApp.Models;
using BlogApp.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IPostTagService, PostTagService>();

builder.Services.AddDbContext<BlogContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity configuration
builder.Services.AddIdentity<ApplicationUser, Role>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = true;
    options.Password.RequireNonAlphanumeric = false;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<BlogContext>()
.AddDefaultTokenProviders();

// Настройка куки
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Auth/Login";
    options.AccessDeniedPath = "/Error/403";
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;
});

var app = builder.Build();

// Настройка обработки ошибок
app.UseStatusCodePagesWithReExecute("/Error/{0}");  // Для кодов 400-599
app.UseExceptionHandler("/Error/500");

// Конвейер middleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error/500");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Error/{0}");
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Seed roles and users
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var context = services.GetRequiredService<BlogContext>();
        context.Database.Migrate();

        var roleManager = services.GetRequiredService<RoleManager<Role>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        // Явно вызываем асинхронный метод сидирования
        DataSeeder.SeedRolesAndUsersAsync(roleManager, userManager).GetAwaiter().GetResult();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, $"Ошибка при миграции или сидировании: {ex.Message} \n {ex.StackTrace}");
    }
}

app.Run();



