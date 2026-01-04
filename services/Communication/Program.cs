using Amazon.Lambda.AspNetCoreServer.Hosting;
using Amazon.SimpleEmail;
using Microsoft.EntityFrameworkCore;
using CommunicationService.Data;
using CommunicationService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add AWS SES - will use IAM role in AWS, credentials file locally
if (builder.Environment.IsDevelopment())
{
    // Local development: uses ~/.aws/credentials
    builder.Services.AddAWSService<IAmazonSimpleEmailService>();
}
else
{
    // Production: uses IAM role attached to Lambda/ECS/EC2
    builder.Services.AddAWSService<IAmazonSimpleEmailService>();
}

// Add Services
builder.Services.AddScoped<ICommunicationService, CommunicationService.Services.CommunicationService>();
builder.Services.AddScoped<IEmailService, EmailService>();

var app = builder.Build();

app.UseCors("AllowAll");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "Communication" }));
app.MapGet("/info", () => Results.Ok(new { name = "Communication Service", version = "0.1" }));

app.Run();
