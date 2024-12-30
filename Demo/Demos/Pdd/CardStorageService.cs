using Blazored.LocalStorage;

namespace Demo.Demos.Pdd
{
    public class CardStorageService(ILocalStorageService localStorage)
    {
        private const string StorageKey = "questionCards";

        public async Task SaveCardsAsync(StoredCardsSet data)
        {
            var compact = data.ToCompact();
            await localStorage.SetItemAsync(StorageKey, compact);
        }

        public async Task<StoredCardsSet?> LoadCardsAsync()
        {
            var compact = await localStorage.GetItemAsync<StoredCardsSetCompact>(StorageKey);
            if (compact is null || string.IsNullOrWhiteSpace(compact.V)) return null;
            return compact.ToDomain();
        }

        //    public async Task<bool> HasSavedCardsAsync()
        //    {
        //        return await localStorage.ContainKeyAsync(StorageKey);
        //    }
    }
}
