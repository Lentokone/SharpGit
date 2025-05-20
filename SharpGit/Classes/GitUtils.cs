using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpGit.Classes
{
    internal class GitUtils
    {
        // Tänne viellä ne gitignore template ja muut.
        // Funktio joka sitten tekee sen annetulla numerolla (esim. 1 = vstudio, 2 = node, 3 = python jne)

        /// <summary>
        /// 
        /// 
        /// </summary>
        public static Repository? TryFindRepositoryFromCurrentDirectory()
        {
            var dir = new DirectoryInfo(Directory.GetCurrentDirectory());

            while (dir != null)
            {
                var gitPath = Path.Combine(dir.FullName, ".git");
                if (Directory.Exists(gitPath) || File.Exists(gitPath)) // file in case it's a submodule or worktree
                {
                    return new Repository(dir.FullName);
                }

                dir = dir.Parent;
            }

            return null;
        }
    }
}
