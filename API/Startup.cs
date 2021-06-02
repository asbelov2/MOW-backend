using API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;

namespace API
{
  public class Startup
  {
    public Startup(IConfiguration configuration)
    {
      Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
      string connection = Configuration.GetConnectionString("DefaultConnection");
      services.AddDbContext<ApplicationContext>(options => options.UseSqlServer(connection));
      services.AddCors(
  options => options.AddPolicy("AllowCors",
  builder =>
  {
    builder.WithOrigins("http://localhost:3000")
    .AllowCredentials()
    .AllowAnyHeader()
    .AllowAnyMethod();
  })
  );
      services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                  options.RequireHttpsMetadata = false;
                  options.TokenValidationParameters = new TokenValidationParameters
                  {
                    ValidateIssuer = true,
                    ValidIssuer = AuthOptions.ISSUER,
                    ValidateAudience = true,
                    ValidAudience = AuthOptions.AUDIENCE,
                    ValidateLifetime = true,
                    IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
                    ValidateIssuerSigningKey = true,
                  };
                  options.Events = new JwtBearerEvents
                  {
                    OnMessageReceived = context =>
                    {
                      var accessToken = context.Request.Query["access_token"];

                      // если запрос направлен хабу
                      var path = context.HttpContext.Request.Path;
                      if (!string.IsNullOrEmpty(accessToken) &&
                          (path.StartsWithSegments("/gamehub")))
                      {
                        // получаем токен из строки запроса
                        context.Token = accessToken;
                      }
                      return Task.CompletedTask;
                    }
                  };
                });
      services.AddSignalR();
      services.AddSingleton<RoomService>();
      services.AddSingleton<GameService>();
      services.AddControllers();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }

      app.UseCors("AllowCors");

      app.UseRouting();

      app.UseAuthentication();

      app.UseAuthorization();

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapControllers();
        endpoints.MapHub<GameHub>("/gamehub", option =>
        {
          option.Transports = HttpTransportType.WebSockets;
        });
      });
      var loggerFactory = LoggerFactory.Create(builder =>
      {
        builder.AddDebug();
      });
      ILogger logger = loggerFactory.CreateLogger<Startup>();
      app.Run(async (context) =>
      {
        logger.LogCritical("LogCritical {0}", context.Request.Path);
        logger.LogDebug("LogDebug {0}", context.Request.Path);
        logger.LogError("LogError {0}", context.Request.Path);
        logger.LogInformation("LogInformation {0}", context.Request.Path);
        logger.LogWarning("LogWarning {0}", context.Request.Path);
      });
    }
  }
}