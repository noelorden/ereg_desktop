using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Hospitality_eReg
{
    public partial class SelectDocument : Form
    {
        public string FileName { get; private set; }
        
        public SelectDocument(string[] args)
        {
            InitializeComponent();

            for (int i = 1; i < args.Length; i++)
            {
                lstDocuments.Items.Add(Path.GetFileNameWithoutExtension(args[i]));
            }
            lstDocuments.SelectedIndex = 0;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            FileName = lstDocuments.Items[lstDocuments.SelectedIndex].ToString()  + ".txt";
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void SelectDocument_Load(object sender, EventArgs e)
        {

        }
    }
}
