# 🔒 Sample Authentication System [ASP.NET]

This project demonstrates an authentication system developed using .NET 8 in ASP.NET Core WebAPI. It showcases user authentication and refresh token management. Additionally, it provides a comprehensive template for building a custom authentication system from scratch, complete with industry-standard responses, validation, token handling, background services, and containerization.

## 🚀 Features

- **Authentication**: Allows users to register and log in, returning responses in an industry-standard format.
- **SMTP**: Supports password reset by sending a recovery email to the user with further instructions.
- **Token Handling**: Efficiently manages refresh tokens for authenticated users.
- **Background Services**: Automatically removes revoked tokens stored as blacklisted entries in the database without explicitly calling a controller.
- **Containerization**: Uses Docker to containerize the application, with MsSQL as the project's database.

## 🛠️ Technologies

- **GIT**: Version control system
- **.NET 8**: Latest .NET framework version
- **ASP.NET Core WebAPI**: API framework for building web applications
- **MsSQL**: Database management system
- **Docker**: Containerization platform for deploying and running the application

## ⚙️ Installation

1. Clone the repository:

   ```bash
   git clone https://github.com/WannaCry081/SampleAuth-ASPNET.git
   ```

2. Update the connection string in appsettings.json to match your configuration:

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Your Connection String Here"
     }
   }
   ```

3. Build and run the program:

   ```bash
   dotnet build   # Builds the application
   dotnet run     # Runs the application
   ```

4. Alternatively, If Docker is installed on your system, you can start the application with Docker Compose. Ensure you have a `.env` file (see below) in the project directory for configuring the environment variables.

   ```bash
   docker compose --env-file=./.env up
   ```

   The application should now be running at [http://localhost:5026/](http://localhost:5026/swagger/index.html)

5. Create a `.env` file in the root directory to configure environment variables for docker-compose.yml. Below is a sample template for your `.env` or check out the `.env.example` file:

   ```
   # Application Base URL
   APPLICATION_URL="http://localhost:5026"

   # Database connection string
   DEFAULT_CONNECTION="YourDatabaseConnectionStringHere"

   # JWT Configuration
   JWT_SECRET="YourJWTSecretHere"
   JWT_ISSUER="YourJWTIssuerHere"
   JWT_AUDIENCE="YourJWTAudienceHere"
   JWT_ACCESS_TOKEN_EXPIRY=1             # Access token expiry in hours
   JWT_REFRESH_TOKEN_EXPIRY=1            # Refresh token expiry in days
   JWT_RESET_TOKEN_EXPIRY=10             # Reset token expiry in minutes

   # SMTP Credentials
   SMTP_USERNAME="YourSMTPUsernameHere"
   SMTP_PASSWORD="YourSMTPPasswordHere"
   ```

## 📖 Usage

After starting the application, you can access the following features:

- **User Registration**: Register a new user by sending a POST request to `/api/v1/auth/register` with the necessary user information.
- **User Login**: Log in with registered credentials by sending a POST request to `/api/v1/auth/login`. This will return an access token and a refresh token.
- **Token Refresh**: Refresh the access token using a valid refresh token by sending a POST request to `/api/v1/auth/refresh`.
- **User Logout**: Log out and invalidate the current refresh token by sending a POST request to `/api/v1/auth/logout`.
- **Request Password Reset**: Send a POST request to `/api/v1/auth/forgot-password` with the user's email. This triggers a password reset email with further instructions.
- **Reset Password**: Complete the password reset by sending a POST request to `/api/v1/auth/reset-password` with a new password and the reset token received in the email.
- **View User Profile**: Access the profile information of the logged-in user by sending a GET request to `/api/v1/users/me`.

Each endpoint ensures secure handling of authentication and user data, following industry best practices.

## 🤝 Contributing

If you'd like to contribute to this project, please follow these guidelines:

1. Fork the repository.
2. Create a new branch for your feature:
   ```bash
   git checkout -b feature/YourFeature
   ```
3. Commit your changes:
   ```bash
   git commit -am 'feat: add new feature'
   ```
4. Push to the branch:
   ```bash
   git push origin feature/YourFeature
   ```
5. Create a new Pull Request for review.

> [!NOTE]
> Ensure that all commits follow the [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0-beta.2/) specification for consistent and meaningful commit messages.

## 📬 Contact

For any questions or feedback, feel free to reach out:

- **Email:** liraedata59@gmail.com
- **GitHub:** [WannaCry081](https://github.com/WannaCry081)
