namespace Botwin.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using FluentValidation.Results;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using Newtonsoft.Json;

    public static class BindExtensions
    {
        private static readonly JsonSerializer JsonSerializer = new JsonSerializer();

        public static (ValidationResult ValidationResult, T Data) BindAndValidate<T>(this HttpRequest request)
        {
            var data = request.Bind<T>();
            if (data == null)
            {
                data = Activator.CreateInstance<T>();
            }

            var validatorLocator = request.HttpContext.RequestServices.GetService<IValidatorLocator>();

            var validator = validatorLocator.GetValidator<T>();

            if (validator == null)
            {
                return (new ValidationResult(new[] { new ValidationFailure(typeof(T).Name, "No validator found") }), default(T));
            }

            var result = validator.Validate(data);
            return (result, data);
        }

        public static T Bind<T>(this HttpRequest request)
        {
            using (var streamReader = new StreamReader(request.Body))
            using (var jsonTextReader = new JsonTextReader(streamReader))
            {
                return JsonSerializer.Deserialize<T>(jsonTextReader);
            }
        }
        
        public static async Task<IEnumerable<IFormFile>> BindFiles(this HttpRequest request)
        {
            var postedFiles = new List<IFormFile>();
            
            if (request.HasFormContentType)
            {
                var form = await request.ReadFormAsync();

                foreach (var file in form.Files)
                {
                    // If there is an <input type="file" ... /> in the form and is left blank.
                    if (file.Length == 0 && string.IsNullOrEmpty(file.FileName))
                    {
                        continue;
                    }

                    postedFiles.Add(file);
                }
            }

            return postedFiles;
        }
        
        public static async Task BindAndSaveFiles(this HttpRequest request, string saveLocation)
        {
            
            if (request.HasFormContentType)
            {
                var form = await request.ReadFormAsync();

                foreach (var file in form.Files)
                {
                    // If there is an <input type="file" ... /> in the form and is left blank.
                    if (file.Length == 0 && string.IsNullOrEmpty(file.FileName))
                    {
                        continue;
                    }

                    using (var fileToSave = File.Create(Path.Combine(saveLocation, file.FileName)))
                    {
                        await file.CopyToAsync(fileToSave);
                    }
                }
            }

        }
    }
}