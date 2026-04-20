using ByteSheild.Models;
using SQLite;
using System.Security.Cryptography;

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
#if !DEBUG
            // Anti-dumping / Anti-debugging check to deter reverse-engineering
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // Note: In a true red-team scenario, you might want a more stealthy abort or crash,
                // but a SecurityException cleanly halts the execution path here.
                throw new System.Security.SecurityException("Debugger detected. Secure vault access is locked to prevent memory dumping and reverse engineering.");
            }
#endif

            if (_db != null)
                return;

            // Generate a secure, randomized encryption key on first run, and store it safely in the OS KeyStore/Keychain.
            string dbKey = string.Empty;
            try
            {
                dbKey = await SecureStorage.Default.GetAsync("database_encryption_key") ?? string.Empty;
                if (string.IsNullOrEmpty(dbKey))
                {
                    byte[] secretKey = new byte[32]; // 256-bit key for AES-256
                    using (var rng = RandomNumberGenerator.Create())
                    {
                        rng.GetBytes(secretKey);
                    }
                    dbKey = Convert.ToBase64String(secretKey);
                    await SecureStorage.Default.SetAsync("database_encryption_key", dbKey);
                }
            }
            catch (Exception ex)
            {
                // FAIL SECURELY: Never fall back to a hardcoded key in a production security app.
                // If SecureStorage is unavailable, we cannot safely persist or retrieve the vault.
                throw new CryptographicException("Critical Security Error: Unable to securely store or retrieve the database encryption key from the device's secure enclave.", ex);
            }

            // Utilize SQLCipher to encrypt the SQLite file at rest.
            var options = new SQLiteConnectionString(_databasePath, Flags, true, key: dbKey);

            try
            {
                _db = new SQLiteAsyncConnection(options);
                await _db.CreateTableAsync<VaultItemModel>();
            }
            catch (Exception ex) when (ex.Message.Contains("not a database", StringComparison.OrdinalIgnoreCase) || ex.Message.Contains("file is not a database", StringComparison.OrdinalIgnoreCase))
            {
                // Handle unencrypted fallback or corrupted DB key by resetting the vault.
                if (_db != null)
                {
                    await _db.CloseAsync();
                }

                if (File.Exists(_databasePath))
                {
                    File.Delete(_databasePath);
                }

                _db = new SQLiteAsyncConnection(options);
                await _db.CreateTableAsync<VaultItemModel>();
            }
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

        public async Task DeleteAllDataAsync()
        {
            await Init();
            await _db!.DropTableAsync<VaultItemModel>();
            await _db!.CloseAsync();
            _db = null;
            
            try 
            {
                if (File.Exists(_databasePath))
                {
                    File.Delete(_databasePath);
                }
            }
            catch 
            {
                // Best effort delete
            }
        }
    }
}
