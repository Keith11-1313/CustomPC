# Installation

## Windows package

Requires 64-bit Windows 10 build 19041 or later, or Windows 11.

1. Download the application ZIP from [Releases](https://github.com/Keith11-1313/CustomPC/releases).
2. Extract the complete ZIP into a folder your Windows account can write to.
3. Open `CustomPC.exe`.

Keep the EXE, supporting libraries, and asset folders together. The self-contained package includes .NET; Visual Studio is not needed. GitHub's automatically generated source archive is not the runnable application package.

## First run

With no accounts, **Set up CustomPC** creates the first Administrator. Choose a personal password with 12–128 characters, uppercase and lowercase letters, a number, and a symbol. It must differ from the email address.

Admin adds Staff accounts and products. Customers can register from login. New products start at zero stock; Staff records receipts through **Adjust stock**.

A fresh database does not include the researched product catalog. If authorized store data accompanies the package, its saved accounts and records are used instead. Obtain account access from the person responsible for that copy.

## Store data

```text
Database/
├── custompc.db
├── ProductImages/
└── Backups/
```

In a source checkout, `Database` is beside `CustomPC.slnx`. Outside the checkout, it is beside the application. `CUSTOMPC_DATABASE_PATH` overrides this location for testing.

The database and photographs are ignored by Git. Keep them together when transferring records. Close the application before manually copying files; use the app's ZIP backup for routine recovery. Copies on different computers do not synchronize.

## Build from source

Install the .NET 10 SDK. Visual Studio users also need the .NET desktop development workload.

From the repository root:

```powershell
dotnet restore CustomPC.slnx
dotnet build CustomPC.slnx -c Release
dotnet run --project CustomPC/CustomPC.csproj
```

NuGet restore requires internet access unless dependencies are cached. Normal operations use local storage.

In Visual Studio, open `CustomPC.slnx` and press F5. To edit a form, right-click its `.cs` file and choose **View Designer**. Generated `bin` and `obj` files are build caches, not source or release packages.

## Publish

Right-click the CustomPC project in Visual Studio and choose **Publish → Folder**. Use Release, win-x64, and Self-contained. Leave single-file publishing, ReadyToRun, and trimming disabled for the standard folder package.

Equivalent command from the repository root:

```powershell
dotnet publish CustomPC/CustomPC.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=false -p:PublishTrimmed=false -p:PublishReadyToRun=false -o artifacts/publish
```

Test the output outside the source checkout. Package the entire output folder, not just the EXE. Public downloads should contain only intended demonstration data, not operational records or account credentials.

See [Testing](VALIDATION.md) before distributing a build and [the user guide](STORE_GUIDE.md#activity--backups) for recovery.
