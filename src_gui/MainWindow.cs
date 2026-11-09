using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Markup;
using System.Windows.Forms;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using Button = System.Windows.Controls.Button;
using TextBox = System.Windows.Controls.TextBox;
using RadioButton = System.Windows.Controls.RadioButton;
using ProgressBar = System.Windows.Controls.ProgressBar;
using Clipboard = System.Windows.Clipboard;

namespace AntigravityLocalizationTool
{
    public class Program
    {
        [STAThread]
        public static void Main()
        {
            try
            {
                AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                {
                    MessageBox.Show("程序发生异常:\r\n" + (e.ExceptionObject != null ? e.ExceptionObject.ToString() : "未知错误"),
                        "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                };

                var app = new Application();
                var controller = new MainController();
                app.Run(controller.Window);
            }
            catch (Exception ex)
            {
                MessageBox.Show("启动失败:\r\n" + ex.ToString(), "启动异常", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public class MainController
    {
        public Window Window { get; private set; }

        private TextBox txtInstallDir;
        private Button btnBrowse;
        private Button btnOpenFolder;
        private TextBlock txtAppStatus;
        private TextBlock txtPatchStatus;
        private TextBlock txtEngineStatus;
        private RadioButton rbLangSimp;
        private RadioButton rbLangTrad;
        private RadioButton rbBrandEnglish;
        private RadioButton rbBrandHidden;
        private RadioButton rbBrandTranslated;
        private Button btnInstall;
        private Button btnRestore;
        private Button btnLaunch;
        private TextBox txtLog;
        private ProgressBar progressBar;
        private TextBlock txtFooterStatus;

        private string workingDir;

        public MainController()
        {
            workingDir = AppDomain.CurrentDomain.BaseDirectory;
            InitializeUi();
            RefreshAllStatus();
        }

        private void InitializeUi()
        {
            string xaml = @"
<Window xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
        xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
        Title='Antigravity 智能中文汉化管理器'
        Width='760' Height='730' MinWidth='680' MinHeight='650'
        WindowStartupLocation='CenterScreen'
        Background='#0D0F17' Foreground='#ECEFF4'
        FontFamily='Microsoft YaHei, Segoe UI, sans-serif'>
    <Window.Resources>
        <Style TargetType='Border' x:Key='CardStyle'>
            <Setter Property='Background' Value='#151824'/>
            <Setter Property='BorderBrush' Value='#222738'/>
            <Setter Property='BorderThickness' Value='1'/>
            <Setter Property='CornerRadius' Value='12'/>
            <Setter Property='Padding' Value='16'/>
        </Style>

        <Style TargetType='RadioButton' x:Key='ModernRadio'>
            <Setter Property='Foreground' Value='#D8DEE9'/>
            <Setter Property='FontSize' Value='13'/>
            <Setter Property='Margin' Value='0,4,18,4'/>
            <Setter Property='Cursor' Value='Hand'/>
        </Style>

        <Style TargetType='Button' x:Key='PrimaryBtn'>
            <Setter Property='Background'>
                <Setter.Value>
                    <LinearGradientBrush StartPoint='0,0' EndPoint='1,0'>
                        <GradientStop Color='#6366F1' Offset='0.0'/>
                        <GradientStop Color='#4F46E5' Offset='1.0'/>
                    </LinearGradientBrush>
                </Setter.Value>
            </Setter>
            <Setter Property='Foreground' Value='#FFFFFF'/>
            <Setter Property='FontSize' Value='14'/>
            <Setter Property='FontWeight' Value='Bold'/>
            <Setter Property='Padding' Value='24,11'/>
            <Setter Property='Cursor' Value='Hand'/>
            <Setter Property='BorderThickness' Value='0'/>
            <Setter Property='Template'>
                <Setter.Value>
                    <ControlTemplate TargetType='Button'>
                        <Border x:Name='border' Background='{TemplateBinding Background}' CornerRadius='8' Padding='{TemplateBinding Padding}'>
                            <ContentPresenter HorizontalAlignment='Center' VerticalAlignment='Center'/>
                        </Border>
                        <ControlTemplate.Triggers>
                            <Trigger Property='IsMouseOver' Value='True'>
                                <Setter TargetName='border' Property='Background'>
                                    <Setter.Value>
                                        <LinearGradientBrush StartPoint='0,0' EndPoint='1,0'>
                                            <GradientStop Color='#818CF8' Offset='0.0'/>
                                            <GradientStop Color='#6366F1' Offset='1.0'/>
                                        </LinearGradientBrush>
                                    </Setter.Value>
                                </Setter>
                            </Trigger>
                            <Trigger Property='IsPressed' Value='True'>
                                <Setter TargetName='border' Property='Background' Value='#3730A3'/>
                            </Trigger>
                            <Trigger Property='IsEnabled' Value='False'>
                                <Setter TargetName='border' Property='Background' Value='#252A38'/>
                                <Setter Property='Foreground' Value='#64748B'/>
                            </Trigger>
                        </ControlTemplate.Triggers>
                    </ControlTemplate>
                </Setter.Value>
            </Setter>
        </Style>

        <Style TargetType='Button' x:Key='SecondaryBtn'>
            <Setter Property='Background' Value='#1E2333'/>
            <Setter Property='Foreground' Value='#E2E8F0'/>
            <Setter Property='FontSize' Value='14'/>
            <Setter Property='Padding' Value='18,10'/>
            <Setter Property='Cursor' Value='Hand'/>
            <Setter Property='BorderThickness' Value='1'/>
            <Setter Property='BorderBrush' Value='#333B52'/>
            <Setter Property='Template'>
                <Setter.Value>
                    <ControlTemplate TargetType='Button'>
                        <Border x:Name='border' Background='{TemplateBinding Background}' BorderBrush='{TemplateBinding BorderBrush}' BorderThickness='{TemplateBinding BorderThickness}' CornerRadius='8' Padding='{TemplateBinding Padding}'>
                            <ContentPresenter HorizontalAlignment='Center' VerticalAlignment='Center'/>
                        </Border>
                        <ControlTemplate.Triggers>
                            <Trigger Property='IsMouseOver' Value='True'>
                                <Setter TargetName='border' Property='Background' Value='#2B3248'/>
                                <Setter TargetName='border' Property='BorderBrush' Value='#475569'/>
                            </Trigger>
                            <Trigger Property='IsPressed' Value='True'>
                                <Setter TargetName='border' Property='Background' Value='#171B26'/>
                            </Trigger>
                            <Trigger Property='IsEnabled' Value='False'>
                                <Setter TargetName='border' Property='Background' Value='#151822'/>
                                <Setter Property='Foreground' Value='#4B5563'/>
                            </Trigger>
                        </ControlTemplate.Triggers>
                    </ControlTemplate>
                </Setter.Value>
            </Setter>
        </Style>

        <Style TargetType='Button' x:Key='SmallBtn'>
            <Setter Property='Background' Value='#1E2333'/>
            <Setter Property='Foreground' Value='#CBD5E1'/>
            <Setter Property='FontSize' Value='12'/>
            <Setter Property='Padding' Value='10,5'/>
            <Setter Property='Cursor' Value='Hand'/>
            <Setter Property='BorderThickness' Value='1'/>
            <Setter Property='BorderBrush' Value='#333B52'/>
            <Setter Property='Template'>
                <Setter.Value>
                    <ControlTemplate TargetType='Button'>
                        <Border x:Name='border' Background='{TemplateBinding Background}' BorderBrush='{TemplateBinding BorderBrush}' BorderThickness='{TemplateBinding BorderThickness}' CornerRadius='6' Padding='{TemplateBinding Padding}'>
                            <ContentPresenter HorizontalAlignment='Center' VerticalAlignment='Center'/>
                        </Border>
                        <ControlTemplate.Triggers>
                            <Trigger Property='IsMouseOver' Value='True'>
                                <Setter TargetName='border' Property='Background' Value='#2F374E'/>
                            </Trigger>
                            <Trigger Property='IsEnabled' Value='False'>
                                <Setter Property='Foreground' Value='#4B5563'/>
                            </Trigger>
                        </ControlTemplate.Triggers>
                    </ControlTemplate>
                </Setter.Value>
            </Setter>
        </Style>
    </Window.Resources>

    <Grid Margin='22'>
        <Grid.RowDefinitions>
            <RowDefinition Height='Auto'/>
            <RowDefinition Height='Auto'/>
            <RowDefinition Height='Auto'/>
            <RowDefinition Height='Auto'/>
            <RowDefinition Height='*'/>
            <RowDefinition Height='Auto'/>
        </Grid.RowDefinitions>

        <!-- 头部 Header -->
        <Border Grid.Row='0' Margin='0,0,0,14'>
            <Grid>
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width='Auto'/>
                    <ColumnDefinition Width='*'/>
                </Grid.ColumnDefinitions>
                <Border Grid.Column='0' Width='48' Height='48' CornerRadius='14' Margin='0,0,16,0'>
                    <Border.Background>
                        <LinearGradientBrush StartPoint='0,0' EndPoint='1,1'>
                            <GradientStop Color='#6366F1' Offset='0.0'/>
                            <GradientStop Color='#06B6D4' Offset='1.0'/>
                        </LinearGradientBrush>
                    </Border.Background>
                    <TextBlock Text='文' FontSize='26' FontWeight='Bold' Foreground='#FFFFFF' HorizontalAlignment='Center' VerticalAlignment='Center'/>
                </Border>
                <StackPanel Grid.Column='1' VerticalAlignment='Center'>
                    <StackPanel Orientation='Horizontal'>
                        <TextBlock Text='Antigravity 智能中文汉化管理器' FontSize='20' FontWeight='Bold' Foreground='#FFFFFF' VerticalAlignment='Center'/>
                        <Border Background='#1F273B' CornerRadius='4' Padding='6,2' Margin='12,0,0,0' VerticalAlignment='Center'>
                            <TextBlock Text='v2.0 增强版' FontSize='11' Foreground='#818CF8' FontWeight='Medium'/>
                        </Border>
                    </StackPanel>
                    <TextBlock Text='免配 Node.js 环境 · 智能双引擎自适应 · 纯原生无损 ASAR 打包与一键恢复' FontSize='12' Foreground='#8896AB' Margin='0,4,0,0'/>
                </StackPanel>
            </Grid>
        </Border>

        <!-- 状态卡片 -->
        <Border Grid.Row='1' Style='{StaticResource CardStyle}' Margin='0,0,0,12'>
            <StackPanel>
                <Grid Margin='0,0,0,12'>
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width='Auto'/>
                        <ColumnDefinition Width='*'/>
                        <ColumnDefinition Width='Auto'/>
                        <ColumnDefinition Width='Auto'/>
                    </Grid.ColumnDefinitions>
                    <TextBlock Grid.Column='0' Text='安装目录：' VerticalAlignment='Center' Foreground='#94A3B8' FontSize='13' Margin='0,0,8,0'/>
                    <TextBox x:Name='txtInstallDir' Grid.Column='1' Background='#0A0C13' Foreground='#F1F5F9' BorderBrush='#262C3F' BorderThickness='1' Padding='8,6' FontSize='12' VerticalAlignment='Center'/>
                    <Button x:Name='btnBrowse' Grid.Column='2' Content='浏览...' Style='{StaticResource SmallBtn}' Margin='8,0,0,0' VerticalAlignment='Center'/>
                    <Button x:Name='btnOpenFolder' Grid.Column='3' Content='打开目录' Style='{StaticResource SmallBtn}' Margin='6,0,0,0' VerticalAlignment='Center'/>
                </Grid>

                <WrapPanel Orientation='Horizontal'>
                    <Border Background='#1A1F2C' CornerRadius='6' Padding='10,6' Margin='0,0,10,4'>
                        <StackPanel Orientation='Horizontal'>
                            <TextBlock Text='客户端：' Foreground='#94A3B8' FontSize='12'/>
                            <TextBlock x:Name='txtAppStatus' Text='检测中...' Foreground='#38BDF8' FontSize='12' FontWeight='SemiBold'/>
                        </StackPanel>
                    </Border>
                    <Border Background='#1A1F2C' CornerRadius='6' Padding='10,6' Margin='0,0,10,4'>
                        <StackPanel Orientation='Horizontal'>
                            <TextBlock Text='汉化状态：' Foreground='#94A3B8' FontSize='12'/>
                            <TextBlock x:Name='txtPatchStatus' Text='检测中...' Foreground='#34D399' FontSize='12' FontWeight='SemiBold'/>
                        </StackPanel>
                    </Border>
                    <Border Background='#1A1F2C' CornerRadius='6' Padding='10,6' Margin='0,0,10,4'>
                        <StackPanel Orientation='Horizontal'>
                            <TextBlock Text='运行引擎：' Foreground='#94A3B8' FontSize='12'/>
                            <TextBlock x:Name='txtEngineStatus' Text='检测中...' Foreground='#FBBF24' FontSize='12' FontWeight='SemiBold'/>
                        </StackPanel>
                    </Border>
                </WrapPanel>
            </StackPanel>
        </Border>

        <!-- 配置卡片 -->
        <Border Grid.Row='2' Style='{StaticResource CardStyle}' Margin='0,0,0,12'>
            <StackPanel>
                <StackPanel Orientation='Horizontal' Margin='0,0,0,10'>
                    <TextBlock Text='目标语言：' Foreground='#94A3B8' FontSize='13' Width='90' VerticalAlignment='Center'/>
                    <RadioButton x:Name='rbLangSimp' Content='简体中文 (Simplified Chinese 推荐)' IsChecked='True' Style='{StaticResource ModernRadio}'/>
                    <RadioButton x:Name='rbLangTrad' Content='繁體中文 (Traditional Chinese)' Style='{StaticResource ModernRadio}'/>
                </StackPanel>

                <StackPanel Orientation='Horizontal'>
                    <TextBlock Text='品牌名称：' Foreground='#94A3B8' FontSize='13' Width='90' VerticalAlignment='Center'/>
                    <RadioButton x:Name='rbBrandEnglish' Content='保持英文 Antigravity (推荐)' IsChecked='True' Style='{StaticResource ModernRadio}'/>
                    <RadioButton x:Name='rbBrandHidden' Content='隐藏品牌名' Style='{StaticResource ModernRadio}'/>
                    <RadioButton x:Name='rbBrandTranslated' Content='汉化品牌名 (反重力)' Style='{StaticResource ModernRadio}'/>
                </StackPanel>
            </StackPanel>
        </Border>

        <!-- 操作按钮栏 -->
        <Border Grid.Row='3' Margin='0,0,0,12'>
            <Grid>
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width='Auto'/>
                    <ColumnDefinition Width='Auto'/>
                    <ColumnDefinition Width='*'/>
                    <ColumnDefinition Width='Auto'/>
                </Grid.ColumnDefinitions>
                <Button x:Name='btnInstall' Grid.Column='0' Content='⚡ 一键安装中文汉化' Style='{StaticResource PrimaryBtn}' Margin='0,0,12,0'/>
                <Button x:Name='btnRestore' Grid.Column='1' Content='🔄 还原官方英文原版' Style='{StaticResource SecondaryBtn}' Margin='0,0,12,0'/>
                <Button x:Name='btnLaunch' Grid.Column='3' Content='🚀 启动 Antigravity' Style='{StaticResource SecondaryBtn}'/>
            </Grid>
        </Border>

        <!-- 实时日志面板 -->
        <Border Grid.Row='4' Style='{StaticResource CardStyle}' Padding='12' Margin='0,0,0,10'>
            <Grid>
                <Grid.RowDefinitions>
                    <RowDefinition Height='Auto'/>
                    <RowDefinition Height='*'/>
                </Grid.RowDefinitions>
                <Grid Grid.Row='0' Margin='0,0,0,8'>
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width='*'/>
                        <ColumnDefinition Width='Auto'/>
                        <ColumnDefinition Width='Auto'/>
                    </Grid.ColumnDefinitions>
                    <TextBlock Grid.Column='0' Text='执行过程输出控制台' FontSize='12' FontWeight='SemiBold' Foreground='#64748B' VerticalAlignment='Center'/>
                    <Button x:Name='btnCopyLog' Grid.Column='1' Content='复制日志' Style='{StaticResource SmallBtn}' Padding='8,3' FontSize='11' Margin='0,0,6,0'/>
                    <Button x:Name='btnClearLog' Grid.Column='2' Content='清空' Style='{StaticResource SmallBtn}' Padding='8,3' FontSize='11'/>
                </Grid>
                <TextBox x:Name='txtLog' Grid.Row='1' Background='#090B10' Foreground='#94A3B8'
                         BorderBrush='#1A1F2C' BorderThickness='1' Padding='10'
                         FontFamily='Consolas, Cascadia Code, Courier New' FontSize='12'
                         TextWrapping='Wrap' AcceptsReturn='True' IsReadOnly='True'
                         VerticalScrollBarVisibility='Auto'/>
            </Grid>
        </Border>

        <!-- 底部状态与进度条 -->
        <Grid Grid.Row='5'>
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width='*'/>
                <ColumnDefinition Width='160'/>
            </Grid.ColumnDefinitions>
            <TextBlock x:Name='txtFooterStatus' Grid.Column='0' Text='就绪，等待操作。' Foreground='#64748B' FontSize='12' VerticalAlignment='Center'/>
            <ProgressBar x:Name='progressBar' Grid.Column='1' Height='6' IsIndeterminate='False' Visibility='Collapsed' Background='#151824' Foreground='#6366F1'/>
        </Grid>
    </Grid>
</Window>
";
            StringReader sr = new StringReader(xaml);
            System.Xml.XmlReader xmlReader = System.Xml.XmlReader.Create(sr);
            Window = (Window)XamlReader.Load(xmlReader);

            // 加载窗口图标
            string icoPath = Path.Combine(workingDir, "app.ico");
            if (File.Exists(icoPath))
            {
                try
                {
                    Window.Icon = BitmapFrame.Create(new Uri(icoPath, UriKind.Absolute));
                }
                catch { }
            }

            // 绑定控件
            txtInstallDir = (TextBox)Window.FindName("txtInstallDir");
            btnBrowse = (Button)Window.FindName("btnBrowse");
            btnOpenFolder = (Button)Window.FindName("btnOpenFolder");
            txtAppStatus = (TextBlock)Window.FindName("txtAppStatus");
            txtPatchStatus = (TextBlock)Window.FindName("txtPatchStatus");
            txtEngineStatus = (TextBlock)Window.FindName("txtEngineStatus");
            rbLangSimp = (RadioButton)Window.FindName("rbLangSimp");
            rbLangTrad = (RadioButton)Window.FindName("rbLangTrad");
            rbBrandEnglish = (RadioButton)Window.FindName("rbBrandEnglish");
            rbBrandHidden = (RadioButton)Window.FindName("rbBrandHidden");
            rbBrandTranslated = (RadioButton)Window.FindName("rbBrandTranslated");
            btnInstall = (Button)Window.FindName("btnInstall");
            btnRestore = (Button)Window.FindName("btnRestore");
            btnLaunch = (Button)Window.FindName("btnLaunch");
            txtLog = (TextBox)Window.FindName("txtLog");
            progressBar = (ProgressBar)Window.FindName("progressBar");
            txtFooterStatus = (TextBlock)Window.FindName("txtFooterStatus");

            Button btnClearLog = (Button)Window.FindName("btnClearLog");
            if (btnClearLog != null)
            {
                btnClearLog.Click += (s, e) => { txtLog.Clear(); };
            }

            Button btnCopyLog = (Button)Window.FindName("btnCopyLog");
            if (btnCopyLog != null)
            {
                btnCopyLog.Click += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(txtLog.Text))
                    {
                        Clipboard.SetText(txtLog.Text);
                        txtFooterStatus.Text = "日志已复制到剪贴板。";
                    }
                };
            }

            btnBrowse.Click += BtnBrowse_Click;
            btnOpenFolder.Click += BtnOpenFolder_Click;
            btnInstall.Click += BtnInstall_Click;
            btnRestore.Click += BtnRestore_Click;
            btnLaunch.Click += BtnLaunch_Click;

            // 初始化默认路径
            string detectedDir = DetectAntigravityDir();
            txtInstallDir.Text = detectedDir;
            txtInstallDir.TextChanged += (s, e) => { RefreshAllStatus(); };

            Log("==================================================");
            Log("  Antigravity 智能中文汉化管理器 已就绪");
            Log("  工具目录: " + workingDir);
            Log("  核心特色: 无需安装 Node.js, 纯原生极速无损打包");
            Log("==================================================");
        }

        private void Log(string message)
        {
            Window.Dispatcher.Invoke(new Action(() =>
            {
                txtLog.AppendText(string.Format("[{0}] {1}\r\n", DateTime.Now.ToString("HH:mm:ss"), message));
                txtLog.ScrollToEnd();
            }));
        }

        private string DetectAntigravityDir()
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string candidate1 = Path.Combine(localAppData, "Programs", "antigravity");
            if (Directory.Exists(candidate1) && File.Exists(Path.Combine(candidate1, "Antigravity.exe")))
            {
                return candidate1;
            }

            string progFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string candidate2 = Path.Combine(progFiles, "Antigravity");
            if (Directory.Exists(candidate2) && File.Exists(Path.Combine(candidate2, "Antigravity.exe")))
            {
                return candidate2;
            }

            return candidate1;
        }

        private void RefreshAllStatus()
        {
            string installDir = txtInstallDir != null ? txtInstallDir.Text.Trim() : "";
            
            // 1. 检查客户端运行状态
            Process[] procs = Process.GetProcessesByName("Antigravity");
            if (procs.Length > 0)
            {
                txtAppStatus.Text = "运行中 (" + procs.Length + " 个进程)";
                txtAppStatus.Foreground = new SolidColorBrush(Color.FromRgb(251, 191, 36));
            }
            else
            {
                txtAppStatus.Text = "已关闭";
                txtAppStatus.Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));
            }

            // 2. 检查汉化状态
            string resourcesDir = Path.Combine(installDir, "resources");
            string asarPath = Path.Combine(resourcesDir, "app.asar");
            string bakPath = Path.Combine(resourcesDir, "app.asar.bak");

            if (!File.Exists(asarPath))
            {
                txtPatchStatus.Text = "未找到主程序资源";
                txtPatchStatus.Foreground = new SolidColorBrush(Color.FromRgb(239, 68, 68));
            }
            else
            {
                bool isPatched = CheckIfAsarPatched(asarPath);
                if (isPatched)
                {
                    txtPatchStatus.Text = "已安装汉化" + (File.Exists(bakPath) ? " (含备份)" : "");
                    txtPatchStatus.Foreground = new SolidColorBrush(Color.FromRgb(52, 211, 153));
                }
                else
                {
                    txtPatchStatus.Text = "官方原版英文" + (File.Exists(bakPath) ? " (含历史备份)" : "");
                    txtPatchStatus.Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));
                }
            }

            // 3. 检查执行引擎
            string engineType = DetectEngine(installDir);
            txtEngineStatus.Text = engineType;
            txtEngineStatus.Foreground = new SolidColorBrush(Color.FromRgb(129, 140, 248));
        }

        private bool CheckIfAsarPatched(string asarPath)
        {
            try
            {
                using (FileStream fs = new FileStream(asarPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    byte[] sig = Encoding.UTF8.GetBytes("/* --- ANTIGRAVITY CHINESE LOCALIZATION START --- */");
                    byte[] buffer = new byte[Math.Min(2 * 1024 * 1024, (int)fs.Length)];
                    int read = fs.Read(buffer, 0, buffer.Length);
                    return IndexOfSequence(buffer, sig) != -1;
                }
            }
            catch
            {
                return false;
            }
        }

        private static int IndexOfSequence(byte[] buffer, byte[] pattern)
        {
            int max = buffer.Length - pattern.Length;
            for (int i = 0; i <= max; i++)
            {
                bool match = true;
                for (int j = 0; j < pattern.Length; j++)
                {
                    if (buffer[i + j] != pattern[j])
                    {
                        match = false;
                        break;
                    }
                }
                if (match) return i;
            }
            return -1;
        }

        private string DetectEngine(string installDir)
        {
            if (IsCommandAvailable("node"))
            {
                return "系统 Node.js";
            }

            string antigravityExe = Path.Combine(installDir, "Antigravity.exe");
            if (File.Exists(antigravityExe))
            {
                return "Antigravity 内置引擎 (免装Node)";
            }

            return "未检测到 Node 或客户端";
        }

        private bool IsCommandAvailable(string cmd)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo("where", cmd);
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;
                using (Process p = Process.Start(psi))
                {
                    p.WaitForExit();
                    return p.ExitCode == 0;
                }
            }
            catch
            {
                return false;
            }
        }

        private void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            dialog.Description = "请选择 Antigravity 的安装目录 (包含 Antigravity.exe 或 resources 目录)";
            if (Directory.Exists(txtInstallDir.Text))
            {
                dialog.SelectedPath = txtInstallDir.Text;
            }
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtInstallDir.Text = dialog.SelectedPath;
                RefreshAllStatus();
            }
        }

        private void BtnOpenFolder_Click(object sender, RoutedEventArgs e)
        {
            string dir = txtInstallDir.Text.Trim();
            if (Directory.Exists(dir))
            {
                Process.Start(new ProcessStartInfo("explorer.exe", "\"" + dir + "\"") { UseShellExecute = true });
            }
            else
            {
                MessageBox.Show("当前目录不存在: " + dir, "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void SetUiBusy(bool busy, string statusText)
        {
            btnInstall.IsEnabled = !busy;
            btnRestore.IsEnabled = !busy;
            btnLaunch.IsEnabled = !busy;
            btnBrowse.IsEnabled = !busy;
            btnOpenFolder.IsEnabled = !busy;
            txtInstallDir.IsEnabled = !busy;
            progressBar.Visibility = busy ? Visibility.Visible : Visibility.Collapsed;
            progressBar.IsIndeterminate = busy;
            txtFooterStatus.Text = statusText;
        }

        private void BtnInstall_Click(object sender, RoutedEventArgs e)
        {
            string installDir = txtInstallDir.Text.Trim();
            if (!Directory.Exists(installDir))
            {
                MessageBox.Show("指定 Antigravity 安装目录不存在，请先检查路径！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool isTw = rbLangTrad.IsChecked == true;
            string brandTitle = "english";
            if (rbBrandHidden.IsChecked == true) brandTitle = "hidden";
            if (rbBrandTranslated.IsChecked == true) brandTitle = "translated";

            SetUiBusy(true, "正在注入汉化包，请稍候...");
            Log(string.Format(">>> 开始执行汉化操作 (语言: {0}, 品牌显示: {1})...", isTw ? "繁體中文" : "简体中文", brandTitle));

            Task.Factory.StartNew(() =>
            {
                RunScript(installDir, isTw, brandTitle, false);
            });
        }

        private void BtnRestore_Click(object sender, RoutedEventArgs e)
        {
            string installDir = txtInstallDir.Text.Trim();
            if (!Directory.Exists(installDir))
            {
                MessageBox.Show("指定 Antigravity 安装目录不存在，请先检查路径！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirm = MessageBox.Show("确定要卸载汉化，恢复为官方原版英文吗？", "确认还原", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm != MessageBoxResult.Yes) return;

            SetUiBusy(true, "正在还原官方原版英文，请稍候...");
            Log(">>> 开始执行官方英文还原操作...");

            Task.Factory.StartNew(() =>
            {
                RunScript(installDir, false, "english", true);
            });
        }

        private void BtnLaunch_Click(object sender, RoutedEventArgs e)
        {
            string installDir = txtInstallDir.Text.Trim();
            string exePath = Path.Combine(installDir, "Antigravity.exe");
            if (!File.Exists(exePath))
            {
                MessageBox.Show("未找到主程序: " + exePath, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo(exePath) { UseShellExecute = true });
                Log("已启动客户端: " + exePath);
                txtFooterStatus.Text = "客户端已启动。";
                Task.Delay(1000).ContinueWith(t => Window.Dispatcher.Invoke(new Action(RefreshAllStatus)));
            }
            catch (Exception ex)
            {
                Log("启动客户端失败: " + ex.Message);
                MessageBox.Show("启动失败: " + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RunScript(string installDir, bool isTw, string brandTitle, bool isRestore)
        {
            try
            {
                string scriptPath = Path.Combine(workingDir, "localization_engine.js");
                if (!File.Exists(scriptPath))
                {
                    Log("[错误] 未在当前目录找到 localization_engine.js，请确保汉化工具与汉化包在同一目录下！");
                    Window.Dispatcher.Invoke(new Action(() => {
                        SetUiBusy(false, "执行失败：缺少核心脚本文件。");
                        RefreshAllStatus();
                    }));
                    return;
                }

                // 判断运行引擎
                bool useSystemNode = IsCommandAvailable("node");
                string runnerExe = "node";
                bool isUsingAntigravityAsNode = false;

                if (!useSystemNode)
                {
                    string antigravityExe = Path.Combine(installDir, "Antigravity.exe");
                    if (File.Exists(antigravityExe))
                    {
                        runnerExe = antigravityExe;
                        isUsingAntigravityAsNode = true;
                        Log("[引擎] 系统未安装 Node.js，自动切换至 Antigravity 内置运行引擎！");
                    }
                    else
                    {
                        Log("[错误] 系统未安装 Node.js，且在指定路径未找到 Antigravity.exe！");
                        Window.Dispatcher.Invoke(new Action(() => {
                            SetUiBusy(false, "执行失败：无法找到可用的 Node.js 运行环境。");
                            MessageBox.Show("无法找到 Node.js 或 Antigravity 内置引擎，请先安装 Node.js 或检查客户端安装路径。", "环境缺失", MessageBoxButton.OK, MessageBoxImage.Error);
                        }));
                        return;
                    }
                }
                else
                {
                    Log("[引擎] 检测到系统已安装 Node.js，优先使用系统环境。");
                }

                Process[] runningProcs = Process.GetProcessesByName("Antigravity");
                if (runningProcs.Length > 0)
                {
                    Log("[进程] 检测到 Antigravity 正在运行，正在安全关闭以解除文件占用...");
                    try
                    {
                        ProcessStartInfo psiKill = new ProcessStartInfo("taskkill", "/f /im Antigravity.exe /t");
                        psiKill.CreateNoWindow = true;
                        psiKill.UseShellExecute = false;
                        using (Process pk = Process.Start(psiKill))
                        {
                            pk.WaitForExit(3000);
                        }
                        System.Threading.Thread.Sleep(1000);
                        Log("[进程] 已成功关闭运行中的 Antigravity 客户端。");
                    }
                    catch (Exception ex)
                    {
                        Log("[警告] 尝试关闭客户端时异常: " + ex.Message);
                    }
                }

                // 拼装参数
                StringBuilder args = new StringBuilder();
                args.Append(string.Format("\"{0}\" ", scriptPath));
                if (isRestore)
                {
                    args.Append("--huifu ");
                }
                else
                {
                    if (isTw) args.Append("--tw ");
                    args.Append(string.Format("--brand-title {0} ", brandTitle));
                }
                args.Append(string.Format("--install-dir \"{0}\" ", installDir));
                args.Append("--no-kill ");

                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = runnerExe;
                psi.Arguments = args.ToString();
                psi.WorkingDirectory = workingDir;
                psi.UseShellExecute = false;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;
                psi.CreateNoWindow = true;

                if (isUsingAntigravityAsNode)
                {
                    psi.EnvironmentVariables["ELECTRON_RUN_AS_NODE"] = "1";
                }

                Log("[执行] 启动命令: " + runnerExe + " " + args.ToString());

                using (Process p = new Process())
                {
                    p.StartInfo = psi;
                    p.OutputDataReceived += (s, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            Log(e.Data);
                        }
                    };
                    p.ErrorDataReceived += (s, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            Log("[ERR] " + e.Data);
                        }
                    };

                    p.Start();
                    p.BeginOutputReadLine();
                    p.BeginErrorReadLine();
                    p.WaitForExit();

                    int exitCode = p.ExitCode;
                    Window.Dispatcher.Invoke(new Action(() =>
                    {
                        RefreshAllStatus();
                        if (exitCode == 0)
                        {
                            string opDesc = isRestore ? "还原官方英文" : "汉化注入";
                            Log("[成功] " + opDesc + "完成！");
                            SetUiBusy(false, opDesc + "成功完成！");
                            MessageBox.Show(opDesc + "成功！现在您可以点击【启动 Antigravity】开始体验！", "操作成功", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            Log(string.Format("[失败] 进程退出代码: {0}", exitCode));
                            SetUiBusy(false, "操作未完成，请检查日志。若提示权限问题，请以管理员身份运行本工具。");
                            MessageBox.Show("操作失败，请查看日志获取详细报错信息。\r\n若提示权限不足(EPERM)，建议右键本工具选择【以管理员身份运行】。", "操作失败", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }));
                }
            }
            catch (Exception ex)
            {
                Log("[致命错误] " + ex.Message + "\r\n" + ex.StackTrace);
                Window.Dispatcher.Invoke(new Action(() =>
                {
                    SetUiBusy(false, "发生未知异常: " + ex.Message);
                    RefreshAllStatus();
                }));
            }
        }
    }
}
