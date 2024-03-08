﻿namespace BurgerRoyale.Domain.Configuration
{
    public class JwtConfiguration
    {
        public string? Issuer { get; set; } = string.Empty;
        public string? Audience { get; set; } = string.Empty;
        public string? SecretKey { get; set; } = string.Empty;
    }
}
