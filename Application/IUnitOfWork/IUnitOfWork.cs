using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HealthcareApi.Application.Interfaces;
using HealthcareApi.Domain.IRepositories;
using HealthcareApi.Domain.IRepositories.IDapperRepositories;
using Microsoft.EntityFrameworkCore.Storage;



namespace HealthcareApi.Application.IUnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IPersonRepository Persons { get; }
        IPatientRepository Patients { get; }
        IDoctorRepository Doctors { get; }

        IDapperPaymentRepository Payments { get; } // added Payment

        IDapperAccountRepository Accounts { get; }

        IDapperAppointmentRepository Appointments { get; }
        IDbTransaction? DapperTransaction { get; }

        public Task DapperTrnsactionAndConnectionAsync();

        public  Task DapperCommitAsync();

        public 

        Task<int> CommitAsync();
        public Task RollbackAsync();
        Task<IDbContextTransaction> BeginTransactionAsync(); // added BeginTransaction method
    }
}
