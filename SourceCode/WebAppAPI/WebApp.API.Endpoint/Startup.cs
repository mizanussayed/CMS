using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using WebApp.API.Endpoint.Exceptions;
using WebApp.API.Persistence;
using WebApp.API.Persistence.Identity;
using WebApp.Core.Constant;
using WebApp.Core.Contract.Infrastructure;
using WebApp.Core.Contract.Persistence;
using WebApp.Infrastructure;
using System;
using System.Linq;
using System.Text;
using System.Threading.RateLimiting;

namespace WebApp.API.Endpoint;

public class Startup
{
	public IConfiguration Configuration { get; }

	public Startup(IConfiguration configuration)
	{
		Configuration = configuration;
	}

	// This method gets called by the runtime. Use this method to add services to the container.
	public void ConfigureServices(IServiceCollection services)
	{
		services.AddInfrastructureServices(Configuration);
		services.AddHttpClient("SMSAPI", c => { c.BaseAddress = new Uri(Configuration.GetValue<string>("SMSSettings:SMSBaseAPIAddress")); });

		services.AddControllers();
		services.AddMemoryCache();

		services.AddCors(options => { options.AddPolicy("CorsPolicy", builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader().AllowCredentials().Build()); });

		services.AddApiVersioning(options =>
		{
			options.AssumeDefaultVersionWhenUnspecified = true;
			options.DefaultApiVersion = new(1, 0);
			options.ReportApiVersions = true;
		});

		services.AddVersionedApiExplorer(options =>
		{
			options.GroupNameFormat = "'v'VVV";
			options.SubstituteApiVersionInUrl = true;
		});

		services.AddSwaggerGen(options =>
		{
			var title = "WebApp.API.Endpoint";
			var description = "This is a Web API that demonstrates Web Application development.";
			var terms = new Uri("https://localhost/terms");
			var license = new OpenApiLicense { Name = "This is my full license information or a link to it." };
			var contact = new OpenApiContact { Name = "WebApp Helpdesk", Email = "help@webapp.com", Url = new Uri("https://www.webapp.com") };

			options.SwaggerDoc("v1", new OpenApiInfo
			{
				Version = "v1",
				Title = $"{title} v1",
				Description = description,
				TermsOfService = terms,
				License = license,
				Contact = contact
			});

			options.SwaggerDoc("v2", new OpenApiInfo
			{
				Version = "v2",
				Title = $"{title} v2",
				Description = description,
				TermsOfService = terms,
				License = license,
				Contact = contact
			});

			options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
			options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
			{
				Name = "Authorization",
				Type = SecuritySchemeType.ApiKey,
				Scheme = "Bearer",
				BearerFormat = "JWT",
				In = ParameterLocation.Header,
				Description = "JWT Authorization header using the Bearer scheme.\r\n\r\nEnter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 1safsfsdfdfd\""
			});
			options.AddSecurityRequirement(new OpenApiSecurityRequirement
			{
				{
					new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }},
					new string[] {}
				}
			});
		});

		services.AddDbContext<MembershipDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("MembershipDatabase")));

		services.AddIdentity<ApplicationUser, IdentityRole>(options =>
		{
			options.SignIn.RequireConfirmedAccount = true;

			options.Password.RequireDigit = false;
			options.Password.RequireLowercase = false;
			options.Password.RequireNonAlphanumeric = false;
			options.Password.RequireUppercase = false;
			options.Password.RequiredLength = 6;

			options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
			options.Lockout.MaxFailedAccessAttempts = 1000;
			options.Lockout.AllowedForNewUsers = true;

			options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+#";
			options.User.RequireUniqueEmail = true;
		})
		.AddEntityFrameworkStores<MembershipDbContext>()
		.AddDefaultUI()
		.AddDefaultTokenProviders();

		services.AddAuthorization(options =>
		{
			options.AddPolicy(Constants.SystemAdmin, policy => { policy.RequireClaim("UserRole", "SystemAdmin"); });
			options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
		});

		services.AddAuthentication(options =>
		{
			options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
		})
		.AddJwtBearer(options =>
		{
			options.TokenValidationParameters = new TokenValidationParameters()
			{
				ValidateIssuer = true,
				ValidateAudience = true,
				ValidateLifetime = true,
				ValidateIssuerSigningKey = true,
				ValidIssuer = Configuration["JWT:Issuer"],
				ValidAudience = Configuration["JWT:Audience"],
				IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWT:SecretKey"]))
			};
		});

		services.AddRateLimiter(_ => _
		.AddFixedWindowLimiter(policyName: "LimiterPolicy", options =>
		{
			options.PermitLimit = 4;
			options.Window = TimeSpan.FromSeconds(10);
			options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
			options.QueueLimit = 2;
		}));

		services.AddSingleton<IDataAccessHelper, DataAccessHelper>();
		services.AddSingleton<ISecurityHelper, SecurityHelper>();
		services.AddScoped<IAuditLogRepository, AuditLogRepository>();
		services.AddScoped<IApplicationLogRepository, ApplicationLogRepository>();
		services.AddScoped<ICsvExporter, CsvExporter>();
		services.AddScoped<IEmailTemplateRepository, EmailTemplateRepository>();
		services.AddScoped<ICategoryRepository, CategoryRepository>();
		services.AddScoped<IPieRepository, PieRepository>();
		services.AddScoped<IMobileAuthRepository, MobileAuthRepository>();
		services.AddScoped<IAuthRepository, AuthRepository>();
	}

	// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
	public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
	{
		app.UseDeveloperExceptionPage();
		app.UseSwagger();

		// Local
		app.UseSwaggerUI(options =>
		{
			options.SwaggerEndpoint("/swagger/v1/swagger.json", "WebApp.API.Endpoint v1");
			options.SwaggerEndpoint("/swagger/v2/swagger.json", "WebApp.API.Endpoint v2");
		});

		//// Stagging
		//app.UseSwaggerUI(options =>
		//{
		//	options.SwaggerEndpoint("/demo/webapp/api/swagger/v1/swagger.json", "WebApp.API.Endpoint v1");
		//	options.SwaggerEndpoint("/demo/webapp/api/swagger/v2/swagger.json", "WebApp.API.Endpoint v2");
		//});

		app.UseHsts();

		app.UseHttpsRedirection();
		//app.UseSerilogRequestLogging(); // Generates entry like this: HTTP "GET" "/" responded 200 in 265.8149 ms
		app.UseRouting();
		app.UseRateLimiter();
		app.UseAuthentication();
		app.UseAuthorization();
		app.ConfigureBuiltInExceptionHandler();
		app.UseEndpoints(endpoints =>
		{
			endpoints.MapControllers();
		});
	}
}