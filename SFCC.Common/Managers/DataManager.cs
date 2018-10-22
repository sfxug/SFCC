using SFCC.Common.Models;
using SFCC.Common.Network;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SFCC.Common.Managers
{
    public class DataManager
    {
        public async Task<List<ToDoItem>> GetTodoItems()
        {
            try
            {
                var uri = Globals.ApiUrl;

                uri = "https://sfcc.scm.azurewebsites.net/api/vfs/site/wwwroot/TodoItems.json";

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
