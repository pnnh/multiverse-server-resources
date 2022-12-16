
using System;
using System.Web;

class HttpUtils
{
    public static bool IsBot(string userAgent)
    {
        userAgent = userAgent.ToLower();
        return userAgent.Contains("bot") || userAgent.Contains("spider");
    }
    public static string? GetClientAddress(HttpContext httpContext)
    {
        var clientIp = "";

        var forwardedValues = httpContext.Request.Headers["X-Forwarded-For"];
        if (forwardedValues.Count > 0)
        {
            clientIp = forwardedValues.FirstOrDefault("");
        }
        else
        {
            var remoteAddrValues = httpContext.Request.Headers["REMOTE_ADDR"];

            if (remoteAddrValues.Count > 0)
            {
                clientIp = remoteAddrValues.FirstOrDefault("");
            }
        }

        if (String.IsNullOrEmpty(clientIp))
        {

            var remoteAddress = httpContext.Connection.RemoteIpAddress;
            clientIp = remoteAddress?.ToString();
        }


        return clientIp;
    }
}