using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Linq.Expressions;
using System.Web.Routing;

namespace Sweaty_T_Shirt.Extensions
{
    public static class Extensions
    {
        /// <summary>
        /// Stolen in part from http://stackoverflow.com/questions/3889397/how-to-create-a-checkboxlistfor-extension-method-in-asp-net-mvc/4057281#4057281
        /// TODO this does not work, need figure out how to set hidden inputs correctly.
        /// </summary>
        public static MvcHtmlString CheckBoxListFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, IEnumerable<TProperty>>> expression, List<SelectListItem> allOptions, object htmlAttributes = null)
        {
            ModelMetadata modelMetadata = ModelMetadata.FromLambdaExpression<TModel, IEnumerable<TProperty>>(expression, htmlHelper.ViewData);

            // Derive property name for checkbox name
            string propertyName = htmlHelper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(modelMetadata.PropertyName);

            // Create div
            TagBuilder divTag = new TagBuilder("div");
            divTag.MergeAttributes(new RouteValueDictionary(htmlAttributes), true);

            // Add checkboxes
            //foreach (SelectListItem item in allOptions)
            //{
            //    divTag.InnerHtml += string.Format(
            //                                      "<div><input type=\"checkbox\" name=\"{0}\" id=\"{1}_{2}\" " +
            //                                      "value=\"{2}\" {3} /><label for=\"{1}_{2}\">{4}</label></div>",
            //                                      propertyName,
            //                                      TagBuilder.CreateSanitizedId(propertyName),
            //                                      item.Value,
            //                                      item.Selected ? "checked=\"checked\"" : string.Empty,
            //                                      item.Text);
            //}            
            int counter = 0;
            foreach (SelectListItem item in allOptions)
            {
                divTag.InnerHtml += string.Format(@"<div>"
                    + "<input class='sweaty-check-box' type='checkbox' id='{0}_{1}_Selected' name='{0}[{1}].Selected' value='{3}' {2} />"
                    + "<label for='{0}_{1}_Selected'>{4}</label>"
                    + "<input name='{0}[{1}].Selected' type='hidden' value='{3}' />"
                    + "<input name='{0}[{1}].Text' id='{0}_{1}_Text' type='hidden' value='{4}' />"
                    + "<input name='{0}[{1}].Value' id='{0}_{1}_Value' type='hidden' value='{5}' />"
                    + "</div>",
                    propertyName,
                    counter++,
                    item.Selected? "checked='checked'" : "",
                    item.Selected? "true" : "false",
                    item.Text,
                    item.Value);
            }

            return MvcHtmlString.Create(divTag.ToString());
        }

    }
}