using SharpGit.Classes;

public static class CommandGuard
{
	public static async Task Run(Func<Task> action)
	{
		await CommandBootstrapper.EnsureReady();

		await action();
	}
}
