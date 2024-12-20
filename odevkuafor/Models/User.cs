using System;
namespace odevkuafor.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Password { get; set; } = string.Empty; 
        public string Email { get; set; } = string.Empty; 
        public bool IsAdmin { get; set; } // Admin olup olmadığını belirten alan

    }
}

