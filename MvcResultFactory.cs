//using Microsoft.AspNetCore.Mvc;
//using SharpGrip.FluentValidation.AutoValidation.Mvc.Results;
//using SharpGrip.FluentValidation.AutoValidation.Mvc.Filters;
//using SharpGrip.FluentValidation.AutoValidation.Mvc.Configuration;
//using Microsoft.AspNetCore.Mvc.Filters;
//using Microsoft.AspNetCore.Mvc.ViewFeatures;
//using Microsoft.AspNetCore.Mvc.Controllers;

//namespace MyProject
//{
//    public class MvcResultFactory : IAutoValidationResultFactory
//    {
//        public IActionResult CreateResult(ActionContext context, AutoValidationResult autoValidationResult)
//        {
//            var controller = context.Controller as Controller;
//            if (controller == null)
//                return new BadRequestResult();

//            foreach (var error in autoValidationResult.Errors)
//            {
//                context.ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
//            }

//            return controller.View(controller.ViewData.Model);
//        }
//    }
//}
