public record ServerConfig
{
    public SSHkey sshkey = new();
    public JWTToken token = new();
}

public record SSHkey
{
    // Emt
}

public record JWTToken
{
    // Token
    // Duration
}
