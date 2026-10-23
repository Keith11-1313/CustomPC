# Role permissions

An account has one role. Admin does not inherit Staff or Customer actions. Guest means no authenticated account. Both controls and business methods enforce permissions.

| Action | Guest | Customer | Staff | Admin |
| --- | --- | --- | --- | --- |
| Browse/search catalog | Yes | Yes | No | No |
| Prepare a PC build | Yes | Yes | No | No |
| Create order | Login required | Own account | No | No |
| View orders/receipts | No | Own only | All | All, read-only |
| Cancel unpaid pending order | No | Own only | No | No |
| Confirm store payment | No | No | Yes | No |
| Advance assembly/pickup status | No | No | Yes | No |
| View inventory | No | No | Yes | Yes |
| Adjust stock with reason | No | No | Yes | No |
| Add/edit/archive products and photos | No | No | No | Yes |
| Manage accounts and assign roles | No | No | No | Yes |
| Reports/CSV export | No | No | No | Yes |
| Activity & Backups | No | No | No | Yes |

Guest registration creates Customer only. Guest checkout preserves selected parts, asks for login and continues only as Customer. Customers cannot choose a different order owner. Admin cannot archive/demote their current Admin account. Refresh reloads account status and role; archived/reassigned accounts lose their former permissions.

Admin assigns roles through User accounts → Add/Edit account → Role → Save. Staff changes stock through Adjust stock; catalog edits cannot bypass that separation.

Compatibility checks are mandatory application logic. No role can switch them off. There is no supplier management or customizable business identity. See [STORE_GUIDE.md](STORE_GUIDE.md) for operating steps.
