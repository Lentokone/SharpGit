using SharpGit.Classes;
using LibGit2Sharp;

namespace SharpGit.Tests.Services;

public class GitServiceTests
{
	// Add Testing for:
	// Existing file but outside of Repository
	// Adding same file twice and checking only one instance of it appears
	[Fact]
	public void AddToRepo_AddsAFile()
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
	public void AddToRepo_DoesNotAddNonExistent()
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

	[Fact]
	public void AddToRepo_WontAddFileFromOutsideRepo()
	{
		var TestingPath = Path.Combine(Path.GetTempPath(), "SharpGitTests-" + Guid.NewGuid());
		try
		{
			var RepoPath = Path.Combine(TestingPath, "test");
			string RootedPath = Repository.Init(RepoPath);
			Assert.True(Directory.Exists(Path.Combine(RootedPath, "objects")));

			var repo = new Repository(RootedPath);
			Assert.Empty(repo.RetrieveStatus());

			string fileOutside = Path.Combine(TestingPath, "test.txt");
			File.WriteAllText(fileOutside, "Test");

			GitService.AddToRepo(repo, fileOutside);
			Assert.Empty(repo.RetrieveStatus());

		}
		finally
		{
			if (Directory.Exists(TestingPath))
				Directory.Delete(TestingPath, true);
		}
	}

	/// <summary>
	///
	/// So to just add the file twice
	/// A stiff testing name for it
	///
	/// </summary>
	[Fact]
	public void AddToRepo_DoesNotAddDuplicate()
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

			GitService.AddToRepo(repo, "test1.txt");
			status = repo.RetrieveStatus();
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
	public static void AddToRepoUpdate_AddTracked()
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
			var status = repo.RetrieveStatus();
			Assert.Single(status);
			Assert.Equal("test1.txt", status.First().FilePath);

			repo.Index.Add("test1.txt");
			repo.Index.Write();
			Signature author = new Signature("James", "@jugglingnutcase", DateTime.Now);
			Signature committer = author;

			Commit commit = repo.Commit("Here's a commit i made!", author, committer);
			status = repo.RetrieveStatus();
			Assert.Empty(status);
			File.WriteAllText(file1path, "Beast of burden");
			GitService.AddToRepoUpdate(repo);
			status = repo.RetrieveStatus();
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
	public static void AddToRepoUpdate_AddOnlyTracked()
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
			string file2path = Path.Combine(RepoPath, "test2.txt");
			File.WriteAllText(file2path, "Test 2");
			var status = repo.RetrieveStatus();
			Assert.Equal(2, status.Count());
			Assert.All(status, s => Assert.Equal(LibGit2Sharp.FileStatus.NewInWorkdir, s.State));
			repo.Index.Add("test1.txt");
			repo.Index.Write();

			var filtered = repo.RetrieveStatus()
			    .Where(i =>
				(i.State & FileStatus.NewInIndex) != 0);
			Assert.Single(filtered);
			File.WriteAllText(file1path, "Beast of Burden");
			File.WriteAllText(file2path, "BoB");

			GitService.AddToRepoUpdate(repo);

			foreach (var entry in repo.RetrieveStatus())
			{
				if (entry.FilePath == "test1.txt")
				{
					Assert.NotEqual(FileStatus.ModifiedInWorkdir, entry.State);
				}
				else
				{
					Assert.Equal(FileStatus.NewInWorkdir, entry.State);
				}
			}
		}
		finally
		{
			if (Directory.Exists(TestingPath))
				Directory.Delete(TestingPath, true);
		}
	}

	[Fact]
	public static void AddToRepoUpdate_DoesNotAddDuplicate()
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
			var status = repo.RetrieveStatus();
			Assert.Single(status);
			Assert.Equal("test1.txt", status.First().FilePath);

			repo.Index.Add("test1.txt");
			repo.Index.Write();
			Signature author = new Signature("James", "@jugglingnutcase", DateTime.Now);
			Signature committer = author;

			Commit commit = repo.Commit("Here's a commit i made!", author, committer);
			status = repo.RetrieveStatus();
			Assert.Empty(status);
			File.WriteAllText(file1path, "Beast of burden");
			GitService.AddToRepoUpdate(repo);
			status = repo.RetrieveStatus();
			Assert.Single(status);
			Assert.Equal("test1.txt", status.First().FilePath);

			GitService.AddToRepoUpdate(repo);
			status = repo.RetrieveStatus();
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
	public static void AddToRepoUpdate_WillNotAddWhenEmpty()
	{
		var TestingPath = Path.Combine(Path.GetTempPath(), "SharpGitTests-" + Guid.NewGuid());
		try
		{
			var RepoPath = Path.Combine(TestingPath, "test");
			string RootedPath = Repository.Init(RepoPath);
			Assert.True(Directory.Exists(Path.Combine(RootedPath, "objects")));

			var repo = new Repository(RootedPath);
			Assert.Empty(repo.RetrieveStatus());

			GitService.AddToRepoUpdate(repo);
			Assert.Empty(repo.RetrieveStatus());
		}
		finally
		{
			if (Directory.Exists(TestingPath))
				Directory.Delete(TestingPath, true);
		}
	}

	[Fact]
	public static void AddToRepoAll_AddEverything()
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

			GitService.AddToRepoAll(repo);
			var status = repo.RetrieveStatus();
			Assert.Single(status);
			Assert.Equal("test1.txt", status.First().FilePath);

			string file2path = Path.Combine(RepoPath, "test2.txt");
			File.WriteAllText(file2path, "Test 2");
			GitService.AddToRepoAll(repo);
			status = repo.RetrieveStatus();
			Assert.Equal(2, status.Count());
		}
		finally
		{
			if (Directory.Exists(TestingPath))
				Directory.Delete(TestingPath, true);
		}

	}

	[Fact]
	public static void AddToRepoAll_DoesNotAddAnythingInAnEmptyRepo()
	{
		var TestingPath = Path.Combine(Path.GetTempPath(), "SharpGitTests-" + Guid.NewGuid());
		try
		{
			var RepoPath = Path.Combine(TestingPath, "test");
			string RootedPath = Repository.Init(RepoPath);
			Assert.True(Directory.Exists(Path.Combine(RootedPath, "objects")));

			var repo = new Repository(RootedPath);
			Assert.Empty(repo.RetrieveStatus());

			GitService.AddToRepoAll(repo);
			Assert.Empty(repo.RetrieveStatus());
		}
		finally
		{
			if (Directory.Exists(TestingPath))
				Directory.Delete(TestingPath, true);
		}
	}

	[Fact]
	public static void AddToRepoAll_DoesNotAddDuplicateIfCalledTwice()
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

			GitService.AddToRepoAll(repo);
			var status = repo.RetrieveStatus();
			Assert.Single(status);
			Assert.Equal("test1.txt", status.First().FilePath);

			GitService.AddToRepoAll(repo);
			status = repo.RetrieveStatus();
			Assert.Single(status);
			Assert.Equal("test1.txt", status.First().FilePath);
		}
		finally
		{
			if (Directory.Exists(TestingPath))
				Directory.Delete(TestingPath, true);
		}
	}
}
