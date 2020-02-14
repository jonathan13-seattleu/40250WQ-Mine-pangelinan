using Mine.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mine.Services
{
    public class DatabaseService : IDataStore<ItemModel>
    {
        static readonly Lazy<SQLiteAsyncConnection> lazyInitializer = new Lazy<SQLiteAsyncConnection>(() =>
        {
            return new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
        });

        static SQLiteAsyncConnection Database => lazyInitializer.Value;
        static bool initialized = false;

        public DatabaseService()
        {
            InitializeAsync().SafeFireAndForget(false);
        }

        async Task InitializeAsync()
        {
            if (!initialized)
            {
                if (!Database.TableMappings.Any(m => m.MappedType.Name == typeof(ItemModel).Name))
                {
                    await Database.CreateTablesAsync(CreateFlags.None, typeof(ItemModel)).ConfigureAwait(false);
                    initialized = true;
                }
            }
        }

        /// <summary>
        /// Create a new record for the data passed in
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public Task<bool> CreateAsync(ItemModel data)
        {
            Database.InsertAsync(data);
            return Task.FromResult(true);
        }

        /// <summary>
        /// Return the record for the ID passed in
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<ItemModel> ReadAsync(string id)
        {
            return Database.Table<ItemModel>().Where(i => i.Id.Equals(id)).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Update the record passed in if it exists
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public Task<bool> UpdateAsync(ItemModel Data)
        {
            var myRead = ReadAsync(Data.Id).GetAwaiter().GetResult();
            if (myRead == null)
            {
                return Task.FromResult(false);

            }

            Database.UpdateAsync(Data);

            return Task.FromResult(true);
        }

        /// <summary>
        /// Delete the record of the ID passed in
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<bool> DeleteAsync(string id)
        {
            // Check if it exists...
            var myRead = ReadAsync(id).GetAwaiter().GetResult();
            if (myRead == null)
            {
                return Task.FromResult(false);

            }

            // Then delete...

            Database.DeleteAsync(myRead);
            return Task.FromResult(true);
        }

        /// <summary>
        /// Return all records in the database
        /// </summary>
        /// <returns></returns>
        public Task<List<ItemModel>> IndexAsync(bool flag = false)
        {
            return Database.Table<ItemModel>().ToListAsync();
        }

        // Delete the Datbase Tables by dropping them
        public async void DeleteTables()
        {
            await Database.DropTableAsync<ItemModel>();
        }

        // Create the Datbase Tables
        public async void CreateTables()
        {
            await Database.CreateTableAsync<ItemModel>();
        }

        public void WipeDataList()
        {
            Database.DropTableAsync<ItemModel>().GetAwaiter().GetResult();
            Database.CreateTablesAsync(CreateFlags.None, typeof(ItemModel)).ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}
