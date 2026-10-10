namespace Weather.CLI.Services.LanguageExamples
{
    /// <summary>
    /// Structs are copied when passed by value, so a method that mutates its parameter leaves the caller's
    /// copy untouched. Moved from the API's <c>GET /api/struc</c>.
    /// </summary>
    public static class StructCopyExample
    {
        public struct Person
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }

        /// <summary>Renames the copy it was given; the caller's struct keeps its name.</summary>
        public static Person RenameCopy(Person person, string newName)
        {
            person.Name = newName;
            return person;
        }
    }
}
