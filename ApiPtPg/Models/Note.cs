using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace ApiPtPg.Models
{
public class Note
{
    public int Id { get; set; }
    public string Heading { get; set; }
    public JsonElement Body { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int UserId { get; set; }
    public int? FolderId { get; set; }
        [Required]
        public bool Private { get; set; } = true;  // New property
                                                   // Relationships: List of Likes and Dislikes
        public List<Like> Likes { get; set; } = new List<Like>();
    public List<Dislike> Dislikes { get; set; } = new List<Dislike>();

    // Remove these if they exist in the model:
    // public int LikesCount { get; set; }
    // public int DislikesCount { get; set; }
}


    public class NoteDTO
    {
        public int Id { get; set; }
        public string Heading { get; set; }
        public JsonElement Body { get; set; }
        public int LikesCount { get; set; }
        public int DislikesCount { get; set; } // Add dislikes count

        // Track whether the user has liked or disliked the note
        public bool IsLiked { get; set; }
        public bool IsDisliked { get; set; } // Add this property
    }
    public class UpdateNoteDto
    {
        public int? FolderId { get; set; }
        // Make Heading optional or remove it
        // public string Heading { get; set; } 
    }
}
