namespace Demo.Services
{
    public static class ExceptionMessageHelper
    {
        public static string BuildExceptionMessage(string context, Exception ex)
        {
            var message = $"{context}: {ex.GetType().Name}: {ex.Message}";
            if (ex.InnerException != null)
            {
                message += $" | Inner: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}";
            }

            return message;
        }
    }
}
