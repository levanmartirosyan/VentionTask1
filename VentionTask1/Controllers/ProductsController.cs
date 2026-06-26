using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VentionTask1.Application.DTOs;
using VentionTask1.Application.Services.Interfaces;

namespace VentionTask1.Controllers
{
    [Route("api/products")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProductsAsync()
        {
            var products = await _productService.GetAllProductsAsync();

            if (products == null || !products.Any())
            {
                return NoContent();
            }

            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductByIdAsync(Guid id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null) return NotFound();

            return Ok(product);
        }

        [HttpPost]
        public async Task<IActionResult> CreateProductAsync([FromBody] CreateProductDTO createProductDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var createdProduct = await _productService.CreateProductAsync(createProductDTO);
                return Ok(createdProduct);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProductAsync(Guid id, [FromBody] UpdateProductDTO updateProductDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var updatedProduct = await _productService.UpdateProductAsync(id, updateProductDTO);
                return Ok(updatedProduct);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProductAsync(Guid id)
        {
            bool result;

            try
            {
                result = await _productService.DeleteProductAsync(id);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            if (!result) return NotFound();

            return Ok();
        }
    }
}
