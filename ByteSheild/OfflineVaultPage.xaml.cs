using ByteSheild.Models;
using System.Collections.ObjectModel;

namespace ByteSheild
{
    public partial class OfflineVaultPage : ContentPage
    {
        // Auto-property initialized directly
        public ObservableCollection<VaultItemModel> VaultItems { get; } = new();
        private readonly Services.DatabaseService _database;

        public OfflineVaultPage()
        {
            InitializeComponent();
            _database = new Services.DatabaseService();
            VaultCollectionView.ItemsSource = VaultItems;
        }

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

        /// <summary>
        /// Edits the selected item in the vault collection.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private async void OnEditInvoked(object? sender, EventArgs e)
        {
            if (sender is SwipeItem { CommandParameter: VaultItemModel selected })
            {
                await Shell.Current.GoToAsync($"{nameof(AddVaultItemPage)}?itemId={selected.Id}");
            }
        }

        /// <summary>
        /// Deletes the selected item from the vault collection.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private async void OnDeleteInvoked(object? sender, EventArgs e)
        {
            if (sender is SwipeItem { CommandParameter: VaultItemModel selected })
            {
                await _database.DeleteVaultItemAsync(selected);
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    VaultItems.Remove(selected);
                });
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            try
            {
                await LoadVaultItems();
            }
            catch (Exception)
            {
                // Suppressed the error popup so as not to degrade the user experience just by clicking the vault tab.
            }
        }

        private async Task LoadVaultItems()
        {
            var items = await _database.GetVaultItemsAsync();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                VaultItems.Clear();
                if (items != null)
                {
                    foreach (var item in items)
                    {
                        VaultItems.Add(item);
                    }
                }
            });
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

            MainThread.BeginInvokeOnMainThread(() =>
            {
                VaultItems.Clear();

                // Avoid creating a LINQ state machine iteration closure by using straightforward loops overhead in fast ops
                if (items != null)
                {
                    foreach (var item in items)
                    {
                        if (item.Title != null && item.Title.Contains(query, StringComparison.OrdinalIgnoreCase))
                        {
                            VaultItems.Add(item);
                        }
                    }
                }
            });
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
                        if (items != null)
                        {
                            await Task.WhenAll(items.Select(item => _database.DeleteVaultItemAsync(item)));
                        }

                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            VaultItems.Clear();
                        });
                    }
                    break;
            }
        }
    }
}