public record SharpGitConfig
{
    public string Username { get; set; } = "unset";
    public string Email { get; set; } = "unset";
    public string ServerAddress { get; set; } = "unset";
    public string SSHKeyPath { get; set; } = "unset";
}
