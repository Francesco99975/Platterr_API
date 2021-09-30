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
    public class PlattersController : BaseApiController
    {
        private readonly IPlattersRepository _plattersRepository;
        private readonly IMapper _mapper;
        public PlattersController(IPlattersRepository plattersRepository, IMapper mapper)
        {
            _mapper = mapper;
            _plattersRepository = plattersRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PlatterDto>>> GetPlatters()
        {
            try
            {
                var res = await _plattersRepository.GetPlatters();

                return Ok(res);
            }
            catch (System.Exception)
            {
                return BadRequest("Could Not Retrieve Platters");
            }
        }

        [HttpPost]
        public async Task<ActionResult<PlatterDto>> AddPlatter(PlatterDto platterDto)
        {
            try
            {
                var newPlatter = new Platter
                {
                    Name = platterDto.Name,
                    Description = platterDto.Description
                };

                _plattersRepository.AddPlatter(newPlatter);

                if (!await _plattersRepository.SaveAllAsync()) throw new Exception("Adding Problem");

                var formats = platterDto.Formats.Select((x) =>
                    new PlatterFormat
                    {
                        Size = x.Size,
                        Price = x.Price,
                        Platter = newPlatter,
                        PlatterId = newPlatter.Id
                    });

                newPlatter.Formats = formats.ToList();

                _plattersRepository.UpdatePlatter(newPlatter);

                if (await _plattersRepository.SaveAllAsync()) return Created("", await _plattersRepository.GetPlatterById(newPlatter.Id));

                return BadRequest("Database Error");
            }
            catch (System.Exception e)
            {
                return BadRequest("Could not add new platter: " + e);
            }
        }

        [HttpPut]
        public async Task<ActionResult<PlatterDto>> UpdatePlatter(PlatterDto platterDto)
        {
            try
            {
                var updatedPlatter = await _plattersRepository.GetDbPlatterById(platterDto.Id);
                _mapper.Map(platterDto, updatedPlatter);

                _plattersRepository.UpdatePlatter(updatedPlatter);

                if (await _plattersRepository.SaveAllAsync()) return Ok(_plattersRepository.GetPlatterById(updatedPlatter.Id));

                return BadRequest("Could not update platter");
            }
            catch (System.Exception e)
            {
                return BadRequest("Could not update platter: " + e);
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<PlatterDto>> DeletePlatter(int id)
        {
            try
            {
                var deleted = await _plattersRepository.DeletePlatter(id);

                if (await _plattersRepository.SaveAllAsync()) return Ok(deleted);

                return BadRequest("Could not delete platter");
            }
            catch (System.Exception e)
            {
                return BadRequest("Could not delete platter: " + e);
            }
        }
    }
}