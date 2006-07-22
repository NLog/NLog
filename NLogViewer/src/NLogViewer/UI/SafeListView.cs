using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace NLogViewer.UI
{
    public class SafeListView : ListView
    {
        public SafeListView()
        {
            DoubleBuffered = true;
        }

        /// <summary>
        /// Called when the control need a virtual item.
        /// </summary>
        /// <param name="e">The event object the user needs to fill up.</param>
        protected override void OnRetrieveVirtualItem(RetrieveVirtualItemEventArgs e)
        {
            // Get the list view item from the user.
            base.OnRetrieveVirtualItem(e);
            if (e.Item != null)
            {
                // Go over all the sub items in the list view
                foreach (ListViewItem.ListViewSubItem subItem in e.Item.SubItems)
                {
                    // If an items text is 260 characters long, add a space so it does
                    // not crash the program.
                    if (subItem.Text.Length == 260)
                    {
                        subItem.Text = subItem.Text + " ";
                    }
                }
            }
        }

        /// <summary>
        /// The size of the VirtualListSize
        /// </summary>

        public new int VirtualListSize
        {
            get { return base.VirtualListSize; }
            set
            {
                // If the new size is smaller than the Index of TopItem, we need to make
                // sure the new TopItem is set to something smaller.
                if (VirtualMode && View == View.Details && TopItem != null && value > 0 && TopItem.Index > value - 1)
                {
                    TopItem = Items[value - 1];
                }

                base.VirtualListSize = value;
            }
        }
    }
}
