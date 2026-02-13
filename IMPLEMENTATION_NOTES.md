# Implementation Notes - Login Screen

## Changes Made

### MainPage.xaml
- Converted the main page to a login screen with:
  - Welcome label for "Sistema Gestor de Parqueaderos Móvil Sigepark"
  - Username entry field (txtEmail)
  - Password entry field (txtPassword)
  - Login button
  - Result label for feedback messages
- Removed the ScrollView and simplified the layout
- Changed title to "Inicio de sesión"

### MainPage.xaml.cs
- Replaced the counter logic with login validation logic
- Added `OnLoginClicked` event handler that:
  - Validates that both username and password fields are not empty
  - Shows "Iniciando sesión..." in blue while processing
  - Calls the real authentication API via AuthService
  - Shows appropriate success or error messages based on API response
- Removed the counter-related code
- **✅ IMPLEMENTED**: Real authentication with backend API

### Models/AuthModels.cs
- Added `LoginRequest` class with Email and Password properties
- Added `AuthResult` class with Success and Message properties
- Used for communication with the authentication service

### Services/AuthService.cs
- Created authentication service that communicates with UserApi backend
- Implements `LoginAsync` method that:
  - Sends POST request to `/api/accounts/login` endpoint
  - Includes `ngrok-skip-browser-warning: true` header to bypass ngrok warning page
  - Handles successful login (200 OK) responses
  - Handles authentication failures (401 Unauthorized)
  - Provides robust error handling for network issues, timeouts, and other exceptions
  - Returns clear error messages to the user

### MauiProgram.cs
- Configured HttpClient with base URL: `https://subpar-overidly-meta.ngrok-free.dev`
- Set timeout to 30 seconds
- Registered AuthService as singleton in DI container
- Registered MainPage in DI container for proper dependency injection

## Backend API Configuration

### Current API Endpoint
**Base URL**: `https://subpar-overidly-meta.ngrok-free.dev`
**Login Endpoint**: `POST /api/accounts/login`

### Changing the API URL
To change the backend API URL (when ngrok URL changes or deploying to production):
1. Open `MauiProgram.cs`
2. Locate the `AddHttpClient<AuthService>` configuration
3. Update the `BaseAddress` to the new URL
4. The app does not need recompilation if using app settings (future enhancement)

### Important Notes about ngrok
- The current URL uses ngrok for temporary public access to the development API
- ngrok URLs require the `ngrok-skip-browser-warning: true` header to bypass the browser warning page
- This header is automatically included in all requests from AuthService
- When deploying to production with a permanent URL, this header can be removed (though it won't hurt to keep it)

## API Request/Response Format

### Request Body
```json
{
  "Email": "usuario@ejemplo.com",
  "Password": "contraseña123"
}
```

### Success Response (200 OK)
```
"Login correcto"
```

### Error Response (401 Unauthorized)
```
"Credenciales incorrectas"
```

## Error Handling
The application handles the following error scenarios:
- **Invalid credentials**: Shows "Credenciales incorrectas" message from API
- **Network errors**: Shows "Error de conexión. Verifica tu conexión a internet."
- **Timeout**: Shows "La solicitud tardó demasiado tiempo. Intenta nuevamente."
- **Server errors**: Shows "Error del servidor: [StatusCode]"
- **Unexpected errors**: Shows descriptive error message

## Pending Items

### Image Replacement
Currently using `dotnet_bot.png` as the logo image. To complete the implementation:
1. Add the `sigsinfondo.png` image to `SigeParkApp/Resources/Images/`
2. Update line 14 in `MainPage.xaml` to change:
   ```xml
   Source="dotnet_bot.png"
   ```
   to:
   ```xml
   Source="sigsinfondo.png"
   ```

### Future Enhancements
The current implementation includes commented placeholders for:
- Navigation to HomePage after successful login
- Optional ticket preview button
- JWT token management (if/when the API adds token-based authentication)
- Persistent configuration for API base URL (app settings or environment variables)
- Remember me / saved credentials functionality

