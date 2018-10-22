using System;
using SFCC.Common.Models;
using SFCC.Models;

namespace SFCC.ViewModels
{
    public class ItemDetailViewModel : BaseViewModel
    {
        public ToDoItem Item { get; set; }
        public ItemDetailViewModel(ToDoItem item = null)
        {
            Title = item?.Text;
            Item = item;
        }
    }
}
