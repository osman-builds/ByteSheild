using ByteSheild.Models;
using System.Collections.ObjectModel;

namespace ByteSheild
{
    public partial class OfflineVaultPage : ContentPage
    {
        // Auto-property initialized directly
        public ObservableCollection<VaultItemModel> VaultItems { get; } = new();
        private readonly Services.DatabaseService _database;

        public OfflineVaultPage(Services.DatabaseService database)
        {
            InitializeComponent();
            _database = database;
            VaultCollectionView.ItemsSource = VaultItems;
        }

        private async void OnShowPasswordInvoked(object? sender, EventArgs e)
        {
            // Modern C# property pattern matching
            if (sender is SwipeItem { CommandParameter: VaultItemModel selected })
            {
                await DisplayAlertAsync("Credentials", $"Username: {selected.EmailOrUsername}\nPassword: {selected.Password}", "OK");
            }
        }

        private async void OnEditInvoked(object? sender, EventArgs e)
        {
            if (sender is SwipeItem { CommandParameter: VaultItemModel selected })
            {
                await Shell.Current.GoToAsync($"{nameof(AddVaultItemPage)}?itemId={selected.Id}");
            }
        }

        private async void OnDeleteInvoked(object? sender, EventArgs e)
        {
            if (sender is SwipeItem { CommandParameter: VaultItemModel selected })
            {
                await _database.DeleteVaultItemAsync(selected);
                VaultItems.Remove(selected);
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadVaultItems();
        }

        private async Task LoadVaultItems()
        {
            var items = await _database.GetVaultItemsAsync();
            VaultItems.Clear();

            // Seed sample data if empty
            if (items.Count == 0)
            {
                // Target-typed new() syntax for cleaner instantiation
                var sampleRecords = new List<VaultItemModel>
                {
                    new() { Title = "Personal Gmail", EmailOrUsername = "google.com", Icon = "✉️", LastUpdated = DateTime.Now },
                    new() { Title = "Bank Account", EmailOrUsername = "chase.com", Icon = "🏦", LastUpdated = DateTime.Now },
                    new() { Title = "Corporate Visa", EmailOrUsername = "Ends in •••• 4242", Icon = "💳", LastUpdated = DateTime.Now },
                    new() { Title = "Master Recovery Key", EmailOrUsername = "Stored locally on device", Icon = "🛡️", LastUpdated = DateTime.Now }
                };

                // Run insertions concurrently for better performance and responsiveness
                await Task.WhenAll(sampleRecords.Select(record => _database.SaveVaultItemAsync(record)));

                items = await _database.GetVaultItemsAsync();
            }

            foreach (var item in items)
            {
                VaultItems.Add(item);
            }
        }

        private async void OnAddClicked(object? sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(AddVaultItemPage));
        }

        private async void OnSearchClicked(object? sender, EventArgs e)
        {
            string query = await DisplayPromptAsync("Search", "Enter account name to search:");

            // Early exit pattern avoids deeply nested code
            if (string.IsNullOrWhiteSpace(query))
            {
                await LoadVaultItems();
                return;
            }

            var items = await _database.GetVaultItemsAsync();
            VaultItems.Clear();

            // Avoid creating a LINQ state machine iteration closure by using straightforward loops overhead in fast ops
            foreach (var item in items)
            {
                if (item.Title.Contains(query, StringComparison.OrdinalIgnoreCase))
                {
                    VaultItems.Add(item);
                }
            }
        }

        private async void OnOptionsClicked(object? sender, EventArgs e)
        {
            string action = await DisplayActionSheetAsync("Options", "Cancel", null, "Refresh", "Clear All");

            // Switch pattern is cleaner than if-else chains for action sheets
            switch (action)
            {
                case "Refresh":
                    await LoadVaultItems();
                    break;

                case "Clear All":
                    bool confirm = await DisplayAlertAsync("Clear All", "Are you sure you want to delete all accounts?", "Yes", "No");
                    if (confirm)
                    {
                        var items = await _database.GetVaultItemsAsync();
                        // Batch deletion concurrently instead of looping sequential awaits
                        await Task.WhenAll(items.Select(item => _database.DeleteVaultItemAsync(item)));
                        VaultItems.Clear();
                    }
                    break;
            }
        }
    }
}