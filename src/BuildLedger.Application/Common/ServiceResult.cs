namespace BuildLedger.Application.Common;

public class ServiceResult
{
    public bool Success { get; init; }
    public string? Error { get; init; }
    public static ServiceResult Ok() => new() { Success = true };
    public static ServiceResult Fail(string error) => new() { Success = false, Error = error };
}

public class ServiceResult<T> : ServiceResult
{
    public T? Data { get; init; }
    public static ServiceResult<T> Ok(T data) => new() { Success = true, Data = data };
    public static new ServiceResult<T> Fail(string error) => new() { Success = false, Error = error };
}

public record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize)
{
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}

public record DateRange(DateTime From, DateTime To)
{
    public static DateRange Today() => new(DateTime.Today, DateTime.Today.AddDays(1).AddTicks(-1));
    public static DateRange Yesterday() => new(DateTime.Today.AddDays(-1), DateTime.Today.AddTicks(-1));
    public static DateRange ThisWeek()
    {
        var start = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
        return new(start, start.AddDays(7).AddTicks(-1));
    }
    public static DateRange ThisMonth()
    {
        var start = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        return new(start, start.AddMonths(1).AddTicks(-1));
    }
    public static DateRange LastMonth()
    {
        var start = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-1);
        return new(start, start.AddMonths(1).AddTicks(-1));
    }
}
