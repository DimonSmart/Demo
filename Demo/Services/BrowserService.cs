using Microsoft.JSInterop;

namespace Demo.Services
{
    public class BrowserService
    {
        private readonly IJSRuntime _js;

        public BrowserService(IJSRuntime js)
        {
            _js = js;
        }

        public async Task<PageDimension> GetPageDimensionsWithoutPaddingAsync(string elementId)
        {
            return await _js.InvokeAsync<PageDimension>("getPageDimensionsWithoutPadding", elementId);
        }
    }
}