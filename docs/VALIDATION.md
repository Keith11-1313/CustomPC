# Testing

## Status

This repository does not include an automated test project or a current release test report. Earlier development checks covered business rules and programmatic WinForms rendering using external tools. Those tools and their results are not reproducible from this checkout, so this page does not claim a passing test total.

Run the checks below against the exact build being distributed. Record its commit/version, Windows version, expected and actual results, and Pass, Fail, or Not run for each check. Screenshots illustrate the interface; they do not prove an operation passed.

## Build checks

From the repository root:

```powershell
dotnet restore CustomPC.slnx
dotnet build CustomPC.slnx -c Release
git diff --check
```

A successful build confirms compilation, not login, order processing, printing, or recovery.

## Practice database

Close the application. From the repository root, run in a PowerShell session:

```powershell
$env:CUSTOMPC_DATABASE_PATH = Join-Path $env:TEMP ('CustomPC-test-' + [guid]::NewGuid().ToString('N') + '/custompc.db')
dotnet run --project CustomPC/CustomPC.csproj
```

Complete setup with practice details, create Staff/Customer accounts, add components, and receive test stock as Staff. The override also selects a separate photograph folder. After closing the app:

```powershell
Remove-Item Env:CUSTOMPC_DATABASE_PATH
```

These variables affect that terminal and its child process, not an already-open Visual Studio session. Keep operational records out of test fixtures.

## Functional checklist

| Area | Check and expected result |
| --- | --- |
| Setup and login | Create the first Admin, reject invalid credentials, and preserve accounts after restart. |
| Registration | Reject duplicates, invalid business text, weak passwords, mismatched confirmation, and an unchecked policy agreement. |
| Roles | Exercise every role in [Roles](ROLES.md); restricted operations must not save changes. |
| Accounts | Create/edit/archive/reactivate accounts. Prevent self-demotion or archiving of the signed-in Admin. |
| Recovery | Reset an active practice account's password; old password fails, new password works, lockout clears. |
| Catalog | Search, filter, inspect details, clear search, and check empty results and page boundaries. |
| Builder | Retain draft selections across navigation; reject invalid socket, RAM, graphics, PSU, and stock combinations. |
| Guest checkout | Customer login continues the same build; cancelled login creates no order and retains the preview. |
| Order creation | Verify owner, line prices, total, 24-hour deadline, and reservations after database reload. |
| Privacy | A second Customer cannot read the first Customer's orders or receipts. |
| Payment and pickup | Staff records payment once and advances one stage at a time; completion deducts stock only once. |
| Cancellation | Own unpaid cancellation releases reservations; paid-order cancellation is rejected. |
| Expiry | With isolated expired fixtures, startup/refresh cancels unpaid orders and releases reservations. |
| Product CRUD | Admin adds/edits/archives/reactivates products. Cancel discards edits; new products start with zero stock. |
| Stock | Staff adjustments require a nonzero quantity/reason and cannot reduce stock below reservations. |
| Photos | Import, cancel, remove, and reopen photographs; deleting the original upload must not remove the managed copy. |
| Reports | Compare totals, inclusive date boundaries, and CSV output to known practice records. Export includes all matching pages. |
| Activity | Saved changes appear with actor and details. Failed changes do not appear as completed actions. |
| Backup | Save a ZIP with records/photos, restore a practice snapshot, reopen, and verify both. Check refused overwrite/corrupt backup. |
| Policies | Open and close each link; cancelling order confirmation creates no order. |

## Desktop and package checks

- Open forms and reusable controls in Visual Studio Designer without accessing the operational database.
- Run at actual Windows 100%, 125%, and 150% scaling where available. Check narrow windows, scrolling, long text, and reachable actions.
- Navigate with Tab, Shift+Tab, Enter, Space, and Escape. Check focus, password visibility, labels, and screen-reader behavior.
- Complete native file dialogs, test Cancel, export CSV, and print a receipt or save through Microsoft Print to PDF.
- Test external-drive backup and recovery separately from local copies.
- Extract the published ZIP outside the source checkout and run it on another supported Windows computer. Test setup and one complete order flow.

Do not mark a check as passed unless it was executed. Keep remaining failures and untested hardware behavior in the release test record.
