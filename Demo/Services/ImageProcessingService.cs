using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace BlazorImageProcessing
{
    public class ImageProcessingService
    {
        private readonly IJSRuntime _jsRuntime;

        public ImageProcessingService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public ValueTask<string> CropImage(string imageDataUrl, int left, int right, int top, int bottom)
        {
            return _jsRuntime.InvokeAsync<string>("imageProcessing.cropImage", imageDataUrl, left, right, top, bottom);
        }

        public ValueTask<string> RotateImage(string imageDataUrl, int angle)
        {
            return _jsRuntime.InvokeAsync<string>("imageProcessing.rotateImage", imageDataUrl, angle);
        }

        public ValueTask<string> AddText(string imageDataUrl, string text, int x, int y, int fontSize, string color)
        {
            return _jsRuntime.InvokeAsync<string>("imageProcessing.addText", imageDataUrl, text, x, y, fontSize, color);
        }
    }
}
