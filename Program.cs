
using Microsoft.EntityFrameworkCore;
using userPanelOMR.Context;
//using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using userPanelOMR.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using userPanelOMR.model;

namespace userPanelOMR
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add class depandancey to the container. 
            // GetConnectionString se hum connection string ko lete hain jo humne appsettings.json me define kiya hai then make ConnectionString
            // Add DbContext Service
            //builder.Services.AddDbContext<JWTContext>(options =>
            //    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
            //    ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))));

            builder.Services.AddDbContext<JWTContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


            // Add Controller Service 
            builder.Services.AddControllers();
            builder.Services.AddScoped<HashEncption>();
            builder.Services.AddScoped<jwtTokenGen>();
            builder.Services.AddScoped<DynamicForm>();
            // Builder.Services.AddTransient<otpMail>();
            builder.Services.AddTransient<userPanelOMR.Service.otpMail>();

            // Add Authentication with JWT
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = "adityaInfotech",

                        ValidateAudience = true,
                        ValidAudience = "GTG's IntoTech",

                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes("aEj7A6mr5yVoDx0wq1jUj0A6xhb/8I+YJ0T+Y8h2sJk=")) //  yeh key secure honi chahiye
                    };
                });


            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                 
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            // 🛡 Use Authentication & Authorization Middlewares
            app.UseAuthentication();  // 👈 Add this
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
