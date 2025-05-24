using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskProject.Models;

namespace Domain.IRepositories
{
    public interface IPersonRepository
    {
        Task<Person> AddPersonAsync(Person person);
    }

}
