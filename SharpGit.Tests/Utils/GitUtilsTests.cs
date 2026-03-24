using SharpGit.Classes;
using LibGit2Sharp;

namespace SharpGit.Tests.Utils;

public class GitUtilsTests
{
	[Fact]
	[Trait("Category", "Utils")]
	public void TryFindRepositoryFromCurrentDirectory_Works()
	{
		var TestingPath = Path.Combine(Path.GetTempPath(), "SharpGitTests-" + Guid.NewGuid());
		try
		{
			var repoPath = Path.Combine(TestingPath, "repository");
			var rootedPath = Repository.Init(repoPath);

			Assert.True(Directory.Exists(Path.Combine(rootedPath, "objects")));
			Directory.SetCurrentDirectory(rootedPath);

			var test = GitUtils.TryFindRepositoryFromCurrentDirectory();
			Assert.NotNull(test);
		}
		finally
		{
			if (Directory.Exists(TestingPath))
				Directory.Delete(TestingPath, true);
		}
	}

	[Fact(Skip = "Unfinished and maybe false and a bad testing area")]
	public void GetUsernameFromConfig_Works()
	{

		var TestingPath = Path.Combine(Path.GetTempPath(), "SharpGitTests-" + Guid.NewGuid());
		try
		{
			var repoPath = Path.Combine(TestingPath, "repository");
			var rootedPath = Repository.Init(repoPath);

			Assert.True(Directory.Exists(Path.Combine(rootedPath, "objects")));

			var result = GitUtils.GetUsernameFromConfig();
			Assert.True(result.Empty);
		}
		finally
		{
			if (Directory.Exists(TestingPath))
				Directory.Delete(TestingPath, true);
		}
	}
}
