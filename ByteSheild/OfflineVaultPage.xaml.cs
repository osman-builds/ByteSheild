using System;
using System.Collections.ObjectModel;
using ByteSheild.Models;

namespace ByteSheild
{
    public partial class OfflineVaultPage : ContentPage
    {
        public ObservableCollection<VaultItemModel> VaultItems { get; set; }

        public OfflineVaultPage()
        {
            InitializeComponent();
            VaultItems = new ObservableCollection<VaultItemModel>();
            VaultCollectionView.ItemsSource = VaultItems;
        }

        private async void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is VaultItemModel selected)
            {
                string action = await DisplayActionSheetAsync($"{selected.Title}", "Cancel", "Delete", "Show Password", "Edit");
                if (action == "Show Password")
                {
                    await DisplayAlertAsync("Credentials", $"Username: {selected.EmailOrUsername}\nPassword: {selected.Password}", "OK");
                }
                else if (action == "Edit")
                {
                    await Shell.Current.GoToAsync($"{nameof(AddVaultItemPage)}?itemId={selected.Id}");
                }
                else if (action == "Delete")
                {
                    await App.Database.DeleteVaultItemAsync(selected);
                    VaultItems.Remove(selected);
                }

                VaultCollectionView.SelectedItem = null;
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadVaultItems();
        }

        private async Task LoadVaultItems()
        {
            var items = await App.Database.GetVaultItemsAsync();
            VaultItems.Clear();

            // Seed sample data if empty
            if (items.Count == 0)
            {
                var sampleRecords = new List<VaultItemModel>
                {
                    new VaultItemModel { Title = "Personal Gmail", EmailOrUsername = "google.com", Icon = "✉️", LastUpdated = DateTime.Now },
                    new VaultItemModel { Title = "Bank Account", EmailOrUsername = "chase.com", Icon = "🏦", LastUpdated = DateTime.Now },
                    new VaultItemModel { Title = "Corporate Visa", EmailOrUsername = "Ends in •••• 4242", Icon = "💳", LastUpdated = DateTime.Now },
                    new VaultItemModel { Title = "Master Recovery Key", EmailOrUsername = "Stored locally on device", Icon = "🛡️", LastUpdated = DateTime.Now }
                };

                foreach (var record in sampleRecords)
                {
                    await App.Database.SaveVaultItemAsync(record);
                }

                items = await App.Database.GetVaultItemsAsync();
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
    }
}