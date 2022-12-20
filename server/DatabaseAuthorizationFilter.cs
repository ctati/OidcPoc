using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class DatabaseAuthorizationFilterAttribute : Attribute, IAsyncAuthorizationFilter
{
    private IAuthorizationService? _authorization = null;
    public DatabaseAuthorizationFilterAttribute()
    {
        // TODO: Inject IAuthorizationService
        //_authorization = authorizationService;
    }

    public string? Policy { get; set; }
    public string? ClaimsProviderName { get; set; }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        // TODO: Use context to get the endpoint/action claims

        if (!string.IsNullOrEmpty(Policy) && (_authorization != null))
        {
            var authResult = await _authorization.AuthorizeAsync(context.HttpContext.User, this.Policy);
            if (!authResult.Succeeded)
            {
                context.Result = new UnauthorizedResult();
            }
        }
    }
}
