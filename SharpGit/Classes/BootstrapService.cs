// Note to self:
// Read this through and ponder long
public class BootstrapService
{
	public bool IsReady()
	{
		return IsConfigured() && HasSshKey();
	}

	public void EnsureReady()
	{
		if (!IsReady())
		{
			RunSetup();
		}
	}
}


public class CommandRunner
{
	private readonly BootstrapService _bootstrap;

	public CommandRunner(BootstrapService bootstrap)
	{
		_bootstrap = bootstrap;
	}

	public void Run(Action action)
	{
		if (!_bootstrap.IsReady())
		{
			_bootstrap.EnsureReady();
			return;
		}

		action();
	}
}
// Example usage
//
// cloneCommand.SetHandler(() =>
// {
//     runner.Run(() => gitService.Clone());
// });
