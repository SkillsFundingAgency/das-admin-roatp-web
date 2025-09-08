using Microsoft.AspNetCore.Razor.TagHelpers;

namespace SFA.DAS.Admin.Roatp.Web.TagHelpers;

[HtmlTargetElement(Attributes = "asp-show")]
public class ShowTagHelper : TagHelper
{
    public bool? AspShow { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (!AspShow.HasValue || !AspShow.Value)
        {
            output.SuppressOutput();
        }
    }
}
