using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;

namespace HistoryClient.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        public IConfiguration Configuration;

        public OrderRepository(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public async Task<int> GetAffectedRows(string source, string itemName, string date)
        {
            var connectionstring = Configuration.GetConnectionString("DefaultConnection");;
            var sql = @"SELECT count(1) 
                      FROM ahs_data_float 
                      WHERE source = @Source 
                      AND item = @Item 
                      AND freq = 'D' 
                      AND modified >= @Date";
            var parameters = new
            {
                Source = source,
                Item = itemName,
                Date = date
            };
            await using var connection =
                new SqlConnection(connectionstring);
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
