using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.IRepositories;
using TaskProject.Models;

namespace MyMigrations.Repositories
{
    public class PersonRepository : IPersonRepository
    {
        private readonly MedicalDbContext _context;
        public PersonRepository(MedicalDbContext context)
        {
            _context = context;
        }

        public async Task<Person> AddPersonAsync(Person person)
        {

            _context.Persons.Add(person);
            await _context.SaveChangesAsync();
            return person;
        }
    }

}
