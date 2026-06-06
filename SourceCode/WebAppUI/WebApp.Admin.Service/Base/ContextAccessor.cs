using Microsoft.AspNetCore.Http;

namespace WebApp.Admin.Service.Base;

public class ContextAccessor : IContextAccessor
{
    private readonly IHttpContextAccessor _accessor;

    public ContextAccessor(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    public HttpContext GetContext()
    {
        return _accessor.HttpContext;
    }
}

public interface IContextAccessor
{
    HttpContext GetContext();
}