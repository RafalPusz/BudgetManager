using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using System.Transactions;

namespace BudgetManager.Services
{
    internal class StorageService
    {
        // Path to the folder where budget data will be stored
        private readonly string folderPath = Path.Combine(AppContext.BaseDirectory, "Data");

        // Path to the JSON file containing budget data
        private readonly string filePath;

        #region Public Methods
        // Constructor ensures the data folder exists and sets the file path
        public StorageService()
        {
            Directory.CreateDirectory(folderPath);
            filePath = Path.Combine(folderPath, "budget.json");
        }

        // Saves the budget data to a JSON file asynchronously
        public async Task SaveAsync(BudgetService budget)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true, // Makes JSON readable with indentation
            };

            string json = JsonSerializer.Serialize(budget, options);
            await File.WriteAllTextAsync(filePath, json);
        }

        // Loads the budget data from the JSON file asynchronously
        // If the file doesn't exist, returns a new empty BudgetService
        public async Task<BudgetService> LoadAsync()
        {
            if (!File.Exists(filePath))
            {
                return new BudgetService();
            }

            string json = await File.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<BudgetService>(json) ?? new BudgetService();
        }
        #endregion
    }
}
