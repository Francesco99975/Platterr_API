using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using platterr_api.Dtos;
using platterr_api.Entities;

namespace platterr_api.Interfaces
{
    public interface IPlattersRepository
    {
        Task<IEnumerable<PlatterDto>> GetPlatters();

        Task<PlatterDto> GetPlatterById(int id);

        Task<Platter> GetDbPlatterById(int id);

        void AddPlatter(Platter platter);

        void UpdatePlatter(Platter platter);

        Task<PlatterDto> DeletePlatter(int Id);

        Task<bool> SaveAllAsync();
    }
}