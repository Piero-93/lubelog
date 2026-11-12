using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text.Encodings.Web;

namespace CarCareTracker.Middleware
{
    /// <summary>
    /// Prepends the request PathBase to src/href attributes holding absolute local paths,
    /// both literal ("/Home") and stored ones (vehicle images, attachments, logos).
    /// Does nothing when LubeLogger is served at the root.
    /// </summary>
    [HtmlTargetElement("*", Attributes = "src")]
    [HtmlTargetElement("*", Attributes = "href")]
    [HtmlTargetElement("*", Attributes = "xlink:href")]
    public class BasePathTagHelper : TagHelper
    {
        private static readonly string[] UrlAttributes = ["src", "href", "xlink:href"];

        //run after the built-in "~/" resolution, so resolved paths are recognized as already prefixed.
        public override int Order => 1000;

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; } = default!;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var pathBase = ViewContext.HttpContext.Request.PathBase.Value;
            if (string.IsNullOrEmpty(pathBase))
            {
                return;
            }
            foreach (var attributeName in UrlAttributes)
            {
                if (!output.Attributes.TryGetAttribute(attributeName, out var attribute))
                {
                    continue;
                }
                switch (attribute.Value)
                {
                    case string value:
                        output.Attributes.SetAttribute(attributeName, BasePath.Apply(pathBase, value));
                        break;
                    case IHtmlContent htmlContent:
                        //HtmlString and mixed-content values are already encoded: prefix them as they are.
                        using (var writer = new StringWriter())
                        {
                            htmlContent.WriteTo(writer, HtmlEncoder.Default);
                            output.Attributes.SetAttribute(attributeName, new HtmlString(BasePath.Apply(pathBase, writer.ToString())));
                        }
                        break;
                    case null:
                        break;
                    default:
                        output.Attributes.SetAttribute(attributeName, BasePath.Apply(pathBase, attribute.Value.ToString() ?? string.Empty));
                        break;
                }
            }
        }
    }
}
