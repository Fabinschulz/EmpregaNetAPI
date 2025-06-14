﻿namespace EmpregaNet.Infra.Configurations;

public class JwtSettings
{
    public required string SecretKey { get; set; }
    public int ExpirationHours { get; set; }
    public required string Issuer { get; set; }
    public required string Audience { get; set; }
}