using Paytak.Models.DTOs;

namespace Paytak.Services
{
    public interface IOpenAIService
    {
        Task<ChatResponseDto> GetChatResponseAsync(ChatRequestDto request);
    }
} 