namespace weather_backend.Models
{
    /// <summary>
    /// A row of the <c>academic</c> table. Only the key columns are NOT NULL in the schema.
    /// </summary>
    public class Academic
    {
        public required int ACNUM { get; set; }
        public required int DEPTNUM { get; set; }
        public string? FAMNAME { get; set; }
        public string? GIVENAME { get; set; }
        public string? INITIALS { get; set; }
        public string? TITLE { get; set; }
    }
}
