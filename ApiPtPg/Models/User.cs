using System;
using System.ComponentModel.DataAnnotations;

namespace ApiPtPg.Models
{
    public class User
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Username { get; set; }

        [Required]
        [MaxLength(255)]
        public string Password { get; set; }

        [Required]
        [MaxLength(100)]
        public string Email { get; set; }

        public string Role { get; set; } = "user";



        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;



    }
}
