using API.Service;
using Common.Domains;
using GestãoDeTurmas.Middlewares;
using Microsoft.EntityFrameworkCore;
using Repository;
using Repository.Context;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(
        _ => "Este campo é obrigatório.");
    options.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor(
        (_, _) => "O valor informado é inválido.");
});


builder.Services.AddScoped<IAlunoRepository, AlunoRepository>();
builder.Services.AddScoped<IAlunoService, AlunoService>();

builder.Services.AddScoped<IDocenteRepository, DocenteRepository>();
builder.Services.AddScoped<IDocenteService, DocenteService>();

builder.Services.AddScoped<IDisciplinaRepository, DisciplinaRepository>();
builder.Services.AddScoped<IDisciplinaService, DisciplinaService>();

builder.Services.AddDbContext<GestaoEscolarContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();


var app = builder.Build();

app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    //app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=PaginaInicial}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
