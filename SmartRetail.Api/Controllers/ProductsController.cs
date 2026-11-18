using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartRetail.Api.Helpers;
using SmartRetail.Application.DTOs.Category;
using SmartRetail.Application.DTOs.Product;
using SmartRetail.Application.Interfaces.Services;

namespace SmartRetail.Api.Controllers
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

        /// <summary>
        /// Get products for store with optional category and visibility filters
        /// </summary>
        [HttpGet("store/{storeId:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetStoreProducts(int storeId, [FromQuery] int? categoryId = null, [FromQuery] bool onlyVisible = false)
        {
            var products = await _productService.GetStoreProductsAsync(storeId, categoryId, onlyVisible);
            return Ok(new APIResponse<IEnumerable<ProductDto>>(200, "Products retrieved successfully.", products));
        }

        /// <summary>
        /// Get product by ID
        /// </summary>
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProductById(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound(new APIResponse<object>(404, "المنتج غير موجود."));
            }

            return Ok(new APIResponse<ProductDto>(200, "Product retrieved successfully.", product));
        }

        /// <summary>
        /// Search product by barcode
        /// </summary>
        [HttpGet("store/{storeId:int}/barcode/{barcode}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProductByBarcode(int storeId, string barcode)
        {
            var product = await _productService.GetProductByBarcodeAsync(storeId, barcode);
            if (product == null)
            {
                return NotFound(new APIResponse<object>(404, "لم يتم العثور على منتج بهذا الباركود في هذا المتجر."));
            }

            return Ok(new APIResponse<ProductDto>(200, "Product found.", product));
        }

        /// <summary>
        /// Create product with fallback to default category
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Owner,SuperAdmin")]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Barcode))
            {
                return BadRequest(new APIResponse<object>(400, "اسم المنتج والباركود كلاهما حقول إلزامية."));
            }

            try
            {
                var created = await _productService.CreateProductAsync(dto);
                return CreatedAtAction(nameof(GetProductById), new { id = created.Id },
                    new APIResponse<ProductDto>(201, "تم إضافة المنتج بنجاح.", created));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new APIResponse<object>(400, ex.Message));
            }
        }

        /// <summary>
        /// Update product
        /// </summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Owner,SuperAdmin")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDto dto)
        {
            try
            {
                var updated = await _productService.UpdateProductAsync(id, dto);
                if (updated == null)
                {
                    return NotFound(new APIResponse<object>(404, "المنتج غير موجود."));
                }

                return Ok(new APIResponse<ProductDto>(200, "تم تعديل بيانات المنتج بنجاح.", updated));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new APIResponse<object>(400, ex.Message));
            }
        }

        /// <summary>
        /// Toggle product visibility
        /// </summary>
        [HttpPatch("{id:int}/visibility")]
        [Authorize(Roles = "Owner,SuperAdmin")]
        public async Task<IActionResult> ToggleVisibility(int id, [FromBody] ToggleVisibilityDto dto)
        {
            var success = await _productService.ToggleVisibilityAsync(id, dto.IsVisible);
            if (!success)
            {
                return NotFound(new APIResponse<object>(404, "المنتج غير موجود."));
            }

            return Ok(new APIResponse<object>(200, dto.IsVisible ? "تم إظهار المنتج بنجاح." : "تم إخفاء المنتج بنجاح."));
        }

        /// <summary>
        /// Delete product permanently
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Owner,SuperAdmin")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var success = await _productService.DeleteProductAsync(id);
            if (!success)
            {
                return NotFound(new APIResponse<object>(404, "المنتج غير موجود."));
            }

            return Ok(new APIResponse<object>(200, "تم حذف المنتج بنجاح."));
        }
    }
}
