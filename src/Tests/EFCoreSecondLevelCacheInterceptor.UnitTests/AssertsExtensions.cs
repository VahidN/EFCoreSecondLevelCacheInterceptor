namespace EFCoreSecondLevelCacheInterceptor.UnitTests;

public static class AssertsExtensions
{
    public static Exception RecordException(Action action)
    {
        try
        {
            action();

            return null;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

    public static async Task<Exception> RecordExceptionAsync(Func<Task> action)
    {
        try
        {
            await action();

            return null;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }
}