using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using HealthcareApi.Application.DTO;
using HealthcareApi.Domain.Models;

namespace HealthcareApi.Application
{
    public class AutoMapperClass : Profile
    {
        public AutoMapperClass() {

            CreateMap<PersonDto, Person>()
            .ForMember(dest => dest.DateOfBirth, opt =>
                opt.MapFrom(src => src.DateOfBirth.HasValue
                    ? DateOnly.FromDateTime(src.DateOfBirth.Value)
                    : (DateOnly?)null));

            CreateMap<Person, PersonDto>()
                .ForMember(dest => dest.DateOfBirth, opt =>
                    opt.MapFrom(src => src.DateOfBirth.HasValue
                        ? src.DateOfBirth.Value.ToDateTime(TimeOnly.MinValue)
                        : (DateTime?)null));

            CreateMap<Patient, PatientDto>().ReverseMap();
            CreateMap<Doctor, DoctorDto>().ReverseMap();



            CreateMap<Appointment, AppointmentsDto>().ReverseMap(); // Add this line to map Appointment to AppointmentDto>

           CreateMap<Payment, PaymentDto>().ReverseMap();
            CreateMap<Account, AccountDto>().ReverseMap();
            

        }
    }
}
