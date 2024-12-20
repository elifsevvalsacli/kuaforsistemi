using System;
namespace odevkuafor.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Password { get; set; } = string.Empty; // Varsayılan değer
        public string Email { get; set; } = string.Empty; // Varsayılan değer
    }
}

