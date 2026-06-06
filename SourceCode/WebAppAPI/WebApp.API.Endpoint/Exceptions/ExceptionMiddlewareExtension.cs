using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using System.Net;
using System.Text.Json;

namespace WebApp.API.Endpoint.Exceptions;

public static class ExceptionMiddlewareExtension
{
	public static void ConfigureBuiltInExceptionHandler(this IApplicationBuilder app)
	{
		app.UseExceptionHandler(appError =>
		{
			appError.Run(async context =>
			{
				context.Response.ContentType = "application/json";
				var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
				if (contextFeature != null)
				{
					var errorModel = new
					{
						StatusCode = (int)HttpStatusCode.InternalServerError,
						Message = contextFeature.Error.Message,
						Path = context.Features.Get<IHttpRequestFeature>().Path
					};
					await context.Response.WriteAsync(JsonSerializer.Serialize(errorModel));
				}
			});
		});
	}
}
