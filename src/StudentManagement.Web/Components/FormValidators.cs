using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace StudentManagement.Web.Components;

public static class FormValidators
{
    /// <summary>
    /// Pass as <c>Validator</c> on every AntDesign <c>Form</c>. AntDesign 1.6.2 never adds its default
    /// DataAnnotationsValidator (its "Validator != null" check compiles to an always-true method-group
    /// comparison), so without this the [Required]/[StringLength] attributes are silently ignored.
    /// </summary>
    public static readonly RenderFragment DataAnnotations = builder =>
    {
        builder.OpenComponent<DataAnnotationsValidator>(0);
        builder.CloseComponent();
    };
}
