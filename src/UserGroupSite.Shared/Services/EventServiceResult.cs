namespace UserGroupSite.Shared.Services;

/// <summary>Represents the outcome of an event service operation.</summary>
/// <param name="IsSuccess">Indicates whether the operation succeeded.</param>
/// <param name="ErrorMessage">Optional error message when the operation fails.</param>
public sealed record EventServiceResult(bool IsSuccess, string? ErrorMessage)
{
    /// <summary>Creates a successful result.</summary>
    /// <returns>A successful result.</returns>
    public static EventServiceResult Success()
    {
        return new EventServiceResult(true, null);
    }

    /// <summary>Creates a failed result.</summary>
    /// <param name="errorMessage">The error message to associate with the failure.</param>
    /// <returns>A failed result.</returns>
    public static EventServiceResult Failure(string? errorMessage)
    {
        return new EventServiceResult(false, errorMessage);
    }
}

/// <summary>Represents the outcome of an event service operation with a value.</summary>
/// <typeparam name="T">The result value type.</typeparam>
/// <param name="IsSuccess">Indicates whether the operation succeeded.</param>
/// <param name="Value">The value when the operation succeeds.</param>
/// <param name="ErrorMessage">Optional error message when the operation fails.</param>
public sealed record EventServiceResult<T>(bool IsSuccess, T? Value, string? ErrorMessage)
{
    /// <summary>Creates a successful result.</summary>
    /// <param name="value">The value to return.</param>
    /// <returns>A successful result with a value.</returns>
    public static EventServiceResult<T> Success(T value)
    {
        return new EventServiceResult<T>(true, value, null);
    }

    /// <summary>Creates a failed result.</summary>
    /// <param name="errorMessage">The error message to associate with the failure.</param>
    /// <returns>A failed result without a value.</returns>
    public static EventServiceResult<T> Failure(string? errorMessage)
    {
        return new EventServiceResult<T>(false, default, errorMessage);
    }
}
