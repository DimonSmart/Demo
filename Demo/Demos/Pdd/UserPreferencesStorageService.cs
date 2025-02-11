using Blazored.LocalStorage;

namespace Demo.Demos.Pdd
{
    public class UserPreferencesStorageService
    {
        private const string StorageKey = "userPreferences";
        private readonly ILocalStorageService _localStorage;

        public UserPreferencesStorageService(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        public async Task SavePreferencesAsync(UserPreferences preferences)
        {
            await _localStorage.SetItemAsync(StorageKey, preferences);
        }

        public async Task<UserPreferences?> LoadPreferencesAsync()
        {
            return await _localStorage.GetItemAsync<UserPreferences>(StorageKey);
        }
    }
}