using System;
using System.Web;
using System.Web.UI;
using Sitecore;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Diagnostics;
using Sitecore.Exceptions;
using Sitecore.Globalization;
using Sitecore.Pipelines;
using Sitecore.Pipelines.Upload;
using Sitecore.Shell.Web.UI;
using Sitecore.Web.UI.XmlControls;

namespace SitecoreEx.Foundation.SitecoreExtensions.sitecore.shell.Applications.Media.UploadMedia
{
  /// <summary>
  /// Upload Media Page 2 
  /// </summary>
  public class UploadMediaPage2 : SecurePage
  {
    protected override void OnInit(EventArgs e)
    {
      Control control = ControlFactory.GetControl("UploadMedia");
      if (control != null)
      {
        Controls.Add(control);
      }
      base.OnInit(e);
    }

    protected override void OnLoad(EventArgs e)
    {
      base.OnLoad(e);
      if (base.MaxRequestLengthExceeded)
      {
        HttpContext.Current.Response.Write("<html><head><script type=\"text/JavaScript\" language=\"javascript\">window.top.scForm.getTopModalDialog().frames[0].scForm.postRequest(\"\", \"\", \"\", 'ShowFileTooBig()')</script></head><body>Done</body></html>");
      }
      else
      {
        if (base.IsEvent || base.Request.Files.Count <= 0)
        {
          return;
        }
        try
        {
          string empty = string.Empty;
          Language contentLanguage = Sitecore.Context.ContentLanguage;
          string text = Sitecore.Context.ClientPage.ClientRequest.Form["ItemUri"];
          ItemUri itemUri = ItemUri.Parse(text);
          if (itemUri != null)
          {
            empty = itemUri.GetPathOrId();
            contentLanguage = itemUri.Language;
            UploadArgs uploadArgs = new UploadArgs
            {
              FileOnly = false,
              Files = base.Request.Files,
              Folder = empty,
              Overwrite = Settings.Upload.SimpleUploadOverwriting,
              Unpack = false,
              Versioned = Settings.Media.UploadAsVersionableByDefault,
              Language = contentLanguage,
              CloseDialogOnEnd = false,
              Destination = (Settings.Media.UploadAsFiles ? UploadDestination.File : UploadDestination.Database)
            };
            Pipeline pipeline = PipelineFactory.GetPipeline("uiUpload");
            pipeline.Start(uploadArgs);
            if (uploadArgs.UploadedItems.Count > 0)
            {
              empty = uploadArgs.UploadedItems[0].ID.ToString();
              Log.Audit(this, "Upload: {0}", StringUtil.Join(uploadArgs.UploadedItems, ", ", "Name"));
            }
            else
            {
              empty = string.Empty;
            }
            if (string.IsNullOrEmpty(uploadArgs.ErrorText))
            {
              HttpContext.Current.Response.Write("<html><head><script type=\"text/JavaScript\" language=\"javascript\">window.top.scForm.getTopModalDialog().frames[0].scForm.postRequest(\"\", \"\", \"\", 'EndUploading(\"" + empty + "\")')</script></head><body>Done</body></html>");
            }
            return;
          }
          SecurityException ex = new SecurityException("Upload ItemUri invalid");
          Log.Error("ItemUri not valid. ItemUri: " + text, ex, this);
          throw ex;
        }
        catch (OutOfMemoryException)
        {
          HttpContext.Current.Response.Write("<html><head><script type=\"text/JavaScript\" language=\"javascript\">window.top.scForm.getTopModalDialog().frames[0].scForm.postRequest(\"\", \"\", \"\", 'ShowFileTooBig(" + StringUtil.EscapeJavascriptString(base.Request.Files[0].FileName) + ")')</script></head><body>Done</body></html>");
        }
        catch (Exception ex3)
        {
          if (ex3.InnerException is OutOfMemoryException)
          {
            HttpContext.Current.Response.Write("<html><head><script type=\"text/JavaScript\" language=\"javascript\">window.top.scForm.getTopModalDialog().frames[0].scForm.postRequest(\"\", \"\", \"\", 'ShowFileTooBig(" + StringUtil.EscapeJavascriptString(base.Request.Files[0].FileName) + ")')</script></head><body>Done</body></html>");
          }
          else if (ex3.InnerException is DuplicateItemNameException)
          {
            HttpContext.Current.Response.Write("<html><head><script type=\"text/JavaScript\" language=\"javascript\">window.top.scForm.getTopModalDialog().frames[0].scForm.postRequest(\"\", \"\", \"\", 'ShowUploadError(\"" + "The item with same name is already defined on this level.\"" + ", " + StringUtil.EscapeJavascriptString(base.Request.Files[0].FileName) + ")')</script></head><body>Done</body></html>");
          }
          else
          {
            HttpContext.Current.Response.Write("<html><head><script type=\"text/JavaScript\" language=\"javascript\">window.top.scForm.getTopModalDialog().frames[0].scForm.postRequest(\"\", \"\", \"\", 'ShowError')</script></head><body>Done</body></html>");
          }
        }
      }
    }
  }
}