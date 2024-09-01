using APIwithSQLLite.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Add the dependancy
builder.Services.AddDbContext<DataContext>
                                        (options => 
                                        options.UseSqlite(
                                            builder.Configuration.GetConnectionString("DefaultConnectionString")
                                            )
                                        );

//Add the serivces for JWT Token Checking
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                              options.TokenValidationParameters = new TokenValidationParameters
                              {
                                  ValidateIssuer = true, // Ensures the token's issuer is validated.
                                  ValidIssuer = builder.Configuration["JWT:Issuer"], // Sets the expected issuer of the token.
                                  ValidateAudience = true, // Ensures the token's audience is validated.
                                  ValidAudience = builder.Configuration["JWT:Audience"], // Sets the expected audience of the token.
                                  ValidateLifetime = true, // Ensures the token hasn't expired.
                                  ValidateIssuerSigningKey = true,
                                  IssuerSigningKey = new SymmetricSecurityKey
                                                        (Encoding.UTF8.GetBytes(builder.Configuration["JWT:SigningKey"])
                                                        ) // Sets the SymmetricSecurityKey to validate the token's signature.
                              });                                

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//use the middleware
app.UseAuthentication();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
