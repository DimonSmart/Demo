namespace Demo.Demos.Pdd
{
    public static class LocalizedTextExtensions
    {
        public static bool IsEmpty(this LocalizedText localizedText) =>
             (string.IsNullOrEmpty(localizedText.Russian) &&
                string.IsNullOrEmpty(localizedText.Spanish) &&
                string.IsNullOrEmpty(localizedText.English));
    }

}