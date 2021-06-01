using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static WpfApp2.ShellAPI;

namespace WpfApp2
{
    /// <summary>
    /// MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ShellItem m_shDesktop = new ShellItem();
        public MainWindow()
        {
            InitializeComponent();
            SystemImageList.SetTVImageList(treeView1.Handle);
            SystemImageList.SetLVImageList(listView1.Handle);
            LoadRootNodes();
        }

        private void setListItem(String filePath)
        {
            listView1.View = System.Windows.Forms.View.Details;
            listView1.Clear();
            listView1.Columns.Add("名前");
            listView1.Columns.Add("サイズ");


            try
            {
                
                DirectoryInfo dirList = new DirectoryInfo(filePath);
                foreach (DirectoryInfo di in dirList.GetDirectories())
                {
                    ListViewItem item = new ListViewItem(di.Name);

                    // get folder type, icon
                    int iconIndex = 0;
                    SHFILEINFO shFileInfo = new SHFILEINFO();

                    IntPtr hSuccess = SHGetFileInfo(di.FullName, 0, out shFileInfo,
                        (uint)Marshal.SizeOf(shFileInfo),
                        SHGFI.SHGFI_ICON | SHGFI.SHGFI_LARGEICON | SHGFI.SHGFI_SMALLICON | SHGFI.SHGFI_SYSICONINDEX | SHGFI.SHGFI_TYPENAME);

                    if (hSuccess != IntPtr.Zero)
                    {
                        iconIndex = shFileInfo.iIcon;
                    }

                    item.ImageIndex = iconIndex;
                    item.SubItems.Add("");

                    // add to listview
                    listView1.Items.Add(item);
                }

                
                List<String> files = Directory.GetFiles(filePath).ToList<String>();
                foreach (String file in files)
                {
                    FileInfo info = new FileInfo(file);
                    ListViewItem item = new ListViewItem(info.Name);


                    // get file type, icon 
                    int iconIndex = 0;
                    SHFILEINFO shinfo = new SHFILEINFO();
                    IntPtr hSuccess = SHGetFileInfo(info.FullName, 0, out shinfo, (uint)Marshal.SizeOf(shinfo),
                        SHGFI.SHGFI_ICON | SHGFI.SHGFI_LARGEICON | SHGFI.SHGFI_SMALLICON | SHGFI.SHGFI_SYSICONINDEX | SHGFI.SHGFI_TYPENAME);
                    if (hSuccess != IntPtr.Zero)
                    {
                        iconIndex = shinfo.iIcon;
                    }

                    // 各列の内容を設定
                    item.ImageIndex = iconIndex;
                    item.SubItems.Add(getFileSize(info.Length));
                    listView1.Items.Add(item);
                }
            }
            catch (IOException ie)
            {
                System.Windows.Forms.MessageBox.Show(ie.Message, "選択エラー");
            }

            listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }


        private String getFileSize(long fileSize)
        {
            String ret = fileSize + " バイト";
            if (fileSize > (1024f * 1024f * 1024f))
            {
                ret = Math.Round((fileSize / 1024f / 1024f / 1024f), 2).ToString() + " GB";
            }
            else if (fileSize > (1024f * 1024f))
            {
                ret = Math.Round((fileSize / 1024f / 1024f), 2).ToString() + " MB";
            }
            else if (fileSize > 1024f)
            {
                ret = Math.Round((fileSize / 1024f)).ToString() + " KB";
            }

            return ret;
        }

        private void LoadRootNodes()
        {
            // Create the root node.
            TreeNode tvwRoot = new TreeNode();
            tvwRoot.Text = m_shDesktop.DisplayName;
            tvwRoot.ImageIndex = m_shDesktop.IconIndex;
            tvwRoot.SelectedImageIndex = m_shDesktop.IconIndex;
            tvwRoot.Tag = m_shDesktop;

            // add any children to the root node.
            ArrayList arrChildren = m_shDesktop.GetSubFolders();
            foreach (ShellItem shChild in arrChildren)
            {
                TreeNode tvwChild = new TreeNode();
                tvwChild.Text = shChild.DisplayName;
                tvwChild.ImageIndex = shChild.IconIndex;
                tvwChild.SelectedImageIndex = shChild.IconIndex;
                tvwChild.Tag = shChild;

                // If this is a folder item and has children then add a place holder node.
                if (shChild.IsFolder && shChild.HasSubFolder)
                    tvwChild.Nodes.Add("PH");
                tvwRoot.Nodes.Add(tvwChild);
            }

            // Add the root node to the tree.
            treeView1.Nodes.Clear();
            treeView1.Nodes.Add(tvwRoot);
            tvwRoot.Expand();
        }

        private void treeView1_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            TreeNode node = e.Node;
            node.Nodes.Clear();

            ShellItem shNode = (ShellItem)e.Node.Tag;
            ArrayList arrSub = shNode.GetSubFolders();

            foreach (ShellItem shChild in arrSub)
            {
                TreeNode tvwChild = new TreeNode();
                tvwChild.Text = shChild.DisplayName;
                tvwChild.ImageIndex = shChild.IconIndex;
                tvwChild.SelectedImageIndex = shChild.IconIndex;
                tvwChild.Tag = shChild;

                if (shChild.IsFolder && shChild.HasSubFolder)
                    tvwChild.Nodes.Add("PH");
                node.Nodes.Add(tvwChild);
            }
            setListItem(shNode.GetPath());
        }



        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            //setListItem();　.. selected item path 
        }
    }
}
