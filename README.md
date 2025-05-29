# Buttercup

## Required software

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [MySQL Server](https://dev.mysql.com/downloads/mysql/)
- [Node.js 20](https://nodejs.org)
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

4.  Insert a user account:

        mysql -u buttercup_dev buttercup_app << SQL
          INSERT users (name, email, security_stamp, time_zone, created, modified, revision)
          VALUES ('<your-name>', '<your-email>', '', 'Etc/UTC', UTC_TIMESTAMP, UTC_TIMESTAMP, 0)
        SQL

    Once the application is running, you'll be able to use the password reset
    flow to set a password.

## Setting user secrets

1.  Change to the web project directory

        cd src/Buttercup.Web

2.  Create a [MailerSend API key](https://app.mailersend.com/domains/) with sending access only, and
    add it as a user secret:

        dotnet user-secrets set "Email:ApiKey" "<replace-with-api-key>"

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

#### Notes

- Before running any tests, Playwright will automatically start an instance of the app on
  http://localhost:5005. However, the app can also be started manually first:

        cd src/Buttercup.Web
        dotnet run --environment E2E --urls http://localhost:5005

- End-to-end tests should be designed to clean up any database records they create, even on failure.
  However, if necessary, the database can be deleted, ready to be recreated on the next run:

        cd src/Buttercup.Web
        DOTNET_ENVIRONMENT=E2E dotnet ef database drop

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
