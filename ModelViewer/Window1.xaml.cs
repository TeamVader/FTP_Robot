
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using System.Xml;

namespace StaubliEasyFTPClient
{
    /// <summary>
    /// Interaction logic for Window1.
    /// </summary>
    public partial class Window1
    {

       

       
        public string ip_ftp ;
        public bool connected;
    
      



        #region Window
        /// <summary>
        /// Initializes a new instance of the <see cref="Window1"/> class.
        /// </summary>
        public Window1()
        {
            this.InitializeComponent();


            this.CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, this.OnCloseWindow));
            this.CommandBindings.Add(new CommandBinding(SystemCommands.MaximizeWindowCommand, this.OnMaximizeWindow, this.OnCanResizeWindow));
            this.CommandBindings.Add(new CommandBinding(SystemCommands.MinimizeWindowCommand, this.OnMinimizeWindow, this.OnCanMinimizeWindow));
            this.CommandBindings.Add(new CommandBinding(SystemCommands.RestoreWindowCommand, this.OnRestoreWindow, this.OnCanResizeWindow));

           
            connected = false;
            ip_ftp = "127.0.0.1";
            Main_Grid.DataContext = this;

            /* Create Object Instance */
            FTP ftpClient = new FTP(@"ftp://"+ip_ftp+@"/", "alex", "alex");

            /* Upload a File */
          //  ftpClient.upload("test.txt", @"C:\Users\alex\Desktop\test.txt");
             

            /* Download a File */
            //ftpClient.download("etc/test.txt", @"C:\Users\metastruct\Desktop\test.txt");

            /* Delete a File */
           //  ftpClient.delete("/test.txt");

            
             treeviewtest.Items.Clear();
             treeviewtest.Items.Add(CreateDirectoryNode(@"ftp://" + ip_ftp + @"/", "root"));
            /* Rename a File */
           // ftpClient.rename("etc/test.txt", "test2.txt");

            /* Create a New Directory */
           // ftpClient.createDirectory("etc/test");

            /* Get the Date/Time a File was Created */
           // string fileDateTime = ftpClient.getFileCreatedDateTime("etc/test.txt");
            //Console.WriteLine(fileDateTime);

            /* Get the Size of a File */
            //string fileSize = ftpClient.getFileSize("etc/test.txt");
            //Console.WriteLine(fileSize);

            /* Get Contents of a Directory (Names Only) */
           // string[] simpleDirectoryListing = ftpClient.directoryListDetailed("/");
           // for (int i = 0; i < simpleDirectoryListing.Length; i++) { MessageBox.Show(simpleDirectoryListing[i]); }

            /* Get Contents of a Directory with Detailed File/Directory Info */
            //string[] detailDirectoryListing = ftpClient.directoryListDetailed("/etc");
            //for (int i = 0; i < detailDirectoryListing.Count(); i++) { Console.WriteLine(detailDirectoryListing[i]); }
            /* Release Resources */
            open_xml_from_ftp(@"usr/usrapp/GRASS_4_Vision/GRASS_4_Vision.dtx",ftpClient);
            ftpClient = null;
            

        }

        private void Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {

            
            // animate_a1(-30, 5);
           // KUKA_A1.BeginAnimation(A1_angle, testanimation);
        }

        private void OnCanResizeWindow(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.ResizeMode == ResizeMode.CanResize || this.ResizeMode == ResizeMode.CanResizeWithGrip;
        }

        private void OnCanMinimizeWindow(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.ResizeMode != ResizeMode.NoResize;
        }

        private void OnCloseWindow(object target, ExecutedRoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
            Application.Current.Shutdown();
            
        }

        private void OnMaximizeWindow(object target, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MaximizeWindow(this);
        }

        private void OnMinimizeWindow(object target, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(this);
        }

        private void OnRestoreWindow(object target, ExecutedRoutedEventArgs e)
        {
            SystemCommands.RestoreWindow(this);
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
       

        #endregion

      
        private void buttonconnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
            ip_ftp = IPControl.Text;
            ConsoleOutput.Items.Add("Try to Connect to IP : " + ip_ftp + " Port 502 " + DateTime.Now);
            //modbusClient = new ModbusClient(ip_modbus, 502);
          //  modbusClient.Connect();
            this.TextConnection.Text = "Connected to Server";
            this.Connected.BorderBrush = Brushes.Green;
            ConsoleOutput.Items.Add("Connected succesfull to IP : " + ip_ftp + " Port 502 ");
            connected = true;

                
              }
            catch (Exception ex)
            {
                this.TextConnection.Text = "Connection Refused to Server";
                this.Connected.BorderBrush = Brushes.Red;
                connected = false;
                //MessageBox.Show(ex.StackTrace);
            }
        }

        private TreeViewItem CreateDirectoryNode(string path, string name)
        {
            var directoryNode = new TreeViewItem() { Header = name, Tag = "Solution" };
            var directoryListing = GetDirectoryListing(path);

            var directories = directoryListing.Where(d => d.IsDirectory);
            var files = directoryListing.Where(d => !d.IsDirectory);

            foreach (var dir in directories)
            {
                directoryNode.Items.Add(CreateDirectoryNode(dir.FullPath, dir.Name));
            }
            foreach (var file in files)
            {
                directoryNode.Items.Add(new TreeViewItem() { Header = file.Name, Tag="Solution"});
            }
            return directoryNode;
        }


        public class FTPListDetail
        {
            public bool IsDirectory
            {
                get
                {
                    return !string.IsNullOrWhiteSpace(Dir) && Dir.ToLower().Equals("d");
                }
            }
            internal string Dir { get; set; }
            public string Permission { get; set; }
            public string Filecode { get; set; }
            public string Owner { get; set; }
            public string Group { get; set; }
            public string Name { get; set; }
            public string FullPath { get; set; }
        }

        public IEnumerable<FTPListDetail> GetDirectoryListing(string rootUri)
        {
            var CurrentRemoteDirectory = rootUri;
            var result = new StringBuilder();
            var request = GetWebRequest(WebRequestMethods.Ftp.ListDirectoryDetails, CurrentRemoteDirectory);
            using (var response = request.GetResponse())
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    string line = reader.ReadLine();
                    while (line != null)
                    {
                        result.Append(line);
                        result.Append("\n");
                        line = reader.ReadLine();
                    }
                    if (string.IsNullOrEmpty(result.ToString()))
                    {
                        return new List<FTPListDetail>();
                    }
                    result.Remove(result.ToString().LastIndexOf("\n"), 1);
                    var results = result.ToString().Split('\n');
                    string regex =
                        @"^" +               //# Start of line
                        @"(?<dir>[\-ld])" +          //# File size          
                        @"(?<permission>[\-rwx]{9})" +            //# Whitespace          \n
                        @"\s+" +            //# Whitespace          \n
                        @"(?<filecode>\d+)" +
                        @"\s+" +            //# Whitespace          \n
                        @"(?<owner>\w+)" +
                        @"\s+" +            //# Whitespace          \n
                        @"(?<group>\w+)" +
                        @"\s+" +            //# Whitespace          \n
                        @"(?<size>\d+)" +
                        @"\s+" +            //# Whitespace          \n
                        @"(?<month>\w{3})" +          //# Month (3 letters)   \n
                        @"\s+" +            //# Whitespace          \n
                        @"(?<day>\d{1,2})" +        //# Day (1 or 2 digits) \n
                        @"\s+" +            //# Whitespace          \n
                        @"(?<timeyear>[\d:]{4,5})" +     //# Time or year        \n
                        @"\s+" +            //# Whitespace          \n
                        @"(?<filename>(.*))" +            //# Filename            \n
                        @"$";                //# End of line

                    var myresult = new List<FTPListDetail>();
                    foreach (var parsed in results)
                    {
                        var split = new Regex(regex)
                            .Match(parsed);
                        var dir = split.Groups["dir"].ToString();
                        var permission = split.Groups["permission"].ToString();
                        var filecode = split.Groups["filecode"].ToString();
                        var owner = split.Groups["owner"].ToString();
                        var group = split.Groups["group"].ToString();
                        var filename = split.Groups["filename"].ToString();
                        myresult.Add(new FTPListDetail()
                        {
                            Dir = dir,
                            Filecode = filecode,
                            Group = group,
                            FullPath = CurrentRemoteDirectory + "/" + filename,
                            Name = filename,
                            Owner = owner,
                            Permission = permission,
                        });
                    };
                    return myresult;
                }
            }
        }

        private FtpWebRequest GetWebRequest(string method, string uri)
        {
            Uri serverUri = new Uri(uri);
            if (serverUri.Scheme != Uri.UriSchemeFtp)
            {
                return null;
            }
            var reqFTP = (FtpWebRequest)FtpWebRequest.Create(serverUri);
            reqFTP.Method = method;
            reqFTP.UseBinary = true;
            reqFTP.Credentials = new NetworkCredential("alex", "alex");
            reqFTP.Proxy = null;
            reqFTP.KeepAlive = false;
            reqFTP.UsePassive = false;
            return reqFTP;
        }

        

        private void treeviewtest_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeViewItem SelectedItem = treeviewtest.SelectedItem as TreeViewItem;
            switch (SelectedItem.Tag.ToString())
            {
                case "Solution":
                    treeviewtest.ContextMenu = treeviewtest.Resources["SolutionContext"] as System.Windows.Controls.ContextMenu;
                    break;
                case "Folder":
                    treeviewtest.ContextMenu = treeviewtest.Resources["FolderContext"] as System.Windows.Controls.ContextMenu;
                    break;
            }
        }

        private void open_xml_from_ftp(string filename, FTP ftpclient)
        {

            try
            {
                string path = @"C:\Users\alex\m\GRASS_4_Vision.dtx";
                ftpclient.download(filename, path);
                XmlDocument xml = new XmlDocument();
                xml.Load(path);
                XmlNodeList xnList = xml.SelectNodes("//*[local-name()='Datas']/*[local-name()='Data']");
                List<Staubli_IO> io_list = new List<Staubli_IO>();
                foreach (XmlNode xn in xnList)
                {
                   
                        
                      //  MessageBox.Show(xn.Name);
                        //  string id = anode["ID"].InnerText;
                        //   string date = anode["Date"].InnerText;
                      
                            // XmlNode example = node.SelectSingleNode("Example");
                            // if (example != null)
                            // {
                            //     string na = example["Name"].InnerText;
                            //    string no = example["NO"].InnerText;
                            //}
                           // MessageBox.Show(xn.Attributes["type"].InnerText);
                            io_list.Add(new Staubli_IO(xn.Attributes["name"].InnerText,xn.Attributes["type"].InnerText,"",xn.Attributes["access"].InnerText));
                   
                }

                this.Datagridio.ItemsSource = io_list;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.StackTrace);
            }
        }
    }
}