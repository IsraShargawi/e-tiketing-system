using ETicketingSystem.Accounting.Services;
using Microsoft.AspNetCore.Mvc;

namespace ETicketingSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LedgerController : ControllerBase
{
    private readonly LedgerService _ledgerService;

    public LedgerController(LedgerService ledgerService)
    {
        _ledgerService = ledgerService;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetLedgerSummary()
    {
        var summary = await _ledgerService.GetLedgerSummaryAsync();
        return Ok(summary);
    }


    [HttpGet("validate")]
    public async Task<IActionResult> ValidateLedger()
    {
        var isBalanced = await _ledgerService.ValidateLedgerBalanceAsync();
        
        return Ok(new
        {
            isBalanced,
            message = isBalanced 
                ? "Ledger is balanced. Total Debits = Total Credits." 
                : "WARNING: Ledger is NOT balanced!"
        });
    }
}
