using System.Collections.Generic;
using System.Threading.Tasks;
using iText.Layout;
using platterr_api.Dtos;
using platterr_api.Entities;

namespace platterr_api.Interfaces
{
    public interface IOrdersRepository
    {
        Task<IEnumerable<OrderDto>> GetOrders();

        Task<Order> GetDbOrderById(int id);

        Task<OrderDto> GetOrderById(int id);

        void AddOrder(Order order);

        void UpdateOrder(Order order);

        Task<OrderDto> DeleteOrder(int Id);

        Task<OrderDto> GeneratePdf(int id);

        Task<bool> SaveAllAsync();
    }
}