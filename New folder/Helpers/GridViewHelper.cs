using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DevExpress.Web.Mvc;

namespace DMSERoute.Helpers
{
    public static class GridViewHelper
    {
        private static GridViewSettings exportGridViewSettings;

        public static GridViewSettings ExportGridViewSettings
        {
            get
            {
                if (exportGridViewSettings == null)
                    exportGridViewSettings = CreateExportGridViewSettings();
                return exportGridViewSettings;
            }
        }

        private static GridViewSettings CreateExportGridViewSettings()
        {
            GridViewSettings settings = new GridViewSettings();

            settings.Name = "gvTypedListDataBinding";
            settings.CallbackRouteValues = new { Controller = "Home", Action = "TypedListDataBindingPartial" };

            settings.KeyFieldName = "ID";
            settings.Settings.ShowFilterRow = true;

            settings.CommandColumn.Visible = true;
            settings.CommandColumn.ShowSelectCheckbox = true;

            settings.SettingsExport.ExportedRowType = DevExpress.Web.GridViewExportedRowType.Selected;

            settings.Columns.Add("ID");
            settings.Columns.Add("Text");
            settings.Columns.Add("Quantity");
            settings.Columns.Add("Price");

            return settings;
        }
    }
}