using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PHPT.Business.Services.Interfaces;
using PHPT.Common.DTOs;
using PHPT.Common.Enums;
using PHPT.Common.Models;

namespace PHPT.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Invoices")]
[Authorize(Roles = UserRoles.FrontDeskStaff)]
public class InvoicesController : ControllerBase
{
    private readonly IInvoiceService _invoiceService;
    private readonly ILogger<InvoicesController> _logger;

    public InvoicesController(IInvoiceService invoiceService, ILogger<InvoicesController> logger)
    {
        _invoiceService = invoiceService;
        _logger = logger;
    }

    /// <summary>
    /// Get detailed information about a specific invoice
    /// </summary>
    [HttpGet("GetInvoice/{Id}")]
    [ProducesResponseType(typeof(ApiResponse<InvoiceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<InvoiceDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<InvoiceDto>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<InvoiceDto>>> GetInvoice(Guid Id)
    {
        try
        {
            var actualFacilityId = GetFacilityId();
            var result = await _invoiceService.GetInvoiceByIdAsync(Id, actualFacilityId);
            
            if (result == null)
            {
                return NotFound(ApiResponse<InvoiceDto>.ErrorResponse("Invoice not found", "The requested invoice does not exist"));
            }

            return Ok(ApiResponse<InvoiceDto>.SuccessResponse(result, "Invoice loaded successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading invoice {InvoiceId}", Id);
            return StatusCode(500, ApiResponse<InvoiceDto>.ErrorResponse("Error loading invoice", ex.Message));
        }
    }

    /// <summary>
    /// Create a new invoice for an appointment
    /// </summary>
    [HttpPost("Create-Invoice")]
    [ProducesResponseType(typeof(ApiResponse<InvoiceDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<InvoiceDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<InvoiceDto>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<InvoiceDto>>> CreateInvoice([FromBody] CreateInvoiceDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<InvoiceDto>.ErrorResponse("Validation failed", errors));
            }

            var actualFacilityId = GetFacilityId();
            var result = await _invoiceService.CreateInvoiceAsync(actualFacilityId, dto);
            return CreatedAtAction(nameof(GetInvoice), new { id = result.Id, facilityId = actualFacilityId }, 
                ApiResponse<InvoiceDto>.SuccessResponse(result, "Invoice created successfully"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while creating invoice");
            return BadRequest(ApiResponse<InvoiceDto>.ErrorResponse("Invalid operation", ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating invoice");
            return StatusCode(500, ApiResponse<InvoiceDto>.ErrorResponse("Error creating invoice", ex.Message));
        }
    }

    /// <summary>
    /// Pay an invoice using patient's wallet
    /// </summary>
    [HttpPost("Pay")]
    [ProducesResponseType(typeof(ApiResponse<InvoiceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<InvoiceDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<InvoiceDto>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<InvoiceDto>>> PayInvoice([FromBody] PayInvoiceDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<InvoiceDto>.ErrorResponse("Validation failed", errors));
            }

            var actualFacilityId = GetFacilityId();
            var result = await _invoiceService.PayInvoiceAsync(actualFacilityId, dto);
            return Ok(ApiResponse<InvoiceDto>.SuccessResponse(result, "Invoice paid successfully"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while paying invoice");
            return BadRequest(ApiResponse<InvoiceDto>.ErrorResponse("Invalid operation", ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error paying invoice");
            return StatusCode(500, ApiResponse<InvoiceDto>.ErrorResponse("Error paying invoice", ex.Message));
        }
    }

    #region Helper Methods
    private Guid GetFacilityId()
    {
        var facilityIdClaim = User.FindFirst("FacilityId")?.Value;
        if (string.IsNullOrEmpty(facilityIdClaim) || !Guid.TryParse(facilityIdClaim, out var facilityId))
        {
            throw new UnauthorizedAccessException("Facility ID not found in claims");
        }
        return facilityId;
    }

    #endregion
}
