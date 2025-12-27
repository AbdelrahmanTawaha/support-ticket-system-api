namespace SupportTicketsAPI.Services.Files
{
    public class TicketAttachmentStorage : ITicketAttachmentStorage
    {
        private readonly IWebHostEnvironment _env;
        private const string RootFolder = "uploads";
        private const string TicketsFolder = "tickets";
        private readonly ILogger<TicketAttachmentStorage> _logger;

        public TicketAttachmentStorage(IWebHostEnvironment env, ILogger<TicketAttachmentStorage> logger)
        {
            _env = env;
            _logger = logger;
        }

        public async Task<(string relativePath, long size, string originalFileName)> SaveAsync(
            int ticketId,
            IFormFile file,
            CancellationToken ct = default)
        {
            var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

            var ticketDir = Path.Combine(webRoot, RootFolder, TicketsFolder, ticketId.ToString());


            Directory.CreateDirectory(ticketDir);

            var safeOriginal = Path.GetFileName(file.FileName);
            var ext = Path.GetExtension(safeOriginal);
            var stored = $"{Guid.NewGuid():N}{ext}";

            var fullPath = Path.Combine(ticketDir, stored);

            await using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream, ct);

            var relative = $"/{RootFolder}/{TicketsFolder}/{ticketId}/{stored}";
            return (relative, file.Length, safeOriginal);
        }

        public Task<bool> DeletePhysicalAsync(string relativePath, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                return Task.FromResult(false);

            var trimmed = relativePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString());
            var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var fullPath = Path.Combine(webRoot, trimmed);

            if (!File.Exists(fullPath))
                return Task.FromResult(false);

            File.Delete(fullPath);
            return Task.FromResult(true);


        }
    }
}
