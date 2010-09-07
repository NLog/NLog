using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace NLogDemo
{
    public partial class _Default : System.Web.UI.Page
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        protected void Page_Load(object sender, EventArgs e)
        {
            logger.Info("Page_Load called");
            if (!this.IsPostBack)
            {
                this.textboxOperand1.Text = "1";
                this.textboxOperand2.Text = "1";
                this.dropdownOperator.SelectedValue = "Add";
            }

            this.Compute();
        }

        protected void Page_LoadComplete(object sender, EventArgs e)
        {
            logger.Trace("Page_LoadComplete called");
        }

        protected void Page_Unload(object sender, EventArgs e)
        {
            logger.Trace("Page_Unload called");
        }

        private void Compute()
        {
            try
            {
                string op1 = this.textboxOperand1.Text;
                string op2 = this.textboxOperand2.Text;

                logger.Trace("Op1: '{0}'", op1);
                logger.Trace("Op2: '{0}'", op2);

                switch (this.dropdownOperator.SelectedValue)
                {
                    case "Add":
                        this.textboxResult.Text = (Convert.ToInt32(op1) + Convert.ToInt32(op2)).ToString();
                        break;

                    case "Subtract":
                        this.textboxResult.Text = (Convert.ToInt32(op1) - Convert.ToInt32(op2)).ToString();
                        break;

                    case "Multiply":
                        this.textboxResult.Text = (Convert.ToInt32(op1) * Convert.ToInt32(op2)).ToString();
                        break;

                    case "Divide":
                        this.textboxResult.Text = (Convert.ToInt32(op1) / Convert.ToInt32(op2)).ToString();
                        break;

                    case "Modulo":
                        this.textboxResult.Text = (Convert.ToInt32(op1) % Convert.ToInt32(op2)).ToString();
                        break;

                    default:
                        throw new NotSupportedException("Not supported operator: " + this.dropdownOperator.SelectedValue);
                }

                this.labelError.Text = string.Empty;
                logger.Info("Result computed successfully.");
            }
            catch (Exception ex)
            {
                logger.Error("ERROR: {0}", ex);
                this.labelError.Text = ex.ToString();
                this.textboxResult.Text = "(error)";
            }
        }
    }
}