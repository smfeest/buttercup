# Buttercup

## Required software

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [MySQL Server](https://dev.mysql.com/downloads/mysql/)
- [Node.js 20](https://nodejs.org)
- [Redis](https://redis.io/open-source/) or [Valkey](https://valkey.io/download/)
- [Visual Studio Code](https://code.visualstudio.com/) with [recommended
  extensions](.vscode/extensions.json)

## Setting up the database

1.  Restore .NET tools:

        dotnet tool restore

2.  Create the _buttercup_dev_ user that will be used to connect to the
    application and test databases:

        mysql -u root -p < scripts/create-dev-user.sql

3.  Create the application database:

        dotnet ef database update -s src/Buttercup.Web

## Setting user secrets

1.  Change to the web project directory

        cd src/Buttercup.Web

2.  Add the client credentials for the 'buttercup-dev' Azure service principal as user secrets:

        dotnet user-secrets set "Azure:ClientCredentials:TenantId" "<replace-with-tenant-id>"
        dotnet user-secrets set "Azure:ClientCredentials:ClientId" "<replace-with-client-id>"
        dotnet user-secrets set "Azure:ClientCredentials:ClientSecret" "<replace-with-client-secret>"

3.  Create a [Bugsnag](https://www.bugsnag.com/) project for the application and
    add the corresponding notifier API key as a user secret:

        dotnet user-secrets set "Bugsnag:ApiKey" "<replace-with-api-key>"

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
