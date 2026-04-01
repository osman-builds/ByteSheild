using SQLite;

namespace ByteSheild.Models
{
    public class VaultItemModel
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;
        public string EmailOrUsername { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;

        public string Icon { get; set; } = string.Empty;

        public DateTime LastUpdated { get; set; }

        [Ignore]
        public string LastUpdatedDisplay => LastUpdated.ToString("g");
    }
}
