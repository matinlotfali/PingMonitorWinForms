using System;
using System.Windows.Forms;

namespace Pinger
{
    public partial class AddForm : Form
    {
        public string name, url;
        public AddForm()
        {
            InitializeComponent();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void OKButton_Click(object sender, EventArgs e)
        {
            name = NameTextBox.Text;
            url = URLTextBox.Text;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
