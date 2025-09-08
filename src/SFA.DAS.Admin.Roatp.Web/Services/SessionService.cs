using System.Text.Json;

namespace SFA.DAS.Admin.Roatp.Web.Services;

public class SessionService(IHttpContextAccessor _httpContextAccessor) : ISessionService
{
    public void Set(string key, string value) => _httpContextAccessor.HttpContext?.Session.SetString(key, value);
    public void Set<T>(string key, T model) => Set(key, JsonSerializer.Serialize(model));

    public string? Get(string key) => _httpContextAccessor.HttpContext?.Session.GetString(key);

    public T Get<T>(string key)
    {
        var json = Get(key);
        return (string.IsNullOrEmpty(json) ? default : JsonSerializer.Deserialize<T>(json))!;
    }

    public void Delete(string key)
    {
        if (_httpContextAccessor.HttpContext != null && _httpContextAccessor.HttpContext.Session.Keys.Any(k => k == key))
            _httpContextAccessor.HttpContext.Session.Remove(key);
    }

    public void Clear() => _httpContextAccessor.HttpContext?.Session.Clear();

    public bool Contains(string key)
    {
        var result = _httpContextAccessor.HttpContext?.Session.Keys.Any(k => k == key);
        return result.GetValueOrDefault();
    }
}
