using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using platterr_api.Dtos;
using platterr_api.Entities;
using platterr_api.Interfaces;

namespace platterr_api.Data
{
    public class OrdersRepository : IOrdersRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        public OrdersRepository(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public void AddOrder(Order order)
        {
            _context.Add(order);
        }

        public async Task<OrderDto> DeleteOrder(int Id)
        {
            var removedOrder = await GetDbOrderById(Id);
            var returnedOrder = await GetOrderById(Id);
            _context.Orders.Remove(removedOrder);

            return returnedOrder;
        }

        public async Task<Order> GetDbOrderById(int id)
        {
            return await _context.Orders.Where(x => x.Id == id)
                                        .Include("Platters.Platter")
                                        .Include("Platters.Format")
                                        .FirstOrDefaultAsync();
        }

        public async Task<OrderDto> GetOrderById(int id)
        {
            return await _context.Orders.Where(x => x.Id == id)
                                        .Include("Platters.Platter")
                                        .Include("Platters.Format")
                                        .ProjectTo<OrderDto>(_mapper.ConfigurationProvider)
                                        .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<OrderDto>> GetOrders()
        {
            return await _context.Orders.Include("Platters.Platter")
                                        .Include("Platters.Format")
                                        .ProjectTo<OrderDto>(_mapper.ConfigurationProvider)
                                        .AsNoTracking()
                                        .ToListAsync();
        }

        public async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public void UpdateOrder(Order order)
        {
            _context.Orders.Update(order);
        }
    }
}