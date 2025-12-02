namespace prgmlab3.Models
{
    // Hata sayfasında gösterilecek hata isteği bilgilerini taşıyan model.
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}