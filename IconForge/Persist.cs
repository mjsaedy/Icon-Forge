using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Linq;
using System.Xml.Linq;

public class XmlFormSettings
{
    private readonly string _filePath;

    public XmlFormSettings(string fileName)
    {
        // XML file will be in the same folder as the executable
        _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
    }

    public void Save(Form form)
    {
        var doc = new XDocument(
            new XElement("FormSettings",
                new XElement("Window",
                    new XAttribute("Width", form.WindowState == FormWindowState.Normal ? form.Width : form.RestoreBounds.Width),
                    new XAttribute("Height", form.WindowState == FormWindowState.Normal ? form.Height : form.RestoreBounds.Height),
                    new XAttribute("X", form.WindowState == FormWindowState.Normal ? form.Left : form.RestoreBounds.Left),
                    new XAttribute("Y", form.WindowState == FormWindowState.Normal ? form.Top : form.RestoreBounds.Top),
                    new XAttribute("State", form.WindowState.ToString())
                ),
                new XElement("Controls", SerializeControls(form))
            )
        );

        doc.Save(_filePath);
    }

    public void Load(Form form)
    {
        if (!File.Exists(_filePath)) return;

        var doc = XDocument.Load(_filePath);
        var window = doc.Root.Element("Window");
        if (window != null)
        {
            form.StartPosition = FormStartPosition.Manual;
            form.Size = new Size(
                (int)window.Attribute("Width"),
                (int)window.Attribute("Height")
            );
            form.Location = new Point(
                (int)window.Attribute("X"),
                (int)window.Attribute("Y")
            );
            FormWindowState state;
            if (Enum.TryParse(window.Attribute("State").Value, out state))
                form.WindowState = state;
        }

        var controlsNode = doc.Root.Element("Controls");
        if (controlsNode != null)
            DeserializeControls(form, controlsNode);
    }

    #region Helpers

    private IEnumerable<XElement> SerializeControls(Control parent)
    {
        foreach (Control ctrl in parent.Controls) {
            TextBox tb = ctrl as TextBox;
            if (tb != null) {
                yield return new XElement("TextBox",
                    new XAttribute("Name", tb.Name),
                    new XAttribute("Text", tb.Text)
                );
                //continue;
            }
           CheckBox cb = ctrl as CheckBox;
           if (cb != null) {
                yield return new XElement("CheckBox",
                    new XAttribute("Name", cb.Name),
                    new XAttribute("Checked", cb.Checked)
                );
            }
            /*
            // Recurse into child containers
            foreach (var child in SerializeControls(ctrl))
                yield return child;
            */
        }
    }

    private void DeserializeControls(Control parent, XElement controlsNode) {
        foreach (Control ctrl in parent.Controls) {
            var name = ctrl.Name;
            TextBox tb = ctrl as TextBox;
            if (tb != null) {
                var node = controlsNode.Elements("TextBox")
                                       .FirstOrDefault(x => (string)x.Attribute("Name") == tb.Name); if (node != null && node.Attribute("Name").Value == name)
                tb.Text = node.Attribute("Text").Value;
            }
            CheckBox cb = ctrl as CheckBox;
            if (cb != null) {
                var node = controlsNode.Elements("CheckBox")
                       .FirstOrDefault(x => (string)x.Attribute("Name") == cb.Name);
                //MessageBox.Show(name + "\n" + node.Attribute("Name").Value);
                if (node != null && node.Attribute("Name").Value == name)
                    cb.Checked = bool.Parse(node.Attribute("Checked").Value);
            }

            /*
            // Recurse
            DeserializeControls(ctrl, controlsNode);
            */
        }
    }

    #endregion
}
