using CalebBender.Atoms.DataAttributes;

namespace AtomsIntegrationTests.Models
{
    [DbEntityName("BlogPostAuthors")]
    public class BlogPostAuthorWithDateTimeOffset
    {
        [UniqueId]
        public long AuthorId { get; set; }
        public string AuthorName { get; set; } = "John Doe";
        [DbPropertyName("UserRegisteredOn")]
        public DateTimeOffset AuthorSinceDate { get; set; }
    }
}
