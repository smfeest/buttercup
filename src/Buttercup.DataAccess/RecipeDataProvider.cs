using Buttercup.Models;
using MySqlConnector;

namespace Buttercup.DataAccess;

/// <summary>
/// The default implementation of <see cref="IRecipeDataProvider" />.
/// </summary>
internal sealed class RecipeDataProvider : IRecipeDataProvider
{
    private readonly IClock clock;

    public RecipeDataProvider(IClock clock) => this.clock = clock;

    /// <inheritdoc />
    public async Task<long> AddRecipe(
        MySqlConnection connection, RecipeAttributes attributes, long currentUserId)
    {
        using var command = connection.CreateCommand();

        command.CommandText = @"INSERT recipe (title, preparation_minutes, cooking_minutes, servings, ingredients, method, suggestions, remarks, source, created, created_by_user_id, modified, modified_by_user_id)
            VALUES (@title, @preparation_minutes, @cooking_minutes, @servings, @ingredients, @method, @suggestions, @remarks, @source, @timestamp, @current_user_id, @timestamp, @current_user_id)";

        this.AddInsertUpdateParameters(command, attributes, currentUserId);

        await command.ExecuteNonQueryAsync();

        return command.LastInsertedId;
    }

    /// <inheritdoc />
    public async Task DeleteRecipe(MySqlConnection connection, long id)
    {
        using var command = connection.CreateCommand();

        command.CommandText = "DELETE FROM recipe WHERE id = @id";
        command.Parameters.AddWithValue("@id", id);

        if (await command.ExecuteNonQueryAsync() == 0)
        {
            throw RecipeNotFound(id);
        }
    }

    /// <inheritdoc />
    public Task<IList<Recipe>> GetAllRecipes(MySqlConnection connection) =>
        GetRecipes(connection, "SELECT * FROM recipe ORDER BY title");

    /// <inheritdoc />
    public async Task<Recipe> GetRecipe(MySqlConnection connection, long id)
    {
        using var command = connection.CreateCommand();

        command.CommandText = "SELECT * FROM recipe WHERE id = @id";
        command.Parameters.AddWithValue("@id", id);

        using var reader = await command.ExecuteReaderAsync();

        return await reader.ReadAsync() ? ReadRecipe(reader) : throw RecipeNotFound(id);
    }

    /// <inheritdoc />
    public Task<IList<Recipe>> GetRecentlyAddedRecipes(MySqlConnection connection) =>
        GetRecipes(connection, "SELECT * FROM recipe ORDER BY created DESC LIMIT 10");

    /// <inheritdoc />
    public Task<IList<Recipe>> GetRecentlyUpdatedRecipes(MySqlConnection connection)
    {
        var query = @"SELECT *
            FROM recipe
            LEFT JOIN (SELECT id AS added_id FROM recipe ORDER BY created DESC LIMIT 10) AS added ON added_id = id
            WHERE created != modified AND added_id IS NULL
            ORDER BY modified DESC LIMIT 10";

        return GetRecipes(connection, query);
    }

    /// <inheritdoc />
    public async Task UpdateRecipe(
        MySqlConnection connection,
        long id,
        RecipeAttributes newAttributes,
        int baseRevision,
        long currentUserId)
    {
        using var command = connection.CreateCommand();

        command.CommandText = @"UPDATE recipe
            SET title = @title, preparation_minutes = @preparation_minutes,
                cooking_minutes = @cooking_minutes, servings = @servings,
                ingredients = @ingredients, method = @method, suggestions = @suggestions,
                remarks = @remarks, source = @source, modified = @timestamp,
                modified_by_user_id = @current_user_id, revision = revision + 1
            WHERE id = @id AND revision = @base_revision";

        AddInsertUpdateParameters(command, newAttributes, currentUserId);
        command.Parameters.AddWithValue("@id", id);
        command.Parameters.AddWithValue("@base_revision", baseRevision);

        if (await command.ExecuteNonQueryAsync() == 0)
        {
            throw await ConcurrencyOrNotFoundException(connection, id, baseRevision);
        }
    }

    private void AddInsertUpdateParameters(
        MySqlCommand command, RecipeAttributes attributes, long currentUserId)
    {
        command.Parameters.AddWithStringValue("@title", attributes.Title);
        command.Parameters.AddWithValue("@preparation_minutes", attributes.PreparationMinutes);
        command.Parameters.AddWithValue("@cooking_minutes", attributes.CookingMinutes);
        command.Parameters.AddWithValue("@servings", attributes.Servings);
        command.Parameters.AddWithStringValue("@ingredients", attributes.Ingredients);
        command.Parameters.AddWithStringValue("@method", attributes.Method);
        command.Parameters.AddWithStringValue("@suggestions", attributes.Suggestions);
        command.Parameters.AddWithStringValue("@remarks", attributes.Remarks);
        command.Parameters.AddWithStringValue("@source", attributes.Source);
        command.Parameters.AddWithValue("@timestamp", this.clock.UtcNow);
        command.Parameters.AddWithValue("@current_user_id", currentUserId);
    }

    private static async Task<Exception> ConcurrencyOrNotFoundException(
        MySqlConnection connection, long id, int revision)
    {
        using var command = connection.CreateCommand();

        command.CommandText = "SELECT revision FROM recipe WHERE id = @id";
        command.Parameters.AddWithValue("@id", id);

        var currentRevision = await command.ExecuteScalarAsync();

        return currentRevision == null ?
            RecipeNotFound(id) :
            new ConcurrencyException(
                $"Revision {revision} does not match current revision {currentRevision}");
    }

    private static async Task<IList<Recipe>> GetRecipes(MySqlConnection connection, string query)
    {
        using var command = connection.CreateCommand();

        command.CommandText = query;

        using var reader = await command.ExecuteReaderAsync();

        var recipes = new List<Recipe>();

        while (await reader.ReadAsync())
        {
            recipes.Add(ReadRecipe(reader));
        }

        return recipes;
    }

    private static Recipe ReadRecipe(MySqlDataReader reader) => new(
        reader.GetInt64("id"),
        reader.GetString("title"),
        reader.GetNullableInt32("preparation_minutes"),
        reader.GetNullableInt32("cooking_minutes"),
        reader.GetNullableInt32("servings"),
        reader.GetString("ingredients"),
        reader.GetString("method"),
        reader.GetNullableString("suggestions"),
        reader.GetNullableString("remarks"),
        reader.GetNullableString("source"),
        reader.GetDateTime("created"),
        reader.GetNullableInt64("created_by_user_id"),
        reader.GetDateTime("modified"),
        reader.GetNullableInt64("modified_by_user_id"),
        reader.GetInt32("revision"));

    private static NotFoundException RecipeNotFound(long id) => new($"Recipe {id} not found");
}
