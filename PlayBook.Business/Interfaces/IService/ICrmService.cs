using PlayBook.Business.BusinessModels.RequestDTOs.CRMRequestDTOs;
using PlayBook.Business.BusinessModels.ResponseDTOs.CRMResponseDTOs;

namespace PlayBook.Business.Interfaces.IService;

public interface ICrmService
{
    Task<IReadOnlyList<EmployeeGradeDto>> GetGradesAsync(
        CancellationToken cancellationToken = default);

    Task<EmployeeGradeDto?> GetGradeAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<EmployeeGradeDto> CreateGradeAsync(
        EmployeeGradeRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateGradeAsync(
        Guid id,
        EmployeeGradeRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteGradeAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EmployeeDto>> GetEmployeesAsync(
        CancellationToken cancellationToken = default);

    Task<EmployeeDto?> GetEmployeeAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<EmployeeDto> CreateEmployeeAsync(
        EmployeeRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateEmployeeAsync(
        Guid id,
        EmployeeRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteEmployeeAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CustomerDto>> GetCustomersAsync(
        CancellationToken cancellationToken = default);

    Task<CustomerDto?> GetCustomerAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<CustomerDto> CreateCustomerAsync(
        CustomerRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateCustomerAsync(
        Guid id,
        CustomerRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteCustomerAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProductDto>> GetProductsAsync(
        CancellationToken cancellationToken = default);

    Task<ProductDto?> GetProductAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ProductDto> CreateProductAsync(
        ProductRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateProductAsync(
        Guid id,
        ProductRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteProductAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OpportunityDto>> GetOpportunitiesAsync(
        CancellationToken cancellationToken = default);

    Task<OpportunityDto?> GetOpportunityAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<OpportunityDto> CreateOpportunityAsync(
        OpportunityRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateOpportunityAsync(
        Guid id,
        OpportunityRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteOpportunityAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProposalDto>> GetProposalsAsync(
        CancellationToken cancellationToken = default);

    Task<ProposalDto?> GetProposalAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ProposalDto> CreateProposalAsync(
        ProposalRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateProposalAsync(
        Guid id,
        ProposalRequest request,
        CancellationToken cancellationToken = default);

    Task<ProposalDto?> CorrectProposalAsync(
        Guid id,
        CorrectProposalRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteProposalAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<VoucherDto>> GetVouchersAsync(
        CancellationToken cancellationToken = default);

    Task<VoucherDto> CreateVoucherAsync(
        VoucherRequest request,
        CancellationToken cancellationToken = default);

    Task<VoucherDto?> ValidateVoucherAsync(
        string code,
        decimal amount,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProposalProductDto>> GetProposalProductsAsync(
        Guid proposalId,
        CancellationToken cancellationToken = default);

    Task<ProposalProductDto?> AddProposalProductAsync(
        Guid proposalId,
        ProposalProductRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateProposalProductAsync(
        Guid proposalId,
        Guid id,
        ProposalProductRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteProposalProductAsync(
        Guid proposalId,
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OrderDto>> GetOrdersAsync(
        CancellationToken cancellationToken = default);

    Task<OrderDto?> GetOrderAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<OrderDto> CreateOrderAsync(
        OrderRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateOrderAsync(
        Guid id,
        OrderRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteOrderAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OrderProductDto>> GetOrderProductsAsync(
        Guid orderId,
        CancellationToken cancellationToken = default);

    Task<OrderProductDto?> AddOrderProductAsync(
        Guid orderId,
        OrderProductRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateOrderProductAsync(
        Guid orderId,
        Guid id,
        OrderProductRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteOrderProductAsync(
        Guid orderId,
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SubscriptionDto>> GetSubscriptionsAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EngagementActivityDto>> GetActivitiesAsync(
        Guid? customerId,
        Guid? opportunityId,
        CancellationToken cancellationToken = default);

    Task<EngagementActivityDto?> CreateActivityAsync(
        EngagementActivityRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ConversationDto>> GetConversationsAsync(
        Guid? customerId,
        Guid? opportunityId,
        CancellationToken cancellationToken = default);

    Task<ConversationDto?> CreateConversationAsync(
        ConversationRequest request,
        CancellationToken cancellationToken = default);
}