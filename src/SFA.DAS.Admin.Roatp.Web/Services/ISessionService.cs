namespace SFA.DAS.Admin.Roatp.Web.Services;

public interface ISessionService
{
    void Clear();
    bool Contains(string key);
    void Delete(string key);
    string Get(string key);
    T Get<T>(string key);
    void Set(string key, string value);
    void Set<T>(string key, T model);
}