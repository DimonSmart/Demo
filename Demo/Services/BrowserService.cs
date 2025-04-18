﻿using Microsoft.JSInterop;

namespace Demo.Services
{
    public class BrowserService(IJSRuntime js)
    {
        public async Task<Dimension> GetPageDimensionsWithoutPaddingAsync(string elementId)
        {
            return await js.InvokeAsync<Dimension>("getPageDimensionsWithoutPadding", elementId);
        }

        public async Task<Dimension> GetElementSizeByIdAsync(string elementId)
        {
            return await js.InvokeAsync<Dimension>("getElementSize", elementId);
        }
    }
}