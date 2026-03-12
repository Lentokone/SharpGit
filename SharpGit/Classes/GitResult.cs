/// <summary>
///
///
/// </summary>
public class GitResult
{
	public bool Success { get; init; }
	public string? Message { get; init; }
	public Exception? Exception { get; init; }

	public static GitResult Ok()
	{
		return new GitResult { Success = true };
	}

	public static GitResult Fail(string message, Exception? ex = null)
	{
		return new GitResult
		{
			Success = false,
			Message = message,
			Exception = ex,
		};
	}
}
