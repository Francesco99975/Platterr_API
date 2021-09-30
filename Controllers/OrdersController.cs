using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using platterr_api.Dtos;
using platterr_api.Entities;
using platterr_api.Interfaces;

namespace platterr_api.Controllers
{
    public class OrdersController : BaseApiController
    {
        private readonly IOrdersRepository _ordersRepository;
        private readonly IPlattersRepository _plattersRepository;
        public OrdersController(IOrdersRepository ordersRepository, IPlattersRepository plattersRepository)
        {
            _plattersRepository = plattersRepository;
            _ordersRepository = ordersRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrders()
        {
            try
            {
                return Ok(await _ordersRepository.GetOrders());
            }
            catch (System.Exception e)
            {
                return BadRequest("Could not get orders: " + e);
            }
        }

        [HttpPost]
        public async Task<ActionResult<OrderDto>> AddOrder(OrderDto orderDto)
        {
            try
            {
                var newOrder = new Order
                {
                    CustomerFirstName = orderDto.CustomerFirstName,
                    CustomerLastName = orderDto.CustomerLastName,
                    PhoneNumber = orderDto.PhoneNumber,
                    CreatedAt = DateTime.Now,
                    DueDate = Convert.ToDateTime(orderDto.DueDate)
                };

                _ordersRepository.AddOrder(newOrder);

                if (!await _ordersRepository.SaveAllAsync()) throw new Exception("Adding Order Problem");

                var requests = orderDto.Platters.Select(async (x) =>
                {
                    var platter = await _plattersRepository.GetDbPlatterById(x.PlatterId);
                    return new PlatterRequest
                    {
                        Order = newOrder,
                        OrderId = newOrder.Id,
                        Format = platter.Formats.SingleOrDefault(fmt => fmt.Id == x.FormatId),
                        Platter = platter,
                        PlatterId = x.PlatterId,
                        Quantity = x.Quantity
                    };
                }).ToList();

                newOrder.Platters = await Task.WhenAll(requests);

                _ordersRepository.UpdateOrder(newOrder);

                if (await _plattersRepository.SaveAllAsync()) return Created("", _ordersRepository.GetOrderById(newOrder.Id));

                return BadRequest("Database Error");
            }
            catch (System.Exception e)
            {
                return BadRequest("Could not add order: " + e);
            }
        }

        [HttpPut]
        public async Task<ActionResult<OrderDto>> UpdateOrder(OrderDto orderDto)
        {
            try
            {
                var updatedOrder = await _ordersRepository.GetDbOrderById(orderDto.Id);

                if (updatedOrder == null) return NotFound();

                updatedOrder.CustomerFirstName = orderDto.CustomerFirstName;
                updatedOrder.CustomerLastName = orderDto.CustomerLastName;
                updatedOrder.PhoneNumber = orderDto.PhoneNumber;
                updatedOrder.DueDate = Convert.ToDateTime(orderDto.DueDate);

                var res = updatedOrder.Platters.OrderBy((x) => x.Id).Select(async (x) =>
                {
                    var pltReq = orderDto.Platters.Where((plt) => plt.Id == x.Id).FirstOrDefault();
                    var platter = await _plattersRepository.GetDbPlatterById(pltReq.PlatterId);
                    x.PlatterId = pltReq.PlatterId;
                    x.Platter = platter;
                    x.Format = platter.Formats.Where(fmt => fmt.Id == pltReq.FormatId).FirstOrDefault();
                    x.Quantity = pltReq.Quantity;

                    return x;
                }).ToList();

                updatedOrder.Platters = await Task.WhenAll(res);

                _ordersRepository.UpdateOrder(updatedOrder);

                if (await _ordersRepository.SaveAllAsync()) return Ok(await _ordersRepository.GetOrderById(updatedOrder.Id));

                return BadRequest("Could not update order");
            }
            catch (System.Exception e)
            {
                return BadRequest("Could not update order: " + e);
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<OrderDto>> DeleteOrder(int id)
        {
            try
            {
                var deleted = await _ordersRepository.DeleteOrder(id);

                if (await _ordersRepository.SaveAllAsync()) return Ok(deleted);

                return BadRequest("Could not delete order");
            }
            catch (System.Exception e)
            {
                return BadRequest("Could not delete order: " + e);
            }
        }
    }
}