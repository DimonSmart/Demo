using Blazored.LocalStorage;

namespace Demo.Demos.Common
{
    public class UserPreferencesStorageService<T>
    {
        private readonly ILocalStorageService _localStorage;
        private const string StorageKey = "userPreferences";

        public UserPreferencesStorageService(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        public async Task SavePreferencesAsync(T preferences)
        {
            await _localStorage.SetItemAsync(StorageKey, preferences);
        }

        public async Task<T?> LoadPreferencesAsync()
        {
            return await _localStorage.GetItemAsync<T>(StorageKey);
        }
    }
}
