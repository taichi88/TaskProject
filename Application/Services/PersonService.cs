using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces;
using Domain.IRepositories;
using Domain.Models.Dto;
using TaskProject.Models;

namespace Application.Services
{
    public class PersonService : IPersonService
    {
        private readonly IPersonRepository _personRepository;

        public PersonService(IPersonRepository personRepository)
        {
            _personRepository = personRepository;
        }

        public async Task<PersonDto> CreatePersonAsync(PersonDto dto)
        {

            var person = new Person
            {
                Name = dto.Name,
                Surname = dto.Surname,
                Email = dto.Email,
                Address = dto.Address,
                PersonalNumber = dto.PersonalNumber,
                DateOfBirth = dto.DateOfBirth,
                Phone = dto.Phone,
                Role = dto.Role.ToString(),

                // Map other fields...
            };

            var createdPerson = await _personRepository.AddPersonAsync(person);

            return new PersonDto
            {
                Name = createdPerson.Name,
                Surname = createdPerson.Surname,
                PersonalNumber = createdPerson.PersonalNumber,
                DateOfBirth = createdPerson.DateOfBirth,
                Phone = createdPerson.Phone,
               
                Email = createdPerson.Email,
                Address = createdPerson.Address,
                // Map other fields...
            };
        }
    }
}
