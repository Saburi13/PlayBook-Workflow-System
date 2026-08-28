using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using PlayBook.Application.CRM;
using PlayBook.Application.Interfaces;
using PlayBook.Domain;
using PlayBook.Infrastructure.Data;
using PlayBook.Application.Workflows;
using PlayBook.Application.Pricing;

namespace PlayBook.API.Controllers;

[ApiController]
[Route("api/crm")]
public sealed class CrmController(
    PlayBookDbContext dbContext,
    ICrmRepository<EmployeeGrade> grades,
    ICrmRepository<Employee> employees,
    ICrmRepository<Customer> customers,
    ICrmRepository<Product> products,
    ICrmRepository<Opportunity> opportunities,
    ICrmRepository<Proposal> proposals,
    ICrmRepository<ProposalProduct> proposalProducts,
    ICrmRepository<Order> orders,
    ICrmRepository<OrderProduct> orderProducts,
    IWorkflowExecutionService workflowExecutionService,
    IPricingService pricingService,
    VoucherService voucherService) : ControllerBase
{
    [HttpGet("employee-grades")]
    public async Task<ActionResult<IEnumerable<EmployeeGradeDto>>> GetGrades(CancellationToken cancellationToken) =>
        Ok(await grades.Query().AsNoTracking().OrderBy(g => g.Name).Select(g => new EmployeeGradeDto(g.Id, g.Name, g.Description, g.ApprovalLimit, g.IsActive)).ToListAsync(cancellationToken));

    [HttpPost("employee-grades")]
    public async Task<ActionResult<EmployeeGradeDto>> CreateGrade(EmployeeGradeRequest request, CancellationToken cancellationToken)
    {
        var entity = new EmployeeGrade { Id = Guid.NewGuid(), Name = request.Name.Trim(), Description = request.Description, ApprovalLimit = request.ApprovalLimit, IsActive = request.IsActive };
        await grades.AddAsync(entity, cancellationToken);
        await grades.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetGrade), new { id = entity.Id }, ToDto(entity));
    }

    [HttpGet("employee-grades/{id:guid}")]
    public async Task<ActionResult<EmployeeGradeDto>> GetGrade(Guid id, CancellationToken cancellationToken)
    {
        var entity = await grades.GetByIdAsync(id, cancellationToken);
        return entity is null ? NotFound() : Ok(ToDto(entity));
    }

    [HttpPut("employee-grades/{id:guid}")]
    public async Task<IActionResult> UpdateGrade(Guid id, EmployeeGradeRequest request, CancellationToken cancellationToken)
    {
        var entity = await grades.GetByIdAsync(id, cancellationToken);
        if (entity is null) return NotFound();
        entity.Name = request.Name.Trim(); entity.Description = request.Description; entity.ApprovalLimit = request.ApprovalLimit; entity.IsActive = request.IsActive; entity.UpdatedAt = DateTime.UtcNow;
        await grades.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("employee-grades/{id:guid}")]
    public async Task<IActionResult> DeleteGrade(Guid id, CancellationToken cancellationToken) => await Delete(grades, id, cancellationToken);

    [HttpGet("employees")]
    public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetEmployees(CancellationToken cancellationToken) =>
        Ok(await employees.Query().AsNoTracking().OrderBy(e => e.LastName).ThenBy(e => e.FirstName).Select(e => new EmployeeDto(e.Id, e.FirstName, e.LastName, e.Email, e.Phone, e.EmployeeGradeId, e.ManagerId, e.Role, e.IsActive)).ToListAsync(cancellationToken));

    [HttpPost("employees")]
    public async Task<ActionResult<EmployeeDto>> CreateEmployee(EmployeeRequest request, CancellationToken cancellationToken)
    {
        if (!await ReferencesExist(request.EmployeeGradeId, request.ManagerId, cancellationToken)) return BadRequest("Employee grade or manager does not exist.");
        var entity = new Employee { Id = Guid.NewGuid(), FirstName = request.FirstName.Trim(), LastName = request.LastName.Trim(), Email = request.Email.Trim().ToLowerInvariant(), Phone = request.Phone, EmployeeGradeId = request.EmployeeGradeId, ManagerId = request.ManagerId, Role = request.Role, IsActive = request.IsActive };
        await employees.AddAsync(entity, cancellationToken); await employees.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetEmployee), new { id = entity.Id }, ToDto(entity));
    }

    [HttpGet("employees/{id:guid}")]
    public async Task<ActionResult<EmployeeDto>> GetEmployee(Guid id, CancellationToken cancellationToken)
    {
        var entity = await employees.GetByIdAsync(id, cancellationToken);
        return entity is null ? NotFound() : Ok(ToDto(entity));
    }

    [HttpPut("employees/{id:guid}")]
    public async Task<IActionResult> UpdateEmployee(Guid id, EmployeeRequest request, CancellationToken cancellationToken)
    {
        var entity = await employees.GetByIdAsync(id, cancellationToken);
        if (entity is null) return NotFound();
        if (request.ManagerId == id || !await ReferencesExist(request.EmployeeGradeId, request.ManagerId, cancellationToken)) return BadRequest("Employee grade or manager is invalid.");
        entity.FirstName = request.FirstName.Trim(); entity.LastName = request.LastName.Trim(); entity.Email = request.Email.Trim().ToLowerInvariant(); entity.Phone = request.Phone; entity.EmployeeGradeId = request.EmployeeGradeId; entity.ManagerId = request.ManagerId; entity.Role = request.Role; entity.IsActive = request.IsActive; entity.UpdatedAt = DateTime.UtcNow;
        await employees.SaveChangesAsync(cancellationToken); return NoContent();
    }

    [HttpDelete("employees/{id:guid}")]
    public async Task<IActionResult> DeleteEmployee(Guid id, CancellationToken cancellationToken) => await Delete(employees, id, cancellationToken);

    [HttpGet("customers")]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> GetCustomers(CancellationToken cancellationToken) =>
        Ok(await customers.Query().AsNoTracking().OrderBy(c => c.Name).Select(c => new CustomerDto(c.Id, c.Name, c.Email, c.Phone, c.Company, c.Address, c.Status)).ToListAsync(cancellationToken));

    [HttpPost("customers")]
    public async Task<ActionResult<CustomerDto>> CreateCustomer(CustomerRequest request, CancellationToken cancellationToken)
    {
        var entity = new Customer { Id = Guid.NewGuid(), Name = request.Name.Trim(), Email = request.Email, Phone = request.Phone, Company = request.Company, Address = request.Address, Status = request.Status };
        await customers.AddAsync(entity, cancellationToken); await customers.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetCustomer), new { id = entity.Id }, ToDto(entity));
    }

    [HttpGet("customers/{id:guid}")]
    public async Task<ActionResult<CustomerDto>> GetCustomer(Guid id, CancellationToken cancellationToken)
    {
        var entity = await customers.GetByIdAsync(id, cancellationToken); return entity is null ? NotFound() : Ok(ToDto(entity));
    }

    [HttpPut("customers/{id:guid}")]
    public async Task<IActionResult> UpdateCustomer(Guid id, CustomerRequest request, CancellationToken cancellationToken)
    {
        var entity = await customers.GetByIdAsync(id, cancellationToken); if (entity is null) return NotFound();
        entity.Name = request.Name.Trim(); entity.Email = request.Email; entity.Phone = request.Phone; entity.Company = request.Company; entity.Address = request.Address; entity.Status = request.Status; entity.UpdatedAt = DateTime.UtcNow;
        await customers.SaveChangesAsync(cancellationToken); return NoContent();
    }

    [HttpDelete("customers/{id:guid}")]
    public async Task<IActionResult> DeleteCustomer(Guid id, CancellationToken cancellationToken) => await Delete(customers, id, cancellationToken);

    [HttpGet("products")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts(CancellationToken cancellationToken) =>
        Ok(await products.Query().AsNoTracking().OrderBy(p => p.Name).Select(p => new ProductDto(p.Id, p.Name, p.Description, p.Category, p.Price, p.IsActive, p.PlanDurationMonths)).ToListAsync(cancellationToken));

    [HttpPost("products")]
    public async Task<ActionResult<ProductDto>> CreateProduct(ProductRequest request, CancellationToken cancellationToken)
    {
        var entity = new Product { Id = Guid.NewGuid(), Name = request.Name.Trim(), Description = request.Description, Category = request.Category, Price = request.Price, PlanDurationMonths = request.PlanDurationMonths, IsActive = request.IsActive };
        await products.AddAsync(entity, cancellationToken); await products.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetProduct), new { id = entity.Id }, ToDto(entity));
    }

    [HttpGet("products/{id:guid}")]
    public async Task<ActionResult<ProductDto>> GetProduct(Guid id, CancellationToken cancellationToken)
    {
        var entity = await products.GetByIdAsync(id, cancellationToken); return entity is null ? NotFound() : Ok(ToDto(entity));
    }

    [HttpPut("products/{id:guid}")]
    public async Task<IActionResult> UpdateProduct(Guid id, ProductRequest request, CancellationToken cancellationToken)
    {
        var entity = await products.GetByIdAsync(id, cancellationToken); if (entity is null) return NotFound();
        entity.Name = request.Name.Trim(); entity.Description = request.Description; entity.Category = request.Category; entity.Price = request.Price; entity.PlanDurationMonths = request.PlanDurationMonths; entity.IsActive = request.IsActive; entity.UpdatedAt = DateTime.UtcNow;
        await products.SaveChangesAsync(cancellationToken); return NoContent();
    }

    [HttpDelete("products/{id:guid}")]
    public async Task<IActionResult> DeleteProduct(Guid id, CancellationToken cancellationToken) => await Delete(products, id, cancellationToken);

    [HttpGet("opportunities")]
    public async Task<ActionResult<IEnumerable<OpportunityDto>>> GetOpportunities(CancellationToken cancellationToken) =>
        Ok(await opportunities.Query().AsNoTracking().OrderByDescending(o => o.CreatedAt).Select(o => new OpportunityDto(o.Id, o.CustomerId, o.AssignedEmployeeId, o.Name, o.Description, o.EstimatedValue, o.Status, o.ExpectedCloseDate)).ToListAsync(cancellationToken));

    [HttpPost("opportunities")]
    public async Task<ActionResult<OpportunityDto>> CreateOpportunity(OpportunityRequest request, CancellationToken cancellationToken)
    {
        if (!await customers.Query().AnyAsync(c => c.Id == request.CustomerId, cancellationToken) || request.AssignedEmployeeId is not null && !await employees.Query().AnyAsync(e => e.Id == request.AssignedEmployeeId, cancellationToken)) return BadRequest("Customer or assigned employee does not exist.");
        var entity = new Opportunity { Id = Guid.NewGuid(), CustomerId = request.CustomerId, AssignedEmployeeId = request.AssignedEmployeeId, Name = request.Name.Trim(), Description = request.Description, EstimatedValue = request.EstimatedValue, Status = request.Status, ExpectedCloseDate = request.ExpectedCloseDate };
        await opportunities.AddAsync(entity, cancellationToken); await opportunities.SaveChangesAsync(cancellationToken);
        await workflowExecutionService.TriggerAsync("Opportunity Created", "Opportunity", entity.Id, JsonSerializer.SerializeToElement(new { discountPercentage = 10m }), cancellationToken);
        return CreatedAtAction(nameof(GetOpportunity), new { id = entity.Id }, ToDto(entity));
    }

    [HttpGet("opportunities/{id:guid}")]
    public async Task<ActionResult<OpportunityDto>> GetOpportunity(Guid id, CancellationToken cancellationToken)
    {
        var entity = await opportunities.GetByIdAsync(id, cancellationToken); return entity is null ? NotFound() : Ok(ToDto(entity));
    }

    [HttpPut("opportunities/{id:guid}")]
    public async Task<IActionResult> UpdateOpportunity(Guid id, OpportunityRequest request, CancellationToken cancellationToken)
    {
        var entity = await opportunities.GetByIdAsync(id, cancellationToken); if (entity is null) return NotFound();
        if (!await customers.Query().AnyAsync(c => c.Id == request.CustomerId, cancellationToken) || request.AssignedEmployeeId is not null && !await employees.Query().AnyAsync(e => e.Id == request.AssignedEmployeeId, cancellationToken)) return BadRequest("Customer or assigned employee does not exist.");
        entity.CustomerId = request.CustomerId; entity.AssignedEmployeeId = request.AssignedEmployeeId; entity.Name = request.Name.Trim(); entity.Description = request.Description; entity.EstimatedValue = request.EstimatedValue; entity.Status = request.Status; entity.ExpectedCloseDate = request.ExpectedCloseDate; entity.UpdatedAt = DateTime.UtcNow;
        await opportunities.SaveChangesAsync(cancellationToken); return NoContent();
    }

    [HttpDelete("opportunities/{id:guid}")]
    public async Task<IActionResult> DeleteOpportunity(Guid id, CancellationToken cancellationToken) => await Delete(opportunities, id, cancellationToken);

    [HttpGet("proposals")]
    public async Task<ActionResult<IEnumerable<ProposalDto>>> GetProposals(CancellationToken cancellationToken) =>
        Ok(await proposals.Query().AsNoTracking().OrderByDescending(p => p.CreatedAt).Select(p => new ProposalDto(p.Id, p.OpportunityId, p.CustomerId, p.CreatedByEmployeeId, p.ProposalNumber, p.Status, p.SubTotal, p.DiscountPercentage, p.DiscountAmount, p.TotalAmount, p.ValidUntil, p.VoucherDiscountAmount, p.VoucherCode, p.Revision)).ToListAsync(cancellationToken));

    [HttpPost("proposals")]
    public async Task<ActionResult<ProposalDto>> CreateProposal(ProposalRequest request, CancellationToken cancellationToken)
    {
        if (!await opportunities.Query().AnyAsync(o => o.Id == request.OpportunityId && o.CustomerId == request.CustomerId, cancellationToken) || !await employees.Query().AnyAsync(e => e.Id == request.CreatedByEmployeeId, cancellationToken)) return BadRequest("Opportunity, customer, or employee relationship is invalid.");
        var pricing = await CalculateAsync(request.Products, request.VoucherCode, cancellationToken);
        if (pricing.Error is not null) return BadRequest(pricing.Error);
        var entity = new Proposal { Id = Guid.NewGuid(), OpportunityId = request.OpportunityId, CustomerId = request.CustomerId, CreatedByEmployeeId = request.CreatedByEmployeeId, ProposalNumber = request.ProposalNumber.Trim(), Status = request.Status, SubTotal = pricing.Result!.Subtotal, DiscountPercentage = pricing.Result.Subtotal == 0 ? 0 : pricing.Result.LineDiscountAmount / pricing.Result.Subtotal * 100m, DiscountAmount = pricing.Result.LineDiscountAmount, VoucherDiscountAmount = pricing.Result.VoucherDiscountAmount, VoucherCode = request.VoucherCode?.Trim(), TotalAmount = pricing.Result.TotalAmount, ValidUntil = request.ValidUntil };
        entity.ProposalProducts = pricing.Result.Lines!.Select(line => new ProposalProduct { Id = Guid.NewGuid(), ProposalId = entity.Id, ProductId = line.ProductId, Quantity = line.Quantity, UnitPrice = line.UnitPrice, DiscountType = line.DiscountType, DiscountValue = line.DiscountValue, DiscountPercentage = line.DiscountType == DiscountType.Percentage ? line.DiscountValue : 0m, DiscountAmount = line.DiscountAmount, TotalPrice = line.TotalAmount }).ToList();
        await proposals.AddAsync(entity, cancellationToken); await proposals.SaveChangesAsync(cancellationToken);
        await workflowExecutionService.TriggerAsync("Proposal Created", "Proposal", entity.Id, null, cancellationToken);
        return CreatedAtAction(nameof(GetProposal), new { id = entity.Id }, ToDto(entity));
    }

    [HttpGet("proposals/{id:guid}")]
    public async Task<ActionResult<ProposalDto>> GetProposal(Guid id, CancellationToken cancellationToken)
    {
        var entity = await proposals.GetByIdAsync(id, cancellationToken); return entity is null ? NotFound() : Ok(ToDto(entity));
    }

    [HttpPut("proposals/{id:guid}")]
    public async Task<IActionResult> UpdateProposal(Guid id, ProposalRequest request, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Proposals.Include(item => item.ProposalProducts).SingleOrDefaultAsync(item => item.Id == id, cancellationToken); if (entity is null) return NotFound();
        if (!await opportunities.Query().AnyAsync(o => o.Id == request.OpportunityId && o.CustomerId == request.CustomerId, cancellationToken) || !await employees.Query().AnyAsync(e => e.Id == request.CreatedByEmployeeId, cancellationToken)) return BadRequest("Opportunity, customer, or employee relationship is invalid.");
        var pricing = await CalculateAsync(request.Products, request.VoucherCode, cancellationToken);
        if (pricing.Error is not null) return BadRequest(pricing.Error);
        entity.OpportunityId = request.OpportunityId; entity.CustomerId = request.CustomerId; entity.CreatedByEmployeeId = request.CreatedByEmployeeId; entity.ProposalNumber = request.ProposalNumber.Trim(); entity.Status = request.Status; entity.SubTotal = pricing.Result!.Subtotal; entity.DiscountPercentage = pricing.Result.Subtotal == 0 ? 0 : pricing.Result.LineDiscountAmount / pricing.Result.Subtotal * 100m; entity.DiscountAmount = pricing.Result.LineDiscountAmount; entity.VoucherDiscountAmount = pricing.Result.VoucherDiscountAmount; entity.VoucherCode = request.VoucherCode?.Trim(); entity.TotalAmount = pricing.Result.TotalAmount; entity.ValidUntil = request.ValidUntil; entity.UpdatedAt = DateTime.UtcNow;
        dbContext.ProposalProducts.RemoveRange(entity.ProposalProducts);
        dbContext.ProposalProducts.AddRange(pricing.Result.Lines!.Select(line => new ProposalProduct { Id = Guid.NewGuid(), ProposalId = entity.Id, ProductId = line.ProductId, Quantity = line.Quantity, UnitPrice = line.UnitPrice, DiscountType = line.DiscountType, DiscountValue = line.DiscountValue, DiscountPercentage = line.DiscountType == DiscountType.Percentage ? line.DiscountValue : 0m, DiscountAmount = line.DiscountAmount, TotalPrice = line.TotalAmount }));
        await proposals.SaveChangesAsync(cancellationToken); return NoContent();
    }

    [HttpDelete("proposals/{id:guid}")]
    public async Task<IActionResult> DeleteProposal(Guid id, CancellationToken cancellationToken) => await Delete(proposals, id, cancellationToken);

    [HttpPost("proposals/{id:guid}/correct")]
    public async Task<ActionResult<ProposalDto>> CorrectProposal(Guid id, CorrectProposalRequest request, CancellationToken cancellationToken)
    {
        var proposal = await dbContext.Proposals.SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (proposal is null) return NotFound();
        if (proposal.Status is not (ProposalStatus.Rejected or ProposalStatus.CustomerRejected)) return Conflict("Only rejected proposals can be corrected.");
        dbContext.ProposalRevisions.Add(new ProposalRevision { Id = Guid.NewGuid(), ProposalId = proposal.Id, Revision = proposal.Revision, CorrectionReason = request.Reason, SubTotal = proposal.SubTotal, DiscountAmount = proposal.DiscountAmount, VoucherDiscountAmount = proposal.VoucherDiscountAmount, TotalAmount = proposal.TotalAmount });
        proposal.Revision++;
        proposal.CorrectionReason = request.Reason?.Trim();
        proposal.Status = ProposalStatus.Draft;
        proposal.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(ToDto(proposal));
    }

    [HttpGet("vouchers")]
    public async Task<ActionResult<IEnumerable<VoucherDto>>> GetVouchers(CancellationToken cancellationToken) =>
        Ok(await dbContext.Vouchers.AsNoTracking().OrderBy(voucher => voucher.Code).Select(voucher => new VoucherDto(voucher.Id, voucher.Code, voucher.DiscountType, voucher.DiscountValue, voucher.IsActive, voucher.ValidFrom, voucher.ValidUntil, voucher.MinimumAmount, voucher.Stackable)).ToListAsync(cancellationToken));

    [HttpPost("vouchers")]
    public async Task<ActionResult<VoucherDto>> CreateVoucher(VoucherRequest request, CancellationToken cancellationToken)
    {
        var code = request.Code.Trim().ToUpperInvariant();
        if (await dbContext.Vouchers.AnyAsync(voucher => voucher.Code == code, cancellationToken)) return Conflict("Voucher code already exists.");
        var voucher = new Voucher { Id = Guid.NewGuid(), Code = code, DiscountType = request.DiscountType, DiscountValue = request.DiscountValue, IsActive = request.IsActive, ValidFrom = request.ValidFrom, ValidUntil = request.ValidUntil, MinimumAmount = request.MinimumAmount, Stackable = request.Stackable };
        dbContext.Vouchers.Add(voucher);
        await dbContext.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetVouchers), new { id = voucher.Id }, ToDto(voucher));
    }

    [HttpGet("vouchers/{code}/validate")]
    public async Task<ActionResult<VoucherDto>> ValidateVoucher(string code, decimal amount, CancellationToken cancellationToken)
    {
        var voucher = await dbContext.Vouchers.AsNoTracking().SingleOrDefaultAsync(item => item.Code == code.Trim().ToUpper(), cancellationToken);
        var validation = voucherService.Validate(voucher, amount, DateTime.UtcNow);
        return validation.IsValid ? Ok(ToDto(voucher!)) : BadRequest(validation.Error);
    }

    [HttpGet("proposals/{proposalId:guid}/products")]
    public async Task<ActionResult<IEnumerable<ProposalProductDto>>> GetProposalProducts(Guid proposalId, CancellationToken cancellationToken) =>
        Ok(await proposalProducts.Query().AsNoTracking().Where(p => p.ProposalId == proposalId).Select(p => new ProposalProductDto(p.Id, p.ProposalId, p.ProductId, p.Quantity, p.UnitPrice, p.DiscountPercentage, p.DiscountAmount, p.TotalPrice)).ToListAsync(cancellationToken));

    [HttpPost("proposals/{proposalId:guid}/products")]
    public async Task<ActionResult<ProposalProductDto>> AddProposalProduct(Guid proposalId, ProposalProductRequest request, CancellationToken cancellationToken)
    {
        if (!await proposals.Query().AnyAsync(p => p.Id == proposalId, cancellationToken) || !await products.Query().AnyAsync(p => p.Id == request.ProductId && p.IsActive, cancellationToken)) return BadRequest("Proposal or active product does not exist.");
        var product = await products.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null || !product.IsActive) return BadRequest("Proposal or active product does not exist.");
        var line = pricingService.Calculate([new PricingLine(request.ProductId, request.Quantity, product.Price, request.DiscountType, request.DiscountType == DiscountType.Percentage ? request.DiscountValue : request.DiscountAmount)]).Lines![0];
        var entity = new ProposalProduct { Id = Guid.NewGuid(), ProposalId = proposalId, ProductId = request.ProductId, Quantity = line.Quantity, UnitPrice = line.UnitPrice, DiscountType = line.DiscountType, DiscountValue = line.DiscountValue, DiscountPercentage = line.DiscountType == DiscountType.Percentage ? line.DiscountValue : 0m, DiscountAmount = line.DiscountAmount, TotalPrice = line.TotalAmount };
        await proposalProducts.AddAsync(entity, cancellationToken); await proposalProducts.SaveChangesAsync(cancellationToken); return Ok(ToDto(entity));
    }

    [HttpPut("proposals/{proposalId:guid}/products/{id:guid}")]
    public async Task<IActionResult> UpdateProposalProduct(Guid proposalId, Guid id, ProposalProductRequest request, CancellationToken cancellationToken)
    {
        var entity = await proposalProducts.GetByIdAsync(id, cancellationToken); if (entity is null || entity.ProposalId != proposalId) return NotFound();
        entity.ProductId = request.ProductId; entity.Quantity = request.Quantity; entity.UnitPrice = request.UnitPrice; entity.DiscountPercentage = request.DiscountPercentage; entity.DiscountAmount = request.DiscountAmount; entity.TotalPrice = request.TotalPrice; entity.UpdatedAt = DateTime.UtcNow;
        await proposalProducts.SaveChangesAsync(cancellationToken); return NoContent();
    }

    [HttpDelete("proposals/{proposalId:guid}/products/{id:guid}")]
    public async Task<IActionResult> DeleteProposalProduct(Guid proposalId, Guid id, CancellationToken cancellationToken) => await DeleteLine(proposalProducts, id, proposalId, cancellationToken);

    [HttpGet("orders")]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrders(CancellationToken cancellationToken) =>
        Ok(await orders.Query().AsNoTracking().OrderByDescending(o => o.OrderDate).Select(o => new OrderDto(o.Id, o.ProposalId, o.CustomerId, o.AssignedEmployeeId, o.OrderNumber, o.Status, o.TotalAmount, o.OrderDate)).ToListAsync(cancellationToken));

    [HttpPost("orders")]
    public async Task<ActionResult<OrderDto>> CreateOrder(OrderRequest request, CancellationToken cancellationToken)
    {
        if (!await proposals.Query().AnyAsync(p => p.Id == request.ProposalId && p.CustomerId == request.CustomerId, cancellationToken) || !await customers.Query().AnyAsync(c => c.Id == request.CustomerId, cancellationToken)) return BadRequest("Proposal and customer relationship is invalid.");
        var proposal = await dbContext.Proposals.Include(item => item.ProposalProducts).SingleAsync(item => item.Id == request.ProposalId, cancellationToken);
        var pricing = await CalculateProposalAsync(proposal, cancellationToken);
        if (pricing.Error is not null) return BadRequest(pricing.Error);
        proposal.SubTotal = pricing.Result!.Subtotal; proposal.DiscountAmount = pricing.Result.LineDiscountAmount; proposal.VoucherDiscountAmount = pricing.Result.VoucherDiscountAmount; proposal.TotalAmount = pricing.Result.TotalAmount;
        var entity = new Order { Id = Guid.NewGuid(), ProposalId = request.ProposalId, CustomerId = request.CustomerId, AssignedEmployeeId = request.AssignedEmployeeId, OrderNumber = request.OrderNumber.Trim(), Status = request.Status, TotalAmount = pricing.Result.TotalAmount, DiscountAmount = pricing.Result.LineDiscountAmount + pricing.Result.VoucherDiscountAmount, OrderDate = request.OrderDate };
        entity.OrderProducts = pricing.Result.Lines!.Select(line => new OrderProduct { Id = Guid.NewGuid(), OrderId = entity.Id, ProductId = line.ProductId, Quantity = line.Quantity, UnitPrice = line.UnitPrice, Discount = line.DiscountAmount, TotalPrice = line.TotalAmount }).ToList();
        await orders.AddAsync(entity, cancellationToken); await orders.SaveChangesAsync(cancellationToken); return CreatedAtAction(nameof(GetOrder), new { id = entity.Id }, ToDto(entity));
    }

    [HttpGet("orders/{id:guid}")]
    public async Task<ActionResult<OrderDto>> GetOrder(Guid id, CancellationToken cancellationToken)
    {
        var entity = await orders.GetByIdAsync(id, cancellationToken); return entity is null ? NotFound() : Ok(ToDto(entity));
    }

    [HttpPut("orders/{id:guid}")]
    public async Task<IActionResult> UpdateOrder(Guid id, OrderRequest request, CancellationToken cancellationToken)
    {
        var entity = await orders.GetByIdAsync(id, cancellationToken); if (entity is null) return NotFound();
        if (!await proposals.Query().AnyAsync(p => p.Id == request.ProposalId && p.CustomerId == request.CustomerId, cancellationToken)) return BadRequest("Proposal and customer relationship is invalid.");
        entity.ProposalId = request.ProposalId; entity.CustomerId = request.CustomerId; entity.AssignedEmployeeId = request.AssignedEmployeeId; entity.OrderNumber = request.OrderNumber.Trim(); entity.Status = request.Status; entity.TotalAmount = request.TotalAmount; entity.OrderDate = request.OrderDate; entity.UpdatedAt = DateTime.UtcNow;
        await orders.SaveChangesAsync(cancellationToken); return NoContent();
    }

    [HttpDelete("orders/{id:guid}")]
    public async Task<IActionResult> DeleteOrder(Guid id, CancellationToken cancellationToken) => await Delete(orders, id, cancellationToken);

    [HttpGet("orders/{orderId:guid}/products")]
    public async Task<ActionResult<IEnumerable<OrderProductDto>>> GetOrderProducts(Guid orderId, CancellationToken cancellationToken) =>
        Ok(await orderProducts.Query().AsNoTracking().Where(p => p.OrderId == orderId).Select(p => new OrderProductDto(p.Id, p.OrderId, p.ProductId, p.Quantity, p.UnitPrice, p.Discount, p.TotalPrice)).ToListAsync(cancellationToken));

    [HttpGet("subscriptions")]
    public async Task<ActionResult<IEnumerable<SubscriptionDto>>> GetSubscriptions(CancellationToken cancellationToken) =>
        Ok(await dbContext.Subscriptions.AsNoTracking().OrderByDescending(s => s.StartDate).Select(s => new SubscriptionDto(s.Id, s.CustomerId, s.ProductId, s.StartDate, s.EndDate, s.Amount, s.Status)).ToListAsync(cancellationToken));

    [HttpGet("activities")]
    public async Task<ActionResult<IEnumerable<EngagementActivityDto>>> GetActivities(Guid? customerId, Guid? opportunityId, CancellationToken cancellationToken) =>
        Ok(await dbContext.EngagementActivities.AsNoTracking()
            .Where(activity => (!customerId.HasValue || activity.CustomerId == customerId) && (!opportunityId.HasValue || activity.OpportunityId == opportunityId))
            .OrderByDescending(activity => activity.ActivityDate)
            .Select(activity => new EngagementActivityDto(activity.Id, activity.CustomerId, activity.EmployeeId, activity.OpportunityId, activity.ProposalId, activity.Type, activity.Subject, activity.Description, activity.ActivityDate))
            .ToListAsync(cancellationToken));

    [HttpPost("activities")]
    public async Task<ActionResult<EngagementActivityDto>> CreateActivity(EngagementActivityRequest request, CancellationToken cancellationToken)
    {
        if (!await EntityReferencesExist(request.CustomerId, request.EmployeeId, request.OpportunityId, request.ProposalId, cancellationToken)) return BadRequest("One or more CRM references do not exist.");
        var activity = new EngagementActivity { Id = Guid.NewGuid(), CustomerId = request.CustomerId, EmployeeId = request.EmployeeId, OpportunityId = request.OpportunityId, ProposalId = request.ProposalId, Type = request.Type.Trim(), Subject = request.Subject?.Trim(), Description = request.Description?.Trim(), ActivityDate = request.ActivityDate };
        dbContext.EngagementActivities.Add(activity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetActivities), new { customerId = activity.CustomerId }, ToDto(activity));
    }

    [HttpGet("conversations")]
    public async Task<ActionResult<IEnumerable<ConversationDto>>> GetConversations(Guid? customerId, Guid? opportunityId, CancellationToken cancellationToken) =>
        Ok(await dbContext.Conversations.AsNoTracking()
            .Where(conversation => (!customerId.HasValue || conversation.CustomerId == customerId) && (!opportunityId.HasValue || conversation.OpportunityId == opportunityId))
            .OrderByDescending(conversation => conversation.CreatedAt)
            .Select(conversation => new ConversationDto(conversation.Id, conversation.CustomerId, conversation.EmployeeId, conversation.OpportunityId, conversation.Message, conversation.Channel, conversation.CreatedAt))
            .ToListAsync(cancellationToken));

    [HttpPost("conversations")]
    public async Task<ActionResult<ConversationDto>> CreateConversation(ConversationRequest request, CancellationToken cancellationToken)
    {
        if (!await EntityReferencesExist(request.CustomerId, request.EmployeeId, request.OpportunityId, null, cancellationToken)) return BadRequest("One or more CRM references do not exist.");
        var conversation = new Conversation { Id = Guid.NewGuid(), CustomerId = request.CustomerId, EmployeeId = request.EmployeeId, OpportunityId = request.OpportunityId, Message = request.Message.Trim(), Channel = request.Channel.Trim() };
        dbContext.Conversations.Add(conversation);
        await dbContext.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetConversations), new { customerId = conversation.CustomerId }, ToDto(conversation));
    }

    [HttpPost("orders/{orderId:guid}/products")]
    public async Task<ActionResult<OrderProductDto>> AddOrderProduct(Guid orderId, OrderProductRequest request, CancellationToken cancellationToken)
    {
        if (!await orders.Query().AnyAsync(o => o.Id == orderId, cancellationToken) || !await products.Query().AnyAsync(p => p.Id == request.ProductId && p.IsActive, cancellationToken)) return BadRequest("Order or active product does not exist.");
        var entity = new OrderProduct { Id = Guid.NewGuid(), OrderId = orderId, ProductId = request.ProductId, Quantity = request.Quantity, UnitPrice = request.UnitPrice, Discount = request.Discount, TotalPrice = request.TotalPrice };
        await orderProducts.AddAsync(entity, cancellationToken); await orderProducts.SaveChangesAsync(cancellationToken); return Ok(ToDto(entity));
    }

    [HttpPut("orders/{orderId:guid}/products/{id:guid}")]
    public async Task<IActionResult> UpdateOrderProduct(Guid orderId, Guid id, OrderProductRequest request, CancellationToken cancellationToken)
    {
        var entity = await orderProducts.GetByIdAsync(id, cancellationToken); if (entity is null || entity.OrderId != orderId) return NotFound();
        entity.ProductId = request.ProductId; entity.Quantity = request.Quantity; entity.UnitPrice = request.UnitPrice; entity.Discount = request.Discount; entity.TotalPrice = request.TotalPrice; entity.UpdatedAt = DateTime.UtcNow;
        await orderProducts.SaveChangesAsync(cancellationToken); return NoContent();
    }

    [HttpDelete("orders/{orderId:guid}/products/{id:guid}")]
    public async Task<IActionResult> DeleteOrderProduct(Guid orderId, Guid id, CancellationToken cancellationToken) => await DeleteLine(orderProducts, id, orderId, cancellationToken);

    private async Task<(string? Error, PricingResult? Result)> CalculateAsync(IReadOnlyList<ProposalProductRequest> requests, string? voucherCode, CancellationToken cancellationToken)
    {
        var productIds = requests.Select(request => request.ProductId).Distinct().ToList();
        var productPrices = await products.Query().Where(product => product.IsActive && productIds.Contains(product.Id)).ToDictionaryAsync(product => product.Id, product => product.Price, cancellationToken);
        if (productPrices.Count != productIds.Count) return ("One or more products are missing or inactive.", null);
        var lines = requests.Select(request => new PricingLine(request.ProductId, request.Quantity, productPrices[request.ProductId], request.DiscountType, request.DiscountType == DiscountType.Percentage ? request.DiscountValue : request.DiscountAmount));
        var withoutVoucher = pricingService.Calculate(lines);
        var vouchers = new List<PricingVoucher>();
        if (!string.IsNullOrWhiteSpace(voucherCode))
        {
            var voucher = await dbContext.Vouchers.SingleOrDefaultAsync(item => item.Code == voucherCode.Trim(), cancellationToken);
            var validation = voucherService.Validate(voucher, withoutVoucher.TotalAmount, DateTime.UtcNow);
            if (!validation.IsValid) return (validation.Error, null);
            vouchers.Add(validation.Voucher!);
        }
        return (null, pricingService.Calculate(lines, vouchers));
    }

    private async Task<(string? Error, PricingResult? Result)> CalculateProposalAsync(Proposal proposal, CancellationToken cancellationToken)
    {
        var productIds = proposal.ProposalProducts.Select(line => line.ProductId).Distinct().ToList();
        var productPrices = await products.Query().Where(product => product.IsActive && productIds.Contains(product.Id)).ToDictionaryAsync(product => product.Id, product => product.Price, cancellationToken);
        if (productPrices.Count != productIds.Count) return ("One or more proposal products are missing or inactive.", null);
        var lines = proposal.ProposalProducts.Select(line => new PricingLine(line.ProductId, line.Quantity, productPrices[line.ProductId], line.DiscountType, line.DiscountValue == 0m ? line.DiscountPercentage : line.DiscountValue));
        var basePricing = pricingService.Calculate(lines);
        var vouchers = new List<PricingVoucher>();
        if (!string.IsNullOrWhiteSpace(proposal.VoucherCode))
        {
            var voucher = await dbContext.Vouchers.SingleOrDefaultAsync(item => item.Code == proposal.VoucherCode, cancellationToken);
            var validation = voucherService.Validate(voucher, basePricing.TotalAmount, DateTime.UtcNow);
            if (!validation.IsValid) return (validation.Error, null);
            vouchers.Add(validation.Voucher!);
        }
        return (null, pricingService.Calculate(lines, vouchers));
    }

    private async Task<bool> ReferencesExist(Guid? gradeId, Guid? managerId, CancellationToken cancellationToken) =>
        (gradeId is null || await grades.Query().AnyAsync(g => g.Id == gradeId, cancellationToken)) &&
        (managerId is null || await employees.Query().AnyAsync(e => e.Id == managerId, cancellationToken));

    private async Task<bool> EntityReferencesExist(Guid customerId, Guid? employeeId, Guid? opportunityId, Guid? proposalId, CancellationToken cancellationToken) =>
        await customers.Query().AnyAsync(customer => customer.Id == customerId, cancellationToken) &&
        (employeeId is null || await employees.Query().AnyAsync(employee => employee.Id == employeeId, cancellationToken)) &&
        (opportunityId is null || await opportunities.Query().AnyAsync(opportunity => opportunity.Id == opportunityId && opportunity.CustomerId == customerId, cancellationToken)) &&
        (proposalId is null || await proposals.Query().AnyAsync(proposal => proposal.Id == proposalId && proposal.CustomerId == customerId, cancellationToken));

    private static async Task<IActionResult> Delete<TEntity>(ICrmRepository<TEntity> repository, Guid id, CancellationToken cancellationToken) where TEntity : class
    {
        var entity = await repository.GetByIdAsync(id, cancellationToken); if (entity is null) return new NotFoundResult();
        repository.Remove(entity); await repository.SaveChangesAsync(cancellationToken); return new NoContentResult();
    }

    private static async Task<IActionResult> DeleteLine<TEntity>(ICrmRepository<TEntity> repository, Guid id, Guid parentId, CancellationToken cancellationToken) where TEntity : class
    {
        var entity = await repository.GetByIdAsync(id, cancellationToken); if (entity is null) return new NotFoundResult();
        var parentProperty = typeof(TEntity) == typeof(ProposalProduct) ? nameof(ProposalProduct.ProposalId) : nameof(OrderProduct.OrderId);
        var actualParentId = (Guid)typeof(TEntity).GetProperty(parentProperty)!.GetValue(entity)!;
        if (actualParentId != parentId) return new NotFoundResult();
        repository.Remove(entity); await repository.SaveChangesAsync(cancellationToken); return new NoContentResult();
    }

    private static EmployeeGradeDto ToDto(EmployeeGrade e) => new(e.Id, e.Name, e.Description, e.ApprovalLimit, e.IsActive);
    private static EmployeeDto ToDto(Employee e) => new(e.Id, e.FirstName, e.LastName, e.Email, e.Phone, e.EmployeeGradeId, e.ManagerId, e.Role, e.IsActive);
    private static CustomerDto ToDto(Customer e) => new(e.Id, e.Name, e.Email, e.Phone, e.Company, e.Address, e.Status);
    private static ProductDto ToDto(Product e) => new(e.Id, e.Name, e.Description, e.Category, e.Price, e.IsActive);
    private static OpportunityDto ToDto(Opportunity e) => new(e.Id, e.CustomerId, e.AssignedEmployeeId, e.Name, e.Description, e.EstimatedValue, e.Status, e.ExpectedCloseDate);
    private static ProposalDto ToDto(Proposal e) => new(e.Id, e.OpportunityId, e.CustomerId, e.CreatedByEmployeeId, e.ProposalNumber, e.Status, e.SubTotal, e.DiscountPercentage, e.DiscountAmount, e.TotalAmount, e.ValidUntil);
    private static VoucherDto ToDto(Voucher e) => new(e.Id, e.Code, e.DiscountType, e.DiscountValue, e.IsActive, e.ValidFrom, e.ValidUntil, e.MinimumAmount, e.Stackable);
    private static ProposalProductDto ToDto(ProposalProduct e) => new(e.Id, e.ProposalId, e.ProductId, e.Quantity, e.UnitPrice, e.DiscountPercentage, e.DiscountAmount, e.TotalPrice);
    private static OrderDto ToDto(Order e) => new(e.Id, e.ProposalId, e.CustomerId, e.AssignedEmployeeId, e.OrderNumber, e.Status, e.TotalAmount, e.OrderDate);
    private static OrderProductDto ToDto(OrderProduct e) => new(e.Id, e.OrderId, e.ProductId, e.Quantity, e.UnitPrice, e.Discount, e.TotalPrice);
    private static EngagementActivityDto ToDto(EngagementActivity e) => new(e.Id, e.CustomerId, e.EmployeeId, e.OpportunityId, e.ProposalId, e.Type, e.Subject, e.Description, e.ActivityDate);
    private static ConversationDto ToDto(Conversation e) => new(e.Id, e.CustomerId, e.EmployeeId, e.OpportunityId, e.Message, e.Channel, e.CreatedAt);
}
