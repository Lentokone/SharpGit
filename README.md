# SharpGit
### What it is

SharpGit is a CLI tool meant to do Git traffic with the project SharpHub.
The project uses LibGit2Sharp to do Git stuff, and it has its own authentication for SharpHub.

### What you can do
 - Clone a repository from the server that is running SharpHub
 - Add untracked files and untracked changes. (git add...)
 - Make a commit message. (git commit...)
 - Push commits to upstream. (git push)
 - Pull commits from upstream. (git pull)
 - Display git repository status. (git status)
 - Display git repository commit log. (git log)
 - Remove file from being tracked. (git remove)
 - Restore a made change. (git restore)
 - Set remote of the upstream. (git set remote ...)
 - Login to the server through CLI. (This is the authentication for SharpHub.)
 It makes an SSH key for the user and sends it to the server.

### Instructions, and what SharpGit does locally
 * I assume this doesn't have any special instructions.
 * Just a basic, build the project with dotnet and point the PATH to the program.
 - Moving to what it does locally, meaning if it changes any local files, adds any files, and so on.
 - Files it creates
    - ".sharpgit" directory under the user's home directory
    - "config.json" under ".sharpgit" directory
    - "ssh" directory under ".sharpgit" directory
    - "SharpHub_key" SSH ed25519 key pair with ssh-keygen under the "ssh" directory

###### This became obsolete after making the project 'sharphub-shell'
 * sharphub-shell is meant to be a 'git-over-ssh' authentication layer.
 If the user does a git action with an SSH key (be it clone, push-pull...)
 the program checks the key, checks the permissions of the key's owner, if they are allowed to handle that repository
 then lets it through.
 * That made SharpGit somewhat obsolete, since now you can just use Git instead of this.
 * SharpGit doesn't really provide anything different or required to do, compared to Git.
 * While SharpGit does have its own login, its main priority was to generate an SSH key and give it to the server.
 * But with the addition of being able to give an SSH key in SharpHub, it opened the door to being able to only use Git
 * Before, you could also use Git by cloning a repository with SharpGit and then switching to Git, since SharpGit makes a functional Git repository,
 but now there is no need to use it at all.

#### I had fun making this.
