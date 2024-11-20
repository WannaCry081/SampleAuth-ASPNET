namespace sample_auth_aspnet.Services.Email;

public static class EmailTemplate
{
    public static string GetForgotPasswordTemplate()
    {
        return (
            """
            <!DOCTYPE html>
            <html lang="en">
            <head>
                <meta charset="UTF-8" />
                <meta name="viewport" content="width=device-width, initial-scale=1.0" />
                <title>Reset Your Password</title>
                <style>
                body {
                    font-family: Arial, sans-serif;
                    background-color: #f9f9f9;
                    color: #333;
                    line-height: 1.6;
                    margin: 0;
                    padding: 0;
                }
                .email-container {
                    max-width: 600px;
                    margin: 20px auto;
                    background-color: #ffffff;
                    border: 1px solid #ddd;
                    border-radius: 8px;
                    padding: 20px;
                    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
                }
                .header {
                    text-align: center;
                    margin-bottom: 20px;
                }
                .header h1 {
                    color: #444;
                    font-size: 24px;
                    margin: 0;
                }
                .content {
                    margin-bottom: 20px;
                }
                .content p {
                    margin: 10px 0;
                }
                .reset-link {
                    display: inline-block;
                    background-color: #d9534f;
                    text-decoration: none;
                    padding: 10px 20px;
                    border-radius: 4px;
                    font-size: 16px;
                    font-weight: bold;
                }
                .footer {
                    text-align: center;
                    font-size: 12px;
                    color: #888;
                    margin-top: 20px;
                }
                </style>
            </head>
            <body>
                <div align="center">
                <h1>Reset your Team Password</h1>
                </div>
                <div class="email-container">
                <div class="header">
                    <h2>Password Reset</h2>
                </div>
                <div class="content">
                    <p>Dear {{email}},</p>
                    <p>
                    We heard that you lost your password. Sorry about that! <br />
                    But don't worry! You can use the following button to reset your
                    password:
                    </p>
                    <p style="text-align: center">
                    <a href="{{reset_link}}" class="reset-link" style="color: #ffffff"
                        >Reset Password</a
                    >
                    </p>
                    <p style="margin-bottom: 10px">
                    If you did not request this, you can safely ignore this email.
                    </p>

                    <p>
                    Thanks, <br />
                    Data Team
                    </p>
                </div>
                </div>
                <div class="footer">
                <p>© 2024 Sample Authentication App. All rights reserved.</p>
                </div>
            </body>
            </html>
            """
        );
    }
}
