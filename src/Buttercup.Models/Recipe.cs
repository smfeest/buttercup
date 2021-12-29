namespace Buttercup.Models
{
    /// <summary>
    /// Represents a recipe.
    /// </summary>
    public class Recipe
    {
        /// <summary>
        /// Gets or sets the recipe ID.
        /// </summary>
        /// <value>
        /// The recipe ID.
        /// </value>
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public string? Title { get; set; }

        /// <summary>
        /// Gets or sets the preparation time in minutes.
        /// </summary>
        /// <value>
        /// The preparation time in minutes.
        /// </value>
        public int? PreparationMinutes { get; set; }

        /// <summary>
        /// Gets or sets the cooking time in minutes.
        /// </summary>
        /// <value>
        /// The cooking time in minutes.
        /// </value>
        public int? CookingMinutes { get; set; }

        /// <summary>
        /// Gets or sets the number of servings.
        /// </summary>
        /// <value>
        /// The number of servings.
        /// </value>
        public int? Servings { get; set; }

        /// <summary>
        /// Gets or sets the ingredients.
        /// </summary>
        /// <value>
        /// The ingredients.
        /// </value>
        public string? Ingredients { get; set; }

        /// <summary>
        /// Gets or sets the method.
        /// </summary>
        /// <value>
        /// The method.
        /// </value>
        public string? Method { get; set; }

        /// <summary>
        /// Gets or sets the suggestions.
        /// </summary>
        /// <value>
        /// The suggestions.
        /// </value>
        public string? Suggestions { get; set; }

        /// <summary>
        /// Gets or sets the remarks.
        /// </summary>
        /// <value>
        /// The remarks.
        /// </value>
        public string? Remarks { get; set; }

        /// <summary>
        /// Gets or sets the source.
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        public string? Source { get; set; }

        /// <summary>
        /// Gets or sets the date and time at which the record was created.
        /// </summary>
        /// <value>
        /// The date and time at which the record was created.
        /// </value>
        public DateTime Created { get; set; }

        /// <summary>
        /// Gets or sets the user ID of the user who created the record.
        /// </summary>
        /// <value>
        /// The user ID of the user who created the record.
        /// </value>
        public long? CreatedByUserId { get; set; }

        /// <summary>
        /// Gets or sets the date and time at which the record was last modified.
        /// </summary>
        /// <value>
        /// The date and time at which the record was last modified.
        /// </value>
        public DateTime Modified { get; set; }

        /// <summary>
        /// Gets or sets the user ID of the user who last modified the record.
        /// </summary>
        /// <value>
        /// The user ID of the user who last modified the record.
        /// </value>
        public long? ModifiedByUserId { get; set; }

        /// <summary>
        /// Gets or sets the revision number.
        /// </summary>
        /// <value>
        /// The revision number.
        /// </value>
        public int Revision { get; set; }
    }
}
