namespace Auth.Domain.Entities;

public class Credentials
{
    public Guid UserId { get; set; }
    public string Email { get; set; }
    public byte[] PasswordHash { get; set; }
    public byte[] Salt { get; set; }
}
