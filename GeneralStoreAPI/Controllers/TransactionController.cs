﻿using GeneralStoreAPI.Models;
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
        public async Task<IHttpActionResult> Post([FromBody] Transaction transaction, Product product)
        {
            if(product != null)
            {
                return Ok(product);
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var products = await _context.Products.FindAsync(transaction.ProductSKU);
            transaction.Product = products;

            if (product == null)
            {
                return BadRequest("Bad Request.");
            }

            _context.Products.Add(product);

            if (await _context.SaveChangesAsync() == 1)
            {
                return Ok(product.IsInStock);
            }

            if(transaction == null)
            {
                return BadRequest("Bad Request");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var customer = await _context.Customers.FindAsync(transaction.CustomerID);
            transaction.Customer = customer;

            transaction.DateOfTransaction = DateTime.Now;

            _context.Transactions.Add(transaction);

            if(await _context.SaveChangesAsync() == 1)
            {
                return Ok($"Transaction: {transaction.Product} was added successfully.");
            }

            _context.Products.Remove(product);

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
            if(transaction == null)
            {
                return NotFound();
            }
            return Ok(transaction);
        }

        [HttpPut]
        public async Task<IHttpActionResult> Put([FromUri] int id, [FromBody] Transaction newTransation)
        {
            if(id < 1)
            {
                return BadRequest("Bad Request");
            }
            if(id != newTransation.ID)
            {
                return BadRequest("Bad Request");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var transaction = await _context.Transactions.FindAsync(id);

            if(transaction == null)
            {
                return NotFound();
            }

            transaction.Customer = newTransation.Customer;
            transaction.CustomerID = newTransation.CustomerID;
            transaction.ProductSKU = newTransation.ProductSKU;
            transaction.ItemCount = newTransation.ItemCount;
            transaction.DateOfTransaction = newTransation.DateOfTransaction;

            if(await _context.SaveChangesAsync() == 1)
            {
                return Ok();
            }

            return InternalServerError();
        }

        [HttpDelete]
        public async Task<IHttpActionResult> Delete([FromUri] int id)
        {
            var transaction = await _context.Transactions.FindAsync();
            
            if(transaction != null)
            {
                _context.Transactions.Remove(transaction);

                if(await _context.SaveChangesAsync() == 1)
                {
                    return Ok();
                }
            }

            return InternalServerError();
        }
    }
}