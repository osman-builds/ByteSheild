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
                if (!string.IsNullOrEmpty(value) && int.TryParse(Uri.UnescapeDataString(value), out int id))
                {
                    LoadItem(id);
                }
            }
        }

        public AddVaultItemPage()
        {
            InitializeComponent();
            _database = new Services.DatabaseService();
        }

        public AddVaultItemPage(Services.DatabaseService database)
        {
            InitializeComponent();
            _database = database;
        }

        private async void LoadItem(int id)
        {
            try
            {
                var items = await _database.GetVaultItemsAsync();

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (items != null)
                    {
                        _editItem = items.FirstOrDefault(i => i.Id == id);
                        if (_editItem != null)
                        {
                            Title = "Edit Account";
                            HeaderLabel.Text = "Edit Account";
                            TitleEntry.Text = _editItem.Title;
                            EmailEntry.Text = _editItem.EmailOrUsername;
                            PasswordEntry.Text = _editItem.Password;
                            IconEntry.Text = _editItem.Icon;
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlertAsync("Error", $"Could not load item: {ex.Message}", "OK");
                });
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

            try
            {
                await _database.SaveVaultItemAsync(_editItem);
                await DisplayAlertAsync("Success", "Account saved securely.", "OK");
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error", $"Could not save account: {ex.Message}", "OK");
            }
        }

        private void OnTogglePasswordVisibility(object? sender, EventArgs e)
        {
            PasswordEntry.IsPassword = !PasswordEntry.IsPassword;
            VisibilityToggle.Source = PasswordEntry.IsPassword ? "settings_icon.svg" : "vault_icon.svg";
        }
    }
}