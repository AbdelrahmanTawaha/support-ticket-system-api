using BusinessLayer.Models;

namespace BusinessLayer.Services.AiAssignSuggest
{
    public interface IAiAssignSuggestService
    {
        Task<AiAssignSuggestionResult> SuggestAsync(int ticketId, CancellationToken ct = default);
    }
}
