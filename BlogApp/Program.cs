using BlogApp;
using BlogApp.InterfaceServices;
using BlogApp.Data.Models;
using BlogApp.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NLog;
using NLog.Web;
using BlogApp.Data;

// Инициализируем логгер до сборки приложения
var logger = LogManager.Setup()
    .LoadConfigurationFromFile("nlog.config") // Подключение конфигурации
    .GetCurrentClassLogger();

try
{
    logger.Info("Запуск приложения...");
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    builder.Services.AddControllersWithViews();
    builder.Services.AddScoped<IPostService, PostService>();
    builder.Services.AddScoped<ITagService, TagService>();
    builder.Services.AddScoped<ICommentService, CommentService>();
    builder.Services.AddScoped<IPostTagService, PostTagService>();
    builder.Services.AddScoped<IUserProfileService, UserProfileService>();

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

    builder.Logging.ClearProviders(); //Убрал стандартные логгеры
    builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information); //Логирую все от уровня инфо

    // Отключил логирование EF Core и системных логов
    builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", Microsoft.Extensions.Logging.LogLevel.Warning);
    builder.Logging.AddFilter("Microsoft", Microsoft.Extensions.Logging.LogLevel.Warning);
    builder.Logging.AddFilter("System", Microsoft.Extensions.Logging.LogLevel.Warning);
    builder.Host.UseNLog(); //Само подключение

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
            logger.Info("Миграция базы данных и сидирование выполнены успешно.");
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Ошибка при миграции или сидировании: {Message} \n {StackTrace}", ex.Message, ex.StackTrace);
        }
    }

    app.Run();
}
catch (Exception ex)
{
    // Лог фатальных ошибок
    logger.Fatal(ex, "Приложениие завершилось аварийно при запуске");
    throw;
}
finally
{
    LogManager.Shutdown(); //Очищаю ресурсы логгера
}


