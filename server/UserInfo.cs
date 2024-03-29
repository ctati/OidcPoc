﻿using System.Security.Claims;
using System.Text.Json.Serialization;

namespace server
{
    public class UserInfo
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("claims")]
        public List<KeyValuePair<string, string>>? ClaimsList { get; set; }
    }

    public static class UserHelpers
    {
        public static string? GetId(this ClaimsPrincipal principal)
        {
            var userIdClaim = principal.FindFirst(c => c.Type == ClaimTypes.NameIdentifier) ?? principal.FindFirst(c => c.Type == "sub");
            if (userIdClaim != null && !string.IsNullOrEmpty(userIdClaim.Value))
            {
                return userIdClaim.Value;
            }

            return null;
        }
    }
}
