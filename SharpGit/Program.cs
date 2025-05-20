using LibGit2Sharp;
using System;
using System.Threading.Tasks;
using System.CommandLine;
using System.CommandLine.Invocation;
using SharpGit.Classes;

////! Eli se libgit2sharp
////! Ja ssh
////! Ja auth tarkistus mvc puolelle

//// Kun on pushaamassa, lähettää ennen sitä jonkun viestin mvc puolelle, joka tekee sen git init --bare puolen

//// Vaikka git remote add kohdassa voisi tehdä sen auth jutun ja tag se local repo jotenkin että se tietää että on auth kontsa

//TODO Muista ottaa nuo writelinet pois


// Nonni
// Vähän ajatuksia tänne, etten unohda.
// Eli tässä tulee myös login
// Kun clone, push,pull jne. niin se kysyy käyttäjätunnuksen ja salasanan.
// Lähetetään se MVC endpoint, joka palauttaa ssh key ja short lived token
// Ja se tallennetaan johonkin, että ei tarvi joka kerta kysyä.

// Kun on action time, se tarkistaa tokenin ja sitten tekee magiaa

// Eli tulen tarviimaan jonkinsorting .sharpgit, johon tallennetaan ssh keyt ja config.json
// Joka on muotoa. ---

/*
{
  "monkey": {
    "username": "veijari",
    "token": "token_for_monkey",
    "token_expires": "2025-05-22T01:00:00Z",
    "ssh_key_path": "/home/user/.sharpgit/keys/monkey"
  },
  "giraffe": {
    "username": "veijari",
    "token": "token_for_giraffe",
    "token_expires": "2025-05-24T01:00:00Z",
    "ssh_key_path": "/home/user/.sharpgit/keys/giraffe"
  }
}
*/
namespace SharpGit;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("SharpGit CLI - a minimal git-like tool");

        var updateOption = new Option<bool>(
            name: "--update",
            description: "Update the local repository"
        );
        updateOption.AddAlias("-u"); // Short form

        // init
        var initCommand = new Command("init", "Initialize a new repository");
        initCommand.SetHandler(() => {
            Console.WriteLine("Init command called");
            GitService.InitRepo();
        });

        // add
        var addCommand = new Command("add", "Add files to staging area");
        var addPathArg = new Argument<IEnumerable<string>>("path", "Path to file or directory");

        addCommand.AddOption(updateOption);
        addCommand.AddArgument(addPathArg);

        addCommand.SetHandler((bool update, IEnumerable<string> paths) =>
        {
            // Handle the update option
            if (update)
            {
                Console.WriteLine("Using '--update' or '-u' to stage modified and deleted files.");
            }

            // Handle paths (files or directories)
            foreach (var path in paths)
            {
                Console.WriteLine($"Adding file or directory: {path}");
            }
        }, updateOption, addPathArg);

        // commit
        var commitCommand = new Command("commit", "Record changes to the repository");
        var messageOption = new Option<string>(
            name: "--message",
            description: "Commit message"
        );
        commitCommand.AddOption(messageOption);
        commitCommand.SetHandler((string message) => {
            Console.WriteLine($"Commit command called with message: {message}");
        }, messageOption);

        // clone
        var cloneCommand = new Command("clone", "Clone a repository");
        var repoUrlArg = new Argument<string>("url", "Repository URL");
        cloneCommand.AddArgument(repoUrlArg);
        cloneCommand.SetHandler((string url) => {
            Console.WriteLine($"Clone command called for: {url}");
        }, repoUrlArg);

        // push
        var pushCommand = new Command("push", "Push changes to remote");
        pushCommand.SetHandler(() => {
            Console.WriteLine("Push command called");
            GitService.PushToRepo();
        });

        // pull
        var pullCommand = new Command("pull", "Pull changes from remote");
        pullCommand.SetHandler(() => {
            Console.WriteLine("Pull command called");
        });

        // status
        var statusCommand = new Command("status", "Show the working tree status");
        statusCommand.SetHandler(() => {
            Console.WriteLine("Status command called");
            string test = Directory.GetCurrentDirectory();
            var repo = GitUtils.TryFindRepositoryFromCurrentDirectory();
            var status = repo.RetrieveStatus();
            if (repo != null)
            {
                GitService.DisplayGitStatus(repo); // Display the status of the repository
            }

        });
        // status
        var logCommand = new Command("log", "Show the commit tree");
        logCommand.SetHandler(() => {
            Console.WriteLine("Log command called");
        });
        
        // status
        var SetRemoteCommand = new Command("remote", "Set the remote destination of the repository");
        SetRemoteCommand.SetHandler(() => {
            Console.WriteLine("Set Remote command called");
        });

        // Add all to root
        rootCommand.AddCommand(initCommand);
        rootCommand.AddCommand(addCommand);
        rootCommand.AddCommand(commitCommand);
        rootCommand.AddCommand(cloneCommand);
        rootCommand.AddCommand(pushCommand);
        rootCommand.AddCommand(pullCommand);
        rootCommand.AddCommand(statusCommand);
        rootCommand.AddCommand(logCommand);
        rootCommand.AddCommand(SetRemoteCommand);

        return await rootCommand.InvokeAsync(args);
    }
}
