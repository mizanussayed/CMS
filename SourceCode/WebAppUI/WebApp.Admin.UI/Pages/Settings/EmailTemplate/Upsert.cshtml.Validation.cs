using WebApp.Core.Validator;

namespace WebApp.Admin.UI.Pages.Settings.EmailTemplate;

public partial class UpsertModel
{
    private async Task<bool> ValidatePost()
    {
        var validationResult = await new EmailTemplateModelValidator().ValidateAsync(EmailTemplate);
        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
                ErrorMessage += error.ErrorMessage + "<br>";
            return false;
        }

        return true;
    }
}