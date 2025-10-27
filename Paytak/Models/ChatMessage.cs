using System.ComponentModel.DataAnnotations;
using Paytak.Models;

namespace Paytak.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        public string Message { get; set; } = string.Empty;
        
        [Required]
        public string Response { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public string? Model { get; set; }
        
        public int? TokensUsed { get; set; }
        
        // Navigation property
        public User User { get; set; } = null!;
    }
} 