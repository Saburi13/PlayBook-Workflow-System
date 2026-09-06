using Microsoft.AspNetCore.Mvc;
using PlayBook.Business.BusinessModels.RequestDTOs.CRMRequestDTOs;
using PlayBook.Business.BusinessModels.ResponseDTOs.CRMResponseDTOs;
using PlayBook.Business.Interfaces.IService;

namespace PlayBook.API.Controllers;

[ApiController]
[Route("api/crm")]
public sealed class CrmController(
    ICrmService crmService) : ControllerBase
{
    // ============================================================
    // Employee Grades
    // ============================================================

    [HttpGet("grades")]
    public async Task<ActionResult<IReadOnlyList<EmployeeGradeDto>>> GetGrades(
        CancellationToken cancellationToken)
        => Ok(await crmService.GetGradesAsync(cancellationToken));

    [HttpGet("grades/{id:guid}")]
    public async Task<ActionResult<EmployeeGradeDto>> GetGrade(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await crmService.GetGradeAsync(id, cancellationToken);

        return result is null
            ? NotFound()
            : Ok(result);
    }

    [HttpPost("grades")]
    public async Task<ActionResult<EmployeeGradeDto>> CreateGrade(
        [FromBody] EmployeeGradeRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await crmService.CreateGradeAsync(
                request,
                cancellationToken);

            return Ok(result);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpPut("grades/{id:guid}")]
    public async Task<IActionResult> UpdateGrade(
        Guid id,
        [FromBody] EmployeeGradeRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return await crmService.UpdateGradeAsync(
                id,
                request,
                cancellationToken)
                ? NoContent()
                : NotFound();
        }
        catch (ArgumentException exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpDelete("grades/{id:guid}")]
    public async Task<IActionResult> DeleteGrade(
        Guid id,
        CancellationToken cancellationToken)
        => await crmService.DeleteGradeAsync(id, cancellationToken)
            ? NoContent()
            : NotFound();


    // ============================================================
    // Employees
    // ============================================================

    [HttpGet("employees")]
    public async Task<ActionResult<IReadOnlyList<EmployeeDto>>> GetEmployees(
        CancellationToken cancellationToken)
        => Ok(await crmService.GetEmployeesAsync(cancellationToken));

    [HttpGet("employees/{id:guid}")]
    public async Task<ActionResult<EmployeeDto>> GetEmployee(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await crmService.GetEmployeeAsync(id, cancellationToken);

        return result is null
            ? NotFound()
            : Ok(result);
    }

    [HttpPost("employees")]
    public async Task<ActionResult<EmployeeDto>> CreateEmployee(
        [FromBody] EmployeeRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await crmService.CreateEmployeeAsync(
                request,
                cancellationToken));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpPut("employees/{id:guid}")]
    public async Task<IActionResult> UpdateEmployee(
        Guid id,
        [FromBody] EmployeeRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return await crmService.UpdateEmployeeAsync(
                id,
                request,
                cancellationToken)
                ? NoContent()
                : NotFound();
        }
        catch (ArgumentException exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpDelete("employees/{id:guid}")]
    public async Task<IActionResult> DeleteEmployee(
        Guid id,
        CancellationToken cancellationToken)
        => await crmService.DeleteEmployeeAsync(id, cancellationToken)
            ? NoContent()
            : NotFound();


    // ============================================================
    // Customers
    // ============================================================

    [HttpGet("customers")]
    public async Task<ActionResult<IReadOnlyList<CustomerDto>>> GetCustomers(
        CancellationToken cancellationToken)
        => Ok(await crmService.GetCustomersAsync(cancellationToken));

    [HttpGet("customers/{id:guid}")]
    public async Task<ActionResult<CustomerDto>> GetCustomer(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await crmService.GetCustomerAsync(id, cancellationToken);

        return result is null
            ? NotFound()
            : Ok(result);
    }

    [HttpPost("customers")]
    public async Task<ActionResult<CustomerDto>> CreateCustomer(
        [FromBody] CustomerRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await crmService.CreateCustomerAsync(
                request,
                cancellationToken));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpPut("customers/{id:guid}")]
    public async Task<IActionResult> UpdateCustomer(
        Guid id,
        [FromBody] CustomerRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return await crmService.UpdateCustomerAsync(
                id,
                request,
                cancellationToken)
                ? NoContent()
                : NotFound();
        }
        catch (ArgumentException exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpDelete("customers/{id:guid}")]
    public async Task<IActionResult> DeleteCustomer(
        Guid id,
        CancellationToken cancellationToken)
        => await crmService.DeleteCustomerAsync(id, cancellationToken)
            ? NoContent()
            : NotFound();


    // ============================================================
    // Products
    // ============================================================

    [HttpGet("products")]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> GetProducts(
        CancellationToken cancellationToken)
        => Ok(await crmService.GetProductsAsync(cancellationToken));

    [HttpGet("products/{id:guid}")]
    public async Task<ActionResult<ProductDto>> GetProduct(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await crmService.GetProductAsync(id, cancellationToken);

        return result is null
            ? NotFound()
            : Ok(result);
    }

    [HttpPost("products")]
    public async Task<ActionResult<ProductDto>> CreateProduct(
        [FromBody] ProductRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await crmService.CreateProductAsync(
                request,
                cancellationToken));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpPut("products/{id:guid}")]
    public async Task<IActionResult> UpdateProduct(
        Guid id,
        [FromBody] ProductRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return await crmService.UpdateProductAsync(
                id,
                request,
                cancellationToken)
                ? NoContent()
                : NotFound();
        }
        catch (ArgumentException exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpDelete("products/{id:guid}")]
    public async Task<IActionResult> DeleteProduct(
        Guid id,
        CancellationToken cancellationToken)
        => await crmService.DeleteProductAsync(id, cancellationToken)
            ? NoContent()
            : NotFound();


    // ============================================================
    // Opportunities
    // ============================================================

    [HttpGet("opportunities")]
    public async Task<ActionResult<IReadOnlyList<OpportunityDto>>> GetOpportunities(
        CancellationToken cancellationToken)
        => Ok(await crmService.GetOpportunitiesAsync(cancellationToken));

    [HttpGet("opportunities/{id:guid}")]
    public async Task<ActionResult<OpportunityDto>> GetOpportunity(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await crmService.GetOpportunityAsync(
            id,
            cancellationToken);

        return result is null
            ? NotFound()
            : Ok(result);
    }

    [HttpPost("opportunities")]
    public async Task<ActionResult<OpportunityDto>> CreateOpportunity(
        [FromBody] OpportunityRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await crmService.CreateOpportunityAsync(
                request,
                cancellationToken));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpPut("opportunities/{id:guid}")]
    public async Task<IActionResult> UpdateOpportunity(
        Guid id,
        [FromBody] OpportunityRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return await crmService.UpdateOpportunityAsync(
                id,
                request,
                cancellationToken)
                ? NoContent()
                : NotFound();
        }
        catch (ArgumentException exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpDelete("opportunities/{id:guid}")]
    public async Task<IActionResult> DeleteOpportunity(
        Guid id,
        CancellationToken cancellationToken)
        => await crmService.DeleteOpportunityAsync(id, cancellationToken)
            ? NoContent()
            : NotFound();


    // ============================================================
    // Proposals
    // ============================================================

    [HttpGet("proposals")]
    public async Task<ActionResult<IReadOnlyList<ProposalDto>>> GetProposals(
        CancellationToken cancellationToken)
        => Ok(await crmService.GetProposalsAsync(cancellationToken));

    [HttpGet("proposals/{id:guid}")]
    public async Task<ActionResult<ProposalDto>> GetProposal(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await crmService.GetProposalAsync(
            id,
            cancellationToken);

        return result is null
            ? NotFound()
            : Ok(result);
    }

    [HttpPost("proposals")]
    public async Task<ActionResult<ProposalDto>> CreateProposal(
        [FromBody] ProposalRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await crmService.CreateProposalAsync(
                request,
                cancellationToken));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(exception.Message);
        }
    }

    [HttpPut("proposals/{id:guid}")]
    public async Task<IActionResult> UpdateProposal(
        Guid id,
        [FromBody] ProposalRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return await crmService.UpdateProposalAsync(
                id,
                request,
                cancellationToken)
                ? NoContent()
                : NotFound();
        }
        catch (ArgumentException exception)
        {
            return BadRequest(exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(exception.Message);
        }
    }

    [HttpPost("proposals/{id:guid}/correct")]
    public async Task<ActionResult<ProposalDto>> CorrectProposal(
        Guid id,
        [FromBody] CorrectProposalRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await crmService.CorrectProposalAsync(
                id,
                request,
                cancellationToken);

            return result is null
                ? NotFound()
                : Ok(result);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(exception.Message);
        }
    }

    [HttpDelete("proposals/{id:guid}")]
    public async Task<IActionResult> DeleteProposal(
        Guid id,
        CancellationToken cancellationToken)
        => await crmService.DeleteProposalAsync(id, cancellationToken)
            ? NoContent()
            : NotFound();


    // ============================================================
    // Vouchers
    // ============================================================

    [HttpGet("vouchers")]
    public async Task<ActionResult<IReadOnlyList<VoucherDto>>> GetVouchers(
        CancellationToken cancellationToken)
        => Ok(await crmService.GetVouchersAsync(cancellationToken));

    [HttpPost("vouchers")]
    public async Task<ActionResult<VoucherDto>> CreateVoucher(
        [FromBody] VoucherRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await crmService.CreateVoucherAsync(
                request,
                cancellationToken));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(exception.Message);
        }
    }

    [HttpGet("vouchers/validate")]
    public async Task<ActionResult<VoucherDto>> ValidateVoucher(
        [FromQuery] string code,
        [FromQuery] decimal amount,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await crmService.ValidateVoucherAsync(
                code,
                amount,
                cancellationToken);

            return result is null
                ? NotFound()
                : Ok(result);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(exception.Message);
        }
    }


    // ============================================================
    // Proposal Products
    // ============================================================

    [HttpGet("proposals/{proposalId:guid}/products")]
    public async Task<ActionResult<IReadOnlyList<ProposalProductDto>>> GetProposalProducts(
        Guid proposalId,
        CancellationToken cancellationToken)
        => Ok(await crmService.GetProposalProductsAsync(
            proposalId,
            cancellationToken));

    [HttpPost("proposals/{proposalId:guid}/products")]
    public async Task<ActionResult<ProposalProductDto>> AddProposalProduct(
        Guid proposalId,
        [FromBody] ProposalProductRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await crmService.AddProposalProductAsync(
                proposalId,
                request,
                cancellationToken);

            return result is null
                ? NotFound()
                : Ok(result);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(exception.Message);
        }
    }

    [HttpPut("proposals/{proposalId:guid}/products/{id:guid}")]
    public async Task<IActionResult> UpdateProposalProduct(
        Guid proposalId,
        Guid id,
        [FromBody] ProposalProductRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return await crmService.UpdateProposalProductAsync(
                proposalId,
                id,
                request,
                cancellationToken)
                ? NoContent()
                : NotFound();
        }
        catch (ArgumentException exception)
        {
            return BadRequest(exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(exception.Message);
        }
    }

    [HttpDelete("proposals/{proposalId:guid}/products/{id:guid}")]
    public async Task<IActionResult> DeleteProposalProduct(
        Guid proposalId,
        Guid id,
        CancellationToken cancellationToken)
        => await crmService.DeleteProposalProductAsync(
            proposalId,
            id,
            cancellationToken)
            ? NoContent()
            : NotFound();


    // ============================================================
    // Orders
    // ============================================================

    [HttpGet("orders")]
    public async Task<ActionResult<IReadOnlyList<OrderDto>>> GetOrders(
        CancellationToken cancellationToken)
        => Ok(await crmService.GetOrdersAsync(cancellationToken));

    [HttpGet("orders/{id:guid}")]
    public async Task<ActionResult<OrderDto>> GetOrder(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await crmService.GetOrderAsync(
            id,
            cancellationToken);

        return result is null
            ? NotFound()
            : Ok(result);
    }

    [HttpPost("orders")]
    public async Task<ActionResult<OrderDto>> CreateOrder(
        [FromBody] OrderRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await crmService.CreateOrderAsync(
                request,
                cancellationToken));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(exception.Message);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(exception.Message);
        }
    }

    [HttpPut("orders/{id:guid}")]
    public async Task<IActionResult> UpdateOrder(
        Guid id,
        [FromBody] OrderRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return await crmService.UpdateOrderAsync(
                id,
                request,
                cancellationToken)
                ? NoContent()
                : NotFound();
        }
        catch (ArgumentException exception)
        {
            return BadRequest(exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(exception.Message);
        }
    }

    [HttpDelete("orders/{id:guid}")]
    public async Task<IActionResult> DeleteOrder(
        Guid id,
        CancellationToken cancellationToken)
        => await crmService.DeleteOrderAsync(
            id,
            cancellationToken)
            ? NoContent()
            : NotFound();


    // ============================================================
    // Order Products
    // ============================================================

    [HttpGet("orders/{orderId:guid}/products")]
    public async Task<ActionResult<IReadOnlyList<OrderProductDto>>> GetOrderProducts(
        Guid orderId,
        CancellationToken cancellationToken)
        => Ok(await crmService.GetOrderProductsAsync(
            orderId,
            cancellationToken));

    [HttpPost("orders/{orderId:guid}/products")]
    public async Task<ActionResult<OrderProductDto>> AddOrderProduct(
        Guid orderId,
        [FromBody] OrderProductRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await crmService.AddOrderProductAsync(
                orderId,
                request,
                cancellationToken);

            return result is null
                ? NotFound()
                : Ok(result);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(exception.Message);
        }
    }

    [HttpPut("orders/{orderId:guid}/products/{id:guid}")]
    public async Task<IActionResult> UpdateOrderProduct(
        Guid orderId,
        Guid id,
        [FromBody] OrderProductRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return await crmService.UpdateOrderProductAsync(
                orderId,
                id,
                request,
                cancellationToken)
                ? NoContent()
                : NotFound();
        }
        catch (ArgumentException exception)
        {
            return BadRequest(exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(exception.Message);
        }
    }

    [HttpDelete("orders/{orderId:guid}/products/{id:guid}")]
    public async Task<IActionResult> DeleteOrderProduct(
        Guid orderId,
        Guid id,
        CancellationToken cancellationToken)
        => await crmService.DeleteOrderProductAsync(
            orderId,
            id,
            cancellationToken)
            ? NoContent()
            : NotFound();


    // ============================================================
    // Subscriptions
    // ============================================================

    [HttpGet("subscriptions")]
    public async Task<ActionResult<IReadOnlyList<SubscriptionDto>>> GetSubscriptions(
        CancellationToken cancellationToken)
        => Ok(await crmService.GetSubscriptionsAsync(
            cancellationToken));


    // ============================================================
    // Engagement Activities
    // ============================================================

    [HttpGet("activities")]
    public async Task<ActionResult<IReadOnlyList<EngagementActivityDto>>> GetActivities(
        [FromQuery] Guid? customerId,
        [FromQuery] Guid? opportunityId,
        CancellationToken cancellationToken)
        => Ok(await crmService.GetActivitiesAsync(
            customerId,
            opportunityId,
            cancellationToken));

    [HttpPost("activities")]
    public async Task<ActionResult<EngagementActivityDto>> CreateActivity(
        [FromBody] EngagementActivityRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await crmService.CreateActivityAsync(
                request,
                cancellationToken);

            return result is null
                ? NotFound()
                : Ok(result);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(exception.Message);
        }
    }


    // ============================================================
    // Conversations
    // ============================================================

    [HttpGet("conversations")]
    public async Task<ActionResult<IReadOnlyList<ConversationDto>>> GetConversations(
        [FromQuery] Guid? customerId,
        [FromQuery] Guid? opportunityId,
        CancellationToken cancellationToken)
        => Ok(await crmService.GetConversationsAsync(
            customerId,
            opportunityId,
            cancellationToken));

    [HttpPost("conversations")]
    public async Task<ActionResult<ConversationDto>> CreateConversation(
        [FromBody] ConversationRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await crmService.CreateConversationAsync(
                request,
                cancellationToken);

            return result is null
                ? NotFound()
                : Ok(result);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(exception.Message);
        }
    }
}