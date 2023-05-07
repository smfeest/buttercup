using Buttercup.EntityModel;
using Buttercup.TestUtils;
using MySqlConnector;

namespace Buttercup.DataAccess;

public sealed class SampleDataHelper
{
    private readonly MySqlConnection connection;
    private readonly ModelFactory modelFactory = new();

    public SampleDataHelper(MySqlConnection connection) => this.connection = connection;

    public Task<Recipe> InsertRecipe(bool includeOptionalAttributes = false) =>
        this.InsertRecipe(this.modelFactory.BuildRecipe(includeOptionalAttributes), true);

    public async Task<Recipe> InsertRecipe(Recipe recipe, bool insertRelatedRecords = false)
    {
        if (insertRelatedRecords)
        {
            if (recipe.CreatedByUser != null)
            {
                await this.InsertUser(recipe.CreatedByUser);
            }
            if (recipe.ModifiedByUser != null)
            {
                await this.InsertUser(recipe.ModifiedByUser);
            }
        }

        using var command = this.connection.CreateCommand();

        command.CommandText = @"INSERT recipes(id, title, preparation_minutes, cooking_minutes, servings, ingredients, method, suggestions, remarks, source, created, created_by_user_id, modified, modified_by_user_id, revision)
            VALUES (@id, @title, @preparation_minutes, @cooking_minutes, @servings, @ingredients, @method, @suggestions, @remarks, @source, @created, @created_by_user_id, @modified, @modified_by_user_id, @revision);";

        command.Parameters.AddWithValue("@id", recipe.Id);
        command.Parameters.AddWithValue("@title", recipe.Title);
        command.Parameters.AddWithValue("@preparation_minutes", recipe.PreparationMinutes);
        command.Parameters.AddWithValue("@cooking_minutes", recipe.CookingMinutes);
        command.Parameters.AddWithValue("@servings", recipe.Servings);
        command.Parameters.AddWithValue("@ingredients", recipe.Ingredients);
        command.Parameters.AddWithValue("@method", recipe.Method);
        command.Parameters.AddWithValue("@suggestions", recipe.Suggestions);
        command.Parameters.AddWithValue("@remarks", recipe.Remarks);
        command.Parameters.AddWithValue("@source", recipe.Source);
        command.Parameters.AddWithValue("@created", recipe.Created);
        command.Parameters.AddWithValue("@created_by_user_id", recipe.CreatedByUserId);
        command.Parameters.AddWithValue("@modified", recipe.Modified);
        command.Parameters.AddWithValue("@modified_by_user_id", recipe.ModifiedByUserId);
        command.Parameters.AddWithValue("@revision", recipe.Revision);

        await command.ExecuteNonQueryAsync();

        return recipe;
    }

    public Task<User> InsertUser(bool includeOptionalAttributes = false) =>
        this.InsertUser(this.modelFactory.BuildUser(includeOptionalAttributes));

    public async Task<User> InsertUser(User user)
    {
        using var command = this.connection.CreateCommand();

        command.CommandText = @"INSERT users(id, name, email, hashed_password, password_created, security_stamp, time_zone, created, modified, revision)
            VALUES (@id, @name, @email, @hashed_password, @password_created, @security_stamp, @time_zone, @created, @modified, @revision);";

        command.Parameters.AddWithValue("@id", user.Id);
        command.Parameters.AddWithValue("@name", user.Name);
        command.Parameters.AddWithValue("@email", user.Email);
        command.Parameters.AddWithValue("@hashed_password", user.HashedPassword);
        command.Parameters.AddWithValue("@password_created", user.PasswordCreated);
        command.Parameters.AddWithValue("@security_stamp", user.SecurityStamp);
        command.Parameters.AddWithValue("@time_zone", user.TimeZone);
        command.Parameters.AddWithValue("@created", user.Created);
        command.Parameters.AddWithValue("@modified", user.Modified);
        command.Parameters.AddWithValue("@revision", user.Revision);

        await command.ExecuteNonQueryAsync();

        return user;
    }
}
