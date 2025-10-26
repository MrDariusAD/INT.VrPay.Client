# Repository Structure

This document describes the organization of the VrPay.Client repository.

## Overview

The repository follows a standard .NET solution structure with projects organized under `src/` and `tests/` folders. The solution uses the modern `.slnx` XML-based format introduced in Visual Studio 2022 17.8+.

## Directory Layout

```
INT.VrPay/
├── .github/              # GitHub-specific files
│   └── workflows/        # GitHub Actions CI/CD workflows
│       └── ci-cd.yml     # Main CI/CD pipeline
│
├── docs/                 # Documentation
│   ├── README.md         # Documentation index
│   ├── QUICK-REFERENCE.md
│   ├── IMPLEMENTATION-PLAN.md
│   ├── ENUM-REFACTORING.md
│   └── [01-09]-*.md      # Detailed documentation chapters
│
├── scripts/              # Build and automation scripts
│   ├── build.ps1         # Build script
│   ├── test.ps1          # Test execution script
│   ├── package.ps1       # NuGet packaging script
│   └── clean.ps1         # Clean build artifacts script
│
├── src/                  # Source code
│   ├── VrPay.slnx        # Solution file (new XML format)
│   ├── VrPay.Client/     # Main client library project
│   │   ├── Configuration/          # Configuration models
│   │   │   └── VrPayConfiguration.cs
│   │   ├── Converters/             # JSON converters
│   │   │   └── EnumMemberConverter.cs
│   │   ├── Exceptions/             # Exception hierarchy
│   │   │   ├── VrPayException.cs
│   │   │   ├── VrPayConfigurationException.cs
│   │   │   ├── VrPayPaymentDeclinedException.cs
│   │   │   ├── VrPayValidationException.cs
│   │   │   └── VrPayCommunicationException.cs
│   │   ├── Extensions/             # Extension methods
│   │   │   ├── PaymentResponseExtensions.cs
│   │   │   └── ServiceCollectionExtensions.cs
│   │   ├── Models/                 # Data models and enums
│   │   │   ├── AddressData.cs
│   │   │   ├── CardData.cs
│   │   │   ├── Currency.cs
│   │   │   ├── CustomerData.cs
│   │   │   ├── PaymentBrand.cs
│   │   │   ├── PaymentRequest.cs
│   │   │   ├── PaymentResponse.cs
│   │   │   ├── PaymentType.cs
│   │   │   ├── TestMode.cs
│   │   │   └── TransactionStatus.cs
│   │   ├── Services/               # Core services
│   │   │   └── ResultCodeAnalyzer.cs
│   │   ├── Testing/                # Test helpers
│   │   │   ├── TestCards.cs
│   │   │   └── TestData.cs
│   │   ├── IVrPayClient.cs
│   │   ├── VrPayClient.cs
│   │   └── VrPay.Client.csproj
│   │
│   └── Tests/            # Test projects
│       ├── VrPay.Client.Tests/              # Unit tests
│       │   ├── Configuration/
│       │   │   └── VrPayConfigurationTests.cs
│       │   ├── Services/
│       │   │   └── ResultCodeAnalyzerTests.cs
│       │   └── VrPay.Client.Tests.csproj
│       │
│       └── VrPay.Client.IntegrationTests/   # Integration tests
│           ├── appsettings.json
│           ├── appsettings.Development.json
│           ├── IntegrationTestFixture.cs
│           ├── PaymentIntegrationTests.cs
│           └── VrPay.Client.IntegrationTests.csproj
│
├── test-console/         # Console app for manual testing
│   ├── Program.cs
│   └── VrPay.TestConsole.csproj
│
├── .editorconfig         # Code style configuration
├── .gitignore            # Git ignore rules
├── Directory.Build.props # MSBuild properties for all projects
├── LICENSE               # MIT License
└── README.md             # Main documentation
```

## Key Files

### Root Level

- **`.editorconfig`**: Defines coding style and conventions for the entire solution
- **`.gitignore`**: Specifies files and folders to be ignored by Git
- **`Directory.Build.props`**: MSBuild properties inherited by all projects (versioning, package metadata, nullable reference types, etc.)
- **`README.md`**: Main documentation with quick start guide and usage examples
- **`LICENSE`**: MIT License file

### src/VrPay.slnx

The solution file uses the new `.slnx` format (XML-based) instead of the traditional `.sln` format. This format is:
- More readable and easier to diff
- Simpler structure with less boilerplate
- Fully compatible with Visual Studio 2022 17.8+ and dotnet CLI

**Structure:**
```xml
<Solution>
  <Folder Name="/src/">
    <Project Path="VrPay.Client\VrPay.Client.csproj" Type="C#" />
  </Folder>
  <Folder Name="/src/">
    <Project Path="VrPay.Client\..." />
    <Folder Name="/Tests/">
      <Project Path="Tests\VrPay.Client.Tests\..." />
      <Project Path="Tests\VrPay.Client.IntegrationTests\..." />
    </Folder>
  </Folder>
</Solution>
```

### src/VrPay.Client/

The main client library organized by functionality:

- **`Configuration/`**: Configuration models and validation
- **`Converters/`**: Custom JSON converters for enum serialization
- **`Exceptions/`**: Strongly-typed exception hierarchy
- **`Extensions/`**: Extension methods for DI and response analysis
- **`Models/`**: Request/response models and enums
- **`Services/`**: Business logic services (result code analysis)
- **`Testing/`**: Test helpers and predefined test data

### src/Tests/

Two test projects organized under the src folder:

1. **VrPay.Client.Tests**: Unit tests (52 tests)
   - Fast, isolated tests
   - No external dependencies
   - Test configuration, validation, result code analysis

2. **VrPay.Client.IntegrationTests**: Integration tests (8 tests)
   - Test real API interactions
   - Require valid VrPay credentials
   - 6 tests skipped by default (need credentials)

### .github/workflows/

CI/CD pipeline configuration:
- **`ci-cd.yml`**: GitHub Actions workflow
  - Builds on every push/PR
  - Runs unit tests
  - Creates NuGet packages on release
  - Publishes to GitHub Packages

### scripts/

PowerShell automation scripts:
- **`build.ps1`**: Clean, restore, and build the solution
- **`test.ps1`**: Run tests (unit, integration, or all) with optional coverage
- **`package.ps1`**: Create NuGet packages
- **`clean.ps1`**: Remove all build artifacts

## Building and Testing

### Using Scripts (Recommended)

```powershell
# Build
.\scripts\build.ps1 -Configuration Release

# Run unit tests
.\scripts\test.ps1 -TestType Unit

# Create package
.\scripts\package.ps1
```

### Using dotnet CLI

```bash
# Build
dotnet build src/VrPay.slnx

# Test
dotnet test src/VrPay.slnx

# Package
dotnet pack src/VrPay.Client/VrPay.Client.csproj -c Release
```

### Using Visual Studio

Open `src/VrPay.slnx` in Visual Studio 2022 (version 17.8 or later).

## Project Dependencies

### VrPay.Client

**Runtime Dependencies:**
- Microsoft.Extensions.Http (9.0.10)
- Microsoft.Extensions.Options (9.0.10)
- Polly (8.6.4)
- Polly.Extensions.Http (3.0.0)
- Microsoft.Extensions.Http.Polly (9.0.10)

### Test Projects

**Test Dependencies:**
- xUnit (2.9.2)
- xUnit.runner.visualstudio (2.8.2)
- FluentAssertions (8.8.0)
- Moq (4.20.72)
- Microsoft.NET.Test.Sdk (17.12.0)
- coverlet.collector (6.0.2)
- Microsoft.Extensions.Configuration.Json (9.0.0)
- Microsoft.Extensions.Configuration.EnvironmentVariables (9.0.0)
- Microsoft.Extensions.Logging.Console (9.0.0)

## Naming Conventions

- **Namespace**: `VrPay.Client[.SubNamespace]`
- **Assembly**: `VrPay.Client.dll`
- **Package**: `VrPay.Client` (on NuGet)
- **Test Projects**: `VrPay.Client.[Type]Tests`

## Version Management

Versioning is centrally managed in `Directory.Build.props`:

```xml
<VersionPrefix>1.0.0</VersionPrefix>
<VersionSuffix></VersionSuffix>
```

The final version is: `$(VersionPrefix)$(VersionSuffix)` (e.g., `1.0.0` or `1.0.0-beta`)

## Solution Upgrade Notes

### Why .slnx?

The repository was upgraded from `.sln` to `.slnx` format for:

1. **Better Readability**: XML format is human-readable and easier to understand
2. **Easier Diffs**: Changes are clearer in version control
3. **Simpler Structure**: Less boilerplate compared to the old format
4. **Future-Proof**: Microsoft's recommended format going forward
5. **Better Tooling Support**: Better integration with modern build tools

### Compatibility

- ✅ Visual Studio 2022 (17.8+)
- ✅ dotnet CLI (all versions supporting .NET 9.0)
- ✅ VS Code with C# Dev Kit
- ✅ JetBrains Rider (2023.3+)
- ❌ Visual Studio 2019 (does not support .slnx)

If you need to use VS 2019, you can create a temporary `.sln` file:
```bash
dotnet new sln -n VrPay
dotnet sln add src/VrPay.Client/VrPay.Client.csproj
dotnet sln add tests/**/*.csproj
```

## Contributing

When adding new projects:

1. Create project in appropriate folder (`src/` or `tests/`)
2. Add to `src/VrPay.slnx` manually or use:
   ```bash
   dotnet sln src/VrPay.slnx add path/to/project.csproj
   ```
3. Ensure project follows naming conventions
4. Update this document if adding new top-level folders

## Additional Resources

- [.slnx Format Documentation](https://learn.microsoft.com/en-us/visualstudio/ide/solutions-and-projects-in-visual-studio)
- [MSBuild Directory.Build.props](https://learn.microsoft.com/en-us/visualstudio/msbuild/customize-your-build)
- [.editorconfig Documentation](https://editorconfig.org/)
