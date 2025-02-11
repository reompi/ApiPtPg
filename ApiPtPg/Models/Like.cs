using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiPtPg.Models
{
    public class Like
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int NoteId { get; set; }

        public DateTime LikedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("NoteId")]
        public Note Note { get; set; }  // Relacionamento com a entidade Note

        [ForeignKey("UserId")]
        public User User { get; set; }  // Relacionamento com a entidade User
    }
    public class Dislike
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int NoteId { get; set; }

        public DateTime LikedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("NoteId")]
        public Note Note { get; set; }  // Relacionamento com a entidade Note

        [ForeignKey("UserId")]
        public User User { get; set; }  // Relacionamento com a entidade User
    }
}
