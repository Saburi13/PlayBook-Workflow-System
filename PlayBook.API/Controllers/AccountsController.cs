using Microsoft.AspNetCore.Mvc;
using PlayBook.Business.BusinessModels.RequestDTOs.AccountRequestDTOs;
using PlayBook.Business.Interfaces.IService;

namespace PlayBook.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountsController(
    IAccountService accountService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAccounts(
        CancellationToken cancellationToken)
    {
        var accounts = await accountService.GetAccountsAsync(
            cancellationToken);

        return Ok(accounts);
    }

    [HttpGet("{accountId}")]
    public async Task<IActionResult> GetAccount(
        string accountId,
        CancellationToken cancellationToken)
    {
        var account = await accountService.GetAccountAsync(
            accountId,
            cancellationToken);

        if (account is null)
        {
            return NotFound();
        }

        return Ok(account);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAccount(
        [FromBody] AccountRequest request,
        CancellationToken cancellationToken)
    {
        var account = await accountService.CreateAccountAsync(
            request,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetAccount),
            new { accountId = account.AccountId },
            account);
    }

    [HttpPut("{accountId}")]
    public async Task<IActionResult> UpdateAccount(
        string accountId,
        [FromBody] AccountRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await accountService.UpdateAccountAsync(
            accountId,
            request,
            cancellationToken);

        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("{accountId}")]
    public async Task<IActionResult> DeleteAccount(
        string accountId,
        CancellationToken cancellationToken)
    {
        var deleted = await accountService.DeleteAccountAsync(
            accountId,
            cancellationToken);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}