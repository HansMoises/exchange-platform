using ExchangePlatform.Domain.Interfaces.Services;
using Microsoft.Extensions.Configuration;

namespace ExchangePlatform.Infrastructure.Services;

public class UrlSettings : IUrlSettings
{
    public string FrontendUrl { get; }

    public UrlSettings(IConfiguration config)
    {
        FrontendUrl = config["FrontendUrl"] ?? "http://localhost:5173";
    }
}
