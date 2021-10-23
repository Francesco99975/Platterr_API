using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using platterr_api.Dtos;
using platterr_api.Entities;
using platterr_api.Interfaces;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace platterr_api.Controllers
{
    public class OrdersController : BaseApiController
    {
        private readonly IOrdersRepository _ordersRepository;
        private readonly IPlattersRepository _plattersRepository;
        private readonly IScheduledNotificationsRepository _scheduledNotificationsRepository;
        private readonly IConfiguration _config;
        public OrdersController(IOrdersRepository ordersRepository, IPlattersRepository plattersRepository, IScheduledNotificationsRepository scheduledNotificationsRepository, IConfiguration config)
        {
            _config = config;
            _scheduledNotificationsRepository = scheduledNotificationsRepository;
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
                    Comment = orderDto.Comment,
                    ExtraFee = orderDto.ExtraFee,
                    Delivery = orderDto.Delivery,
                    Paid = orderDto.Paid,
                    CreatedAt = orderDto.CreatedAt,
                    DueDate = orderDto.DueDate
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

                _scheduledNotificationsRepository.scheduleNotification(newOrder.Id, newOrder.CustomerLastName, DateTime.Parse(newOrder.DueDate).Subtract(TimeSpan.FromDays(1)));

                if (await _plattersRepository.SaveAllAsync())
                {
                    return Created("", _ordersRepository.GetOrderById(newOrder.Id));
                }

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
                updatedOrder.Comment = orderDto.Comment;
                updatedOrder.ExtraFee = orderDto.ExtraFee;
                updatedOrder.Delivery = orderDto.Delivery;
                updatedOrder.Paid = orderDto.Paid;
                updatedOrder.DueDate = orderDto.DueDate;

                var res = updatedOrder.Platters
                    .Where((plt) => orderDto.Platters.Select((x) => x.Id).Contains(plt.Id))
                    .OrderBy((x) => x.Id)
                    .Select(async (x) =>
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

                var newRequests = orderDto.Platters.Where((req) => !updatedOrder.Platters.Select(x => x.Id).Contains(req.Id)).ToList();

                List<PlatterRequest> tmp = new List<PlatterRequest>();

                for (int i = 0; i < newRequests.Count; i++)
                {
                    var platter = await _plattersRepository.GetDbPlatterById(newRequests[i].PlatterId);
                    tmp.Add(new PlatterRequest
                    {
                        PlatterId = platter.Id,
                        Platter = platter,
                        Format = platter.Formats.Where(x => x.Id == newRequests[i].FormatId).FirstOrDefault(),
                        Quantity = newRequests[i].Quantity,
                        Order = updatedOrder,
                        OrderId = updatedOrder.Id
                    });
                }

                updatedOrder.Platters = updatedOrder.Platters.Concat(tmp).ToList();

                _ordersRepository.UpdateOrder(updatedOrder);

                _scheduledNotificationsRepository.updateScheduledNotification(updatedOrder.Id, updatedOrder.CustomerLastName, DateTime.Parse(updatedOrder.DueDate).Subtract(TimeSpan.FromDays(1)));

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

                _scheduledNotificationsRepository.removeScheduledNotification(id);

                if (await _ordersRepository.SaveAllAsync()) return Ok(deleted);

                return BadRequest("Could not delete order");
            }
            catch (System.Exception e)
            {
                return BadRequest("Could not delete order: " + e);
            }
        }

        [HttpPost("pdf/{id}")]
        public async Task<ActionResult> GeneratePdfDocument(int id)
        {
            try
            {
                var order = await _ordersRepository.GeneratePdf(id);

                var client = new SendGridClient(_config.GetSection("MAIL_API_KEY").Value);

                var from = new EmailAddress("francescobarranca@outlook.com", "AUTOMATIC SERVICE");
                var to = new EmailAddress(_config.GetSection("RECIPIENT").Value);
                var subject = "Order Transcripted to PDF";
                var htmlContent = "<strong>Order Transcripted to PDF</strong>";

                var message = MailHelper.CreateSingleEmail(from, to, subject, "", htmlContent);

                var attachment = new Attachment();
                attachment.Type = "application/pdf";
                attachment.Filename = $"{order.Id}.pdf";
                byte[] bytes = System.IO.File.ReadAllBytes($"Data/Files/{order.Id}.pdf");
                attachment.Content = Convert.ToBase64String(bytes);

                message.AddAttachment(attachment);

                var res = await client.SendEmailAsync(message);

                System.IO.File.Delete($"Data/Files/{order.Id}.pdf");

                return Created("", res.IsSuccessStatusCode);

            }
            catch (System.Exception e)
            {
                return BadRequest("Could not generate pdf: " + e);
            }
        }
    }
}