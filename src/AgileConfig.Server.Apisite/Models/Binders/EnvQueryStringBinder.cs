using System;
using System.Threading.Tasks;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AgileConfig.Server.Apisite.Models.Binders;

public class EnvQueryStringBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext == null) throw new ArgumentNullException(nameof(bindingContext));

        var instance = new EnvString
        {
            Value = ISettingService.EnvironmentList[0]
        };

        var modelName = bindingContext.ModelName;
        var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);

        if (valueProviderResult == ValueProviderResult.None)
        {
            bindingContext.Result = ModelBindingResult.Success(instance);
            return Task.CompletedTask;
        }

        bindingContext.ModelState.SetModelValue(modelName, valueProviderResult);

        var value = valueProviderResult.FirstValue;
        if (!string.IsNullOrEmpty(value)) instance.Value = value;

        bindingContext.Result = ModelBindingResult.Success(instance);
        return Task.CompletedTask;
    }
}