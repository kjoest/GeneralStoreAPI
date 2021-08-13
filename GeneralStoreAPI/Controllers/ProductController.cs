using GeneralStoreAPI.Models;
using GeneralStoreAPI.Models.Products;
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
    public class ProductController : ApiController
    {
        private readonly ApplicationDbContext _context = new ApplicationDbContext();

        [HttpPost]
        public async Task<IHttpActionResult> Post([FromBody] Product product)
        {
            if (product == null)
            {
                return BadRequest("Bad Request");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Products.Add(product);

            if (await _context.SaveChangesAsync() == 1)
            {
                return Ok("The product was added successfully.");
            }

            return InternalServerError();
        }

        [HttpGet]
        public async Task<IHttpActionResult> GetAllProducts()
        {
            var products = await _context.Products.ToListAsync();
            return Ok(products);
        }

        [HttpGet]
        public async Task<IHttpActionResult> GetProductBySKU([FromUri] string sku)
        {
            var product = await _context.Products.FindAsync(sku);

            if (sku == null)
            {
                return BadRequest("Bad Request");
            }

            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        [HttpPut]
        public async Task<IHttpActionResult> Put([FromBody] Product newProduct, [FromUri] string sku)
        {
            if(newProduct.SKU != null || newProduct == null)
            {
                return BadRequest("Bad Request");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest("Bad Request");
            }
            var product = await _context.Products.FindAsync(sku);

            if(product == null)
            {
                return NotFound();
            }

            product.Name = newProduct.Name;
            product.Cost = newProduct.Cost;
            product.NumberInInventory = newProduct.NumberInInventory;

            if(await _context.SaveChangesAsync() == 1)
            {
                return Ok();
            }

            return InternalServerError();
        }

        [HttpDelete]
        public async Task<IHttpActionResult> Delete([FromUri] string sku)
        {
            if (sku == null)
            {
                return BadRequest("Bad Request");
            }
            var product = await _context.Products.FindAsync();
            if(product != null)
            {
                _context.Products.Remove(product);
                if(await _context.SaveChangesAsync() == 1)
                {
                    return Ok($"Product: {product.Name} removed successfully.");
                }

            }

            return InternalServerError();
        }
    }
}
