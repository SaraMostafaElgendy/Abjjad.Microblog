namespace Abjjad.Microblog.Models
{
    public class Post
    {
        public int Id { get; set; }
        public int AuthorId { get; set; }
        public User? Author { get; set; }
        public string Text { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? OriginalImagePath { get; set; }
        public bool ImageProcessed { get; set; } = false;
        public string? ProcessedImages { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
