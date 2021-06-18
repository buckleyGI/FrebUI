using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Text.RegularExpressions;

namespace FREBUI
{
    public class Form1 : Form
    {
        private IContainer components = null;
        private FolderBrowserDialog folderBrowserDialog1;
        private TextBox textBox2;
        private Button button4;
        private SplitContainer splitContainer1;
        private DataGridView dataGridView1;
        private WebBrowser webBrowser1;
        private Button button6;
        private Label label1;
        private TextBox textBox1;
        private Button button1;
        private Button button2;

        public Form1() => this.InitializeComponent();

        private void LoadSelectedFolder()
        {
            if (this.textBox1.Text.Trim() == "")
            {
                MessageBox.Show("Please select the correct folder.");
            }
            else
            {
                this.dataGridView1.Visible = true;
                this.webBrowser1.Visible = true;
                this.webBrowser1.Height = this.splitContainer1.Panel2.Height - 20;
                this.dataGridView1.Columns.Clear();
                this.dataGridView1.Columns.Add("lastSegment", "Endpoint");
                this.dataGridView1.Columns.Add("headless", "Headless");
                this.dataGridView1.Columns.Add("created", "Created");
                this.dataGridView1.Columns.Add("failureReason", "failureReason");
                this.dataGridView1.Columns.Add("statusCode", "StatusCode");
                this.dataGridView1.Columns.Add("timeTaken", "TimeTaken");
                this.dataGridView1.Columns.Add("remote", "Remote");
                this.dataGridView1.Columns.Add("response", "Response");
                this.dataGridView1.Columns.Add("remoteUserName", "remoteUserName");
                this.dataGridView1.Columns.Add("userName", "userName");
                this.dataGridView1.Columns.Add("authenticationType", "authenticationType");
                this.dataGridView1.Columns.Add("userAgent", "UserAgent");
                this.dataGridView1.Columns.Add("url", "url");
                this.dataGridView1.Columns.Add("verb", "verb");
                this.dataGridView1.Columns.Add("appPoolID", "AppPoolName");
                this.dataGridView1.Columns.Add("processId", "processId");
                this.dataGridView1.Columns.Add("filename", "FileName");

                try
                {
                    FileInfo[] files =
                        new DirectoryInfo(this.textBox1.Text).GetFiles("fr*.xml", SearchOption.AllDirectories);
                    if (files.Length == 0)
                    {
                        MessageBox.Show(
                            "There are no FREB trace files in the selected folder. Please select the correct folder.");
                    }
                    else
                    {
                        foreach (FileInfo fileInfo in files)
                        {
                            this.GetDetailsFromFREBFile(
                                fileInfo.FullName,
                                out var url,
                                out var verb,
                                out var appPool,
                                out var statusCode,
                                out var timeTaken,
                                out var created,
                                out var userAgent,
                                out var headless,
                                out var remote,
                                out var response,
                                out var processId,
                                out var remoteUserName,
                                out var userName,
                                out var authenticationType,
                                out var failureReason,
                                out _,
                                out var lastSegment);

                            this.dataGridView1.Rows.Add(
                                lastSegment,
                                headless,
                                created,
                                failureReason,
                                statusCode,
                                timeTaken,
                                remote,
                                response,
                                remoteUserName,
                                userName,
                                authenticationType,
                                userAgent,
                                url,
                                verb,
                                appPool,
                                processId,
                                fileInfo.FullName
                            );
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Something bad happened. Message : " +
                                    ex.Message + " Stack : " + ex.StackTrace);
                }
            }
        }

        internal void GetDetailsFromFREBFile(string fileName, out string url, out string verb, out string appPool,
            out string statusCode, out int timeTaken, out string created, out string userAgent, out bool headless,
            out string remote, out string response, out string processId, out string remoteUserName,
            out string userName, out string authenticationType, out string failureReason, out string triggerStatusCode,
            out string lastSegment)
        {
            statusCode = created = userAgent = remote = response = lastSegment = "";
            headless = false;
            string str1;
            verb = str1 = "";
            url = appPool = str1;
            timeTaken = 0;
            processId = "";
            remoteUserName = "";
            userName = "";
            authenticationType = "";
            failureReason = "";
            triggerStatusCode = "";

            try
            {
                XmlDocument xmlDocument = new XmlDocument();

                xmlDocument.Load(fileName);

                XmlNode xmlNode = xmlDocument.SelectSingleNode("failedRequest");
                url = xmlNode.Attributes[nameof(url)].Value;
                verb = xmlNode.Attributes[nameof(verb)].Value;
                appPool = xmlNode.Attributes["appPoolId"].Value;
                timeTaken = int.Parse(xmlNode.Attributes[nameof(timeTaken)].Value);
                triggerStatusCode = xmlNode.Attributes[nameof(triggerStatusCode)].Value;
                statusCode = xmlNode.Attributes[nameof(statusCode)].Value;
                if (triggerStatusCode != statusCode)
                {
                    statusCode = triggerStatusCode + "->" + statusCode;
                }

                processId = xmlNode.Attributes[nameof(processId)].Value;
                remoteUserName = xmlNode.Attributes[nameof(remoteUserName)]?.Value;
                userName = xmlNode.Attributes[nameof(userName)]?.Value;
                authenticationType = xmlNode.Attributes[nameof(authenticationType)].Value;
                failureReason = xmlNode.Attributes[nameof(failureReason)].Value;

                var uri = new Uri(url);
                lastSegment = uri.Segments.Reverse().FirstOrDefault(x => !Guid.TryParse(x, out _));
                //lastSegment = uri.Segments.Last();


                // Is it possible to ignore namespaces in c# when using xPath?
                // https://stackoverflow.com/a/4313696/381995
                // https://stackoverflow.com/questions/9032493/xmldocument-ignore-xmlns
                XmlNode xmlNode2 = xmlDocument.SelectSingleNode(
                    "/*[local-name() = 'failedRequest']/*[local-name() = 'Event']/*[local-name() = 'System']/*[local-name() = 'TimeCreated']");
                created = xmlNode2.Attributes["SystemTime"].Value;

                userAgent = Regex.Match(xmlDocument.InnerText, "(?i)user-agent: .*").ToString().Replace("\r", "").Replace("\n", "");
                remote = Regex.Match(xmlDocument.InnerXml, @"(?i)<Data Name=""RemoteAddress"">(.*?)</Data>").Groups[1]
                    .ToString();
                // + ":" +  Regex.Match(xmlDocument.InnerXml,@"(?i)<Data Name=""RemotePort"">(.*?)</Data>").Groups[1].ToString();
                response = new string(Regex.Match(xmlDocument.InnerXml, @"(?is)<Data Name=""Buffer"">(.*?)</Data>").Groups[1]
                    .ToString().Take(10000).ToArray());
                headless = Regex.IsMatch(xmlDocument.InnerText, "(?i)headless");
            }
            catch (Exception ex1)
            {
                try
                {
                    if (ex1.GetType().ToString() == "System.Xml.XmlException")
                    {
                        string str2 = " ";
                        string str3 = " ";
                        string str4 = " ";
                        TextReader textReader = new StreamReader(fileName);
                        while (!str2.Contains("failedRequest url"))
                        {
                            str2 = textReader.ReadLine();
                            if (str2 == null || str2.Contains("xmlns:freb="))
                                break;
                        }

                        string str5 = str2.Substring(20).Replace('"', ' ');
                        while (!appPool.Contains("appPoolId="))
                        {
                            appPool = textReader.ReadLine();
                            if (appPool.Contains("xmlns:freb="))
                                break;
                        }

                        appPool = appPool.Substring(27).Replace('"', ' ').Trim();
                        while (!verb.Contains("verb="))
                        {
                            verb = textReader.ReadLine();
                            if (verb.Contains("xmlns:freb="))
                                break;
                        }

                        verb = verb.Substring(20).Replace('"', ' ').Trim();
                        while (!str3.Contains("statusCode="))
                        {
                            str3 = textReader.ReadLine();
                            if (str3.Contains("xmlns:freb="))
                                break;
                        }

                        string str6 = str3.Substring(27).Replace('"', ' ');
                        while (!str4.Contains("timeTaken="))
                        {
                            str4 = textReader.ReadLine();
                            if (str4.Contains("xmlns:freb="))
                                break;
                        }

                        string str7 = str4.Substring(25).Replace('"', ' ');
                        url = str5.Trim();
                        statusCode = str6.Trim();
                        timeTaken = int.Parse(str7.Trim());
                    }
                    else
                    {
                        MessageBox.Show("Something bad happened. Message : " +
                                        ex1.Message + " Stack : " + ex1.StackTrace);
                    }
                }
                catch (Exception ex2)
                {
                    MessageBox.Show("Something bad happened. Message : " +
                                    ex2.Message + " Stack : " + ex2.StackTrace);
                }
            }
        }

        private void Search(object sender, EventArgs e)
        {
            if (this.button4.Text == "search text")
            {
                foreach (DataGridViewRow row in this.dataGridView1.Rows)
                {
                    bool flag = false;
                    for (int index = 0; index < row.Cells.Count; ++index)
                    {
                        if (row.Cells[index].Value != null && row.Cells[index].Value.ToString().ToLower()
                            .Contains(this.textBox2.Text.ToLower()))
                            flag = true;
                    }

                    row.Visible = flag;
                }

                this.button4.Text = "load all";
            }
            else
            {
                foreach (DataGridViewBand row in this.dataGridView1.Rows)
                    row.Visible = true;
                this.button4.Text = "search text";
            }
        }

        private void ShowSlowness(object sender, EventArgs e)
        {
            Result slowEvent = this.FindSlowEvent(this.dataGridView1.CurrentRow.Cells.Cast<DataGridViewCell>().Single(x => x.OwningColumn.Name == "filename").Value.ToString());

            MessageBox.Show("Maximum delay of " + slowEvent.delay.ToString() + "ms between '" + slowEvent.pEventName +
                            "' and '" + slowEvent.eventName + "'");
        }

        private Result FindSlowEvent(string fileName)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(fileName);
            XmlNode xmlNode1 = xmlDocument.SelectSingleNode("failedRequest");
            long num1 = 0;
            long num2 = 0;
            XmlNode xmlNode2 = null;
            XmlNode xmlNode3 = null;
            XmlNode xmlNode4 = null;
            foreach (XmlNode childNode in xmlNode1.ChildNodes)
            {
                DateTime dateTime = DateTime.Parse(childNode["System"]["TimeCreated"].GetAttribute("SystemTime"));
                if (num1 != 0L)
                {
                    long num3 = dateTime.Ticks - num1;
                    if (num2 < num3)
                    {
                        num2 = num3;
                        xmlNode2 = childNode;
                        xmlNode3 = xmlNode4;
                    }
                }

                xmlNode4 = childNode;
                num1 = dateTime.Ticks;
            }

            string innerText = xmlNode2["RenderingInfo"].ChildNodes[0].InnerText;
            return new Result()
            {
                delay = num2 / 10000L,
                eventName = innerText,
                pEventName = xmlNode3["RenderingInfo"].ChildNodes[0].InnerText
            };
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            this.splitContainer1.Visible = false;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            this.splitContainer1.Width = this.Width - 18;
            this.splitContainer1.Height = this.Height - 70;
            this.dataGridView1.Width = this.splitContainer1.Width - 10;
            this.dataGridView1.Height = this.splitContainer1.Panel1.Height;
            this.webBrowser1.Height = this.splitContainer1.Panel2.Height - 20;
            this.webBrowser1.Width = this.splitContainer1.Panel2.Width - 5;
        }

        private void splitContainer1_Resize(object sender, EventArgs e)
        {
            this.splitContainer1.Width = this.Width - 18;
            this.splitContainer1.Height = this.Height - 70;
            this.dataGridView1.Width = this.splitContainer1.Width - 10;
            this.dataGridView1.Height = this.splitContainer1.Panel1.Height;
            this.webBrowser1.Height = this.splitContainer1.Panel2.Height - 10;
            this.webBrowser1.Width = this.splitContainer1.Panel2.Width - 5;
        }

        private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
        {
            this.splitContainer1.Width = this.Width - 18;
            this.splitContainer1.Height = this.Height - 70;
            this.dataGridView1.Width = this.splitContainer1.Width - 10;
            this.dataGridView1.Height = this.splitContainer1.Panel1.Height;
            this.webBrowser1.Height = this.splitContainer1.Panel2.Height - 20;
            this.webBrowser1.Width = this.splitContainer1.Panel2.Width - 5;
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (this.dataGridView1.CurrentRow.IsNewRow)
                return;
            this.dataGridView1.CurrentRow.Selected = true;
            //this.webBrowser1.Navigate(((DataGridView) sender).CurrentRow.Cells[0].Value.ToString());
            this.webBrowser1.Navigate(((DataGridView) sender).CurrentRow.Cells.Cast<DataGridViewCell>().Single(x => x.OwningColumn.Name == "filename").Value.ToString());
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e) =>
            this.dataGridView1.CurrentRow.Selected = true;

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e) =>
            this.dataGridView1.CurrentRow.Selected = true;

        private void dataGridView1_KeyUp(object sender, KeyEventArgs e)
        {
            int index = this.dataGridView1.CurrentRow.Index;
            if (index >= this.dataGridView1.Rows.Count - 1)
            {
                --index;
            }

            this.dataGridView1.Rows[index].Selected = true;
            //this.webBrowser1.Navigate(((DataGridView) sender).Rows[index].Cells.[1].Value.ToString());
            this.webBrowser1.Navigate(((DataGridView) sender).Rows[index].Cells.Cast<DataGridViewCell>().Single(x => x.OwningColumn.Name == "filename").Value.ToString());
        }

        private void dataGridView1_SortCompare(object sender, DataGridViewSortCompareEventArgs e)
        {
            if (e.Column.Name == "StatusCode" || e.Column.Name == "TimeTaken")
            {
                e.SortResult = int.Parse(e.CellValue1.ToString()) >= int.Parse(e.CellValue2.ToString())
                    ? (int.Parse(e.CellValue1.ToString()) <= int.Parse(e.CellValue2.ToString()) ? 0 : -1)
                    : 1;
                e.Handled = true;
            }
            else
                e.Handled = false;
        }

        private void SelectFolder(object sender, EventArgs e)
        {
            this.folderBrowserDialog1.ShowDialog();
            this.textBox1.Text = this.folderBrowserDialog1.SelectedPath;
        }

        private void LoadFolder(object sender, EventArgs e)
        {
            this.LoadSelectedFolder();
            
            this.dataGridView1.Sort(this.dataGridView1.Columns["created"], ListSortDirection.Descending);

            this.splitContainer1.Visible = true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && this.components != null)
                this.components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.button4 = new System.Windows.Forms.Button();
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.button6 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(407, 14);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(124, 23);
            this.textBox2.TabIndex = 8;
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(537, 10);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(112, 28);
            this.button4.TabIndex = 9;
            this.button4.Text = "Search the grid";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.Search);
            // 
            // webBrowser1
            // 
            this.webBrowser1.Location = new System.Drawing.Point(0, 0);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.Size = new System.Drawing.Size(642, 374);
            this.webBrowser1.TabIndex = 1;
            this.webBrowser1.Visible = false;
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AllowUserToOrderColumns = true;
            this.dataGridView1.AllowUserToResizeRows = false;
            this.dataGridView1.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(5, 3);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.Size = new System.Drawing.Size(358, 181);
            this.dataGridView1.TabIndex = 0;
            this.dataGridView1.Visible = false;
            this.dataGridView1.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellClick);
            this.dataGridView1.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellContentClick);
            this.dataGridView1.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellDoubleClick);
            this.dataGridView1.SortCompare += new System.Windows.Forms.DataGridViewSortCompareEventHandler(this.dataGridView1_SortCompare);
            this.dataGridView1.KeyUp += new System.Windows.Forms.KeyEventHandler(this.dataGridView1_KeyUp);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Location = new System.Drawing.Point(7, 44);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.dataGridView1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.webBrowser1);
            this.splitContainer1.Size = new System.Drawing.Size(1042, 731);
            this.splitContainer1.SplitterDistance = 285;
            this.splitContainer1.TabIndex = 6;
            this.splitContainer1.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.splitContainer1_SplitterMoved);
            this.splitContainer1.Resize += new System.EventHandler(this.splitContainer1_Resize);
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(655, 9);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(232, 28);
            this.button6.TabIndex = 11;
            this.button6.Text = "Show the slowness for the selected row";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.ShowSlowness);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(92, 15);
            this.label1.TabIndex = 13;
            this.label1.Text = "Select the folder";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(107, 12);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(204, 23);
            this.textBox1.TabIndex = 14;
            this.textBox1.Text = "";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(317, 10);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(24, 28);
            this.button1.TabIndex = 15;
            this.button1.Text = "...";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.SelectFolder);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(347, 10);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(54, 28);
            this.button2.TabIndex = 16;
            this.button2.Text = "load";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.LoadFolder);
            // 
            // Form1
            // 
            this.AutoSize = true;
            this.BackColor = System.Drawing.Color.WhiteSmoke;
            this.ClientSize = new System.Drawing.Size(1356, 772);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button6);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.splitContainer1);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "Form1";
            this.Text = "FREBUI - navigate your FREB traces easily";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Resize += new System.EventHandler(this.Form1_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

    }
}