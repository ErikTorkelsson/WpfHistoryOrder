using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace HistoryClient.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        public async Task<int> GetAffectedRows(string source, string itemName)
        {
            var sql = "SELECT COUNT(1) FROM ahs_data_float WHERE source = @Source AND item = @Item";
            var parameters = new
            {
                Source = source,
                Item = itemName
            };
            await using var connection =
                new SqlConnection("Persist Security Info = False; Integrated Security = SSPI; server = hadley; database = seb_ahshist_20210201_anna");
            IEnumerable<int> result = new List<int>();
            try
            {
                await connection.OpenAsync();
                result = await connection
                    .QueryAsync<int>(sql,
                        parameters, null, 30);
            }
            catch (Exception exception)
            {
                throw;
            }
            return result.FirstOrDefault();
        }
    }
}
