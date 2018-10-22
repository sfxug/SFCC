using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SFCC.Common.Abstractions;
using SFCC.Common.Models;
using SFCC.Models;

namespace SFCC.Services
{
    public class MockDataStore : IDataStore<ToDoItem>
    {
        List<ToDoItem> items;

        public MockDataStore()
        {
            items = new List<ToDoItem>();
            var mockItems = new List<ToDoItem>
            {
                new ToDoItem { Id = Guid.NewGuid().ToString(), Text = "First item", Description="This is an item description." },
                new ToDoItem { Id = Guid.NewGuid().ToString(), Text = "Second item", Description="This is an item description." },
                new ToDoItem { Id = Guid.NewGuid().ToString(), Text = "Third item", Description="This is an item description." },
                new ToDoItem { Id = Guid.NewGuid().ToString(), Text = "Fourth item", Description="This is an item description." },
                new ToDoItem { Id = Guid.NewGuid().ToString(), Text = "Fifth item", Description="This is an item description." },
                new ToDoItem { Id = Guid.NewGuid().ToString(), Text = "Sixth item", Description="This is an item description." },
            };

            foreach (var item in mockItems)
            {
                items.Add(item);
            }
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
            return await Task.FromResult(items);
        }
    }
}