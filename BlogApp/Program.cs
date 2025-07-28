using BlogApp;
using BlogApp.InterfaceServices;
using BlogApp.Data.Models;
using BlogApp.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NLog;
using NLog.Web;
using BlogApp.Data;

// �������������� ������ �� ������ ����������
var logger = LogManager.Setup()
    .LoadConfigurationFromFile("nlog.config") // ����������� ������������
    .GetCurrentClassLogger();

try
{
    logger.Info("������ ����������...");
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

    // ��������� ����
    builder.Services.ConfigureApplicationCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Error/403";
        options.Cookie.HttpOnly = true;
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;
    });

    builder.Logging.ClearProviders(); //����� ����������� �������
    builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information); //������� ��� �� ������ ����

    // �������� ����������� EF Core � ��������� �����
    builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", Microsoft.Extensions.Logging.LogLevel.Warning);
    builder.Logging.AddFilter("Microsoft", Microsoft.Extensions.Logging.LogLevel.Warning);
    builder.Logging.AddFilter("System", Microsoft.Extensions.Logging.LogLevel.Warning);
    builder.Host.UseNLog(); //���� �����������

    var app = builder.Build();

    // ��������� ��������� ������
    app.UseStatusCodePagesWithReExecute("/Error/{0}");  // ��� ����� 400-599
    app.UseExceptionHandler("/Error/500");

    // �������� middleware
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

            // ���� �������� ����������� ����� �����������
            DataSeeder.SeedRolesAndUsersAsync(roleManager, userManager).GetAwaiter().GetResult();
            logger.Info("�������� ���� ������ � ����������� ��������� �������.");
        }
        catch (Exception ex)
        {
            logger.Error(ex, "������ ��� �������� ��� �����������: {Message} \n {StackTrace}", ex.Message, ex.StackTrace);
        }
    }

    app.Run();
}
catch (Exception ex)
{
    // ��� ��������� ������
    logger.Fatal(ex, "����������� ����������� �������� ��� �������");
    throw;
}
finally
{
    LogManager.Shutdown(); //������ ������� �������
}


