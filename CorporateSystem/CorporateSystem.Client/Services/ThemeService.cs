using Microsoft.JSInterop;

namespace CorporateSystem.Client.Services;

public class ThemeService
{
    private readonly IJSRuntime _js;
    private string _currentTheme = "theme-light";

    public event Action? OnThemeChanged;

    public string CurrentTheme => _currentTheme;
    public bool IsDark => _currentTheme == "theme-dark";

    public ThemeService(IJSRuntime js)
    {
        _js = js;
    }

    public async Task InitializeAsync()
    {
        try
        {
            var savedTheme = await _js.InvokeAsync<string>("localStorage.getItem", "app-theme");
            if (!string.IsNullOrEmpty(savedTheme))
            {
                _currentTheme = savedTheme;
            }
        }
        catch
        {
            // localStorage may not be available during prerendering
        }
    }

    public async Task ToggleThemeAsync()
    {
        _currentTheme = _currentTheme == "theme-light" ? "theme-dark" : "theme-light";

        try
        {
            await _js.InvokeVoidAsync("localStorage.setItem", "app-theme", _currentTheme);
        }
        catch
        {
            // Ignore during prerendering
        }

        OnThemeChanged?.Invoke();
    }
}
