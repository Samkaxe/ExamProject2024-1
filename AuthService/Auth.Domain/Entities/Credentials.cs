﻿namespace Auth.Domain.Entities;

public class Credentials
{
    public Guid UserId { get; set; }
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public string Salt { get; set; }
}