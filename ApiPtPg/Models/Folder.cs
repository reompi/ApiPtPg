using ApiPtPg.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiPtPg.Models
{
    public class Folder
    {
        [Key]
        public int FolderId { get; set; }

        [Required]
        public string Name { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int? ParentFolderId { get; set; }

        // Foreign key for the User who created the folder
        public int UserId { get; set; }


    }
    public class UpdateFolderDto
    {
        public int? ParentFolderId { get; set; }
    }


}
