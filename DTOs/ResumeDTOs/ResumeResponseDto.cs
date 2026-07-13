namespace LoginRegistration.DTOs.ResumeDTOs
{
    public class ResumeResponseDto
    {
        public string FullText { get; set; } = string.Empty;
        public List<string> Skills { get; set; } = new();
        public List<string> Keywords { get; set; } = new();
    }
}
