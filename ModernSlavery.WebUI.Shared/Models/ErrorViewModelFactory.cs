using System;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using ModernSlavery.Core.Classes.ErrorMessages;
using ModernSlavery.Core.Extensions;
using ModernSlavery.WebUI.Shared.Classes;

namespace ModernSlavery.WebUI.Shared.Models
{
    public interface IErrorViewModelFactory
    {
        ErrorViewModel Create(string message, string description = null);

        ErrorViewModel Create(int errorCode, object parameters = null);
    }

    public class ErrorViewModelFactory: IErrorViewModelFactory
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ErrorViewModelFactory(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public ErrorViewModel Create(string message, string description = null)
        {
            var errorViewModel = new ErrorViewModel();
            errorViewModel.Title = message;
            errorViewModel.Description = description;
            return errorViewModel;
        }

        public ErrorViewModel Create(int errorCode, object parameters = null)
        {
            CustomErrorMessage customErrorMessage = CustomErrorMessages.GetPageError(errorCode) ?? CustomErrorMessages.DefaultPageError;

            var errorViewModel = new ErrorViewModel();

            errorViewModel.ErrorCode = errorCode;
            errorViewModel.Title = customErrorMessage.Title;
            errorViewModel.Subtitle = customErrorMessage.Subtitle;
            errorViewModel.Description = customErrorMessage.Description;
            errorViewModel.CallToAction = customErrorMessage.CallToAction;

            
            Uri uri = _httpContextAccessor?.HttpContext?.GetUri();
            errorViewModel.ActionUrl = customErrorMessage.ActionUrl == "#" && uri != null ? uri.PathAndQuery : customErrorMessage.ActionUrl;
            errorViewModel.ActionText = customErrorMessage.ActionText;

            //Assign any values to variables
            if (parameters != null)
            {
                foreach (PropertyInfo prop in parameters.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
                {
                    var value = prop.GetValue(parameters, null) as string;
                    if (string.IsNullOrWhiteSpace(prop.Name) || string.IsNullOrWhiteSpace(value))
                    {
                        continue;
                    }

                    errorViewModel.Title = errorViewModel.Title.ReplaceI("{" + prop.Name + "}", value);
                    errorViewModel.Subtitle = errorViewModel.Subtitle.ReplaceI("{" + prop.Name + "}", value);
                    errorViewModel.Description = errorViewModel.Description.ReplaceI("{" + prop.Name + "}", value);
                    errorViewModel.CallToAction = errorViewModel.CallToAction.ReplaceI("{" + prop.Name + "}", value);
                    errorViewModel.ActionUrl = errorViewModel.ActionUrl.ReplaceI("{" + prop.Name + "}", value);
                    errorViewModel.ActionText = errorViewModel.ActionText.ReplaceI("{" + prop.Name + "}", value);
                }
            }

            return errorViewModel;
        }
    }
}
