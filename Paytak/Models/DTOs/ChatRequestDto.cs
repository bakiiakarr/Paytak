using System.ComponentModel.DataAnnotations;

namespace Paytak.Models.DTOs
{
    public class ChatRequestDto
    {
        [Required]
        public string Message { get; set; } = string.Empty;
        
        public string? Model { get; set; } = "gpt-4o";
        
        public int? MaxTokens { get; set; } = 1000;
        
        public double? Temperature { get; set; } = 0.7;
    }
} 