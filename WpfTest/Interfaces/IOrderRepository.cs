using System.Threading.Tasks;

namespace HistoryClient.Repositories
{
    public interface IOrderRepository
    {
        Task<int> GetAffectedRows(string source, string itemName, string date);
    }
}