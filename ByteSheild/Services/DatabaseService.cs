using ByteSheild.Models;
using SQLite;

namespace ByteSheild.Services
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection? _db;
        private readonly string _databasePath;

        public DatabaseService(string? customPath = null)
        {
            try
            {
                _databasePath = customPath ?? Path.Combine(FileSystem.AppDataDirectory, "ByteSheildVault.db3");
            }
            catch
            {
                // Fallback for tests if FileSystem is unavailable
                _databasePath = customPath ?? Path.Combine(Path.GetTempPath(), "ByteSheildVault.db3");
            }
        }

        private const SQLiteOpenFlags Flags =
            SQLiteOpenFlags.ReadWrite |
            SQLiteOpenFlags.Create |
            SQLiteOpenFlags.SharedCache;

        async Task Init()
        {
            if (_db != null)
                return;

            _db = new SQLiteAsyncConnection(_databasePath, Flags);
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
