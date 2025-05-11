using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace CodeChat.Client.Services
{
    public class UserSessionService
    {
        private readonly IJSRuntime _jsRuntime;
        private const string USERNAME_KEY = "username";
        private const string IS_LOGGED_IN_KEY = "isLoggedIn";

        public string? Username { get; private set; }
        public bool IsLoggedIn { get; private set; }

        public UserSessionService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task InitializeAsync()
        {
            Username = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", USERNAME_KEY);
            IsLoggedIn = await _jsRuntime.InvokeAsync<bool>("localStorage.getItem", IS_LOGGED_IN_KEY);
        }

        public async Task SetUserSession(string username)
        {
            Username = username;
            IsLoggedIn = true;
            
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", USERNAME_KEY, username);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", IS_LOGGED_IN_KEY, true);
        }

        public async Task ClearSession()
        {
            Username = null;
            IsLoggedIn = false;
            
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", USERNAME_KEY);
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", IS_LOGGED_IN_KEY);
        }
    }
}
