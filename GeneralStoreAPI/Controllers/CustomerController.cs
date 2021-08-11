using GeneralStoreAPI.Models;
using GeneralStoreAPI.Models.Customers;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace GeneralStoreAPI.Controllers
{
    public class CustomerController : ApiController
    {
        private readonly ApplicationDbContext _context = new ApplicationDbContext();

        [HttpPost]
        public async Task<IHttpActionResult> Post([FromBody] Customer customer)
        {
            if (customer == null)
            {
                return BadRequest("Bad Request");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Customers.Add(customer);

            if (await _context.SaveChangesAsync() == 1)
            {
                return Ok($"{customer.FullName} was added successfully.");
            }

            return InternalServerError();
        }

        [HttpGet]
        public async Task<IHttpActionResult> GetAllCustomers()
        {
            var customer = await _context.Customers.ToListAsync();
            return Ok(customer);
        }

        [HttpGet]
        public async Task<IHttpActionResult> GetACustomerByID([FromUri] int id)
        {
            var customer = await _context.Customers.FindAsync(id);

            if(id < 1)
            {
                return BadRequest("Bad Request");
            }

            if(customer == null)
            {
                return NotFound();
            }

            return Ok(customer.FullName);
        }

        [HttpPut]
        public async Task<IHttpActionResult> UpdateExistingCustomerByID([FromBody] Customer newCustomer, [FromUri] int id)
        {
            var customer = await _context.Customers.FindAsync(id);

            if (id != newCustomer?.ID || customer == null)
            {
                return BadRequest("Bad Request");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var selectedCustomer = await _context.Customers.FindAsync(id);

            if(selectedCustomer == null)
            {
                return NotFound();
            }

            selectedCustomer.ID = newCustomer.ID;
            selectedCustomer.FirstName = newCustomer.FirstName;
            selectedCustomer.LastName = newCustomer.LastName;

            if (await _context.SaveChangesAsync() == 1)
            {
                return Ok("The Customer was updated.");
            } 

            return InternalServerError();
        }

        [HttpDelete]
        public async Task<IHttpActionResult> DeleteExistingCustomerByID([FromUri] int id)
        {
            var customer = await _context.Customers.FindAsync(id);

            if(customer == null)
            {
                return NotFound();
            }

            _context.Customers.Remove(customer);

            if (await _context.SaveChangesAsync() == 1)
            {
                return Ok($"{customer.FullName} was deleted.");
            }

            return InternalServerError();
        }
    }
}
