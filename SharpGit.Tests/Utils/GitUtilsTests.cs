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

	[Fact]
	public void GetConfig_Works()
	{
		var TestingPath = Path.Combine(Path.GetTempPath(), "SharpGitTests-" + Guid.NewGuid());
		try
		{
			var repoPath = Path.Combine(TestingPath, "repository");
			var rootedPath = Repository.Init(repoPath);

			Assert.True(Directory.Exists(Path.Combine(rootedPath, "objects")));

			var result = GitUtils.GetConfig();
			Assert.NotNull(result);
		}
		finally
		{
			if (Directory.Exists(TestingPath))
				Directory.Delete(TestingPath, true);
		}
	}

	[Fact()]
	public void JokuSSHkey()
	{
		try
		{
			bool result = GitUtils.HasSSHKeygen();
			Assert.True(result);
		}
		finally
		{
			if (File.Exists("/home/welho/.sharpgit/ssh/SharpHub_key") || File.Exists("/home/welho/.sharpgit/ssh/SharpHub_key.pub"))
			{
				File.Delete("/home/welho/.sharpgit/ssh/SharpHub_key");
				File.Delete("/home/welho/.sharpgit/ssh/SharpHub_key.pub");
			}
		}
	}
}
