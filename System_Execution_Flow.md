# ByteSheild: Under the Hood - System Execution Flow

This document provides a comprehensive, step-by-step technical walkthrough of the ByteSheild application. It maps out the logical flow of data and execution from the moment the user launches the app to their interactions with core features. 

---

## 1. Phase 1: Application Launch & Initialization
**Files Involved:** `MauiProgram.cs`, `App.xaml.cs`, `.env`

When the user taps the ByteSheild app icon, the operating system triggers the application lifecycle.

*   **The Builder Pattern (`MauiProgram.cs`):** The app starts by invoking `MauiProgram.CreateMauiApp()`. This uses the .NET generic host builder to configure fonts, lifecycle events, and dependencies.
*   **Dependency Injection (DI):** 
    *   `DatabaseService` is registered as a *Singleton* (`AddSingleton`). This ensures that across the entire application, only one SQLite connection pool is created, optimizing memory and preventing file-lock issues.
    *   Pages like `OfflineVaultPage` and `AddVaultItemPage` are registered as *Transient* (created fresh on every request) to ensure stale UI states aren't carried over.
*   **Environment Variable Loading:** The system securely reads the embedded `.env` file using the `DotNetEnv` library. This injects sensitive configuration (like the HIBP API key) into the runtime environment (`Environment.GetEnvironmentVariable`) without hardcoding it in the source files.

## 2. Phase 2: The Security Gatekeeper (Authentication)
**Files Involved:** `BiometricPage.xaml.cs`, `SplashPage.xaml.cs` (if applicable)

Before any sensitive data is loaded, the app forces a security check.

*   **Lifecycle Hook:** Inside `BiometricPage.xaml.cs`, the `OnNavigatedTo` event triggers as soon as the page is visible.
*   **Hardware Integration:** The system invokes `CrossFingerprint.Current.AuthenticateAsync()` (using `Plugin.Fingerprint`). It asks the OS (Android/iOS/Windows) to present the native biometric prompt (Face ID / Fingerprint).
*   **Fallback Logic:** If the user fails biometrics or cancels, the system gracefully falls back to a PIN mechanism. It reads the securely encrypted `AppPasscode` from the hardware-backed keystore using `Microsoft.Maui.Storage.SecureStorage.Default`.
*   **Routing:** Once `result.Authenticated` is true, or the PIN matches, the app uses `Shell.Current.GoToAsync("//MainDashboardPage")` to transition the user securely to the main interface.

## 3. Phase 3: The Dashboard (Central Hub)
**Files Involved:** `MainDashboardPage.xaml.cs`

Once authenticated, the dashboard aggregates system states and provides real-time computations.

*   **Cross-page State Loading:** In the `OnAppearing` override, it instantiates the `DatabaseService` (via the DI container) to count the total stored vault items. It also reads `Preferences.Default` to retrieve the latest "Email Breach Status" cached by the `BreachCheckerPage`.
*   **Real-Time Computations (Password Strength):**
    *   When a user types in the quick password checker, the `OnPasswordTextChanged` event fires.
    *   **High Performance Regex:** Instead of compiling Regular Expressions on every keystroke, the app utilizes compile-time source-generated regex (`[GeneratedRegex]`). The system checks for uppercase, lowercase, digits, and symbols in microseconds.
    *   **Dynamic UI Binding:** The dashboard calculates a score (0 to 5) and dynamically updates SVG strokes, rings, and color codes (Danger, Warning, Success) without pausing the UI thread.

## 4. Phase 4: Data Persistence (The Offline Vault)
**Files Involved:** `Services/DatabaseService.cs`, `Models/VaultItemModel.cs`, `OfflineVaultPage.xaml.cs`

This phase handles the core offline password manager functionality.

*   **SQLite Initialization:** The `DatabaseService` relies on `sqlite-net-pcl`. On the very first request (e.g., `GetVaultItemsAsync`), the `Init()` method is called. It locates the application's isolated AppData directory (`FileSystem.AppDataDirectory`) and creates `ByteSheildVault.db3`.
*   **ORM Mapping:** `Init()` also calls `CreateTableAsync<VaultItemModel>()`. This scans the `VaultItemModel.cs` class for attributes like `[PrimaryKey, AutoIncrement]` and maps the C# object directly to a SQL database table.
*   **Offline First CRUD:** When a user adds or deletes a password from the `OfflineVaultPage`, the system executes asynchronous SQL commands (`InsertAsync`, `UpdateAsync`, `DeleteAsync`). Everything happens locally on the device—no data is ever transmitted to the cloud, ensuring strict user privacy.

## 5. Phase 5: External API Integration (Breach Checker)
**Files Involved:** `BreachCheckerPage.xaml.cs`

The singular area where the app communicates with the outside world is to verify email safety.

*   **Validation:** When "Check Now" is clicked, `EmailFormatRegex().IsMatch()` ensures the input is a structurally valid email. This prevents the app from making wasteful network boundaries calls.
*   **Secure API Requests:**
    *   The app uses `HttpClient`. It retrieves the HIBP API key from the environment and injects it securely into the Request Headers (`hibp-api-key`).
    *   It URL-encodes the email to prevent injection attacks and sends an asynchronous `GET` request to the *HaveIBeenPwned* v3 API.
*   **Response Parsing & State Management:**
    *   **Success (200 OK):** The JSON response is deserialized into a `List<BreachModel>`. It counts the breaches and extracts and displays the specific sources of the leaks. `Preferences.Default` is updated with `EmailIsSafe = false` and cached.
    *   **Not Found (404):** Means the API successfully searched but found no breaches. `Preferences.Default` is updated with `EmailIsSafe = true` and cached.
    *   Exceptions and unexpected HTTP codes (like 429 Too Many Requests or 401 Unauthorized) are gracefully caught and presented as human-readable display alerts.
*   **Dynamic UI Reset:** The `OnEmailTextChanged` event is attached to the email input. If the user begins typing, clears, or alters a previously searched email address, the UI dynamically clears out the previous result container and resets the global security preference to "UNKNOWN".

---

## Technical Summary for Panelists

ByteSheild is architected with a strict separation of concerns and a focus on **Offline-First Security**. 
- Application state and native hardware features (Biometrics, Secure Storage) guard the entry.
- External interaction is heavily isolated to a specific page (`BreachCheckerPage`) and authenticated via hidden environments variables.
- Internal persistent data relies entirely on a fast, asynchronous, local implementation of SQLite, ensuring zero knowledge architecture for vault credentials.

---

## 6. App Architecture Flow Diagram

Below is a visual representation of how a user navigates and data flows through ByteSheild:

```mermaid
graph TD
    %% Define Styles
    classDef startNode fill:#4CAF50,stroke:#2E7D32,color:white,stroke-width:2px
    classDef pageNode fill:#2196F3,stroke:#1565C0,color:white,stroke-width:2px
    classDef serviceNode fill:#FF9800,stroke:#EF6C00,color:white,stroke-width:2px
    classDef dataNode fill:#9C27B0,stroke:#6A1B9A,color:white,stroke-width:2px

    %% App Launch
    A[User Launches App]:::startNode --> B(MauiProgram.cs<br/>Config & DI):::pageNode
    B --> C(Hardware Check / OnNavigatedTo)

    %% Authentication
    C --> D{BiometricPage}:::pageNode
    D -- Biometrics Succeed --> E(MainDashboardPage)
    D -- Biometrics Fail --> F{Passcode Fallback}
    F -- Passcode Correct --> E
    F -- Passcode Incorrect --> D

    %% Dashboard (Main Hub)
    E:::pageNode --> G(Vault Service Check)
    E --> H(Email Breach Check)
    E --> I[Password Strength Sandbox]

    %% Realtime Computations
    I -->|User Typing| J(Regex Pattern Check)
    J --> K(UI Update: Ring & Color)

    %% Vault Operations
    G --> L[OfflineVaultPage]:::pageNode
    L --> M(AddVaultItemPage):::pageNode
    L --> N[DatabaseService.cs]:::serviceNode
    M --> N
    
    %% SQLite
    N -- CRUD Operations --> O[(Local SQLite DB<br/>ByteSheildVault.db3)]:::dataNode

    %% Network Operations
    H --> P[BreachCheckerPage]:::pageNode
    P -- Reads Env Var --> Q(HIBP API Key)
    P -- Validates Format --> R(Regex Email Check)
    R -- HttpClient Get --> S{{Have I Been Pwned API}}
    
    %% API Return
    S -- HTTP 200 --> T(Set Status: AT RISK)
    S -- HTTP 404 --> U(Set Status: SECURE)
    
    T --> V(Preferences.Default Cached)
    U --> V(Preferences.Default Cached)
    
    %% Cache back to Dashboard
    V -.->|OnAppearing Updates| E
