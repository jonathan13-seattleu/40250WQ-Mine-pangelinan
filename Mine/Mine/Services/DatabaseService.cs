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
        public async Task<bool> CreateAsync(ItemModel data)
        {
            var result = await Database.InsertAsync(data);
            return (result == 1);
        }

        /// <summary>
        /// Return the record for the ID passed in
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ItemModel> ReadAsync(string id)
        {
            ItemModel data;

            try
            {
                data = await Database.Table<ItemModel>().Where((ItemModel arg) => ((ItemModel)(object)arg).Id.Equals(id)).FirstOrDefaultAsync();
            }
            catch (Exception)
            {
                data = default(ItemModel);
            }

            return data;
        }

        /// <summary>
        /// Update the record passed in if it exists
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<bool> UpdateAsync(ItemModel data)
        {
            var myRead = await ReadAsync(((ItemModel)(object)data).Id);
            if (myRead == null)
            {
                return false;
            }

            var result = await Database.UpdateAsync(data);

            return (result == 1);
        }

        /// <summary>
        /// Delete the record of the ID passed in
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<bool> DeleteAsync(string id)
        {
            var data = await ReadAsync(id);
            if (data == null)
            {
                return false;
            }

            var result = await Database.DeleteAsync(data);

            return (result == 1);
        }

        /// <summary>
        /// Return all records in the database
        /// </summary>
        /// <returns></returns>
        public async Task<List<ItemModel>> IndexAsync(bool forceRefresh = false)
        {
            return await Database.Table<ItemModel>().ToListAsync();
        }
    }
}
