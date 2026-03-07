using SharpGit.Classes;

namespace SharpGit.Tests.Utils;

public class GitUtilsTests
{
	[Fact]
	[Trait("Category", "Utils")]
	public void TryFindRepository()
	{
		var test = GitUtils.TryFindRepositoryFromCurrentDirectory();
		Assert.NotNull(test);
	}
}
