public record SharpGitConfig
{
    public string Username { get; set; } = "";
    public string Email { get; set; } = "";
    public string ServerAddress { get; set; } = "192.168.1.114";
    public string SSHKeyPath { get; set; } = "unset";
}
