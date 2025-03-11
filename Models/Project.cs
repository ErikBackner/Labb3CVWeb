namespace Labb3CVWeb.Models
{
    public class Project
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Course { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string GitHubLink { get; set; } = string.Empty;
    }
}
