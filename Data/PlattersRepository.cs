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
    public class PlattersRepository : IPlattersRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        public PlattersRepository(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public void AddPlatter(Platter platter)
        {
            _context.Platters.Add(platter);
        }

        public async Task<PlatterDto> DeletePlatter(int id)
        {
            var removedPlatter = await GetDbPlatterById(id);
            var returnedPlatter = await GetPlatterById(id);
            _context.Platters.Remove(removedPlatter);

            return returnedPlatter;
        }

        public async Task<Platter> GetDbPlatterById(int id)
        {
            return await _context.Platters.Where(plt => plt.Id == id).Include("Formats").SingleOrDefaultAsync();
        }

        public async Task<PlatterDto> GetPlatterById(int id)
        {
            return await _context.Platters.Where(plt => plt.Id == id).Include("Formats").ProjectTo<PlatterDto>(_mapper.ConfigurationProvider).SingleOrDefaultAsync();
        }

        public async Task<IEnumerable<PlatterDto>> GetPlatters()
        {
            return await _context.Platters.Include("Formats").ProjectTo<PlatterDto>(_mapper.ConfigurationProvider).AsNoTracking().ToListAsync();
        }

        public async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public void UpdatePlatter(Platter platter)
        {
            _context.Platters.Update(platter);
        }
    }
}