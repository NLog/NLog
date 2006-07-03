using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;
using NLogViewer.Configuration;

namespace NLogViewer.UI
{
    public partial class WizardForm : Form
    {
        private int _iCurrentPage;
        public delegate void PageIndexChangedDelegate(int pageNumber);
        private Dictionary<int, bool> pagesActivated = new Dictionary<int, bool>();
        protected bool _bAllowBack = true;
        public event PageIndexChangedDelegate PageIndexChanged;
        public List<IWizardPage> Pages = new List<IWizardPage>();

        public WizardForm()
        {
            InitializeComponent();
            PageIndexChanged += new PageIndexChangedDelegate(EnablePrevNextButton);
            PageIndexChanged += new PageIndexChangedDelegate(DisplayCurrentPage);
        }

        protected virtual bool ShouldSkipPage(int page)
        {
            return false;
        }

        public virtual int ForwardOffset(int currentPage)
        {
            int nextPage = currentPage + 1;
            while (ShouldSkipPage(nextPage))
                nextPage++;

            return nextPage;
        }

        public virtual int PreviousOffset(int currentPage)
        {
            int prevPage = currentPage - 1;
            while (ShouldSkipPage(prevPage))
                prevPage--;

            return prevPage;
        }

        private void wizardButtonNext_Click(object sender, System.EventArgs e)
        {
            if (ValidatePage(_iCurrentPage))
                PageIndexChanged(ForwardOffset(_iCurrentPage));
        }

        private void wizardButtonBack_Click(object sender, System.EventArgs e)
        {
            PageIndexChanged(PreviousOffset(_iCurrentPage));
        }

        protected void EnablePrevNextButton(int pageNumber)
        {
            if (pageNumber == 0)
                wizardButtonBack.Enabled = false;
            else
                if (_bAllowBack)
                    wizardButtonBack.Enabled = true;
                else
                    wizardButtonBack.Enabled = false;

            if (pageNumber == Pages.Count - 1)
            {
                wizardButtonNext.Enabled = false;
                AcceptButton = wizardButtonFinish;
            }
            else
            {
                wizardButtonNext.Enabled = true;
                AcceptButton = null;
                //AcceptButton = wizardButtonNext;
            }
            AcceptButton = null;
            if (NextButtonIsDefault(pageNumber))
            {
                if (pageNumber == Pages.Count - 1)
                {
                    AcceptButton = wizardButtonFinish;
                }
                else
                {
                    AcceptButton = wizardButtonNext;
                }
            }
        }

        protected virtual bool NextButtonIsDefault(int pageNumber)
        {
            return Pages[pageNumber].NextButtonIsDefault;
        }

        protected virtual void DisplayCurrentPage(int pageNumber)
        {
            _iCurrentPage = pageNumber;
            if (_iCurrentPage >= 0 && _iCurrentPage < Pages.Count)
            {
                for (int i = 0; i < Pages.Count; ++i)
                {
                    Pages[i].Control.Hide();
                }
                if (!wizardContentPanel.Visible)
                    wizardContentPanel.Show();

                if (!pagesActivated.ContainsKey(pageNumber))
                {
                    pagesActivated[pageNumber] = true;
                    ActivatePage(pageNumber);
                }
                Text = Pages[pageNumber].Title;
                label1.Text = Pages[pageNumber].Label1;
                label2.Text = Pages[pageNumber].Label2;
                Pages[pageNumber].Control.Show();
                Pages[pageNumber].Control.Focus();

                EnablePrevNextButton(pageNumber);
            }
        }

        protected void ReplacePage(int index, IWizardPage newPage)
        {
            if (Pages[index] != null)
            {
                wizardContentPanel.Controls.Remove(Pages[index].Control);
                Pages[index].Control.Dispose();
            }
            newPage.Control.Parent = wizardContentPanel;
            newPage.Control.Dock = DockStyle.Fill;
            Pages[index] = newPage;
        }

        private void wizardButtonFinish_Click(object sender, System.EventArgs e)
        {
            for (int i = CurrentPage; i < Pages.Count; i++)
            {
                if (!ValidatePage(i))
                    return;
            }
            DialogResult = DialogResult.OK;
            Close();
        }

        private void wizardButtonCancel_Click(object sender, System.EventArgs e)
        {
            DialogResult = wizardButtonCancel.DialogResult;
            Close();
        }
        /// <summary>
        /// This method is used by the wizard to knwo if it can go to the next displayed page
        /// This method must be overrided
        /// </summary>
        /// <param name="pageNumber">Number of the current displayed page</param>
        /// <returns><c>true</c> if the page had been validated, else <c>false</c></returns>
        protected virtual bool ValidatePage(int pageNumber)
        {
            return Pages[pageNumber].ValidatePage();
        }

        public int CurrentPage
        {
            get
            {
                return _iCurrentPage;
            }
        }

        /// <summary>
        /// This method is called before a page is displayed by the wizard
        /// This method must be overrided
        /// </summary>
        /// <param name="pageNumber">Number of the page to be displayed</param>
        protected virtual void ActivatePage(int pageNumber)
        {
            Pages[pageNumber].ActivatePage();
        }
        /// <summary>
        /// override of the ShowDialog of base form
        /// </summary>
        /// <param name="poOwner">Window handle of the owner of the wizard</param>
        /// <param name="pageNumber">Number of the page displayed at startup</param>
        /// <returns></returns>
        public DialogResult ShowDialog(System.Windows.Forms.IWin32Window poOwner, int pageNumber)
        {
            _iCurrentPage = pageNumber;
            return base.ShowDialog(poOwner);
        }

        protected void InitializeWizard()
        {
            foreach (IWizardPage wp in Pages)
            {
                wp.Control.Parent = wizardContentPanel;
                wp.Control.Dock = DockStyle.Fill;
                wp.Control.Hide();
            }
            wizardContentPanel.Show();

            int iPageTo = CurrentPage;
            for (int i = 0; i <= iPageTo; )
            {
                DisplayCurrentPage(i);
                i++;
                if ((i - 1) > 0)
                    ValidatePage(i - 1);
            }
        }

        private void WizardForm_Load(object sender, System.EventArgs e)
        {
        }
        /// <summary>
        /// This method is used to unactivate a page that had been previously activated 
        /// by the <c>ActivatePage</c> method. Use this method to force activation of a page
        /// in case of use of the back button
        /// </summary>
        /// <param name="pageNumber">Number of the page to deactivate</param>
        public void UnActivatePage(int pageNumber)
        {
            if (pagesActivated != null)
            {
                if (pageNumber < Pages.Count)
                {
                    for (int i = pageNumber; i < Pages.Count; i++)
                        pagesActivated[i] = false;
                }
            }
        }
        /// <summary>
        /// This method allow the back button
        /// </summary>
        /// <param name="pbAllowBack"><c>true</c> to allow back button, else <c>false</c></param>
        public void AllowBack(bool pbAllowBack)
        {
            _bAllowBack = pbAllowBack;
        }

        public T FindPage<T>() where T : Control
        {
            foreach (IWizardPage c in Pages)
            {
                if (c is T)
                    return (T)c;
            }
            return null;
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            
            g.DrawLine(SystemPens.ControlDark, 0, panel2.Bottom - 2, panel2.Width, panel2.Bottom - 2);
        }
    }
}