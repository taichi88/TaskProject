using HealthcareApi.application.Interfaces;
using Application.Services;
using HealthcareApi.Domain.IRepositories;
using Microsoft.EntityFrameworkCore;
using HealthcareApi.Infrastructure.Repositories;
using HealthcareApi.Application.IUnitOfWork;
using HealthcareApi.Infrastructure;
using HealthcareApi.Application.Interfaces;
using HealthcareApi.Infrastructure.Data.Dapper.DapperDbContext;
using HealthcareApi.Domain.IRepositories.IDapperRepositories;
using HealthcareApi.Infrastructure.Repositories.DapperRepository;
using Microsoft.Identity.Client;
using System.Data;
using Microsoft.Data.SqlClient;





var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<HealthcareApiContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));





// Add services to the container.
builder.Services.AddScoped<IPersonRepository, PersonRepository>();
builder.Services.AddScoped<IPersonService, PersonService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<IDoctorService, DoctorService>();
builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IDapperPaymentRepository, DapperPaymentRepository>();
builder.Services.AddScoped<IDapperAppointmentRepository, DapperAppointmentRepository>();
builder.Services.AddScoped<IDapperAccountRepository, DapperAccountRepository>();

builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<DapperDbContext>();
builder.Services.AddAutoMapper(typeof(HealthcareApi.Application.AutoMapperClass));
builder.Services.AddLogging();
builder.Services.AddScoped<IDbConnection>(_ => new SqlConnection(builder.Configuration.GetConnectionString("DefaultConnection"))); 






builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Get the XML documentation file path
    var xmlFile = Path.Combine(AppContext.BaseDirectory, "TaskProject.xml");
    options.IncludeXmlComments(xmlFile); // This tells Swagger to include the XML fileas comments
});


var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    var dbContext = serviceProvider.GetRequiredService<HealthcareApiContext>();
    if (!await dbContext.Database.CanConnectAsync())
    {
        await dbContext.Database.MigrateAsync();
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
