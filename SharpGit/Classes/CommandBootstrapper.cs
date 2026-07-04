namespace SharpGit.Classes
{
    public static class CommandBootstrapper
    {
        public static async Task EnsureReady()
        {
            if (!GitUtils.ConfigExists())
            {
                GitUtils.CreateDefaultConfig();
            }

            if (!IsLoggedIn())
            {
                await GitService.Login();
            }
        }

        private static bool IsLoggedIn()
        {
            var config = GitUtils.GetConfig();
            return !string.IsNullOrWhiteSpace(config.Username) &&
                !string.IsNullOrWhiteSpace(config.Email);
        }
    }
}
