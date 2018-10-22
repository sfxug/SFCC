using SFCC.Common.Abstractions;
using SFCC.Common.Models;
using SFCC.Common.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFCC.Common.Managers
{
    public class TodoItemDataStore : IDataStore<ToDoItem>
    {
        List<ToDoItem> items;

        public TodoItemDataStore()
        {
        }

        public async Task<bool> AddItemAsync(ToDoItem item)
        {
            items.Add(item);

            return await Task.FromResult(true);
        }

        public async Task<bool> UpdateItemAsync(ToDoItem item)
        {
            var oldItem = items.Where((ToDoItem arg) => arg.Id == item.Id).FirstOrDefault();
            items.Remove(oldItem);
            items.Add(item);

            return await Task.FromResult(true);
        }

        public async Task<bool> DeleteItemAsync(string id)
        {
            var oldItem = items.Where((ToDoItem arg) => arg.Id == id).FirstOrDefault();
            items.Remove(oldItem);

            return await Task.FromResult(true);
        }

        public async Task<ToDoItem> GetItemAsync(string id)
        {
            return await Task.FromResult(items.FirstOrDefault(s => s.Id == id));
        }

        public async Task<IEnumerable<ToDoItem>> GetItemsAsync(bool forceRefresh = false)
        {
            try
            {
                var uri = Globals.ApiUrl;

                uri = "https://sfcc.azurewebsites.net/TodoItems.json";

                var result = await ServiceClient.Instance.GET<List<ToDoItem>>(uri, 3);

                if (result != null)
                    return result.Result;

                return null;
            }
            catch (Exception ex)
            {
                LoggingManager.LogException("Error getting TodoItems", ex);
                return null;
            }

        }
    }
}
