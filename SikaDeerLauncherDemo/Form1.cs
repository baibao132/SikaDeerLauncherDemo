using System;
using System.Diagnostics;
using System.Windows.Forms;
using Gac;
using SikaDeerLauncher.Minecraft;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;

namespace SikaDeerLauncherDemo
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        DownLoadFile dlf = new DownLoadFile();//下载类
        Tools tools = new Tools();
        SikaDeerLauncher.MinecraftDownload Minecraft = new SikaDeerLauncher.MinecraftDownload();
        mcbbs.mcbbsnews mcbbsnews = new mcbbs.mcbbsnews();
        int xc = 0;
        mcbbs.mcbbsnews.newsArray[] news = new mcbbs.mcbbsnews.newsArray[0];
        mcbbs.mcbbsnews.API[] pIs = new mcbbs.mcbbsnews.API[0];
        private void Form1_Load(object sender, EventArgs e)
        {
            dlf.ThreadNum = 4;//线程数
            dlf.doSendMsg += SendMsgHander;//增加事件

            if (mcbbsnews.News(ref news))//新闻
            {
                pictureBox1.ImageLocation = news[0].IMG;
                timer1.Interval = 3000;
                timer1.Start();
            }
            if (mcbbsnews.McbbsAPI(ref pIs))//api新闻
            {
                foreach (var p in pIs)
                {
                    listBox1.Items.AddRange(new object[] { p.title});
                }
            }
            try
            {
                MCVersionList[] mc = tools.GetMCVersionList();//取mc列表
                foreach (var a in mc)
                {
                    ListViewItem list = new ListViewItem(new string[] { a.id, a.type, a.releaseTime }, -1);
                    listView1.Items.Add(list);
                }
            }
            catch (SikaDeerLauncher.SikaDeerLauncherException ex)
            {
                MessageBox.Show(ex.Message);
            }
            AllTheExistingVersion[] all = new AllTheExistingVersion[0];
            try
            {
               all = tools.GetAllTheExistingVersion();//取本地所有版本，忽略不完整版本
            }
            catch (SikaDeerLauncher.SikaDeerLauncherException ex) { }
           
            if (all.Length != 0)
            {
                foreach (var a in all)
                {
                    comboBox1.Items.Add(a.version);
                    version.Items.Add(a.version);
                    comboBox2.Items.Add(a.version);
                }
                comboBox1.Text = comboBox1.Items[0].ToString();
                version.Text = version.Items[0].ToString();
                comboBox2.Text = comboBox2.Items[0].ToString();
            }
            textBox2.Hide();
            var memory = tools.GetMemorySize();
            RAM.Text = memory.AppropriateMemory.ToString();
            java.Text = tools.GetJavaPath();
        }

        private void PictureBox1_Click(object sender, EventArgs e)
        {
            Process.Start(news[xc].Url);
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            if (xc == news.Length - 1)
            {
                xc = -1;
            }
            xc++;
            pictureBox1.ImageLocation = news[xc].IMG;

        }

        private void Button1_Click(object sender, EventArgs e)
        {
            try
            {
                MCDownload ms = Minecraft.MCjarDownload(listView1.Items[listView1.Items.IndexOf(listView1.FocusedItem)].Text);
                MCDownload ma = Minecraft.MCjsonDownload(listView1.Items[listView1.Items.IndexOf(listView1.FocusedItem)].Text);
                download(ms.path,ms.Url, listView1.Items[listView1.Items.IndexOf(listView1.FocusedItem)].Text+".jar");
                download(ma.path, ma.Url, listView1.Items[listView1.Items.IndexOf(listView1.FocusedItem)].Text+".json");
                Console.WriteLine(ms.Url);
                Console.WriteLine(ma.Url);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                MessageBox.Show("请选择需要下载的版本");
            }

        }

        /// <summary>
        /// 下载
        /// </summary>
        /// <param name="path">下载路径</param>
        /// <param name="url">下载网址</param>
        /// <param name="name">名称</param>
        public void download(string path, string url, string name)//建议采用该写法进行下载
        {
            string[] a = path.Split(Convert.ToChar(@"\"));
            string ap = null;
            for (int i = 0; i < a.Length - 1; i++)
            {
                if (i == a.Length - 2)
                {
                    ap += a[i];
                    break;
                }
                ap += a[i] + @"\";
            }
            int id = listView2.Items.Count;
            ListViewItem item = listView2.Items.Add(new ListViewItem(new string[] { (listView2.Items.Count + 1).ToString(), name, "0%", "等待中", "0B/S" }));
            dlf.AddDown(url, ap, a[a.Length - 1], id);//增加下载
            dlf.StartDown();//开始下载
        }
        private void SendMsgHander(DownMsg msg)//下载事件
        {
            int id = msg.Id;
                switch (msg.Tag)//下载类型
                {            
                    case DownStatus.Start://开始下载
                    this.Invoke((MethodInvoker)delegate ()
                    {
                        listView2.Items[id].SubItems[3].Text = "开始下载";

                    });
                    break;
                    case DownStatus.End://下载结束
                        this.Invoke((MethodInvoker)delegate ()//加入该段可以防止使用方法时的异常
                    {
                        if (listView2.Items[id].SubItems[1].Text == "Forge")
                        {
                            if (tools.ForgeInstallation(intall,intallverison))//forge安装
                            {
                                MessageBox.Show("Forge安装成功");
                            }
                        }
                        listView2.Items[id].SubItems[3].Text = "下载完成";
                    });
                    break;
                     case DownStatus.DownLoad://下载中
                       this.Invoke((MethodInvoker)delegate ()
                        {
                            listView2.Items[id].SubItems[2].Text = msg.Progress.ToString() + "%";//下载进度
                            listView2.Items[id].SubItems[4].Text = msg.SpeedInfo;//速度
                            listView2.Items[id].SubItems[3].Text = "下载中";

                        });
                    break;
                    case DownStatus.Error://下载错误
                    this.Invoke((MethodInvoker)delegate ()
                    {
                        listView2.Items[id].SubItems[3].Text = msg.ErrMessage;//下载错误提示
                        Application.DoEvents();
                    });
                    break;
                }
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            MCDownload mc = Minecraft.JavaFileDownload();//java下载
            download(mc.path, mc.Url, "java");
        }

        private void Button9_Click(object sender, EventArgs e)
        {
            try
            {
                MCVersionList[] mc = tools.GetMCVersionList();//取mc列表
                foreach (var a in mc)
                {
                    ListViewItem list = new ListViewItem(new string[] { a.id, a.type, a.releaseTime }, -1);
                    listView1.Items.Add(list);
                }
            }
            catch (SikaDeerLauncher.SikaDeerLauncherException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        string intall = null;
        string intallverison = null;
        private void Button6_Click(object sender, EventArgs e)
        {
            try
            {
                ForgeList forge = tools.GetMaxForge(comboBox2.Text);//取版本最新forge版本
                MCDownload mc = Minecraft.ForgeDownload(forge.version, forge.ForgeVersion);//取Forge下载网址
                Console.WriteLine(mc.Url);
                download(mc.path, mc.Url, "Forge");
                intall = mc.path;
                intallverison = comboBox2.Text;
            }
            catch (SikaDeerLauncher.SikaDeerLauncherException ex)
            {
                MessageBox.Show(ex.Message);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                MessageBox.Show("请选择需要下载的版本");
            }
}

        private void Button7_Click(object sender, EventArgs e)
        {
            try
            {
                tools.liteloaderInstall(comboBox2.Text);//liteloader安装
            }
            catch (SikaDeerLauncher.SikaDeerLauncherException ex)
            {
                MessageBox.Show(ex.Message);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                MessageBox.Show("请选择需要下载的版本");
            }
        }

        private void Button8_Click(object sender, EventArgs e)
        {
            try
            {
                OptiFineList[] lists = tools.GetOptiFineList(comboBox2.Text);//取OptiFine列表
                tools.OptifineInstall(lists[0].mcversion,lists[0].patch);//OptiFine安装
            }
            catch (SikaDeerLauncher.SikaDeerLauncherException ex)
            {
                MessageBox.Show(ex.Message);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                MessageBox.Show("请选择需要下载的版本");
            }
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            listView3.Items.Clear();
            try
            {
               MCDownload[] mc = tools.GetMissingLibrary(comboBox1.Text);//取所有libraries补全
                foreach (var v in mc)
                {
                    listView3.Items.Add(new ListViewItem (new string[] { v.path,v.Url} ));
                }
            }
            catch (SikaDeerLauncher.SikaDeerLauncherException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Button9_Click_1(object sender, EventArgs e)
        {
            for (int i = 0; listView3.Items.Count>i;i++)
            {
                download(listView3.Items[i].SubItems[0].Text, listView3.Items[i].SubItems[1].Text,"补全");
            }
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            listView3.Items.Clear();
            try
            {
                MCDownload[] mc = tools.GetMissingNatives(comboBox1.Text);//取所有natives补全
                foreach (var v in mc)
                {
                    listView3.Items.Add(new ListViewItem(new string[] { v.path, v.Url }));
                }
            }
            catch (SikaDeerLauncher.SikaDeerLauncherException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Button5_Click(object sender, EventArgs e)
        {
            listView3.Items.Clear();
            try
            {
                MCDownload[] mc = tools.GetMissingFile(comboBox1.Text);//取所有补全
                foreach (var v in mc)
                {
                    listView3.Items.Add(new ListViewItem(new string[] { v.path, v.Url }));
                    Console.WriteLine(v.Url);
                }
            }
            catch (SikaDeerLauncher.SikaDeerLauncherException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void TextBox1_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
        }

        private void TextBox2_Click(object sender, EventArgs e)
        {
            textBox2.Text = "";
        }

        private void ListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Process.Start(pIs[listBox1.SelectedIndex].link);
        }

        private void ComboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox3.SelectedIndex == 0)
            {
                textBox2.Hide();
                return;
            }
            if (comboBox3.SelectedIndex == 1)
            {
                textBox2.Show();
                return;
            }
            if (comboBox3.SelectedIndex == 2)
            {
                textBox2.Show();
                return;
            }
            if (comboBox3.SelectedIndex == 3)
            {
                textBox2.Show();
                return;
            }

        }
        private void TextBox6_TextChanged(object sender, EventArgs e)
        {

        }

        private void A6_Click(object sender, EventArgs e)
        {
            
        }
        private void Down_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (down.SelectedIndex == 0)
            {
                tools.DownloadSourceInitialization(DownloadSource.bmclapiSource);//bmclapi下载源
            }
            else
            {
                tools.DownloadSourceInitialization(DownloadSource.MinecraftSource);//Minecraft官方源
            }
        }

        private void Button10_Click(object sender, EventArgs e)
        {
            Game game = new Game();
            string arg = null;
            if (ip.Text != "")
            {
                arg = "--server " + ip.Text;
                if (port.Text == "")
                {
                    arg += " --port 25565";
                }
                else
                {
                    arg += " --port "+port.Text;
                }
            }
            if (hzcs.Text != "" && arg == null)
            {
                arg = hzcs.Text;
            }
            else if (hzcs.Text != "" && arg != null)
            {
                arg += " " + hzcs.Text;
            }
            if (TB.Text != "")
            {
                timer2.Interval = 2000;
                timer2.Start();
            }
            try
            {
                game.ErrorEvent += new Game.ErrorDel(error);//错误事件
                game.LogEvent += new Game.LogDel(log);//log事件
                if (comboBox3.SelectedIndex == 0)
                {
                    game.StartGame(version.Text, java.Text, Convert.ToInt32(RAM.Text), textBox1.Text, qzcs.Text, hzcs.Text);//离线登录启动游戏
                }
                if (comboBox3.SelectedIndex == 1)
                {
                    game.StartGame(version.Text, java.Text, Convert.ToInt32(RAM.Text), textBox1.Text, textBox2.Text, qzcs.Text, hzcs.Text);//正版登录启动游戏
                }
                if (comboBox3.SelectedIndex == 2)
                {
                    Skin skin = tools.GetAuthlib_Injector("https://mcskin.i-creator.cn/api/yggdrasil", textBox1.Text, textBox2.Text);
                    game.StartGame(version.Text, java.Text, Convert.ToInt32(RAM.Text), skin.NameItem[0].Name, skin.NameItem[1].uuid, skin.accessToken, "https://mcskin.i-creator.cn/api/yggdrasil", qzcs.Text, hzcs.Text,AuthenticationServerMode.yggdrasil);//外置登录启动游戏
                }
                if (comboBox3.SelectedIndex == 3)
                {
                    UnifiedPass UP = tools.GetUnifiedPass(ID.Text, textBox1.Text, textBox2.Text);
                    game.StartGame(version.Text, java.Text, Convert.ToInt32(RAM.Text), UP.name, UP.id, UP.accessToken, ID.Text, qzcs.Text, hzcs.Text, AuthenticationServerMode.UnifiedPass);//外置登录启动游戏

                }
            }
            catch (SikaDeerLauncher.SikaDeerLauncherException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void error(Game.Error error)
        {
            System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = false;
            textBox3.Text += error.Message + "\n";
        }
        private void log(Game.Log log)
        {
            System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = false;
            textBox4.Text += log.Message + "\n";
        }
        private void Button12_Click(object sender, EventArgs e)
        {
            if (ip.Text != "")
            {
                if (port.Text == "")
                {
                    port.Text = "25565";
                }
                try
                {
                    MinecraftServer.Server.ServerInfo info = tools.GetServerInformation(ip.Text, Convert.ToInt32(port.Text));//取服务器信息
                    pictureBox3.Image = info.Icon;
                    label10.Text = info.MOTD + "\n延迟：" + info.Ping + "   在线人数：" + info.CurrentPlayerCount;
                    Console.WriteLine(info.JsonResult);
                }
                catch (SikaDeerLauncher.SikaDeerLauncherException ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void Button11_Click(object sender, EventArgs e)
        {
            listView3.Items.Clear();
            try
            {
                MCDownload[] mc = tools.GetMissingAsset(comboBox1.Text);
                foreach (var v in mc)
                {
                    listView3.Items.Add(new ListViewItem(new string[] { v.path, v.Url }));
                }
            }
            catch (SikaDeerLauncher.SikaDeerLauncherException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void Timer2_Tick(object sender, EventArgs e)
        {
                if (tools.ChangeTheTitle(TB.Text))//修改标题
                {
                    timer2.Stop();
                }
        }
    }
}
