# Learn CustomPC one step at a time

Start here if you can open Visual Studio but do not yet understand how a Windows Forms project works. This guide teaches through this project's actual code. Read one lesson, try its task, and stop at its checkpoint.

Your first goal is small: **find the code behind Sign in, explain what it does, and watch it run**. Start with lessons 1–5. Leave the database and the rest of the screens until those make sense.

The longer [code reference](CODE_REFERENCE.md) contains the complete file map, C# lookup table, role rules, and detailed database explanation. Use it when you have a specific question. [Validation results](VALIDATION.md) describe what was tested.

## 1. Find your way around Visual Studio

**Goal:** open the application and find one form's code.

1. Open `C:\Users\Jerald\Desktop\CustomPC\CustomPC.slnx` in Visual Studio. A solution is the file Visual Studio uses to open the project.
2. Open **View → Solution Explorer**. Expand the `CustomPC` project using the arrow beside its name. A project groups the source files and settings needed to build one application.
3. Find `LoginForm.cs`. Right-click it and choose **View Code**. If you see a drawn window instead of code, press **F7**.
4. Press **Ctrl+F** and find `LoginButton_Click`.
5. Press **F5** to run the application. You should see a dashboard with the Welcome to CustomPC login window in front of it.
6. Click **Browse as guest**, then **Parts catalog**. You can look at parts without placing an order. Close the catalog and dashboard when done.

An application must be **built** before it runs: the compiler checks your C# and creates the executable. If F5 reports an error, open **View → Error List** and read the first error before trying later lessons. This project requires .NET 10 and Visual Studio's .NET desktop development tools.

A **method** is a named block of instructions. `LoginButton_Click` is a method: the instructions between its `{` and `}` run when it is called. You will follow those instructions in lesson 4.

**Checkpoint:** Where would you start looking for the Sign in button's behavior?

**Answer:** `LoginForm.cs`, in `LoginButton_Click`.

## 2. Connect what you see to a control in the code

**Goal:** understand why one screen has more than one file.

A **form** is a window. A **control** is something placed in that window: a button, text box, label, checkbox, or dropdown. A label displays text; a text box accepts text you type.

1. Stop the running application before editing. **Shift+F5** stops a debugging session.
2. Select `LoginForm.cs`, then press **Shift+F7** for View Designer.
3. Select the email text box. Press **F4** to show the Properties window. Find **(Name)**: it is `email`.
4. Select the Sign in button. Its **(Name)** is `login`; its **Text** is `Sign in`.

`(Name)` is how the code identifies the control. `Text` is the text the control displays or contains. For the email box, `email.Text` means **the text typed into the control named email**. The dot means “get something belonging to this object.” A **property** is a named value on an object, such as `Text`.

Expand the arrow beside `LoginForm.cs` in Solution Explorer if its related files are nested:

| File | What you use it for |
| --- | --- |
| `LoginForm.cs` | Instructions for button clicks and other behavior |
| `LoginForm.Designer.cs` | Code that creates and positions the controls; the visual Designer edits this |
| `LoginForm.resx` | Resources associated with the form, such as images when a form uses them |

The keyword `partial` lets the same class be written across multiple files. Here the behavior file and Designer file both belong to `LoginForm`. A **class** describes the information and instructions an object will have. The running login window is an **object** created from the `LoginForm` class.

**Try one reversible edit:** select the large heading in the Designer. Record its original Text, `Welcome to CustomPC`. Change Text to `My CustomPC login`, save, and press F5. You should see the new heading. Stop, restore `Welcome to CustomPC`, and save again. Change **Text**, keeping **(Name)** as `heading`, so existing code can still find it.

**Checkpoint:** Does changing a button's Text make it call a different method?

**Answer:** No. Text changes its caption. The event connection determines which method runs.

## 3. Understand the connection between a click and a method

**Goal:** read one small piece of working behavior.

Open `LoginForm.cs`. Near the top, find:

```csharp
login.Click += LoginButton_Click;
showPassword.CheckedChanged += ShowPassword_CheckedChanged;
```

An **event** is a notification that something happened. `Click` happens when a button is clicked. `CheckedChanged` happens when the checkbox's checked state changes. In these lines, `+=` connects the event to the method that should respond. A method connected this way is called an **event handler**.

Read the first line as: **when login is clicked, run LoginButton_Click**.

These connections are inside `public LoginForm()`. That is a **constructor**: setup instructions that run when code creates a new login window. `InitializeComponent()` creates the controls before the constructor connects their events. `AcceptButton = login;` also lets Enter activate Sign in.

Now find this complete method:

```csharp
private void ShowPassword_CheckedChanged(object? sender, EventArgs e)
{
    password.UseSystemPasswordChar = !showPassword.Checked;
}
```

For this lesson, concentrate on the line inside the braces:

- `showPassword.Checked` is `true` when the checkbox is ticked and `false` when unticked. `true` and `false` are the two possible values of a `bool`.
- `!` reverses a true/false value.
- `password.UseSystemPasswordChar` decides whether the text box masks its characters. `true` means masked; `false` means visible.
- `=` assigns a value. The semicolon ends the instruction.

Ticked checkbox → `!true` → `false` → password characters become visible.

In the method's first line, `private` means other classes cannot directly call it; `void` means it does not return a value. Windows supplies `sender` and `e` when raising the event. This handler uses the form's controls directly, so it does not need those two arguments. You can leave their declaration unchanged.

**Try it:** run the app, type the dummy text `practice` in Password, and toggle Show password. You should see the same text become visible and masked. This action does not sign you in or change a saved password.

**Checkpoint:** Why does ticking the checkbox set masking to false?

**Answer:** The `!` reverses its `Checked` value from true to false.

## 4. Read the Sign in handler, a few lines at a time

**Goal:** understand what the form does and where its work goes.

Open `LoginForm.cs` and find this method:

```csharp
private void LoginButton_Click(object? sender, EventArgs e)
{
    try
    {
        AppStore.Current.Login(email.Text, password.Text);
        DialogResult = DialogResult.OK;
    }
    catch (Exception exception)
    {
        Ui.ShowError(this, exception);
    }
}
```

Read it from top to bottom:

1. **`try`** starts the instructions we want to attempt.
2. **`email.Text` and `password.Text`** read what was entered into the two text boxes.
3. **`AppStore.Current`** gives the form the shared object that manages the loaded records and signed-in account. Different forms use the same object. You can understand this call before learning how `Current` creates that object.
4. **`Login(...)`** calls another method. The two comma-separated values inside the parentheses are the inputs passed to that method. It checks the account and password.
5. **`DialogResult = DialogResult.OK;`** reports a successful result. When this form is opened as a dialog, that closes it and lets the dashboard continue. “OK” here is a window result, not an order status.
6. **`catch`** handles an error raised inside `try`. An **exception** is the error information C# passes to this block. If `Login` fails, execution skips the OK line and enters `catch`.
7. **`Ui.ShowError(this, exception);`** shows the error's message. `this` means this login form, which owns the message box. `Ui` contains display helpers shared by the forms.

**Try it:** reuse the login window from lesson 3, or press F5 if the app is closed. Clear both Email address and Password, including the dummy text `practice`. Click Sign in. Expect `Enter your email address and password.` Dismiss it; the login window should still be open.

**Checkpoint:** Which line is skipped when login fails, and why?

**Answer:** The `DialogResult.OK` assignment. The exception interrupts the normal path and sends execution to `catch`.

## 5. Watch that handler run in the debugger

**Goal:** see the program execute instead of guessing its order.

A **breakpoint** tells Visual Studio to pause before executing a particular line. Pausing lets you inspect the program. It does not mean the program has crashed.

1. Stop the previous run. Open `LoginForm.cs` in code view.
2. Click the line `AppStore.Current.Login(email.Text, password.Text);`, then press **F9**. A red dot appears in the left margin.
3. Press **F5**. Type the dummy email `practice@example.com`, leave Password empty, and click Sign in.
4. Visual Studio should come forward with a yellow arrow at the breakpoint. The highlighted line has **not executed yet**.
5. Hover over `email.Text` to see `practice@example.com`. If hovering does not show it, open **Debug → Windows → Watch → Watch 1**, enter `email.Text`, and press Enter. A Watch expression lets you request a value while the program is paused.
6. Press **F10** once. This executes the call while keeping your focus in the form's method. Because Password is empty, `Login` raises an error. The next path is `catch`, rather than the OK assignment. If Visual Studio pauses on an exception notification, press F5 to let the handler catch it.
7. Press **F5** to continue. The app should display the blank-input message. F5 resumes a paused app; it does not start a second copy in this situation.
8. Dismiss the message, stop debugging, and press F9 on the breakpoint line to remove it.

If the red dot is hollow or the breakpoint is not hit, check that the selected startup project is `CustomPC` and that you started with F5. If the app is paused, its controls may not respond until you continue.

**Checkpoint:** When the yellow arrow is at the Login call, has that call finished?

**Answer:** No. The debugger is paused before executing that line.

Stop here for your first reading session. You have found a control, followed its event, read its handler, and observed it running. Continue when you can explain those steps in your own words.

## 6. Follow the call into AppStore

**Goal:** understand how a method receives input and remembers the signed-in user.

In `LoginForm.cs`, put the cursor on the `Login` word in `AppStore.Current.Login(...)` and press **F12**. It should open `AppStore.Accounts.cs` at:

```csharp
public void Login(string email, string password)
```

Here `string` means text. `email` and `password` are **parameters**: names for the inputs this method receives. Inside this method, `email` is a text value, rather than the form's TextBox control. The form passed its `email.Text` value to it.

Find the first condition:

```csharp
if (string.IsNullOrWhiteSpace(email) || password.Length == 0)
{
    throw new InvalidOperationException("Enter your email address and password.");
}
```

`if` runs its block when its condition is true. `IsNullOrWhiteSpace` detects missing or blank text. `Length` counts the password's characters. `==` compares values; `||` means either condition is enough. `throw` raises the error that the form catches in lesson 4.

Next, `Refresh()` reloads saved records. Find:

```csharp
UserAccount? matchingAccount = null;
```

A **variable** is a name for a value the method is using. This variable will hold a user account. `null` means no account is selected yet. The `?` says this value is allowed to be missing.

The `foreach` loop checks one account at a time from `Users`, the collection of loaded accounts. It compares emails and ignores archived accounts. An **archived account** is disabled for login, with its saved record retained. When the loop finds a match, it assigns that account to `matchingAccount`; `break` stops the loop. `VerifyPassword` then checks the entered password against the saved password hash. A **hash** is a value produced from a password for verification; the database does not store the password as plain text. Leave the hashing algorithm for the reference document until you are comfortable with this flow.

At the end, find:

```csharp
Session = matchingAccount;
```

`Session` holds the currently signed-in account in memory. Memory is the app's working data while it runs. `Logout()` sets Session to null, meaning no signed-in account. Closing the app also ends its session.

**Trace task:** write this chain on paper. Locate the two methods, `LoginButton_Click` and `Login`, and the two assignments to `Session` and `DialogResult` in the source:

```text
Click Sign in → LoginButton_Click → AppStore.Login
             → Session assigned → DialogResult.OK → dashboard resumes
```

For a successful retry, the initial demo Admin is `admin@custompc.local` with `CustomPC123!`. Those credentials apply to initial demo data; an edited account keeps its saved password.

An account's **role** determines its permitted actions: Customer orders for themselves, Staff handles store operations such as payments and stock adjustments, and Admin also manages accounts, parts, suppliers, and rules. The reference has the full permissions table.

**Checkpoint:** Which object checks the password: the login window or AppStore?

**Answer:** AppStore checks it. The window reads the input, calls AppStore, and displays the result.

## 7. Understand the data and the builder without placing an order

**Goal:** connect a displayed price to a saved record and a calculation.

Open `Models.cs` and find `public class Part`. A **model** describes the information belonging to one record. In this case, one Part object represents one component:

```csharp
public string Name { get; set; } = "";
public decimal Price { get; set; }
public int Stock { get; set; }
```

`Name` stores text. `Price` stores a decimal number suitable for money. `Stock` stores a whole number. `{ get; set; }` permits code to read and assign the property. The empty string `""` is Name's starting value. `Part : DatabaseRecord` also gives the part an `Id`, used to identify the same record even if its name changes.

Open `BuilderForm.cs` and find `UpdateBuildSummary`. Read this excerpt:

```csharp
List<Part> selectedParts = GetSelectedParts();
List<string> errors = store.CheckBuild(selectedParts);
decimal buildTotal = 0;

foreach (Part part in selectedParts)
{
    buildTotal += part.Price;
}

total.Text = "Build total: " + Ui.Money(buildTotal);
```

`List<Part>` means a collection of Part objects. `GetSelectedParts()` returns the chosen components; `=` puts that result into `selectedParts`. `List<string>` is a collection of text messages. `CheckBuild` puts compatibility errors in that collection.

The total starts at zero. `foreach` visits each chosen part. Here `+=` adds its price to the running total. The same symbol connected an event in lesson 3; what it does depends on the values being used. `Ui.Money` formats a number like `PHP 1,000.00`; assigning the finished text to `total.Text` displays it.

**Try it:** run, Browse as guest, and open PC builder. Choose a CPU and note the total. Add a motherboard and check that the total is their two prices added together. Add RAM; the messages should still list the other missing categories. CPU Ryzen 5 5600, B550M motherboard, and DDR4 RAM are matching sample choices when those parts remain available. Switching to an Intel CPU while keeping the B550M board should show a socket mismatch. As guest, confirmation remains disabled.

Find `PartSelectionChanged`: it calls `UpdateBuildSummary`. This explains why changing a dropdown updates the screen immediately.

**Checkpoint:** Does changing a dropdown create an order?

**Answer:** No. It recalculates the preview. Creating an order happens later in `SubmitButton_Click`, through `CreateOrder`.

## 8. Follow a save using a separate practice database

**Goal:** see the full path from an input to saved data.

This lesson changes stock. Use a separate practice database so the exercise has its own records. Stop any current app run, open PowerShell, and run:

```powershell
$env:CUSTOMPC_DATABASE_PATH = Join-Path $env:TEMP 'CustomPC-Learning\guide.db'
dotnet run --project 'C:\Users\Jerald\Desktop\CustomPC\CustomPC\CustomPC.csproj'
```

The first line chooses a database for apps started from **this PowerShell process**. It does not change an already-open Visual Studio's F5 environment. The second line builds and runs this project. On the first launch at this new path, the app creates demo data. Later launches reuse that practice data.

1. Sign in as `staff@custompc.local`, password `CustomPC123!` in the fresh practice database.
2. Open Parts & inventory. Select one component row and note its **OnHand** number.
3. Click **Adjust stock**. Set **Quantity change** to `2`, enter `Learning exercise` in **Reason / supply reference**, and click **Record adjustment**.
4. Expect OnHand to increase by 2. Open **Movement history**. This opens the stock movements report for all parts; find the row matching your component, quantity 2, and reason. Close the report.
5. Select the component again and click Adjust stock. Set Quantity change to `0` with the same reason, then click Record adjustment. Expect an error and no extra stock change. Close the unsaved editor.
6. Close the app. The PowerShell command should finish. Run the same `dotnet run` line again; the saved stock should remain.
7. Close the app. Remove the override from this PowerShell process:

```powershell
Remove-Item Env:\CUSTOMPC_DATABASE_PATH
```

The practice database remains in `%TEMP%\CustomPC-Learning\guide.db`; removing the environment variable does not delete it. Keep it for later practice. Future runs from this shell now use the default database unless another override is supplied.

Now open `StockAdjustmentForm.cs` and find `SaveClicked`. Its important lines are:

```csharp
int stockChange = (int)quantity.Value;
AppStore.Current.AdjustStock(partId, stockChange, reason.Text);
DialogResult = DialogResult.OK;
```

`quantity.Value` reads the numeric control. `(int)` converts its value to a whole number; `stockChange` stores that number. `partId` identifies the selected component. `reason.Text` reads the explanation. `AdjustStock` validates and saves before the form reports OK.

Press F12 on `AdjustStock` to reach `AppStore.Inventory.cs`. It reloads saved records, requires Staff/Admin access, rejects zero or a blank reason, checks stock limits and reservations, changes the Part, records the movement, and calls `SaveData`.

Find `SaveData` in `AppStore.cs`. It calls `database.WriteRecords` for each collection, then `transaction.Commit()`. A **transaction** groups the related database writes so they succeed together. If saving fails, `UndoFailedChange` restores both saved data and the loaded lists.

Find `WriteRecords` in `Database.cs`. It writes records into SQLite, a database kept in a file. This project stores a record as JSON: text containing named values such as `Name` and `Stock`. Serialization converts the C# record to that text; loading converts it back to an object.

The path you just followed is:

```text
Save click → StockAdjustmentForm.SaveClicked
           → AppStore.AdjustStock → AppStore.SaveData
           → Database.WriteRecords → SQLite file
```

The inventory screen then loads its rows again. Its DataTable is the set of columns and rows prepared for display in the grid; that display table is different from the SQLite file holding saved records.

**Checkpoint:** If you assign `part.Stock = 20` in memory, have you already saved 20 to disk?

**Answer:** No. The program must successfully write and commit the database change. Memory and the saved file are separate.

## 9. Use what you learned to read the rest

You can now follow one button at a time. Open its form, find the event connection, read the handler, then follow the AppStore method it calls. Return to the screen and compare the visible result with that code.

| What you want to understand | Open first | Follow next |
| --- | --- | --- |
| Why the dashboard opens first | `Program.cs`: `Application.Run(new DashboardForm())` | `DashboardForm_Shown`, then `SignIn` in `DashboardForm.cs` |
| Customer registration | `RegisterForm.cs`: `SaveButton_Click` | `SaveUser` in `AppStore.Accounts.cs` |
| Catalog search | `CatalogForm.cs`: `FilterChanged` | `LoadRows` in the same file |
| Build errors | `BuilderForm.cs`: `UpdateBuildSummary` | `CheckBuild` in `AppStore.Compatibility.cs` |
| Creating an order | `BuilderForm.cs`: `SubmitButton_Click` | `CreateOrder` in `AppStore.Orders.cs` |
| Payment and assembly stages | `OrdersForm.cs`: `PayButton_Click`, `AdvanceButton_Click` | `ConfirmPayment`, `AdvanceOrder` in `AppStore.Orders.cs` |
| Supplier, user, part, or rule edits | The corresponding edit form's `SaveClicked` | `SaveSupplier`, `SaveUser`, `SavePart`, or `SaveRule` |
| Report totals | `ReportsForm.cs`: `LoadReport` | The selected report's loader in the same file |
| Saved data returning after restart | `AppStore.cs`: `LoadData` | `ReadRecords` in `Database.cs` |

Start `Program.cs` by reading `Application.Run(new DashboardForm())` as “create a dashboard window and keep the app running.” `new` creates an object. The dashboard opens LoginForm when shown, which is why both windows appear at startup.

The `AppStore.*.cs` files use `partial` just like the form files: they are sections of **one class**, grouped by subject. `AppStore.Accounts.cs` is not a second store or a second database.

For ordering, learn the stock formula before the longer methods:

```text
Available = On hand − Reserved
```

With 10 units on hand, a pending order reserves 1: available becomes 9, while on hand stays 10. At completed pickup, on hand becomes 9 and the reservation ends. Cancelling an unpaid order ends its reservation without reducing on hand. See the worked table in the reference when tracing `Reserved` and `Available`.

For any feature, answer these four questions before moving on:

1. Which control starts the action?
2. Which method handles it?
3. Where is invalid input rejected?
4. Does this action only change the screen, or does it save records?

If you cannot answer one, return to that part of the flow. You do not need to memorize all 17 forms. The [reference](CODE_REFERENCE.md) provides their map, less common C# expressions, password hashing, database details, and the advanced manual validation checklist.
