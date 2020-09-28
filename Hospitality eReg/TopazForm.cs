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
    public partial class TopazForm : Form
    {
        public delegate void DelSignature(string name, string file, int x, int y, int width, int height);
        public event DelSignature SignCompletted;

        const string TOPAZFILE = "topaz.jpg";
        int _xLoc;
        int _yLoc;
        int _width;
        int _height;
        string _name;

        public TopazForm(string name, int xLoc, int yLoc, int width, int height)
        {
            InitializeComponent();

            _name = name;
            _xLoc = xLoc;
            _yLoc = yLoc;
            _width = width;
            _height = height;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            try
            {
                sigPlusNET1.SetImageXSize(_width );
                sigPlusNET1.SetImageYSize(_height );
                sigPlusNET1.SetJustifyMode(5);

                string basePath = AppDomain.CurrentDomain.BaseDirectory;
                basePath = Path.Combine(basePath, TOPAZFILE);

                Image img = sigPlusNET1.GetSigImage();
                img.Save(basePath);

                if (File.Exists(basePath))
                {
                    SignCompletted?.Invoke(_name, basePath, _xLoc, _yLoc, _width, _height);

                    this.DialogResult = DialogResult.OK;

                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
            }

        }

        private void TopazForm_Load(object sender, EventArgs e)
        {
            sigPlusNET1.SetTabletState(1);

            try
            {
                string basePath = AppDomain.CurrentDomain.BaseDirectory;
                basePath = Path.Combine(basePath, TOPAZFILE);
                if (File.Exists(basePath))
                {
                    File.Delete(basePath);
                }
            }
            catch
            {
                this.Close();
            }

        }

        private void TopazForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            sigPlusNET1.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            sigPlusNET1.ClearTablet();
        }

        private void sigPlusNET1_Click(object sender, EventArgs e)
        {

        }
    }
}
