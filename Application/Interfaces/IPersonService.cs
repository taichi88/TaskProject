using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Models.Dto;

namespace Application.Interfaces
{
    public interface IPersonService
    {
        Task<PersonDto> CreatePersonAsync(PersonDto dto);
    }
}
