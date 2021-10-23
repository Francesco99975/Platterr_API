using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Draw;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
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

        public async Task<OrderDto> GeneratePdf(int id)
        {
            var order = await GetDbOrderById(id);
            PdfWriter writer = new PdfWriter($"Data/Files/{order.Id}.pdf");
            PdfDocument pdf = new PdfDocument(writer);
            Document document = new Document(pdf);

            Paragraph header = new Paragraph($"ORDER ID: {order.Id}")
               .SetTextAlignment(TextAlignment.CENTER)
               .SetFontSize(20);

            Paragraph subheader = new Paragraph($"CUSTOMER: {order.CustomerFirstName} {order.CustomerLastName}")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(15);

            Paragraph pPhoneNumber = new Paragraph($"Phone Number: {order.PhoneNumber}")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(15);

            Paragraph pOrderDate = new Paragraph($"Ordered on: {DateTime.Parse(order.CreatedAt).ToLongDateString()} at {DateTime.Parse(order.CreatedAt).ToShortTimeString()}")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(15);

            Paragraph pDetails = new Paragraph("Delivery: " + (order.Delivery ? "YES" : "NO") + " / " + "Paid: " + (order.Paid ? "YES" : "NO"))
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(15);

            LineSeparator ls = new LineSeparator(new SolidLine());

            Paragraph pPlattersHeader = new Paragraph("ORDERED DELI PLATTERS")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(20);

            List<Paragraph> pPlattersRequests = order.Platters.Select(plt => new Paragraph($"{plt.Platter.Name} / {plt.Format.Size}\" - {plt.Format.Price.ToString("C", CultureInfo.CurrentCulture)}")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(15)).ToList();

            Paragraph pCommentHeader = new Paragraph("COMMENT")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(20);

            Paragraph pComment = new Paragraph(order.Comment)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(15);

            Paragraph pExtraFee = new Paragraph($"Extra Fee: {order.ExtraFee.ToString("C", CultureInfo.CurrentCulture)}")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(20);

            var total = order.Platters.Aggregate(0.0, (prev, cur) => prev + cur.Format.Price) + order.ExtraFee;

            Paragraph pTotal = new Paragraph($"Total: {total.ToString("C", CultureInfo.CurrentCulture)}")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(20);

            Paragraph pDueDate = new Paragraph($"Due on: {DateTime.Parse(order.DueDate).ToLongDateString()} at {DateTime.Parse(order.DueDate).ToShortTimeString()}")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(15);


            document.Add(header);
            document.Add(subheader);
            document.Add(pPhoneNumber);
            document.Add(pOrderDate);
            document.Add(pDetails);
            document.Add(ls);
            document.Add(pPlattersHeader);

            foreach (var req in pPlattersRequests)
            {
                document.Add(req);
            }

            document.Add(ls);
            document.Add(pCommentHeader);
            document.Add(pComment);
            document.Add(pExtraFee);
            document.Add(pTotal);
            document.Add(ls);
            document.Add(pDueDate);

            document.Close();

            return await GetOrderById(id);
        }
    }
}