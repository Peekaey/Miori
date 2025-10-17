namespace ShoukoV2.Api;

public static class OAuthResponseBuilder
{
    public static string BuildSuccessPage()
    {
        return @"
            <!DOCTYPE html>
            <html>
            <head>
                <title>Success!</title>
                <style>
                    body { 
                        font-family: Arial, sans-serif; 
                        text-align: center; 
                        padding: 50px;
                    }
                    .message { 
                        padding: 30px; 
                        border-radius: 10px;
                        max-width: 500px;
                        margin: 0 auto;
                    }
                </style>
            </head>
            <body>
                <div class='message'>
                    <h1>Success!</h1>
                    <p>Authentication successful</p>
                    <p>This window can safely be closed</p>
                </div>
            </body>
            </html>
        ";
    }

    public static string BuildErrorPage(string errorMessage)
    {
        return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <title>Error</title>
                <style>
                    body {{ 
                        font-family: Arial, sans-serif; 
                        text-align: center; 
                        padding: 50px;
                    }}
                </style>
            </head>
            <body>
                <h1 class='error'>{errorMessage}</h1>
                <p>Please close the window and try again</p>
            </body>
            </html>
        ";
    }
}