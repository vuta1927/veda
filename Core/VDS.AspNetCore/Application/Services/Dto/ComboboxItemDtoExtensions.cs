using VDS.Application.Services.Dto;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace VDS.AspNetCore.Application.Services.Dto
{
    public static class ComboboxItemDtoExtensions
    {
        public static SelectListItem ToSelectListItem(this ComboboxItemDto comboboxItem)
        {
            return new SelectListItem
            {
                Value = comboboxItem.Value,
                Text = comboboxItem.DisplayText,
                Selected = comboboxItem.IsSelected
            };
        }
    }
}