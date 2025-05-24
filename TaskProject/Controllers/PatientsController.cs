using Application;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using System.Collections.Generic;
using TaskProject.Models;
using TaskProject.Models.Dto;
namespace TaskProject.Controllers
{
   

    [Route("api/[controller]")]
    [ApiController]
    public class PatientsController : ControllerBase
    {
        // Mock data for patients
        private static readonly List<Patient> Patients = new List<Patient>
    {
        
    };
        /// <summary>
        /// Gets all Registered Patients.
        /// </summary>
        /// <returns> A list of all Patients.</returns>
        
        // GET: api/Patients
        [HttpGet]
        public ActionResult<IEnumerable<Patient>> GetPatients()
        {
            return Ok(Patients);  // Return mocked patient data
        }
        /// <summary>
        /// Register New Patient in our hospital.
        /// </summary>
        /// <returns> Added Patient.</returns>
        /// 
        // POST: api/Patients
        [HttpPost]
        public ActionResult<Patient> CreatePatient([FromBody] CreatePatientDto dto)
        {
   return Ok();

        }


    }

}
