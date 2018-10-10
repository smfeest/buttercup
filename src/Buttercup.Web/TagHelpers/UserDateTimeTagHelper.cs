using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Buttercup.Web.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Buttercup.Web.TagHelpers
{
    /// <summary>
    /// A tag helper that renders a date and time in the current user's time zone.
    /// </summary>
    public class UserDateTimeTagHelper : TagHelper
    {
        /// <summary>
        /// Gets or sets the date and time.
        /// </summary>
        /// <value>
        /// The date and time.
        /// </value>
        public DateTime? DateTime { get; set; }

        /// <summary>
        /// Gets or sets the format string.
        /// </summary>
        /// <value>
        /// The format string.
        /// </value>
        public string Format { get; set; }

        /// <summary>
        /// Gets or sets the view context.
        /// </summary>
        /// <value>
        /// The view context.
        /// </value>
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (!this.DateTime.HasValue)
            {
                output.SuppressOutput();
                return;
            }

            var userDateTime = this.ViewContext.HttpContext.ToUserTime(this.DateTime.Value);

            output.TagName = "span";
            output.Content.SetContent(
                userDateTime.ToString(this.Format ?? "G", CultureInfo.CurrentCulture));
            output.Attributes.SetAttribute(
                "title", userDateTime.ToString("u", CultureInfo.CurrentCulture));
        }
    }
}
