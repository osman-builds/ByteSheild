using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ByteSheild.Models;

namespace ByteSheild.Services
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection? _db;

        async Task Init()
        {
            if (_db != null)
                return;

            var databasePath = Path.Combine(FileSystem.AppDataDirectory, "ByteSheildVault.db3");
            _db = new SQLiteAsyncConnection(databasePath);
            await _db.CreateTableAsync<VaultItemModel>();
        }

        public async Task<List<VaultItemModel>> GetVaultItemsAsync()
        {
            await Init();
            return await _db!.Table<VaultItemModel>().ToListAsync();
        }

        public async Task<int> SaveVaultItemAsync(VaultItemModel item)
        {
            await Init();
            if (item.Id != 0)
                return await _db!.UpdateAsync(item);
            else
                return await _db!.InsertAsync(item);
        }

        public async Task<int> DeleteVaultItemAsync(VaultItemModel item)
        {
            await Init();
            return await _db!.DeleteAsync(item);
        }
    }
}
