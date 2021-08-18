using GeneralStoreAPI.Models;
using GeneralStoreAPI.Models.Customers;
using GeneralStoreAPI.Models.Products;
using GeneralStoreAPI.Models.Transactions;
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
    public class TransactionController : ApiController
    {
        private readonly ApplicationDbContext _context = new ApplicationDbContext();

        [HttpPost]
        public async Task<IHttpActionResult> Post([FromBody] Transaction transaction)
        {
            if (transaction == null)
            {
                return BadRequest("Bad Request");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Lookup both the customer and product using the _context
            var customer = await _context.Customers.FindAsync(transaction.CustomerID);
            var product = await _context.Products.FindAsync(transaction.ProductSKU);

            var validateResult = ValidateTransaction(transaction, product, customer);

            if (!string.IsNullOrWhiteSpace(validateResult))
            {
                return BadRequest(validateResult);
            }

            product.NumberInInventory -= transaction.ItemCount;
            _context.Transactions.Add(transaction);

            if (await _context.SaveChangesAsync() > 0)
            {
                return Ok(transaction);
            }

            return InternalServerError();
        }

        [HttpGet]
        public async Task<IHttpActionResult> GetAllTransactions()
        {
            var transaction = await _context.Transactions.ToListAsync();
            return Ok(transaction);
        }

        [HttpGet]
        public async Task<IHttpActionResult> GetTransactionByID([FromUri] int id)
        {
            var transaction = await _context.Transactions.SingleOrDefaultAsync(transactions => transactions.ID == id);
            if (transaction == null)
            {
                return NotFound();
            }
            return Ok(transaction);
        }

        [HttpPut]
        public async Task<IHttpActionResult> Put([FromUri] int id, [FromBody] Transaction newTransation)
        {
            if (id < 1)
            {
                return BadRequest("Bad Request");
            }
            if (id != newTransation.ID)
            {
                return BadRequest("Bad Request");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var transaction = await _context.Transactions.FindAsync(id);

            if (transaction == null)
            {
                return NotFound();
            }

            transaction.Customer = newTransation.Customer;
            transaction.CustomerID = newTransation.CustomerID;
            transaction.ProductSKU = newTransation.ProductSKU;
            transaction.ItemCount = newTransation.ItemCount;
            transaction.DateOfTransaction = newTransation.DateOfTransaction;

            if (await _context.SaveChangesAsync() == 1)
            {
                return Ok();
            }

            return InternalServerError();
        }

        [HttpDelete]
        public async Task<IHttpActionResult> Delete([FromUri] int id)
        {
            var transaction = await _context.Transactions.FindAsync();

            if (transaction != null)
            {
                _context.Transactions.Remove(transaction);

                if (await _context.SaveChangesAsync() == 1)
                {
                    return Ok();
                }
            }

            return InternalServerError();
        }

        private string ValidateTransaction(Transaction transaction, Product product, Customer customer)
        {
            if(customer == null)
            {
                return $"Invalid Transaction: Customer with ID: {transaction.CustomerID} does not exist.";
            }

            if(product == null)
            {
                return $"Invalid Transaction: Product with Sku: {transaction.ProductSKU} does not exist.";
            }

            if (!product.IsInStock)
            {
                return $"Invalid Transaction: Product with Sku: {transaction.ProductSKU} is not in stock.";
            }

            if(product.NumberInInventory < transaction.ItemCount)
            {
                return $"Invalid Transaction: Product with Sku: {transaction.ProductSKU} doest not have enough in the inventory to complete the transaction.";
            }

            return string.Empty;
        }
    }
}
