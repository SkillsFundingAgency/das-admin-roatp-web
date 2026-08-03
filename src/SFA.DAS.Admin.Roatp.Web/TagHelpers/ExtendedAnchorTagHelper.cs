using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace SFA.DAS.Admin.Roatp.Web.TagHelpers;

[ExcludeFromCodeCoverage]
[HtmlTargetElement("a", Attributes = "asp-querystring")]
public class ExtendedAnchorTagHelper(IHtmlGenerator generator) : AnchorTagHelper(generator)
{
    [HtmlAttributeName("asp-querystring")]
    public string? QueryString { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        base.Process(context, output);

        if (string.IsNullOrEmpty(QueryString))
        {
            return;
        }

        output.Attributes.TryGetAttribute("href", out var attribute);
        output.Attributes.SetAttribute("href", $"{attribute?.Value}{QueryString}");
    }
}
