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

	[Fact]
	public static void RemoveFromRepo_Works()
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
			Assert.Equal(FileStatus.NewInWorkdir, repo.RetrieveStatus().First().State);

			repo.Index.Add("test1.txt");
			repo.Index.Write();

			Assert.Equal(FileStatus.NewInIndex, repo.RetrieveStatus().First().State);

			GitService.RemoveFromRepo(repo, "test1.txt");

			Assert.Empty(repo.RetrieveStatus());
		}
		finally
		{
			if (Directory.Exists(TestingPath))
				Directory.Delete(TestingPath, true);
		}
	}

	[Fact]
	public static void RemoveFromRepo_DoesntRemoveNonExistent()
	{
		var TestingPath = Path.Combine(Path.GetTempPath(), "SharpGitTests-" + Guid.NewGuid());
		try
		{
			var RepoPath = Path.Combine(TestingPath, "test");
			string RootedPath = Repository.Init(RepoPath);
			Assert.True(Directory.Exists(Path.Combine(RootedPath, "objects")));

			var repo = new Repository(RootedPath);
			Assert.Empty(repo.RetrieveStatus());

			GitService.RemoveFromRepo(repo, "nonExistent.txt");
			var status = repo.RetrieveStatus();
			Assert.Empty(status);
		}
		finally
		{
			if (Directory.Exists(TestingPath))
				Directory.Delete(TestingPath, true);
		}
	}

	[Fact]
	public static void RemoveFromRepo_DoesntRemoveFileFromOutsideRepo()
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

			GitService.RemoveFromRepo(repo, fileOutside);
			var status = repo.RetrieveStatus();
			Assert.Empty(status);
		}
		finally
		{
			if (Directory.Exists(TestingPath))
				Directory.Delete(TestingPath, true);
		}
	}

	[Fact]
	public static void RemoveFromRepo_RemoveCommittedFile()
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
			Assert.Equal(FileStatus.NewInWorkdir, repo.RetrieveStatus().First().State);

			repo.Index.Add("test1.txt");
			repo.Index.Write();
			Signature author = new Signature("James", "@jugglingnutcase", DateTime.Now);
			Signature committer = author;
			// Just to mention. Those weird author and Commit message additions are straight from the LibGit2Sharp's Github wiki.
			Commit commit = repo.Commit("Here's a commit i made!", author, committer);
			var status = repo.RetrieveStatus();
			Assert.Empty(status);

			GitService.RemoveFromRepo(repo, "test1.txt");
			status = repo.RetrieveStatus();

			Assert.True(status.First().State.HasFlag(FileStatus.DeletedFromIndex));
			Assert.False(File.Exists(file1path));
		}
		finally
		{
			if (Directory.Exists(TestingPath))
				Directory.Delete(TestingPath, true);
		}
	}


	// Tähän väliin nuo commit jutut.

	[Fact]
	public static void CloneRepo_WorksWithNoGivenPath()
	{
		var TestingPath = Path.Combine(Path.GetTempPath(), "SharpGitTests-" + Guid.NewGuid());
		try
		{
			var RepoPath = Path.Combine(TestingPath, "remote/test");
			string RootedPath = Repository.Init(RepoPath);
			Assert.True(Directory.Exists(Path.Combine(RootedPath, "objects")));

			var repo = new Repository(RootedPath);
			Assert.Empty(repo.RetrieveStatus());

			string file1path = Path.Combine(RepoPath, "test1.txt");
			File.WriteAllText(file1path, "Test 1");
			Assert.Equal(FileStatus.NewInWorkdir, repo.RetrieveStatus().First().State);

			repo.Index.Add("test1.txt");
			repo.Index.Write();
			Signature author = new Signature("James", "@jugglingnutcase", DateTime.Now);
			Signature committer = author;
			Commit commit = repo.Commit("A test commit for the cloning.", author, committer);
			var localPath = Path.Combine(TestingPath, "local");
			Directory.CreateDirectory(localPath);
			Directory.SetCurrentDirectory(localPath);
			GitService.CloneRepo(RepoPath);

			Assert.True(Directory.Exists(localPath + "/test"));
			var ClonedRepo = new Repository(Path.Combine(localPath, "test"));
			Assert.Empty(ClonedRepo.RetrieveStatus());
			Assert.NotNull(ClonedRepo.Head.Tip);
		}
		finally
		{
			if (Directory.Exists(TestingPath))
				Directory.Delete(TestingPath, true);
		}
	}

	[Fact]
	public static void CloneRepo_WorksWithRelativePath()
	{
		var TestingPath = Path.Combine(Path.GetTempPath(), "SharpGitTests-" + Guid.NewGuid());
		try
		{
			var RepoPath = Path.Combine(TestingPath, "remote/test");
			string RootedPath = Repository.Init(RepoPath);
			Assert.True(Directory.Exists(Path.Combine(RootedPath, "objects")));

			var repo = new Repository(RootedPath);
			Assert.Empty(repo.RetrieveStatus());

			string file1path = Path.Combine(RepoPath, "test1.txt");
			File.WriteAllText(file1path, "Test 1");
			Assert.Equal(FileStatus.NewInWorkdir, repo.RetrieveStatus().First().State);

			repo.Index.Add("test1.txt");
			repo.Index.Write();
			Signature author = new Signature("James", "@jugglingnutcase", DateTime.Now);
			Signature committer = author;
			Commit commit = repo.Commit("A test commit for the cloning.", author, committer);
			Directory.SetCurrentDirectory(TestingPath);
			GitService.CloneRepo(RepoPath, "important");

			Assert.True(Directory.Exists(TestingPath + "/important/test"));
			var ClonedRepo = new Repository(Path.Combine(TestingPath, "important/test"));
			Assert.Empty(ClonedRepo.RetrieveStatus());
			Assert.NotNull(ClonedRepo.Head.Tip);
		}
		finally
		{
			if (Directory.Exists(TestingPath))
				Directory.Delete(TestingPath, true);
		}
	}
	// <summary>
	//
	// This tests if the CloneRepo() function works with an absolute path
	// and also to see if it will make a directory that doesn't exist
	// 
	// </summary>
	[Fact]
	public static void CloneRepo_WorksWithAbsolutePath()
	{
		var TestingPath = Path.Combine(Path.GetTempPath(), "SharpGitTests-" + Guid.NewGuid());
		try
		{
			var RepoPath = Path.Combine(TestingPath, "remote/test");
			string RootedPath = Repository.Init(RepoPath);
			Assert.True(Directory.Exists(Path.Combine(RootedPath, "objects")));

			var repo = new Repository(RootedPath);
			Assert.Empty(repo.RetrieveStatus());

			string file1path = Path.Combine(RepoPath, "test1.txt");
			File.WriteAllText(file1path, "Test 1");
			Assert.Equal(FileStatus.NewInWorkdir, repo.RetrieveStatus().First().State);

			repo.Index.Add("test1.txt");
			repo.Index.Write();
			Signature author = new Signature("James", "@jugglingnutcase", DateTime.Now);
			Signature committer = author;
			Commit commit = repo.Commit("A test commit for the cloning.", author, committer);

			var AbsolutePath = Path.Combine(TestingPath, "AbsoluteDir");
			// This is for the cloning function to also try to make a new directory called "/repos"
			GitService.CloneRepo(RepoPath, AbsolutePath + "/repos");
			Assert.True(Directory.Exists(AbsolutePath + "/repos/test"));

			var ClonedRepo = new Repository(Path.Combine(AbsolutePath, "repos/test"));
			Assert.Empty(ClonedRepo.RetrieveStatus());
			Assert.NotNull(ClonedRepo.Head.Tip);
		}
		finally
		{
			if (Directory.Exists(TestingPath))
				Directory.Delete(TestingPath, true);
		}
	}

	[Fact]
	public static void PushToRepo_Works()
	{
		var TestingPath = Path.Combine(Path.GetTempPath(), "SharpGitTests-" + Guid.NewGuid());
		try
		{
			var LocalRepoPath = Path.Combine(TestingPath, "remote/testlocal");
			string RootedPathForLocal = Repository.Init(LocalRepoPath);
			Assert.True(Directory.Exists(Path.Combine(RootedPathForLocal, "objects")));
			var BareRepoPath = Path.Combine(TestingPath, "remote/testbare.git");
			string RootedPathForBare = Repository.Init(BareRepoPath, true);
			Assert.True(Directory.Exists(Path.Combine(RootedPathForBare, "objects")));

			var LocalRepo = new Repository(RootedPathForLocal);
			Assert.Empty(LocalRepo.RetrieveStatus());
			var BareRepo = new Repository(RootedPathForBare);
			Assert.True(BareRepo.Info.IsBare);

			string file1path = Path.Combine(LocalRepoPath, "test1.txt");
			File.WriteAllText(file1path, "Test 1");
			Assert.Equal(FileStatus.NewInWorkdir, LocalRepo.RetrieveStatus().First().State);

			LocalRepo.Index.Add("test1.txt");
			LocalRepo.Index.Write();
			Signature author = new Signature("James", "@jugglingnutcase", DateTime.Now);
			Signature committer = author;
			Commit commit = LocalRepo.Commit("A test commit for the cloning.", author, committer);

			if (LocalRepo.Network.Remotes["origin"] == null)
			{
				LocalRepo.Network.Remotes.Add("origin", RootedPathForBare);
			}
			LocalRepo.Branches.Update(LocalRepo.Head, b =>
			{
				b.Remote = "origin";
				b.UpstreamBranch = "refs/heads/master";
			});

			Assert.Empty(BareRepo.Commits);
			GitService.PushToRepo(LocalRepo);
			Assert.Equal(LocalRepo.Head.Tip.Sha, BareRepo.Head.Tip.Sha);
		}
		finally
		{
			if (Directory.Exists(TestingPath))
				Directory.Delete(TestingPath, true);
		}
	}

	[Fact]
	public static void PushToRepo_WithNoCommits()
	{
		var TestingPath = Path.Combine(Path.GetTempPath(), "SharpGitTests-" + Guid.NewGuid());
		try
		{
			var LocalRepoPath = Path.Combine(TestingPath, "remote/testlocal");
			string RootedPathForLocal = Repository.Init(LocalRepoPath);
			Assert.True(Directory.Exists(Path.Combine(RootedPathForLocal, "objects")));
			var BareRepoPath = Path.Combine(TestingPath, "remote/testbare.git");
			string RootedPathForBare = Repository.Init(BareRepoPath, true);
			Assert.True(Directory.Exists(Path.Combine(RootedPathForBare, "objects")));

			var LocalRepo = new Repository(RootedPathForLocal);
			Assert.Empty(LocalRepo.RetrieveStatus());
			var BareRepo = new Repository(RootedPathForBare);
			Assert.True(BareRepo.Info.IsBare);

			if (LocalRepo.Network.Remotes["origin"] == null)
			{
				LocalRepo.Network.Remotes.Add("origin", RootedPathForBare);
			}
			LocalRepo.Branches.Update(LocalRepo.Head, b =>
			{
				b.Remote = "origin";
				b.UpstreamBranch = "refs/heads/master";
			});

			Assert.False(BareRepo.Commits.Any());
			GitService.PushToRepo(LocalRepo);
			Assert.False(BareRepo.Commits.Any());
		}
		finally
		{
			if (Directory.Exists(TestingPath))
				Directory.Delete(TestingPath, true);
		}
	}

	[Fact]
	public static void PushToRepo_WithNoUpstream()
	{
		var TestingPath = Path.Combine(Path.GetTempPath(), "SharpGitTests-" + Guid.NewGuid());
		try
		{
			var LocalRepoPath = Path.Combine(TestingPath, "remote/testlocal");
			string RootedPathForLocal = Repository.Init(LocalRepoPath);
			Assert.True(Directory.Exists(Path.Combine(RootedPathForLocal, "objects")));
			var BareRepoPath = Path.Combine(TestingPath, "remote/testbare.git");
			string RootedPathForBare = Repository.Init(BareRepoPath, true);
			Assert.True(Directory.Exists(Path.Combine(RootedPathForBare, "objects")));

			var LocalRepo = new Repository(RootedPathForLocal);
			Assert.Empty(LocalRepo.RetrieveStatus());
			var BareRepo = new Repository(RootedPathForBare);
			Assert.True(BareRepo.Info.IsBare);

			string file1path = Path.Combine(LocalRepoPath, "test1.txt");
			File.WriteAllText(file1path, "Test 1");
			Assert.Equal(FileStatus.NewInWorkdir, LocalRepo.RetrieveStatus().First().State);

			LocalRepo.Index.Add("test1.txt");
			LocalRepo.Index.Write();
			Signature author = new Signature("James", "@jugglingnutcase", DateTime.Now);
			Signature committer = author;
			Commit commit = LocalRepo.Commit("A test commit for the cloning.", author, committer);

			Assert.False(BareRepo.Commits.Any());
			GitService.PushToRepo(LocalRepo);
			Assert.False(BareRepo.Commits.Any());
		}
		finally
		{
			if (Directory.Exists(TestingPath))
				Directory.Delete(TestingPath, true);
		}
	}

	// <summary
	// This test verifies that pushing works for both an empty and a non-empty remote repository.
	//
	// The first push initializes the remote repository and sets its HEAD.
	// The second push verifies that pushing again correctly updates the remote HEAD
	// when the repository already contains commits.
	// </summary>
	[Fact]
	public static void PushToRepo_WorksToNonEmptyBare()
	{
		var TestingPath = Path.Combine(Path.GetTempPath(), "SharpGitTests-" + Guid.NewGuid());
		try
		{
			var LocalRepoPath = Path.Combine(TestingPath, "remote/testlocal");
			string RootedPathForLocal = Repository.Init(LocalRepoPath);
			Assert.True(Directory.Exists(Path.Combine(RootedPathForLocal, "objects")));
			var BareRepoPath = Path.Combine(TestingPath, "remote/testbare.git");
			string RootedPathForBare = Repository.Init(BareRepoPath, true);
			Assert.True(Directory.Exists(Path.Combine(RootedPathForBare, "objects")));

			var LocalRepo = new Repository(RootedPathForLocal);
			Assert.Empty(LocalRepo.RetrieveStatus());
			var BareRepo = new Repository(RootedPathForBare);
			Assert.True(BareRepo.Info.IsBare);

			if (LocalRepo.Network.Remotes["origin"] == null)
			{
				LocalRepo.Network.Remotes.Add("origin", RootedPathForBare);
			}
			LocalRepo.Branches.Update(LocalRepo.Head, b =>
			{
				b.Remote = "origin";
				b.UpstreamBranch = "refs/heads/master";
			});

			string file1path = Path.Combine(LocalRepoPath, "test1.txt");
			File.WriteAllText(file1path, "Test 1");
			Assert.Equal(FileStatus.NewInWorkdir, LocalRepo.RetrieveStatus().First().State);

			LocalRepo.Index.Add("test1.txt");
			LocalRepo.Index.Write();
			Signature author = new Signature("James", "@jugglingnutcase", DateTime.Now);
			Signature committer = author;
			Commit commit = LocalRepo.Commit("A test commit for the cloning.", author, committer);

			Assert.Empty(BareRepo.Commits);
			GitService.PushToRepo(LocalRepo);

			File.WriteAllText(file1path, "Beast of burden");
			Assert.Equal(FileStatus.ModifiedInWorkdir, LocalRepo.RetrieveStatus().First().State);

			LocalRepo.Index.Add("test1.txt");
			LocalRepo.Index.Write();
			author = new Signature("James", "@jugglingnutcase", DateTime.Now);
			committer = author;
			commit = LocalRepo.Commit("A test commit for the cloning.", author, committer);

			GitService.PushToRepo(LocalRepo);

			Assert.Equal(LocalRepo.Head.Tip.Sha, BareRepo.Head.Tip.Sha);
		}
		finally
		{
			if (Directory.Exists(TestingPath))
				Directory.Delete(TestingPath, true);
		}
	}

	// Tests for pulling
	[Fact(Skip = "Unfinished")]
	public static void PullFromRepo_Works()
	{

	}
}
