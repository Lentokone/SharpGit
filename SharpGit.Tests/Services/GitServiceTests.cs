using SharpGit.Classes;
using LibGit2Sharp;

namespace SharpGit.Tests.Services;

public class GitServiceTests
{
	// Add Testing for:
	// Existing file but outside of Repository
	// Adding same file twice and checking only one instance of it appears
	// Adding two separate files and checking that both appear 
	[Fact]
	public void TestAddToRepo()
	{
		var TestingPath = Path.Combine(Path.GetTempPath(), "SharpGitTests-" + Guid.NewGuid());
		try
		{
			var RepoPath = Path.Combine(TestingPath, "test");
			string RootedPath = Repository.Init(RepoPath);
			Assert.True(Directory.Exists(Path.Combine(RootedPath, "objects")));

			var repo = new Repository(RootedPath);
			Assert.Empty(repo.RetrieveStatus());
			string file1path = Path.Combine(RepoPath, "test1.txt");
			File.WriteAllText(file1path, "Test 1");

			GitService.AddToRepo(repo, "test1.txt");
			var status = repo.RetrieveStatus();
			Assert.Single(status);
			Assert.Equal("test1.txt", status.First().FilePath);
		}
		finally
		{
			if (Directory.Exists(TestingPath))
				Directory.Delete(TestingPath, true);
		}
	}

	[Fact]
	public void TestAddNonExistentToRepo()
	{

		var TestingPath = Path.Combine(Path.GetTempPath(), "SharpGitTests-" + Guid.NewGuid());
		try
		{
			var RepoPath = Path.Combine(TestingPath, "test");
			string RootedPath = Repository.Init(RepoPath);
			Assert.True(Directory.Exists(Path.Combine(RootedPath, "objects")));

			var repo = new Repository(RootedPath);
			Assert.Empty(repo.RetrieveStatus());
			GitService.AddToRepo(repo, "test2.txt");
			Assert.Empty(repo.RetrieveStatus());
		}
		finally
		{
			if (Directory.Exists(TestingPath))
				Directory.Delete(TestingPath, true);
		}
	}
}
