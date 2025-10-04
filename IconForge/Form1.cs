using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IconForge {
    public partial class frmMain : Form {

        private readonly XmlFormSettings _settings;
        private Image _theImage = null;
        private string _theFilename = null;

        public frmMain() {
            DPIAwareness.SetDpiAwareness();
            InitializeComponent();
            _settings = new XmlFormSettings("settings.xml");
            lblImageInfo.Text = "";
        }

        private void btnSelectPngImage_Click(object sender, EventArgs e) {
            using (OpenFileDialog dlg = new OpenFileDialog()) {
                dlg.Filter = "Images|*.png;*.jpg;*.jpeg;*.bmp;*.gif";
                dlg.Title = "Select Image";
                if (dlg.ShowDialog() == DialogResult.OK) {
                    _theFilename = dlg.FileName;
                    _theImage = Image.FromFile(dlg.FileName);
                    lblImageInfo.Text = $"Original size: {_theImage.Width}x{_theImage.Height}";
                    this.Text = $"Icon Forge — {Path.GetFileName(_theFilename)}";
                    chkBox512.Checked = _theImage.Width >= 512;
                    chkBox256.Checked = _theImage.Width >= 256;
                    updatePictureBoxes();
                }
            }
        }


        private void updatePictureBoxes() {
            if (_theImage == null)
                return;
            if (pic512.Image != null) {
                pic512.Image.Dispose();
                pic256.Image.Dispose();
                pic128.Image.Dispose();
                pic64.Image.Dispose();
                pic48.Image.Dispose();
                pic32.Image.Dispose();
                pic24.Image.Dispose();
                pic16.Image.Dispose();
            }
            int width = _theImage.Width;
            int height = _theImage.Height;
            pic512.Image = ImageHelper.ResizeImage(_theImage, new Size(512, 512), keepAspect: chkKeepAspect.Checked);
            pic256.Image = ImageHelper.ResizeImage(_theImage, new Size(256, 256), keepAspect: chkKeepAspect.Checked);
            pic128.Image = ImageHelper.ResizeImage(_theImage, new Size(128, 128), keepAspect: chkKeepAspect.Checked);
            pic64.Image = ImageHelper.ResizeImage(_theImage, new Size(64, 64), keepAspect: chkKeepAspect.Checked);
            pic48.Image = ImageHelper.ResizeImage(_theImage, new Size(48, 48), keepAspect: chkKeepAspect.Checked);
            pic32.Image = ImageHelper.ResizeImage(_theImage, new Size(32, 32), keepAspect: chkKeepAspect.Checked);
            pic24.Image = ImageHelper.ResizeImage(_theImage, new Size(24, 24), keepAspect: chkKeepAspect.Checked);
            pic16.Image = ImageHelper.ResizeImage(_theImage, new Size(16, 16), keepAspect: chkKeepAspect.Checked);
            updateCustomImage();

        }

        private void updateCustomImage() {
            if (_theImage == null)
                return;
            if (chkCustomSize.Checked) {
                int customWidth;
                var resultW = int.TryParse(txtCustomWidth.Text, out customWidth);
                if (resultW) {
                    customWidth = Math.Min(512, Math.Max(0, customWidth));
                    txtCustomWidth.Text = customWidth.ToString();
                    picCustomSize.Image = ImageHelper.ResizeImage(_theImage, new Size(customWidth, customWidth), keepAspect: chkKeepAspect.Checked);
                }
            }
        }

        private void btnSaveIcon_Click(object sender, EventArgs e) {
            if (_theImage == null)
                return;
            using (SaveFileDialog sfd = new SaveFileDialog()) {
                sfd.Title = "Save Icon File";
                sfd.Filter = "Icon files (*.ico)|*.ico";
                sfd.DefaultExt = "ico";
                sfd.AddExtension = true;
                if (!string.IsNullOrEmpty(_theFilename)) {
                    sfd.InitialDirectory = Path.GetDirectoryName(_theFilename);
                    sfd.FileName = Path.ChangeExtension(Path.GetFileName(_theFilename), ".ico");
                }

                if (sfd.ShowDialog() != DialogResult.OK)
                    return;
                var images = new List<Image>();
                if (chkBox512.Checked && pic512.Image != null) images.Add(pic512.Image);
                if (chkBox256.Checked && pic256.Image != null) images.Add(pic256.Image);
                if (chkBox128.Checked && pic128.Image != null) images.Add(pic128.Image);
                if (chkBox64.Checked && pic64.Image != null) images.Add(pic64.Image);
                if (chkBox48.Checked && pic48.Image != null) images.Add(pic48.Image);
                if (chkBox32.Checked && pic32.Image != null) images.Add(pic32.Image);
                if (chkBox24.Checked && pic24.Image != null) images.Add(pic24.Image);
                if (chkBox16.Checked && pic16.Image != null) images.Add(pic16.Image);
                if (chkCustomSize.Checked && picCustomSize.Image != null) images.Add(picCustomSize.Image);

                IconHelper.SaveIcon(sfd.FileName, images.ToArray());
            }
        }

        private void frmMain_Load(object sender, EventArgs e) {
            _settings.Load(this);
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e) {
            _settings.Save(this);
        }

        private void frmMain_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Escape) {
                this.Close();
            }
        }

        private void chkCustomSize_CheckedChanged(object sender, EventArgs e) {
            if (chkCustomSize.Checked)
                updateCustomImage();
            else
                picCustomSize.Image = null;
                //picCustomSize.Image.Dispose();
        }

        private void txtCustomWidth_TextChanged(object sender, EventArgs e) {
            if (chkCustomSize.Checked)
                updateCustomImage();
        }

        private void chkKeepAspect_CheckedChanged(object sender, EventArgs e) {
            updatePictureBoxes();
        }

        private void frmMain_Shown(object sender, EventArgs e) {
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1) {
                _theFilename = args[1];
                _theImage = Image.FromFile(_theFilename);
                lblImageInfo.Text = $"Original size: {_theImage.Width}x{_theImage.Height}";
                this.Text = $"Icon Forge — {Path.GetFileName(_theFilename)}";
                chkBox512.Checked = _theImage.Width >= 512;
                chkBox256.Checked = _theImage.Width >= 256;
                updatePictureBoxes();
            }
        }
    }
}
