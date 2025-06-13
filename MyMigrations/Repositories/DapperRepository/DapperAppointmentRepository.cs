using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HealthcareApi.Domain.Models;
using Microsoft.Data.SqlClient;
using HealthcareApi.Domain.IRepositories.IDapperRepositories;
using Dapper;
using System.Data;
using HealthcareApi.Infrastructure.Data.Dapper.DapperDbContext;
using HealthcareApi.Application.DTO;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace HealthcareApi.Infrastructure.Repositories.DapperRepository
{
    public class DapperAppointmentRepository : IDapperAppointmentRepository
    {
       
        private readonly IDbConnection _connection;
        private readonly IDbTransaction? _transaction;

        public DapperAppointmentRepository(IDbConnection connection, IDbTransaction? transaction = null)
        {
            _connection = connection;
            _transaction = transaction;
            
        }

        public async Task<Appointment?> CreateAppointmentAsync(Appointment appointment)
        {
            const string sql = @"
                INSERT INTO Clinical.Appointments (PatientId, DoctorId, AppointmentDateTime, ReasonForVisit)
                VALUES (@PatientId, @DoctorId, @AppointmentDateTime, @ReasonForVisit);
                SELECT CAST(SCOPE_IDENTITY() as int);";

            if (_connection.State != ConnectionState.Open)
                _connection.Open();

            var id = await _connection.QuerySingleAsync<int>(sql, appointment, _transaction);
            appointment.AppointmentId = id; 
            return appointment;
        }



        public async Task<IEnumerable<Appointment>> GetAllAppointmentsAsync()
        {
            const string sql = @"
        SELECT PatientId, DoctorId, AppointmentDateTime, ReasonForVisit 
        FROM Clinical.Appointments";

            if (_connection.State != ConnectionState.Open)
                _connection.Open();

            var appointments = await _connection.QueryAsync<Appointment>(sql, transaction: _transaction);
            return appointments;
        }

        public async Task<Appointment?> GetAppointmentByIdAsync(int id)
        {
            const string sql = @"
            SELECT AppointmentId, PatientId, DoctorId, AppointmentDateTime, ReasonForVisit
            FROM Clinical.Appointments
            WHERE AppointmentId = @Id";

            if (_connection.State != ConnectionState.Open)
                _connection.Open();

            return await _connection.QuerySingleOrDefaultAsync<Appointment>(sql, new { Id = id }, _transaction);
        }


        public async Task<bool> UpdateAppointmentAsync(int id, Appointment appointment)
        {
            const string sql = @"
        UPDATE Clinical.Appointments 
        SET PatientId = @PatientId, 
            DoctorId = @DoctorId, 
            AppointmentDateTime = @AppointmentDateTime, 
            ReasonForVisit = @ReasonForVisit
        WHERE AppointmentId = @Id";

            if (_connection.State != ConnectionState.Open)
                _connection.Open();

            var parameters = new
            {
                Id = id,
                appointment.PatientId,
                appointment.DoctorId,
                appointment.AppointmentDateTime,
                appointment.ReasonForVisit
            };

            var rowsAffected = await _connection.ExecuteAsync(sql, parameters, _transaction);
            return rowsAffected > 0;
        }


        public async Task<bool> DeleteAppointmentAsync(int id)
        {
            const string sql = "DELETE FROM Clinical.Appointments WHERE AppointmentId = @Id";

            if (_connection.State != ConnectionState.Open)
                _connection.Open();

            var rowsAffected = await _connection.ExecuteAsync(sql, new { Id = id }, _transaction);
            return rowsAffected > 0;
        }



    }

}
