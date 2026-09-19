using LinqToDB;
using LinqToDB.Data;

namespace Infa;

public class GroceryDatabase(DataOptions<GroceryDatabase> dataopts) : DataConnection(dataopts.Options)
{
    public ITable<GroceryItem> Groceries()
    {
        return this.GetTable<GroceryItem>();
    }
}