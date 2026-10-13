# Who can do what

Each account has **one role**: Admin, Staff or Customer. Admin is not a superuser that inherits everybody else's actions. Guest means browsing without signing in.

This is the strict split you approved. It replaces the proposal's broader permissions for Staff-created orders, Staff reports and Admin stock edits. Existing accounts and records keep their saved roles and data.

| Action | Guest | Customer | Staff | Admin |
| --- | --- | --- | --- | --- |
| Browse/search computer parts | Yes | Yes | Yes | Yes |
| Preview the builder without ordering | Yes | Yes | No | No |
| Confirm a build and place an order | No | Own account only | No | No |
| View orders and existing receipts | No | Own orders only | All orders | All orders, read-only |
| Cancel a pending unpaid order | No | Own order only | No | No |
| Confirm payment received at the store | No | No | Yes | No |
| Move an order through assembly and pickup | No | No | Yes | No |
| View inventory quantities | No | No | Yes | Yes |
| Add/edit/archive catalog details and prices | No | No | No | Yes |
| Change stock quantities with a reason | No | No | Yes | No |
| Create/edit/archive accounts and assign roles | No | No | No | Yes |
| View/export inventory, orders, movements and sales reports | No | No | No | Yes |
| Audit history, manual backup/restore | No | No | No | Yes |

Guests can register their own Customer account from login. Registration always assigns Customer; it cannot assign Admin or Staff. An existing order receipt can be viewed, saved or printed by its Customer owner or by Staff/Admin with read access. Reading a receipt does not confirm payment or change the order.

Guests see **Sign in** on the homepage. Once their build is valid, **Confirm build & order** opens login. Customer login continues that same checkout, retains the chosen parts, refreshes prices/availability and saves the build/order to the signed-in Customer after the normal confirmation. There is no second checkout-button click. Cancelling or continuing as Guest preserves the preview without saving an order. Staff/Admin login returns to their own dashboard.

A permitted action can still be disabled because of the selected record. On Orders, select a row first. Payment applies to an unexpired pending order; progress applies to a paid/in-progress order; cancellation applies to the Customer's own pending order. Completed/cancelled rows enable receipt viewing only. This differs from a prohibited action, which is hidden for that role.

Automatic unpaid-order expiry is a system action. It runs on startup/refresh even when the current user cannot cancel orders manually.

## Assign a role in the app

1. Sign in with an Admin account. Open **User accounts**.
2. Select an account and click **Edit account**, or use **Add account** for a new employee.
3. Choose **Admin**, **Staff** or **Customer** in **Role**, then save.
4. Use **Sign out** and sign in as that user to check their workspace. An open screen also rechecks the saved role when loading or saving data.
5. Expect Staff to have stock adjustment and payment/progress actions. Expect Admin to have account, catalog and report actions, with order monitoring read-only.

You cannot archive or demote your currently signed-in Admin account. Sign in as another active Admin if that account must be changed. Choosing Admin for a Staff account removes its Staff actions; a person who performs a different job must sign in with the appropriate account.

## Add a part and receive its stock

1. Admin adds the component name, price, specifications and supplier through **Parts & inventory → Add part**. Its initial stock is zero. The stock field in this editor is read-only.
2. Admin signs out. Staff signs in and opens **Parts & inventory → Adjust stock**.
3. Staff enters the received quantity and a reason/reference, then saves.
4. Expect the stock count and movement history to update. Admin can review movement history through Reports.

This prevents the part editor from becoming a second route around Staff-only inventory transactions. Editing catalog details preserves the current stock, including a delivery recorded while the editor was open.

## Follow the permission check in C#

Start in `CustomPC/AppStore.cs`:

```csharp
public bool IsStaff
{
    get { return Session != null && Session.Role == "Staff"; }
}
```

`Session` is the signed-in account. This expression is true only when that account is Staff. An Admin account returns false. `IsAdmin` and `IsCustomer` work the same way for their own roles.

Now open `CustomPC/AppStore.Orders.cs` and find `ConfirmPayment`. Inside its transaction:

```csharp
LoadData(connection);
RequireStaff();
```

`LoadData` reads the saved account again, including any changed role or archive status. `RequireStaff` throws an error unless the current role is exactly Staff. It runs before creating a payment or changing the order. The transaction rolls back rejected changes.

Finally open `OrdersForm.cs` and find `LoadRows`. `pay.Enabled = store.IsStaff` communicates the permission in the interface. Hiding or disabling a button helps the user, while the AppStore check still protects the operation if an event handler is called directly.

**Checkpoint:** Would making the Admin's payment button visible grant permission to pay?

**Answer:** No. `ConfirmPayment` still calls `RequireStaff`, so Admin is rejected and no payment is saved.


## Modern dashboards and photographs

Admin alone adds/changes products and their photographs, and sees confirmed-payment sales and top-selling-part quantities. Staff alone records real stock receipts and sees incoming-order counts, current work queue and stock readiness. Both can read current inventory and permitted order records. Customers and Guests cannot access operational charts. Chart drill-downs apply filters after permission checks and never expand access.

The prepared products have zero stock. Admin reviews reference prices ; Staff receives actual deliveries. See [the catalog manifest](CATALOG_MANIFEST.md) and [modern code walkthrough](MODERN_UI_GUIDE.md).
Compatibility checks are mandatory application logic in AppStore.Compatibility.cs. There is no Admin rules page, rule editor, Rules table or toggle. Socket, memory, graphics output and PSU checks run during preview and checkout.

Supplier management is removed: no supplier pages, part selector, inventory column, model association or Suppliers table. Existing product sources in the research manifest are documentation, not supplier records.
