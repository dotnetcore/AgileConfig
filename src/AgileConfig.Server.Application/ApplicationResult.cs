namespace AgileConfig.Server.Application;

public sealed class ApplicationResult
{
    private ApplicationResult(bool succeeded, ApplicationError error)
    {
        Succeeded = succeeded;
        Error = error;
    }

    public bool Succeeded { get; }

    public ApplicationError Error { get; }

    public static ApplicationResult Success()
    {
        return new ApplicationResult(true, ApplicationError.None);
    }

    public static ApplicationResult Failure(ApplicationError error)
    {
        return new ApplicationResult(false, error);
    }
}

public sealed class ApplicationResult<T>
{
    private ApplicationResult(bool succeeded, T value, ApplicationError error)
    {
        Succeeded = succeeded;
        Value = value;
        Error = error;
    }

    public bool Succeeded { get; }

    public T Value { get; }

    public ApplicationError Error { get; }

    public static ApplicationResult<T> Success(T value)
    {
        return new ApplicationResult<T>(true, value, ApplicationError.None);
    }

    public static ApplicationResult<T> Failure(ApplicationError error)
    {
        return new ApplicationResult<T>(false, default, error);
    }

    public static ApplicationResult<T> Failure(ApplicationError error, T value)
    {
        return new ApplicationResult<T>(false, value, error);
    }
}
