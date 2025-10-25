# Buttercup

## Setting up a dev environment

1.  Install required tools and dependencies:

    - [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
    - [Mailpit](https://mailpit.axllent.org/)
    - [MySQL Server](https://dev.mysql.com/downloads/mysql/)
    - [Node.js 20](https://nodejs.org)
    - [Redis](https://redis.io/open-source/) or [Valkey](https://valkey.io/download/)
    - [Visual Studio Code](https://code.visualstudio.com/) with [recommended
      extensions](.vscode/extensions.json)

2.  Restore .NET tools:

        dotnet tool restore

3.  Create the _buttercup_dev_ user that will be used to connect to the
    application and test databases:

        mysql -u root -p < scripts/create-dev-user.sql

4.  Create the application database:

        dotnet ef database update -s src/Buttercup.Web

5.  Create a [Bugsnag](https://www.bugsnag.com/) project for the application and use the ASP.NET
    Core Secret Manager to add the project's API key as a user secret:

        dotnet user-secrets set "Bugsnag:ApiKey" "<replace-with-api-key>" -p src/Buttercup.Web

## Running the app

1.  Change to the web project directory:

        cd src/Buttercup.Web

2.  Install node dependencies:

        npm install

3.  Build all development and production assets once:

        npx gulp

    Or build only development assets and watch for changes:

        npx gulp watch

4.  Run the app:

        dotnet run

5.  Navigate to https://localhost:5000 and sign in using email 'dev@example.com' and password
    'dev-pass'

## Running tests

### .NET tests

- To run all .NET tests:

      dotnet test

### Jest tests

1.  Change to the web project directory:

        cd src/Buttercup.Web

2.  Install node dependencies:

        npm install

3.  Run all tests once:

        npx jest

    Or run tests for changed files in watch mode:

        npx jest --watch

### Playwright end-to-end tests

1.  Change to the web project directory:

        cd src/Buttercup.Web

2.  Install node dependencies:

        npm install

3.  Build frontend assets:

        npx gulp

4.  Run all tests once:

        npx playwright test

    Or run the tests in UI mode:

        npx playwright test --ui

## Checking test coverage

- To generate and open the .NET coverage report:

      dotnet test --collect "XPlat Code Coverage"
      ./scripts/build-dotnet-coverage-report.sh
      open coverage/index.html

- To generate and open the TypeScript coverage report:

      cd src/Buttercup.Web
      npx jest --coverage
      open coverage/lcov-report/index.html

## Linting

- To lint scripts:

      cd src/Buttercup.Web
      npx eslint .

- To lint styles:

      cd src/Buttercup.Web
      npx stylelint styles/

## Database migrations

- To create a new database migration:

      dotnet ef migrations add <MIGRATION_NAME> -s src/Buttercup.Web -p src/Buttercup.EntityModel.Migrations

- To run all pending database migrations:

      dotnet ef database update -s src/Buttercup.Web

## Sending emails with Azure Communication Services

In development, emails are sent to [Mailpit](https://mailpit.axllent.org/) by default.

To send emails using Azure Communication Services:

1.  Create an Azure Communication Service for Buttercup and set it up with an email domain.

2.  Create an Entra ID app registration for Buttercup development. Keep a copy of the Application
    (client) ID and Directory (tenant) ID for use later on.

3.  Create a client secret for the Buttercup development app. Keep a copy of the client secret value
    for use later on.

4.  Create an Entra ID security group for Buttercup development and add the service principal for
    the Buttercup development app as member.

5.  Assign the Communication and Email Service Owner role for the Buttercup communication service to
    the Buttercup development group.

6.  Use the ASP.NET Core Secret Manager to set the service principal's tenant ID, client ID and
    client secret as user secrets:

        dotnet user-secrets set "Azure:ClientCredentials:TenantId" "<replace-with-tenant-id>" -p src/Buttercup.Web
        dotnet user-secrets set "Azure:ClientCredentials:ClientId" "<replace-with-client-id>" -p src/Buttercup.Web
        dotnet user-secrets set "Azure:ClientCredentials:ClientSecret" "<replace-with-client-secret>" -p src/Buttercup.Web

7.  Set the Email > Provider config value to 'Azure'.

    Using an environment variable:

        export EMAIL__PROVIDER=Azure
        dotnet run

    Or using a command line argument:

        dotnet run Email:Provider=Azure
