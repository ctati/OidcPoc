using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

public class ApplicationOwnerRequirement : IAuthorizationRequirement
{
    public const string CurrentUser = "CurrentOwner";

    public string Owner { get; private set; }

    public ApplicationOwnerRequirement(string appOwner)
    {
        Owner = appOwner;
    }
}

public class ApplicationOwnerRequirementHandler :
             AuthorizationHandler<ApplicationOwnerRequirement>
{
    protected override Task HandleRequirementAsync(
      AuthorizationHandlerContext context,
      ApplicationOwnerRequirement requirement)
    {
        var user = context.User;
        if (!user.HasClaim(c => c.Type == ClaimTypes.Email))
            return Task.CompletedTask;

        var owner = requirement.Owner;
        if (requirement.Owner == ApplicationOwnerRequirement.CurrentUser)
        {
            // TCK: we only get the HTTP context NOT the MVC context
            if (context.Resource is DefaultHttpContext httpContext)
            {
                var url = httpContext.Request.GetDisplayUrl();
                // TODO: do some logic based on the HTTP context
            }
        }

        var emailAddress = user.FindFirst(ClaimTypes.Email)?.Value;
        if ((emailAddress != null) && emailAddress.StartsWith("ctati"))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
