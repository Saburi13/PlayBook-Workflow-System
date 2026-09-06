using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using PlayBook.Business.BusinessModels.RequestDTOs.CRMRequestDTOs;
using PlayBook.Business.BusinessModels.ResponseDTOs.CRMResponseDTOs;
using PlayBook.Business.Interfaces.IService;
using PlayBook.Data.Repositories.Interfaces;
using PlayBook.Domain;

namespace PlayBook.Business.Implementations.Service;

public sealed class CrmService(
    ICrmRepository<EmployeeGrade> grades,
    ICrmRepository<Employee> employees,
    ICrmRepository<Customer> customers,
    ICrmRepository<Product> products,
    ICrmRepository<Opportunity> opportunities,
    ICrmRepository<Proposal> proposals,
    ICrmRepository<ProposalProduct> proposalProducts,
    ICrmRepository<Order> orders,
    ICrmRepository<OrderProduct> orderProducts,
    ICrmDataRepository crmDataRepository,
    IWorkflowExecutionService workflowExecutionService,
    IPricingService pricingService,
    VoucherService voucherService) : ICrmService
{
    // ---------------------------------------------------------
    // EMPLOYEE GRADES
    // ---------------------------------------------------------

    public async Task<IReadOnlyList<EmployeeGradeDto>> GetGradesAsync(
        CancellationToken cancellationToken = default)
    {
        return await grades.Query()
            .AsNoTracking()
            .OrderBy(grade => grade.Name)
            .Select(grade => new EmployeeGradeDto(
                grade.Id,
                grade.Name,
                grade.Description,
                grade.ApprovalLimit,
                grade.IsActive))
            .ToListAsync(cancellationToken);
    }

    public async Task<EmployeeGradeDto?> GetGradeAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await grades.GetByIdAsync(id, cancellationToken);

        return entity is null ? null : ToDto(entity);
    }

    public async Task<EmployeeGradeDto> CreateGradeAsync(
        EmployeeGradeRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = new EmployeeGrade
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description,
            ApprovalLimit = request.ApprovalLimit,
            IsActive = request.IsActive
        };

        await grades.AddAsync(entity, cancellationToken);
        await grades.SaveChangesAsync(cancellationToken);

        return ToDto(entity);
    }

    public async Task<bool> UpdateGradeAsync(
        Guid id,
        EmployeeGradeRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await grades.GetByIdAsync(id, cancellationToken);

        if (entity is null)
        {
            return false;
        }

        entity.Name = request.Name.Trim();
        entity.Description = request.Description;
        entity.ApprovalLimit = request.ApprovalLimit;
        entity.IsActive = request.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await grades.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> DeleteGradeAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await DeleteEntityAsync(
            grades,
            id,
            cancellationToken);
    }

    // ---------------------------------------------------------
    // EMPLOYEES
    // ---------------------------------------------------------

    public async Task<IReadOnlyList<EmployeeDto>> GetEmployeesAsync(
        CancellationToken cancellationToken = default)
    {
        return await employees.Query()
            .AsNoTracking()
            .OrderBy(employee => employee.LastName)
            .ThenBy(employee => employee.FirstName)
            .Select(employee => new EmployeeDto(
                employee.Id,
                employee.FirstName,
                employee.LastName,
                employee.Email,
                employee.Phone,
                employee.EmployeeGradeId,
                employee.ManagerId,
                employee.Role,
                employee.IsActive))
            .ToListAsync(cancellationToken);
    }

    public async Task<EmployeeDto?> GetEmployeeAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await employees.GetByIdAsync(id, cancellationToken);

        return entity is null ? null : ToDto(entity);
    }

    public async Task<EmployeeDto> CreateEmployeeAsync(
        EmployeeRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!await ReferencesExist(
                request.EmployeeGradeId,
                request.ManagerId,
                cancellationToken))
        {
            throw new ArgumentException(
                "Employee grade or manager does not exist.");
        }

        var entity = new Employee
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            Phone = request.Phone,
            EmployeeGradeId = request.EmployeeGradeId,
            ManagerId = request.ManagerId,
            Role = request.Role,
            IsActive = request.IsActive
        };

        await employees.AddAsync(entity, cancellationToken);
        await employees.SaveChangesAsync(cancellationToken);

        return ToDto(entity);
    }

    public async Task<bool> UpdateEmployeeAsync(
        Guid id,
        EmployeeRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await employees.GetByIdAsync(id, cancellationToken);

        if (entity is null)
        {
            return false;
        }

        if (request.ManagerId == id ||
            !await ReferencesExist(
                request.EmployeeGradeId,
                request.ManagerId,
                cancellationToken))
        {
            throw new ArgumentException(
                "Employee grade or manager is invalid.");
        }

        entity.FirstName = request.FirstName.Trim();
        entity.LastName = request.LastName.Trim();
        entity.Email = request.Email.Trim().ToLowerInvariant();
        entity.Phone = request.Phone;
        entity.EmployeeGradeId = request.EmployeeGradeId;
        entity.ManagerId = request.ManagerId;
        entity.Role = request.Role;
        entity.IsActive = request.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await employees.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> DeleteEmployeeAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await DeleteEntityAsync(
            employees,
            id,
            cancellationToken);
    }

    // ---------------------------------------------------------
    // CUSTOMERS
    // ---------------------------------------------------------

    public async Task<IReadOnlyList<CustomerDto>> GetCustomersAsync(
        CancellationToken cancellationToken = default)
    {
        return await customers.Query()
            .AsNoTracking()
            .OrderBy(customer => customer.Name)
            .Select(customer => new CustomerDto(
                customer.Id,
                customer.Name,
                customer.Email,
                customer.Phone,
                customer.Company,
                customer.Address,
                customer.Status))
            .ToListAsync(cancellationToken);
    }

    public async Task<CustomerDto?> GetCustomerAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await customers.GetByIdAsync(id, cancellationToken);

        return entity is null ? null : ToDto(entity);
    }

    public async Task<CustomerDto> CreateCustomerAsync(
        CustomerRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = new Customer
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Email = request.Email,
            Phone = request.Phone,
            Company = request.Company,
            Address = request.Address,
            Status = request.Status
        };

        await customers.AddAsync(entity, cancellationToken);
        await customers.SaveChangesAsync(cancellationToken);

        return ToDto(entity);
    }

    public async Task<bool> UpdateCustomerAsync(
        Guid id,
        CustomerRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await customers.GetByIdAsync(id, cancellationToken);

        if (entity is null)
        {
            return false;
        }

        entity.Name = request.Name.Trim();
        entity.Email = request.Email;
        entity.Phone = request.Phone;
        entity.Company = request.Company;
        entity.Address = request.Address;
        entity.Status = request.Status;
        entity.UpdatedAt = DateTime.UtcNow;

        await customers.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> DeleteCustomerAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await DeleteEntityAsync(
            customers,
            id,
            cancellationToken);
    }

    // ---------------------------------------------------------
    // PRODUCTS
    // ---------------------------------------------------------

    public async Task<IReadOnlyList<ProductDto>> GetProductsAsync(
        CancellationToken cancellationToken = default)
    {
        return await products.Query()
            .AsNoTracking()
            .OrderBy(product => product.Name)
            .Select(product => new ProductDto(
                product.Id,
                product.Name,
                product.Description,
                product.Category,
                product.Price,
                product.IsActive,
                product.PlanDurationMonths))
            .ToListAsync(cancellationToken);
    }

    public async Task<ProductDto?> GetProductAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await products.GetByIdAsync(id, cancellationToken);

        return entity is null ? null : ToDto(entity);
    }

    public async Task<ProductDto> CreateProductAsync(
        ProductRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = new Product
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description,
            Category = request.Category,
            Price = request.Price,
            PlanDurationMonths = request.PlanDurationMonths,
            IsActive = request.IsActive
        };

        await products.AddAsync(entity, cancellationToken);
        await products.SaveChangesAsync(cancellationToken);

        return ToDto(entity);
    }

    public async Task<bool> UpdateProductAsync(
        Guid id,
        ProductRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await products.GetByIdAsync(id, cancellationToken);

        if (entity is null)
        {
            return false;
        }

        entity.Name = request.Name.Trim();
        entity.Description = request.Description;
        entity.Category = request.Category;
        entity.Price = request.Price;
        entity.PlanDurationMonths = request.PlanDurationMonths;
        entity.IsActive = request.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await products.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> DeleteProductAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await DeleteEntityAsync(
            products,
            id,
            cancellationToken);
    }

    // ---------------------------------------------------------
    // OPPORTUNITIES
    // ---------------------------------------------------------

    public async Task<IReadOnlyList<OpportunityDto>> GetOpportunitiesAsync(
        CancellationToken cancellationToken = default)
    {
        return await opportunities.Query()
            .AsNoTracking()
            .OrderByDescending(opportunity => opportunity.CreatedAt)
            .Select(opportunity => new OpportunityDto(
                opportunity.Id,
                opportunity.CustomerId,
                opportunity.AssignedEmployeeId,
                opportunity.Name,
                opportunity.Description,
                opportunity.EstimatedValue,
                opportunity.Status,
                opportunity.ExpectedCloseDate))
            .ToListAsync(cancellationToken);
    }

    public async Task<OpportunityDto?> GetOpportunityAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await opportunities.GetByIdAsync(
            id,
            cancellationToken);

        return entity is null ? null : ToDto(entity);
    }

    public async Task<OpportunityDto> CreateOpportunityAsync(
        OpportunityRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!await crmDataRepository.CustomerExistsAsync(
                request.CustomerId,
                cancellationToken) ||
            request.AssignedEmployeeId is not null &&
            !await crmDataRepository.EmployeeExistsAsync(
                request.AssignedEmployeeId.Value,
                cancellationToken))
        {
            throw new ArgumentException(
                "Customer or assigned employee does not exist.");
        }

        var entity = new Opportunity
        {
            Id = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            AssignedEmployeeId = request.AssignedEmployeeId,
            Name = request.Name.Trim(),
            Description = request.Description,
            EstimatedValue = request.EstimatedValue,
            Status = request.Status,
            ExpectedCloseDate = request.ExpectedCloseDate
        };

        await opportunities.AddAsync(entity, cancellationToken);
        await opportunities.SaveChangesAsync(cancellationToken);

        await workflowExecutionService.TriggerAsync(
            "Opportunity Created",
            "Opportunity",
            entity.Id,
            JsonSerializer.SerializeToElement(
                new { discountPercentage = 10m }),
            cancellationToken);

        return ToDto(entity);
    }

    public async Task<bool> UpdateOpportunityAsync(
        Guid id,
        OpportunityRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await opportunities.GetByIdAsync(
            id,
            cancellationToken);

        if (entity is null)
        {
            return false;
        }

        if (!await crmDataRepository.CustomerExistsAsync(
                request.CustomerId,
                cancellationToken) ||
            request.AssignedEmployeeId is not null &&
            !await crmDataRepository.EmployeeExistsAsync(
                request.AssignedEmployeeId.Value,
                cancellationToken))
        {
            throw new ArgumentException(
                "Customer or assigned employee does not exist.");
        }

        entity.CustomerId = request.CustomerId;
        entity.AssignedEmployeeId = request.AssignedEmployeeId;
        entity.Name = request.Name.Trim();
        entity.Description = request.Description;
        entity.EstimatedValue = request.EstimatedValue;
        entity.Status = request.Status;
        entity.ExpectedCloseDate = request.ExpectedCloseDate;
        entity.UpdatedAt = DateTime.UtcNow;

        await opportunities.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> DeleteOpportunityAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await DeleteEntityAsync(
            opportunities,
            id,
            cancellationToken);
    }

    // ---------------------------------------------------------
    // PROPOSALS
    // ---------------------------------------------------------

    public async Task<IReadOnlyList<ProposalDto>> GetProposalsAsync(
        CancellationToken cancellationToken = default)
    {
        return await proposals.Query()
            .AsNoTracking()
            .OrderByDescending(proposal => proposal.CreatedAt)
            .Select(proposal => new ProposalDto(
                proposal.Id,
                proposal.OpportunityId,
                proposal.CustomerId,
                proposal.CreatedByEmployeeId,
                proposal.ProposalNumber,
                proposal.Status,
                proposal.SubTotal,
                proposal.DiscountPercentage,
                proposal.DiscountAmount,
                proposal.TotalAmount,
                proposal.ValidUntil,
                proposal.VoucherDiscountAmount,
                proposal.VoucherCode,
                proposal.Revision))
            .ToListAsync(cancellationToken);
    }

    public async Task<ProposalDto?> GetProposalAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await proposals.GetByIdAsync(
            id,
            cancellationToken);

        return entity is null ? null : ToDto(entity);
    }

    public async Task<ProposalDto> CreateProposalAsync(
        ProposalRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!await crmDataRepository.CustomerExistsForOpportunityAsync(
                request.OpportunityId,
                request.CustomerId,
                cancellationToken) ||
            !await crmDataRepository.EmployeeExistsAsync(
                request.CreatedByEmployeeId,
                cancellationToken))
        {
            throw new ArgumentException(
                "Opportunity, customer, or employee relationship is invalid.");
        }

        var pricing = await CalculateAsync(
            request.Products,
            request.VoucherCode,
            cancellationToken);

        if (pricing.Error is not null)
        {
            throw new ArgumentException(pricing.Error);
        }

        var result = pricing.Result!;

        var entity = new Proposal
        {
            Id = Guid.NewGuid(),
            OpportunityId = request.OpportunityId,
            CustomerId = request.CustomerId,
            CreatedByEmployeeId = request.CreatedByEmployeeId,
            ProposalNumber = request.ProposalNumber.Trim(),
            Status = request.Status,
            SubTotal = result.Subtotal,
            DiscountPercentage = result.Subtotal == 0
                ? 0
                : result.LineDiscountAmount / result.Subtotal * 100m,
            DiscountAmount = result.LineDiscountAmount,
            VoucherDiscountAmount = result.VoucherDiscountAmount,
            VoucherCode = request.VoucherCode?.Trim(),
            TotalAmount = result.TotalAmount,
            ValidUntil = request.ValidUntil
        };

        entity.ProposalProducts = result.Lines!
            .Select(line => new ProposalProduct
            {
                Id = Guid.NewGuid(),
                ProposalId = entity.Id,
                ProductId = line.ProductId,
                Quantity = line.Quantity,
                UnitPrice = line.UnitPrice,
                DiscountType = line.DiscountType,
                DiscountValue = line.DiscountValue,
                DiscountPercentage =
                    line.DiscountType == DiscountType.Percentage
                        ? line.DiscountValue
                        : 0m,
                DiscountAmount = line.DiscountAmount,
                TotalPrice = line.TotalAmount
            })
            .ToList();

        await proposals.AddAsync(entity, cancellationToken);
        await proposals.SaveChangesAsync(cancellationToken);

        await workflowExecutionService.TriggerAsync(
            "Proposal Created",
            "Proposal",
            entity.Id,
            null,
            cancellationToken);

        return ToDto(entity);
    }

    public async Task<bool> UpdateProposalAsync(
        Guid id,
        ProposalRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await crmDataRepository.GetProposalWithProductsAsync(
            id,
            cancellationToken);

        if (entity is null)
        {
            return false;
        }

        if (!await crmDataRepository.CustomerExistsForOpportunityAsync(
                request.OpportunityId,
                request.CustomerId,
                cancellationToken) ||
            !await crmDataRepository.EmployeeExistsAsync(
                request.CreatedByEmployeeId,
                cancellationToken))
        {
            throw new ArgumentException(
                "Opportunity, customer, or employee relationship is invalid.");
        }

        var pricing = await CalculateAsync(
            request.Products,
            request.VoucherCode,
            cancellationToken);

        if (pricing.Error is not null)
        {
            throw new ArgumentException(pricing.Error);
        }

        var result = pricing.Result!;

        entity.OpportunityId = request.OpportunityId;
        entity.CustomerId = request.CustomerId;
        entity.CreatedByEmployeeId = request.CreatedByEmployeeId;
        entity.ProposalNumber = request.ProposalNumber.Trim();
        entity.Status = request.Status;
        entity.SubTotal = result.Subtotal;
        entity.DiscountPercentage = result.Subtotal == 0
            ? 0
            : result.LineDiscountAmount / result.Subtotal * 100m;
        entity.DiscountAmount = result.LineDiscountAmount;
        entity.VoucherDiscountAmount = result.VoucherDiscountAmount;
        entity.VoucherCode = request.VoucherCode?.Trim();
        entity.TotalAmount = result.TotalAmount;
        entity.ValidUntil = request.ValidUntil;
        entity.UpdatedAt = DateTime.UtcNow;

        foreach (var existingLine in entity.ProposalProducts.ToList())
        {
            proposalProducts.Remove(existingLine);
        }

        foreach (var line in result.Lines!)
        {
            await proposalProducts.AddAsync(
                new ProposalProduct
                {
                    Id = Guid.NewGuid(),
                    ProposalId = entity.Id,
                    ProductId = line.ProductId,
                    Quantity = line.Quantity,
                    UnitPrice = line.UnitPrice,
                    DiscountType = line.DiscountType,
                    DiscountValue = line.DiscountValue,
                    DiscountPercentage =
                        line.DiscountType == DiscountType.Percentage
                            ? line.DiscountValue
                            : 0m,
                    DiscountAmount = line.DiscountAmount,
                    TotalPrice = line.TotalAmount
                },
                cancellationToken);
        }

        await proposals.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<ProposalDto?> CorrectProposalAsync(
        Guid id,
        CorrectProposalRequest request,
        CancellationToken cancellationToken = default)
    {
        var proposal = await crmDataRepository.GetProposalAsync(
            id,
            cancellationToken);

        if (proposal is null)
        {
            return null;
        }

        if (proposal.Status is not
            (ProposalStatus.Rejected or ProposalStatus.CustomerRejected))
        {
            throw new InvalidOperationException(
                "Only rejected proposals can be corrected.");
        }

        await crmDataRepository.AddProposalRevisionAsync(
            new ProposalRevision
            {
                Id = Guid.NewGuid(),
                ProposalId = proposal.Id,
                Revision = proposal.Revision,
                CorrectionReason = request.Reason,
                SubTotal = proposal.SubTotal,
                DiscountAmount = proposal.DiscountAmount,
                VoucherDiscountAmount = proposal.VoucherDiscountAmount,
                TotalAmount = proposal.TotalAmount
            },
            cancellationToken);

        proposal.Revision++;
        proposal.CorrectionReason = request.Reason?.Trim();
        proposal.Status = ProposalStatus.Draft;
        proposal.UpdatedAt = DateTime.UtcNow;

        await crmDataRepository.SaveChangesAsync(cancellationToken);

        return ToDto(proposal);
    }

    public async Task<bool> DeleteProposalAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await DeleteEntityAsync(
            proposals,
            id,
            cancellationToken);
    }

    // ---------------------------------------------------------
    // VOUCHERS
    // ---------------------------------------------------------

    public async Task<IReadOnlyList<VoucherDto>> GetVouchersAsync(
        CancellationToken cancellationToken = default)
    {
        var vouchers = await crmDataRepository.GetVouchersAsync(
            cancellationToken);

        return vouchers
            .Select(ToDto)
            .ToList();
    }

    public async Task<VoucherDto> CreateVoucherAsync(
        VoucherRequest request,
        CancellationToken cancellationToken = default)
    {
        var code = request.Code.Trim().ToUpperInvariant();

        if (await crmDataRepository.VoucherCodeExistsAsync(
                code,
                cancellationToken))
        {
            throw new InvalidOperationException(
                "Voucher code already exists.");
        }

        var voucher = new Voucher
        {
            Id = Guid.NewGuid(),
            Code = code,
            DiscountType = request.DiscountType,
            DiscountValue = request.DiscountValue,
            IsActive = request.IsActive,
            ValidFrom = request.ValidFrom,
            ValidUntil = request.ValidUntil,
            MinimumAmount = request.MinimumAmount,
            Stackable = request.Stackable
        };

        await crmDataRepository.AddVoucherAsync(
            voucher,
            cancellationToken);

        await crmDataRepository.SaveChangesAsync(
            cancellationToken);

        return ToDto(voucher);
    }

    public async Task<VoucherDto?> ValidateVoucherAsync(
        string code,
        decimal amount,
        CancellationToken cancellationToken = default)
    {
        var voucher = await crmDataRepository.GetVoucherByCodeAsync(
            code.Trim().ToUpperInvariant(),
            cancellationToken);

        var validation = voucherService.Validate(
            voucher,
            amount,
            DateTime.UtcNow);

        if (!validation.IsValid || voucher is null)
        {
            throw new ArgumentException(validation.Error);
        }

        return ToDto(voucher);
    }

    // ---------------------------------------------------------
    // PROPOSAL PRODUCTS
    // ---------------------------------------------------------

    public async Task<IReadOnlyList<ProposalProductDto>> GetProposalProductsAsync(
        Guid proposalId,
        CancellationToken cancellationToken = default)
    {
        return await proposalProducts.Query()
            .AsNoTracking()
            .Where(product => product.ProposalId == proposalId)
            .Select(product => new ProposalProductDto(
                product.Id,
                product.ProposalId,
                product.ProductId,
                product.Quantity,
                product.UnitPrice,
                product.DiscountPercentage,
                product.DiscountAmount,
                product.TotalPrice))
            .ToListAsync(cancellationToken);
    }

    public async Task<ProposalProductDto?> AddProposalProductAsync(
        Guid proposalId,
        ProposalProductRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!await crmDataRepository.ProposalExistsAsync(
                proposalId,
                cancellationToken) ||
            !await crmDataRepository.ProductExistsAsync(
                request.ProductId,
                true,
                cancellationToken))
        {
            throw new ArgumentException(
                "Proposal or active product does not exist.");
        }

        var product = await products.GetByIdAsync(
            request.ProductId,
            cancellationToken);

        if (product is null || !product.IsActive)
        {
            throw new ArgumentException(
                "Proposal or active product does not exist.");
        }

        var line = pricingService.Calculate(
            [
                new PricingLine(
                    request.ProductId,
                    request.Quantity,
                    product.Price,
                    request.DiscountType,
                    request.DiscountType == DiscountType.Percentage
                        ? request.DiscountValue
                        : request.DiscountAmount)
            ]).Lines![0];

        var entity = new ProposalProduct
        {
            Id = Guid.NewGuid(),
            ProposalId = proposalId,
            ProductId = request.ProductId,
            Quantity = line.Quantity,
            UnitPrice = line.UnitPrice,
            DiscountType = line.DiscountType,
            DiscountValue = line.DiscountValue,
            DiscountPercentage =
                line.DiscountType == DiscountType.Percentage
                    ? line.DiscountValue
                    : 0m,
            DiscountAmount = line.DiscountAmount,
            TotalPrice = line.TotalAmount
        };

        await proposalProducts.AddAsync(
            entity,
            cancellationToken);

        await proposalProducts.SaveChangesAsync(
            cancellationToken);

        return ToDto(entity);
    }

    public async Task<bool> UpdateProposalProductAsync(
        Guid proposalId,
        Guid id,
        ProposalProductRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await proposalProducts.GetByIdAsync(
            id,
            cancellationToken);

        if (entity is null || entity.ProposalId != proposalId)
        {
            return false;
        }

        entity.ProductId = request.ProductId;
        entity.Quantity = request.Quantity;
        entity.UnitPrice = request.UnitPrice;
        entity.DiscountPercentage = request.DiscountPercentage;
        entity.DiscountAmount = request.DiscountAmount;
        entity.TotalPrice = request.TotalPrice;
        entity.UpdatedAt = DateTime.UtcNow;

        await proposalProducts.SaveChangesAsync(
            cancellationToken);

        return true;
    }

    public async Task<bool> DeleteProposalProductAsync(
        Guid proposalId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await DeleteLineAsync(
            proposalProducts,
            id,
            proposalId,
            nameof(ProposalProduct.ProposalId),
            cancellationToken);
    }

    // ---------------------------------------------------------
    // ORDERS
    // ---------------------------------------------------------

    public async Task<IReadOnlyList<OrderDto>> GetOrdersAsync(
        CancellationToken cancellationToken = default)
    {
        return await orders.Query()
            .AsNoTracking()
            .OrderByDescending(order => order.OrderDate)
            .Select(order => new OrderDto(
                order.Id,
                order.ProposalId,
                order.CustomerId,
                order.AssignedEmployeeId,
                order.OrderNumber,
                order.Status,
                order.TotalAmount,
                order.OrderDate))
            .ToListAsync(cancellationToken);
    }

    public async Task<OrderDto?> GetOrderAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await orders.GetByIdAsync(
            id,
            cancellationToken);

        return entity is null ? null : ToDto(entity);
    }

    public async Task<OrderDto> CreateOrderAsync(
        OrderRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!await crmDataRepository.ProposalExistsForCustomerAsync(
                request.ProposalId,
                request.CustomerId,
                cancellationToken) ||
            !await crmDataRepository.CustomerExistsAsync(
                request.CustomerId,
                cancellationToken))
        {
            throw new ArgumentException(
                "Proposal and customer relationship is invalid.");
        }

        var proposal = await crmDataRepository.GetProposalWithProductsAsync(
            request.ProposalId,
            cancellationToken);

        if (proposal is null)
        {
            throw new KeyNotFoundException(
                "Proposal does not exist.");
        }

        var pricing = await CalculateProposalAsync(
            proposal,
            cancellationToken);

        if (pricing.Error is not null)
        {
            throw new ArgumentException(pricing.Error);
        }

        var result = pricing.Result!;

        proposal.SubTotal = result.Subtotal;
        proposal.DiscountAmount = result.LineDiscountAmount;
        proposal.VoucherDiscountAmount = result.VoucherDiscountAmount;
        proposal.TotalAmount = result.TotalAmount;

        var entity = new Order
        {
            Id = Guid.NewGuid(),
            ProposalId = request.ProposalId,
            CustomerId = request.CustomerId,
            AssignedEmployeeId = request.AssignedEmployeeId,
            OrderNumber = request.OrderNumber.Trim(),
            Status = request.Status,
            TotalAmount = result.TotalAmount,
            DiscountAmount =
                result.LineDiscountAmount +
                result.VoucherDiscountAmount,
            OrderDate = request.OrderDate
        };

        entity.OrderProducts = result.Lines!
            .Select(line => new OrderProduct
            {
                Id = Guid.NewGuid(),
                OrderId = entity.Id,
                ProductId = line.ProductId,
                Quantity = line.Quantity,
                UnitPrice = line.UnitPrice,
                Discount = line.DiscountAmount,
                TotalPrice = line.TotalAmount
            })
            .ToList();

        await orders.AddAsync(
            entity,
            cancellationToken);

        await orders.SaveChangesAsync(
            cancellationToken);

        return ToDto(entity);
    }

    public async Task<bool> UpdateOrderAsync(
        Guid id,
        OrderRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await orders.GetByIdAsync(
            id,
            cancellationToken);

        if (entity is null)
        {
            return false;
        }

        if (!await crmDataRepository.ProposalExistsForCustomerAsync(
                request.ProposalId,
                request.CustomerId,
                cancellationToken))
        {
            throw new ArgumentException(
                "Proposal and customer relationship is invalid.");
        }

        entity.ProposalId = request.ProposalId;
        entity.CustomerId = request.CustomerId;
        entity.AssignedEmployeeId = request.AssignedEmployeeId;
        entity.OrderNumber = request.OrderNumber.Trim();
        entity.Status = request.Status;
        entity.TotalAmount = request.TotalAmount;
        entity.OrderDate = request.OrderDate;
        entity.UpdatedAt = DateTime.UtcNow;

        await orders.SaveChangesAsync(
            cancellationToken);

        return true;
    }

    public async Task<bool> DeleteOrderAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await DeleteEntityAsync(
            orders,
            id,
            cancellationToken);
    }

    // ---------------------------------------------------------
    // ORDER PRODUCTS
    // ---------------------------------------------------------

    public async Task<IReadOnlyList<OrderProductDto>> GetOrderProductsAsync(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        return await orderProducts.Query()
            .AsNoTracking()
            .Where(product => product.OrderId == orderId)
            .Select(product => new OrderProductDto(
                product.Id,
                product.OrderId,
                product.ProductId,
                product.Quantity,
                product.UnitPrice,
                product.Discount,
                product.TotalPrice))
            .ToListAsync(cancellationToken);
    }

    public async Task<OrderProductDto?> AddOrderProductAsync(
        Guid orderId,
        OrderProductRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!await crmDataRepository.OrderExistsAsync(
                orderId,
                cancellationToken) ||
            !await crmDataRepository.ProductExistsAsync(
                request.ProductId,
                true,
                cancellationToken))
        {
            throw new ArgumentException(
                "Order or active product does not exist.");
        }

        var entity = new OrderProduct
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            ProductId = request.ProductId,
            Quantity = request.Quantity,
            UnitPrice = request.UnitPrice,
            Discount = request.Discount,
            TotalPrice = request.TotalPrice
        };

        await orderProducts.AddAsync(
            entity,
            cancellationToken);

        await orderProducts.SaveChangesAsync(
            cancellationToken);

        return ToDto(entity);
    }

    public async Task<bool> UpdateOrderProductAsync(
        Guid orderId,
        Guid id,
        OrderProductRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await orderProducts.GetByIdAsync(
            id,
            cancellationToken);

        if (entity is null || entity.OrderId != orderId)
        {
            return false;
        }

        entity.ProductId = request.ProductId;
        entity.Quantity = request.Quantity;
        entity.UnitPrice = request.UnitPrice;
        entity.Discount = request.Discount;
        entity.TotalPrice = request.TotalPrice;
        entity.UpdatedAt = DateTime.UtcNow;

        await orderProducts.SaveChangesAsync(
            cancellationToken);

        return true;
    }

    public async Task<bool> DeleteOrderProductAsync(
        Guid orderId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await DeleteLineAsync(
            orderProducts,
            id,
            orderId,
            nameof(OrderProduct.OrderId),
            cancellationToken);
    }

    // ---------------------------------------------------------
    // SUBSCRIPTIONS
    // ---------------------------------------------------------

    public async Task<IReadOnlyList<SubscriptionDto>> GetSubscriptionsAsync(
        CancellationToken cancellationToken = default)
    {
        var subscriptions =
            await crmDataRepository.GetSubscriptionsAsync(
                cancellationToken);

        return subscriptions
            .Select(subscription => new SubscriptionDto(
                subscription.Id,
                subscription.CustomerId,
                subscription.ProductId,
                subscription.StartDate,
                subscription.EndDate,
                subscription.Amount,
                subscription.Status))
            .ToList();
    }

    // ---------------------------------------------------------
    // ACTIVITIES
    // ---------------------------------------------------------

    public async Task<IReadOnlyList<EngagementActivityDto>> GetActivitiesAsync(
        Guid? customerId,
        Guid? opportunityId,
        CancellationToken cancellationToken = default)
    {
        var activities =
            await crmDataRepository.GetActivitiesAsync(
                customerId,
                opportunityId,
                cancellationToken);

        return activities
            .Select(ToDto)
            .ToList();
    }

    public async Task<EngagementActivityDto?> CreateActivityAsync(
        EngagementActivityRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!await EntityReferencesExist(
                request.CustomerId,
                request.EmployeeId,
                request.OpportunityId,
                request.ProposalId,
                cancellationToken))
        {
            throw new ArgumentException(
                "One or more CRM references do not exist.");
        }

        var activity = new EngagementActivity
        {
            Id = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            EmployeeId = request.EmployeeId,
            OpportunityId = request.OpportunityId,
            ProposalId = request.ProposalId,
            Type = request.Type.Trim(),
            Subject = request.Subject?.Trim(),
            Description = request.Description?.Trim(),
            ActivityDate = request.ActivityDate
        };

        await crmDataRepository.AddActivityAsync(
            activity,
            cancellationToken);

        await crmDataRepository.SaveChangesAsync(
            cancellationToken);

        return ToDto(activity);
    }

    // ---------------------------------------------------------
    // CONVERSATIONS
    // ---------------------------------------------------------

    public async Task<IReadOnlyList<ConversationDto>> GetConversationsAsync(
        Guid? customerId,
        Guid? opportunityId,
        CancellationToken cancellationToken = default)
    {
        var conversations =
            await crmDataRepository.GetConversationsAsync(
                customerId,
                opportunityId,
                cancellationToken);

        return conversations
            .Select(ToDto)
            .ToList();
    }

    public async Task<ConversationDto?> CreateConversationAsync(
        ConversationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!await EntityReferencesExist(
                request.CustomerId,
                request.EmployeeId,
                request.OpportunityId,
                null,
                cancellationToken))
        {
            throw new ArgumentException(
                "One or more CRM references do not exist.");
        }

        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            EmployeeId = request.EmployeeId,
            OpportunityId = request.OpportunityId,
            Message = request.Message.Trim(),
            Channel = request.Channel.Trim()
        };

        await crmDataRepository.AddConversationAsync(
            conversation,
            cancellationToken);

        await crmDataRepository.SaveChangesAsync(
            cancellationToken);

        return ToDto(conversation);
    }

    // ---------------------------------------------------------
    // PRICING
    // ---------------------------------------------------------

    private async Task<(string? Error, PricingResult? Result)> CalculateAsync(
        IReadOnlyList<ProposalProductRequest> requests,
        string? voucherCode,
        CancellationToken cancellationToken)
    {
        var productIds = requests
            .Select(request => request.ProductId)
            .Distinct()
            .ToList();

        var productPrices =
            await crmDataRepository.GetActiveProductPricesAsync(
                productIds,
                cancellationToken);

        if (productPrices.Count != productIds.Count)
        {
            return (
                "One or more products are missing or inactive.",
                null);
        }

        var lines = requests.Select(request =>
            new PricingLine(
                request.ProductId,
                request.Quantity,
                productPrices[request.ProductId],
                request.DiscountType,
                request.DiscountType == DiscountType.Percentage
                    ? request.DiscountValue
                    : request.DiscountAmount));

        var withoutVoucher = pricingService.Calculate(lines);

        var vouchers = new List<PricingVoucher>();

        if (!string.IsNullOrWhiteSpace(voucherCode))
        {
            var voucher =
                await crmDataRepository.GetVoucherByCodeAsync(
                    voucherCode.Trim(),
                    cancellationToken);

            var validation = voucherService.Validate(
                voucher,
                withoutVoucher.TotalAmount,
                DateTime.UtcNow);

            if (!validation.IsValid)
            {
                return (validation.Error, null);
            }

            vouchers.Add(validation.Voucher!);
        }

        return (
            null,
            pricingService.Calculate(lines, vouchers));
    }

    private async Task<(string? Error, PricingResult? Result)>
        CalculateProposalAsync(
            Proposal proposal,
            CancellationToken cancellationToken)
    {
        var productIds = proposal.ProposalProducts
            .Select(line => line.ProductId)
            .Distinct()
            .ToList();

        var productPrices =
            await crmDataRepository.GetActiveProductPricesAsync(
                productIds,
                cancellationToken);

        if (productPrices.Count != productIds.Count)
        {
            return (
                "One or more proposal products are missing or inactive.",
                null);
        }

        var lines = proposal.ProposalProducts.Select(line =>
            new PricingLine(
                line.ProductId,
                line.Quantity,
                productPrices[line.ProductId],
                line.DiscountType,
                line.DiscountValue == 0m
                    ? line.DiscountPercentage
                    : line.DiscountValue));

        var basePricing = pricingService.Calculate(lines);

        var vouchers = new List<PricingVoucher>();

        if (!string.IsNullOrWhiteSpace(proposal.VoucherCode))
        {
            var voucher =
                await crmDataRepository.GetVoucherByCodeAsync(
                    proposal.VoucherCode,
                    cancellationToken);

            var validation = voucherService.Validate(
                voucher,
                basePricing.TotalAmount,
                DateTime.UtcNow);

            if (!validation.IsValid)
            {
                return (validation.Error, null);
            }

            vouchers.Add(validation.Voucher!);
        }

        return (
            null,
            pricingService.Calculate(lines, vouchers));
    }

    // ---------------------------------------------------------
    // VALIDATION / REFERENCES
    // ---------------------------------------------------------

    private async Task<bool> ReferencesExist(
        Guid? gradeId,
        Guid? managerId,
        CancellationToken cancellationToken)
    {
        return
            (gradeId is null ||
             await crmDataRepository.EmployeeGradeExistsAsync(
                 gradeId.Value,
                 cancellationToken))
            &&
            (managerId is null ||
             await crmDataRepository.EmployeeExistsAsync(
                 managerId.Value,
                 cancellationToken));
    }

    private async Task<bool> EntityReferencesExist(
        Guid customerId,
        Guid? employeeId,
        Guid? opportunityId,
        Guid? proposalId,
        CancellationToken cancellationToken)
    {
        if (!await crmDataRepository.CustomerExistsAsync(
                customerId,
                cancellationToken))
        {
            return false;
        }

        if (employeeId is not null &&
            !await crmDataRepository.EmployeeExistsAsync(
                employeeId.Value,
                cancellationToken))
        {
            return false;
        }

        if (opportunityId is not null &&
            !await crmDataRepository.CustomerExistsForOpportunityAsync(
                opportunityId.Value,
                customerId,
                cancellationToken))
        {
            return false;
        }

        if (proposalId is not null &&
            !await crmDataRepository.ProposalExistsForCustomerAsync(
                proposalId.Value,
                customerId,
                cancellationToken))
        {
            return false;
        }

        return true;
    }

    // ---------------------------------------------------------
    // GENERIC DELETE HELPERS
    // ---------------------------------------------------------

    private static async Task<bool> DeleteEntityAsync<TEntity>(
        ICrmRepository<TEntity> repository,
        Guid id,
        CancellationToken cancellationToken)
        where TEntity : class
    {
        var entity = await repository.GetByIdAsync(
            id,
            cancellationToken);

        if (entity is null)
        {
            return false;
        }

        repository.Remove(entity);

        await repository.SaveChangesAsync(
            cancellationToken);

        return true;
    }

    private static async Task<bool> DeleteLineAsync<TEntity>(
        ICrmRepository<TEntity> repository,
        Guid id,
        Guid parentId,
        string parentProperty,
        CancellationToken cancellationToken)
        where TEntity : class
    {
        var entity = await repository.GetByIdAsync(
            id,
            cancellationToken);

        if (entity is null)
        {
            return false;
        }

        var actualParentId =
            (Guid)typeof(TEntity)
                .GetProperty(parentProperty)!
                .GetValue(entity)!;

        if (actualParentId != parentId)
        {
            return false;
        }

        repository.Remove(entity);

        await repository.SaveChangesAsync(
            cancellationToken);

        return true;
    }

    // ---------------------------------------------------------
    // DTO MAPPERS
    // ---------------------------------------------------------

    private static EmployeeGradeDto ToDto(
        EmployeeGrade entity) =>
        new(
            entity.Id,
            entity.Name,
            entity.Description,
            entity.ApprovalLimit,
            entity.IsActive);

    private static EmployeeDto ToDto(
        Employee entity) =>
        new(
            entity.Id,
            entity.FirstName,
            entity.LastName,
            entity.Email,
            entity.Phone,
            entity.EmployeeGradeId,
            entity.ManagerId,
            entity.Role,
            entity.IsActive);

    private static CustomerDto ToDto(
        Customer entity) =>
        new(
            entity.Id,
            entity.Name,
            entity.Email,
            entity.Phone,
            entity.Company,
            entity.Address,
            entity.Status);

    private static ProductDto ToDto(
        Product entity) =>
        new(
            entity.Id,
            entity.Name,
            entity.Description,
            entity.Category,
            entity.Price,
            entity.IsActive,
            entity.PlanDurationMonths);

    private static OpportunityDto ToDto(
        Opportunity entity) =>
        new(
            entity.Id,
            entity.CustomerId,
            entity.AssignedEmployeeId,
            entity.Name,
            entity.Description,
            entity.EstimatedValue,
            entity.Status,
            entity.ExpectedCloseDate);

    private static ProposalDto ToDto(
        Proposal entity) =>
        new(
            entity.Id,
            entity.OpportunityId,
            entity.CustomerId,
            entity.CreatedByEmployeeId,
            entity.ProposalNumber,
            entity.Status,
            entity.SubTotal,
            entity.DiscountPercentage,
            entity.DiscountAmount,
            entity.TotalAmount,
            entity.ValidUntil,
            entity.VoucherDiscountAmount,
            entity.VoucherCode,
            entity.Revision);

    private static VoucherDto ToDto(
        Voucher entity) =>
        new(
            entity.Id,
            entity.Code,
            entity.DiscountType,
            entity.DiscountValue,
            entity.IsActive,
            entity.ValidFrom,
            entity.ValidUntil,
            entity.MinimumAmount,
            entity.Stackable);

    private static ProposalProductDto ToDto(
        ProposalProduct entity) =>
        new(
            entity.Id,
            entity.ProposalId,
            entity.ProductId,
            entity.Quantity,
            entity.UnitPrice,
            entity.DiscountPercentage,
            entity.DiscountAmount,
            entity.TotalPrice);

    private static OrderDto ToDto(
        Order entity) =>
        new(
            entity.Id,
            entity.ProposalId,
            entity.CustomerId,
            entity.AssignedEmployeeId,
            entity.OrderNumber,
            entity.Status,
            entity.TotalAmount,
            entity.OrderDate);

    private static OrderProductDto ToDto(
        OrderProduct entity) =>
        new(
            entity.Id,
            entity.OrderId,
            entity.ProductId,
            entity.Quantity,
            entity.UnitPrice,
            entity.Discount,
            entity.TotalPrice);

    private static EngagementActivityDto ToDto(
        EngagementActivity entity) =>
        new(
            entity.Id,
            entity.CustomerId,
            entity.EmployeeId,
            entity.OpportunityId,
            entity.ProposalId,
            entity.Type,
            entity.Subject,
            entity.Description,
            entity.ActivityDate);

    private static ConversationDto ToDto(
        Conversation entity) =>
        new(
            entity.Id,
            entity.CustomerId,
            entity.EmployeeId,
            entity.OpportunityId,
            entity.Message,
            entity.Channel,
            entity.CreatedAt);
}