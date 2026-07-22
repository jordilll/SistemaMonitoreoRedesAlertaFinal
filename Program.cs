using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;
using SistemaMonitoreoRedes.Data;
using SistemaMonitoreoRedes.Hubs;
using SistemaMonitoreoRedes.Repositories;
using SistemaMonitoreoRedes.Services;

QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

// ── MVC + API + SignalR ────────────────────────────────────────────
builder.Services.AddControllersWithViews();
builder.Services.AddControllers();
builder.Services.AddSignalR();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ── Entity Framework Core ─────────────────────────────────────────
builder.Services.AddDbContext<RedesDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Servicios ─────────────────────────────────────────────────────
builder.Services.AddScoped<IRedRepository, RedRepository>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<IaService>();
builder.Services.AddScoped<ChatbotService>();
builder.Services.AddScoped<ExportService>();
builder.Services.AddScoped<AlertService>();

// ── Session ───────────────────────────────────────────────────────
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ── CORS para Android ─────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AndroidApp", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// ── Middleware ────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();

    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseCors("AndroidApp");

app.UseSession();

app.UseAuthorization();

// ── SignalR ───────────────────────────────────────────────────────
app.MapHub<AlertasHub>("/alertasHub");

// ── API ───────────────────────────────────────────────────────────
app.MapControllers();

// ── MVC ───────────────────────────────────────────────────────────
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();