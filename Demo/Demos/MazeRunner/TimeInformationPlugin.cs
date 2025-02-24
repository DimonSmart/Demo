using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace Demo.Demos.MazeRunner
{
    /// <summary>
    /// A class with a method that returns the current time.
    /// </summary>
    public class TimeInformationPlugin
    {
        [KernelFunction]
        [Description("Retrieves the current time in UTC.")]
        public string GetCurrentUtcTime() => DateTime.UtcNow.ToString("R");
    }
}
