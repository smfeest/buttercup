# Buttercup

## Required software

- [.NET 5 SDK](https://dotnet.microsoft.com/download/dotnet/5.0)
- [MySQL Server](https://dev.mysql.com/downloads/mysql/)
- [Node.js](https://nodejs.org)
- [Visual Studio Code](https://code.visualstudio.com/) with [recommended
  extensions](.vscode/extensions.json)

## Setting up the database

1.  Create the _buttercup_dev_ user that will be used to connect to the
    application and test databases:

        mysql -u root -p < db/dev-user.sql

2.  Create the _buttercup_app_ application database:

        scripts/create_db.sh buttercup_app -u buttercup_dev

3.  Insert a user account:

        mysql -u buttercup_dev buttercup_app << SQL
          INSERT user (name, email, security_stamp, time_zone, created, modified)
          VALUES ('<your-name>', '<your-email>', '', 'Etc/UTC', UTC_TIMESTAMP, UTC_TIMESTAMP)
        SQL

    Once the application is running, you'll be able to use the password reset
    flow to set a password.

## Setting up SendGrid

1.  Create a [SendGrid API Key](https://app.sendgrid.com/settings/api_keys) with
    full access to _Mail Send_ > _Mail Send_ only

2.  Add the API key to the user secrets for the application:

        dotnet user-secrets set "Email:ApiKey" "<your-api-key>" --project src/Buttercup.Web

## Running the app

1.  Change to the web project directory

        cd src/Buttercup.Web

2.  Install node dependencies:

        npm install

3.  Build all development and production assets once:

        npx gulp

    Or build only development assets and watch for changes:

        npx gulp watch

4.  Run the app

        dotnet run

## Running tests

- To run all .NET tests:

      dotnet test

- To run all Jasmine tests using Headless Chrome:

      cd src/Buttercup.Web
      npx gulp test

- To run all Jasmine tests in a Chrome window (for interactive debugging):

      cd src/Buttercup.Web
      npx gulp testDebug

## Linting

- To lint scripts:

      cd src/Buttercup.Web
      npx eslint .

- To lint styles:

      cd src/Buttercup.Web
      npx stylelint styles/
