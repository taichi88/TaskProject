using HealthcareApi.Application.IUnitOfWork;
using HealthcareApi.Domain.IRepositories.IDapperRepositories;
using HealthcareApi.Domain.IRepositories;
using HealthcareApi.Infrastructure.Repositories.DapperRepository;
using HealthcareApi.Infrastructure.Repositories;
using HealthcareApi.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;
using System.Formats.Asn1;

public class UnitOfWork : IUnitOfWork
{
    private readonly HealthcareApiContext _context;
    private IDbContextTransaction _transaction;

    public IPatientRepository Patients { get; }
    public IPersonRepository Persons { get; }
    public IDoctorRepository Doctors { get; }

    public IDapperPaymentRepository Payments { get; private set; }
    public IDapperAppointmentRepository Appointments { get; private set; }
    public IDapperAccountRepository Accounts { get; private set; }

    public IDbTransaction DapperTransaction { get; private set; }

    private IDbConnection DapperConnection;


    public UnitOfWork(HealthcareApiContext context, IDbConnection dapperConnection)
    {
        DapperConnection = dapperConnection;


        DapperConnection.Open();
        DapperTransaction = DapperConnection.BeginTransaction();

        _context = context;
        Persons = new PersonRepository(_context);
        Doctors = new DoctorRepository(_context);
        Patients = new PatientRepository(_context);
        // Initialize Dapper repos without transaction initially
        Payments = new DapperPaymentRepository(DapperConnection, DapperTransaction);
        Appointments = new DapperAppointmentRepository(DapperConnection, DapperTransaction);
        Accounts = new DapperAccountRepository(DapperConnection, DapperTransaction);


        DapperTransaction.Commit();
        DapperConnection.Close();
        DapperTransaction.Dispose();          
        DapperTransaction = null;

    }

   
    public async Task DapperTrnsactionAndConnectionAsync()
    {
        DapperTransaction = null;
       
            if ( DapperConnection.State != ConnectionState.Open)
            {
                DapperConnection.Open();
            }

            if (DapperTransaction == null)
            {
                DapperTransaction = DapperConnection.BeginTransaction();
            }
    }

    public async Task DapperCommitAsync()
    {
        if (DapperTransaction != null)
        {
             DapperTransaction.Commit(); // ✅ Commit transaction
             DapperConnection.Close();   // ✅ Close the connection
            DapperTransaction.Dispose();           // Clean up
            DapperTransaction = null;
        }
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();

        // Re-initialize Dapper repositories with the transaction
        var dbTransaction = _transaction.GetDbTransaction();
        return _transaction;
    }

    public async Task<int> CommitAsync()
    {
        if (_transaction != null)
        {
            await _context.SaveChangesAsync();
            await _transaction.CommitAsync();
        }
        else
        {
            return await _context.SaveChangesAsync();
        }

        return 1;
    }

    public async Task RollbackAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
