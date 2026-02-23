namespace SwitchyLingus.Core.Model
{
    public class LanguageProfile
    {
        public required string Name { get; init; }
        public required IEnumerable<Language> Languages { get; init; }

        public bool IsMainProfile { get; init; } = false;

        public override string ToString()
        {
            return string.Join(",", Languages.Select(l => l.Tag));
        }
    }
}