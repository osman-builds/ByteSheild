using ByteSheild.Models;

namespace ByteSheild
{
    [QueryProperty(nameof(ItemId), "itemId")]
    public partial class AddVaultItemPage : ContentPage
    {
        private VaultItemModel? _editItem;
        private readonly Services.DatabaseService _database;

        public string ItemId
        {
            set
            {
                if (int.TryParse(Uri.UnescapeDataString(value), out int id))
                {
                    LoadItem(id);
                }
            }
        }

        public AddVaultItemPage(Services.DatabaseService database)
        {
            InitializeComponent();
            _database = database;
        }

        private async void LoadItem(int id)
        {
            var items = await _database.GetVaultItemsAsync();
            _editItem = items.FirstOrDefault(i => i.Id == id);

            if (_editItem != null)
            {
                Title = "Edit Account";
                TitleEntry.Text = _editItem.Title;
                EmailEntry.Text = _editItem.EmailOrUsername;
                PasswordEntry.Text = _editItem.Password;
                IconEntry.Text = _editItem.Icon;
            }
        }

        private async void OnSaveClicked(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TitleEntry.Text) ||
                string.IsNullOrWhiteSpace(EmailEntry.Text) ||
                string.IsNullOrWhiteSpace(PasswordEntry.Text))
            {
                await DisplayAlertAsync("Error", "Please fill in all required fields.", "OK");
                return;
            }

            if (_editItem == null)
            {
                _editItem = new VaultItemModel();
            }

            _editItem.Title = TitleEntry.Text;
            _editItem.EmailOrUsername = EmailEntry.Text;
            _editItem.Password = PasswordEntry.Text;
            _editItem.Icon = string.IsNullOrWhiteSpace(IconEntry.Text) ? "🔑" : IconEntry.Text;
            _editItem.LastUpdated = DateTime.Now;

            await _database.SaveVaultItemAsync(_editItem);
            await DisplayAlertAsync("Success", "Account saved securely.", "OK");

            await Shell.Current.GoToAsync("..");
        }
    }
}