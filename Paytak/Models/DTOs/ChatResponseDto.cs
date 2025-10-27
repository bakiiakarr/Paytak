namespace Paytak.Models.DTOs
{
    public class ChatResponseDto
    {
        public string Response { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int TokensUsed { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }
} 