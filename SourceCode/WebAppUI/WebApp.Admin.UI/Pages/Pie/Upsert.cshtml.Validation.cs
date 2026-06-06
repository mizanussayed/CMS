using WebApp.Core.Resources;
using WebApp.Core.Validator;

namespace WebApp.Admin.UI.Pages.Pie;

public partial class UpsertModel
{
    private DateTime dateExpiryDate;

    private async Task<bool> ValidatePost()
    {
        bool IsValid = true;

        IsValid = await ValidateModel();
        if (IsValid) IsValid = ValidateExpiryDate();
        if (IsValid) IsValid = ValidateImage();

        return IsValid;
    }

    private async Task<bool> ValidateModel()
    {
        var validationResult = await new PieModelValidator().ValidateAsync(Pie);
        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
                ErrorMessage += error.ErrorMessage + "<br>";
            return false;
        }

        return true;
    }

    private bool ValidateExpiryDate()
    {
        var isDate = DateTime.TryParse(ExpiryDate, out dateExpiryDate);
        if (!isDate)
        {
            ErrorMessage = ValidationMessages.Pie_ExpiryDate;
            return false;
        }

        return true;
    }

    private bool ValidateImage()
    {
        if (PieImage != null)
        {
            string[] supportedImageTypes = _config["SiteSettings:SupportedImageTypes"].Split(',');
            if (!supportedImageTypes.Contains(Path.GetExtension(PieImage.FileName).Substring(1)))
            {
                ErrorMessage = ValidationMessages.Pie_FileExtension;
                return false;
            }

            if ((PieImage.Length / 1024) > Convert.ToInt32(_config["SiteSettings:MaxImageSize"]))
            {
                ErrorMessage = ValidationMessages.Pie_FileSize;
                return false;
            }
        }

        return true;
    }
}