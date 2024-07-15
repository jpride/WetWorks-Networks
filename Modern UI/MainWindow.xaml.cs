using System;
using System.Windows;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Windows.Controls;
using System.Threading;
using System.Windows.Input;
using System.Reflection;


namespace WetWorks_NetWorks
{ 
    public partial class MainWindow : Window
    {
       
        public int choiceSelect = 0;

        public bool ethernetSelected = false;
        public bool wifiSelected = false;

        public int selectChoiceInt = 0;

        private static System.Windows.Forms.Timer _timer;

        private NetworkInterface _nic;
        private string _adapter;
        private long speed;
        private int _adapterCount = 0;
        private NetworkInterfaceType _adapterType;


        readonly string _dhcpChoiceContent = "DHCP";
        readonly string _choice2Content = "10.10.1.253 / 16";
        readonly string _choice3Content = "192.168.1.253 / 24";
        readonly string _choice4Content = "169.254.1.253 / 16";


        readonly string _choice2Address = "10.10.1.253";
        readonly string _choice3Address = "192.168.1.253";
        readonly string _choice4Address = "169.254.1.253";

        readonly string _dhcpNetShChoiceString = "dhcp";
        readonly string _choice2NetShString = "static address=10.10.1.253 mask=255.255.0.0 gateway=10.10.1.1";
        readonly string _choice3NetShString = "static address=192.168.1.253 mask=255.255.255.0 gateway=192.168.1.1";
        readonly string _choice4NetShString = "static address=169.254.1.253 mask=255.255.0.0 gateway=169.254.1.1";




        public MainWindow()
        {
            InitializeComponent();

            //Event handler watching for Network Address Change, triggers the UpdateAdapterInfo() method
            NetworkChange.NetworkAddressChanged += new NetworkAddressChangedEventHandler(AddressChangedCallback);
            NetworkChange.NetworkAvailabilityChanged += new NetworkAvailabilityChangedEventHandler(NetworkAvailabilityChangedCallback);
            

            //Initialize button content
            choice1Btn.Content = _dhcpChoiceContent;
            choice2Btn.Content = _choice2Content;
            choice3Btn.Content = _choice3Content;
            choice4Btn.Content = _choice4Content;

            //get assembly version for version label
            Version version = Assembly.GetEntryAssembly().GetName().Version;
            assemblyLbl.Content = String.Format($"v{version}");

            //intialize adapter info
            GetAdapterInfoAtStartup();
        }


        #region ButtonEvents
        private void choiceBtn_Click(object sender, RoutedEventArgs e)
        {
            RadioButton myButton = (RadioButton)sender;
            string myName = myButton.Name;

            Console.WriteLine(myName);
            switch (myButton.Name)
            {
                case "choice1Btn":
                    choiceSelect = 1;
                    ChoiceInterlock(choiceSelect);
                    ResetUserEntryText();

                    if (choiceSelect == 1)
                    {
                        Process p = CreateProcess(_adapter, _dhcpNetShChoiceString);
                        ProcessRequest(p);

                        UpdateStatusLbl("waiting for DHCP...");
                        SetIpaText(String.Empty);

                        _timer = new System.Windows.Forms.Timer();
                        _timer.Tick += new EventHandler(CheckAdapterAfterTimeOut);
                        _timer.Interval = 5000;
                        _timer.Start();
                        UpdateStatusLbl("Waiting for active adapter");
                    }
                    break;
                case "choice2Btn":
                    choiceSelect = 2;
                    ChoiceInterlock(choiceSelect);
                    ResetUserEntryText();

                    if (choiceSelect == 2)
                    {                      
                        Process p = CreateProcess(_adapter, _choice2NetShString);
                        ProcessRequest(p);
                        UpdateAdapterInfo();
                    }
                    break;
                case "choice3Btn":
                    choiceSelect = 3;
                    ChoiceInterlock(choiceSelect);
                    ResetUserEntryText();

                    if (choiceSelect == 3)
                    {
                        Process p = CreateProcess(_adapter, _choice3NetShString);
                        ProcessRequest(p);
                        UpdateAdapterInfo();
                    }
                    break;
                case "choice4Btn":
                    choiceSelect = 4;
                    ChoiceInterlock(choiceSelect);
                    ResetUserEntryText();

                    if (choiceSelect == 4)
                    {
                        Process p = CreateProcess(_adapter, _choice4NetShString);
                        ProcessRequest(p);
                        UpdateAdapterInfo();
                    }
                    break;
                case "choice5Btn":
                    choiceSelect = 5;
                    ChoiceInterlock(choiceSelect);
                    UpdateStatusLbl(String.Empty);
                    break;

                default:
                    break;
            }
        }

        private void ChoiceInterlock(int index)
        {
            choiceSelect = index;

            this.Dispatcher.Invoke(() => 
            {
                choice1AccentBtn.Visibility = Visibility.Hidden;
                choice2AccentBtn.Visibility = Visibility.Hidden;
                choice3AccentBtn.Visibility = Visibility.Hidden;
                choice4AccentBtn.Visibility = Visibility.Hidden;
                choice5AccentBtn.Visibility = Visibility.Hidden;
            });


            switch (index)
            {
                case 1:
                    this.Dispatcher.Invoke(() => 
                    {
                        choice1AccentBtn.Visibility = Visibility.Visible;
                        choice1Btn.IsChecked = true;
                        ResetUserEntryText();
                    });

                    break;
                case 2:
                    this.Dispatcher.Invoke(() =>
                    {
                        choice2AccentBtn.Visibility = Visibility.Visible;
                        choice2Btn.IsChecked = true;
                        ResetUserEntryText();
                    });
                    break;
                case 3:
                    this.Dispatcher.Invoke(() =>
                    {
                        choice3AccentBtn.Visibility = Visibility.Visible;
                        choice3Btn.IsChecked = true;
                        ResetUserEntryText();
                    });
                    break;
                case 4:
                    this.Dispatcher.Invoke(() =>
                    {
                        choice4AccentBtn.Visibility = Visibility.Visible;
                        choice4Btn.IsChecked = true;
                        ResetUserEntryText();
                    });
                    break;
                case 5:
                    this.Dispatcher.Invoke(() =>
                    {
                        choice5AccentBtn.Visibility = Visibility.Visible;
                        choice5Btn.IsChecked = true;
                    });
                    break;
                default:
                    break;
            }

        }

        private void userEntry_GotFocus(object sender, RoutedEventArgs e)
        {
            Console.WriteLine($"UserEntry Got Focus");
            choiceSelect = 5;
            ChoiceInterlock(choiceSelect);
        }

        private void LogoBtn_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Logo");

            string exePath = "C:\\windows\\system32\\control.exe";
            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = "/name Microsoft.NetworkAndSharingCenter",
                Verb = "runas",
                UseShellExecute = true,
                CreateNoWindow = true,
            };

            Process process = new Process();
            process.StartInfo = processStartInfo;
            process.Start();
        }

        private void EthernetBtn_Click(object sender, RoutedEventArgs e)
        {
            ethernetSelected = true;
            ResetUserEntryText();
            _adapterType = NetworkInterfaceType.Ethernet;
            UpdateAdapterInfo();
        }

        private void WifiBtn_Click(object sender, RoutedEventArgs e)
        {
            wifiSelected = true;
            ResetUserEntryText();
            _adapterType = NetworkInterfaceType.Wireless80211;
            UpdateAdapterInfo();
        }

        #endregion


        //*************************************************************//
        #region Network Functions
        private void AddressChangedCallback(object sender, EventArgs e)
        {
            Console.WriteLine($"AddressChangedCallback Entered");
            
            //Refresh apapters and find the one that matches _adapter from UpdateAdapterInfo()
            NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface n in adapters)
            {
                if (n.Name == _adapter)
                {
                    _nic = n;
                    break;
                }
            }

            UpdateStatusLbl(String.Format($"Address Changed Detected on adapter: {_nic.Name}"));

            //Run UpdateAdapterInfo() if the adapter is Up, if not, place message in label that reports it down
            if (_nic.OperationalStatus == OperationalStatus.Up)
            {
                try
                {
                    UpdateAdapterInfo();
                }
                catch (Exception ex)
                {
                    Console.Write("Error in Callback: {0}", ex.Message);
                }
            }
            else
            {
                UpdateStatusLbl(String.Format($"{_adapter} is down..."));
                SetIpaText(String.Empty);
                SetSpeed();
            }
        }

        private void NetworkAvailabilityChangedCallback(object sender, NetworkAvailabilityEventArgs e)
        {
            Console.WriteLine($"NetworkAvailabilityChangedCallback Entered");
            Console.WriteLine($"{sender} availibility: {e.IsAvailable}");

            //Refresh apapters and find the one that matches _adapter from UpdateAdapterInfo()
            NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();

            
            //loop thru adapters and find the one 
            foreach (NetworkInterface n in adapters)
            {
                if (n.Name == _adapter)
                {
                    _nic = n;
                    break;
                }
            }

            UpdateStatusLbl(String.Format($"Address Changed Detected on adapter: {_nic.Name}"));

            //Run UpdateAdapterInfo() if the adapter is Up, if not, place message in label that reports it down
            if (_nic.OperationalStatus == OperationalStatus.Up)
            {
                try
                {
                    UpdateAdapterInfo();
                }
                catch (Exception ex)
                {
                    Console.Write("Error in Callback: {0}", ex.Message);
                }
            }
            else
            {
                UpdateStatusLbl(String.Format($"{_adapter} is down..."));
                SetIpaText(String.Empty);
                SetSpeed();
            }
        }

        private void CheckAdapterAfterTimeOut(object sender, EventArgs e)
        {
            //upon expiration of timer, Rerun UpdateAdapterInfo()
            _timer.Stop();
            UpdateAdapterInfo();
        }

        public void ProcessRequest(Process p)
        {
            try
            {
                _ = p.Start();
                _ = p.WaitForExit(10000);
            }
            catch (Exception ex)
            {
                UpdateStatusLbl(String.Format($"Error Processing Request:{ex.Message}\n"));
            }
        }

        public void GetAdapterInfoAtStartup()
        {
            try
            {
                UpdateStatusLbl("Waiting for active adapter");
                SetIpaText(String.Empty);

                //grab list of all NetworkInterfaces
                NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

                //loop through list and find interface that is both "Up" and contains the word 'Ethernet' in it
                foreach (NetworkInterface nic in interfaces)
                {

                    IPInterfaceProperties adapterProps = nic.GetIPProperties();
                    IPv4InterfaceProperties ipv4Props = adapterProps.GetIPv4Properties();

                    if (nic.OperationalStatus == OperationalStatus.Up & nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    {
                        _adapterType = nic.NetworkInterfaceType; 

                        if (_adapterType == NetworkInterfaceType.Wireless80211) //selected adapter 
                        {
                            if (nic.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                            {
                                if (nic.Name.Contains("Wi-Fi"))
                                {
                                    if (!nic.Name.Contains("vEthernet") & !nic.Name.Contains("Loopback") & !nic.Name.Contains("Bluetooth"))
                                    {
                                        //once a valid adapter is found, places the name in the adapterName box and sets the adapter variable used in the processes to the name
                                        _nic = nic;
                                        _adapter = nic.Name;
                                        

                                        speed = SpeedCalc(nic);
                                        SetSpeed();

                                        this.Dispatcher.Invoke(() =>
                                        {
                                            adapterTxt.Content = String.Format($"{_adapter}");
                                            domainTxt.Content = String.Format($"{_nic.GetIPProperties().DnsSuffix}");
                                            UpdateStatusLbl(String.Empty);
                                        });

                                        wifiSelected = true;
                                        ethernetSelected = false;

                                        UdpateInterfaceButton();

                                        if (ipv4Props.IsDhcpEnabled)
                                        {
                                            choiceSelect = 1;
                                            ChoiceInterlock(choiceSelect);
                                        }

                                        _adapterCount++;

                                        foreach (UnicastIPAddressInformation ip in nic.GetIPProperties().UnicastAddresses)
                                        {
                                            UpdateAdapterUIInfo(ip);
                                        }

                                        if (_adapterCount != 0)
                                        {
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                        else if (_adapterType == NetworkInterfaceType.Ethernet) //selected adapter 
                        {
                            if (nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                            {
                                if (nic.Name.Contains("Ethernet"))
                                {
                                    if (!nic.Name.Contains("vEthernet") & !nic.Name.Contains("Loopback") & !nic.Name.Contains("Bluetooth"))
                                    {
                                        //once a valid adapter is found, places the name in the adapterName box and sets the adapter variable used in the processes to the name
                                        _nic = nic;
                                        _adapter = nic.Name;
                                        

                                        speed = SpeedCalc(nic);
                                        SetSpeed();

                                        this.Dispatcher.Invoke(() =>
                                        {
                                            adapterTxt.Content = String.Format($"{_adapter}");
                                            domainTxt.Content = String.Format($"{_nic.GetIPProperties().DnsSuffix}");
                                            UpdateStatusLbl(String.Empty);
                                        });

                                        ethernetSelected = true;
                                        wifiSelected = false;

                                        UdpateInterfaceButton();

                                        if (ipv4Props.IsDhcpEnabled)
                                        {
                                            choiceSelect = 1;
                                            ChoiceInterlock(choiceSelect);
                                        }

                                        _adapterCount++;

                                        foreach (UnicastIPAddressInformation ip in nic.GetIPProperties().UnicastAddresses)
                                        {
                                            UpdateAdapterUIInfo(ip);
                                        }

                                        if (_adapterCount != 0)
                                        {
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                UpdateStatusLbl("Error in UpdateAdapterInfo");
            }

            if (_adapterCount == 0)
            {
                //if no active wired adapters found, start a 5 second timer
                //after 5 seconds, run UpdateAdapterInfo() again
                _timer = new System.Windows.Forms.Timer();
                _timer.Tick += new EventHandler(CheckAdapterAfterTimeOut);
                _timer.Interval = 5000;
                _timer.Start();
                UpdateStatusLbl("Waiting for active adapter");
            }
        }

        public void UpdateAdapterInfo()
        {
            try
            {
                UpdateStatusLbl("Waiting for active adapter");
                SetIpaText(String.Empty);

                //grab list of all NetworkInterfaces
                NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

                //loop through list and find interface that is both "Up" and contains the word 'Ethernet' in it
                foreach (NetworkInterface nic in interfaces)
                {

                    IPInterfaceProperties adapterProps = nic.GetIPProperties();
                    IPv4InterfaceProperties ipv4Props = adapterProps.GetIPv4Properties();

                    if (nic.OperationalStatus == OperationalStatus.Up & nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    {
                        
                        if (_adapterType == NetworkInterfaceType.Wireless80211) //selected adapter 
                        {
                            if (nic.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                            {
                                if (nic.Name.Contains("Wi-Fi"))
                                {
                                    if (!nic.Name.Contains("vEthernet") & !nic.Name.Contains("Loopback") & !nic.Name.Contains("Bluetooth"))
                                    {
                                        //once a valid adapter is found, places the name in the adapterName box and sets the adapter variable used in the processes to the name
                                        _nic = nic;
                                        _adapter = nic.Name;
                                        

                                        speed = SpeedCalc(nic);
                                        SetSpeed();

                                        this.Dispatcher.Invoke(() =>
                                        {
                                            adapterTxt.Content = String.Format($"{_adapter}");
                                            domainTxt.Content = String.Format($"{_nic.GetIPProperties().DnsSuffix}");
                                            UpdateStatusLbl(String.Empty);
                                        });

                                        wifiSelected = true;
                                        ethernetSelected = false;

                                        UdpateInterfaceButton();

                                        if (ipv4Props.IsDhcpEnabled)
                                        {
                                            choiceSelect = 1;
                                            ChoiceInterlock(choiceSelect);
                                        }

                                        _adapterCount++;

                                        foreach (UnicastIPAddressInformation ip in nic.GetIPProperties().UnicastAddresses)
                                        {
                                            UpdateAdapterUIInfo(ip);
                                        }

                                        if (_adapterCount != 0)
                                        {
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                        else if (_adapterType == NetworkInterfaceType.Ethernet) //selected adapter 
                        {
                            if (nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                            {
                                if (nic.Name.Contains("Ethernet"))
                                {
                                    if (!nic.Name.Contains("vEthernet") & !nic.Name.Contains("Loopback") & !nic.Name.Contains("Bluetooth"))
                                    {
                                        //once a valid adapter is found, places the name in the adapterName box and sets the adapter variable used in the processes to the name
                                        _nic = nic;
                                        _adapter = nic.Name;
                                        

                                        speed = SpeedCalc(nic);
                                        SetSpeed();

                                        this.Dispatcher.Invoke(() =>
                                        {
                                            adapterTxt.Content = String.Format($"{_adapter}");
                                            domainTxt.Content = String.Format($"{_nic.GetIPProperties().DnsSuffix}");
                                            UpdateStatusLbl(String.Empty);
                                        });

                                        ethernetSelected = true;
                                        wifiSelected = false;

                                        UdpateInterfaceButton();

                                        if (ipv4Props.IsDhcpEnabled)
                                        {
                                            choiceSelect = 1;
                                            ChoiceInterlock(choiceSelect);
                                        }

                                        _adapterCount++;

                                        foreach (UnicastIPAddressInformation ip in nic.GetIPProperties().UnicastAddresses)
                                        {
                                            UpdateAdapterUIInfo(ip);
                                        }

                                        if (_adapterCount != 0)
                                        {
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                UpdateStatusLbl("Error in UpdateAdapterInfo");
            }

            if (_adapterCount == 0)
            {
                //if no active wired adapters found, start a 5 second timer
                //after 5 seconds, run UpdateAdapterInfo() again
                _timer = new System.Windows.Forms.Timer();
                _timer.Tick += new EventHandler(CheckAdapterAfterTimeOut);
                _timer.Interval = 5000;
                _timer.Start();
                UpdateStatusLbl("Waiting for active adapter");
            }
        }

        private void UpdateAdapterUIInfo(UnicastIPAddressInformation ip)
        {

            if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                SetSpeed();
 
                this.Dispatcher.Invoke(() => 
                {
                    adapterTxt.Content = String.Format($"{_adapter}");
                    hostnameTxt.Content = Dns.GetHostName();
                    SetIpaText(String.Format($"{ip.Address.ToString()} / {ip.PrefixLength.ToString()}"));
                });
      

                if (ip.Address.ToString() == _choice2Address)
                {
                    choiceSelect = 2;
                    ChoiceInterlock(choiceSelect);
                }
                else if (ip.Address.ToString() == _choice3Address)
                {
                    choiceSelect = 3;
                    ChoiceInterlock(choiceSelect);
                }
                else if (ip.Address.ToString() == _choice4Address)
                {
                    choiceSelect = 4;
                    ChoiceInterlock(choiceSelect);
                }
            }
        }

        public Process CreateProcess(string adapter, string ipMaskString)
        {
            try
            {
                var p = new Process();
                ProcessStartInfo info = new ProcessStartInfo
                {
                    FileName = "netsh.exe",
                    Arguments = $"interface ipv4 set address name=\"{adapter}\" {ipMaskString}\"",
                    CreateNoWindow = true,
                    UseShellExecute = true,
                    Verb = "runas",
                    RedirectStandardOutput = false,
                };

                p.StartInfo = info;

                return p;
            }
            catch (Exception)
            {
                UpdateStatusLbl("Error Creating Process");
            }

            return null;

        }
        #endregion



        //*******************   UI events   ***************************//
        #region UI Processing
        private void UpdateStatusLbl(string msg)
        {
            if (!msg.Equals(String.Empty))
            {
                this.Dispatcher.Invoke(() =>
                {
                    statusLbl.Visibility = Visibility.Visible;
                    statusTxt.Content = String.Format($"{msg}");
                });
                
            }
            else
            {
                this.Dispatcher.Invoke(() => 
                {
                    statusLbl.Visibility = Visibility.Hidden;
                    statusTxt.Content = String.Empty;
                });

            }
        }

        private void UdpateInterfaceButton()
        {
            this.Dispatcher.Invoke(() => 
            {
                wifiBtn.IsChecked = wifiSelected;
                ethernetBtn.IsChecked = ethernetSelected;
            });
        }

        private void ResetUserEntryText()
        {
            this.Dispatcher.Invoke(() => 
            {
                userEntryTxt.Text = String.Empty;
            });

        }

        private void OnKeyDownInUserEntryBoxHandler(object sender, KeyEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Key == Key.Escape)
                {
                    e.Handled = true;
                    System.Windows.Forms.Application.Exit();
                }

                if (e.Key == Key.Enter || e.Key == Key.Return )
                {
                    Console.WriteLine($"UserEntrTxt: {userEntryTxt.Text}");

                    if (choiceSelect == 5)
                    {

                        //split character
                        string sep = @" ";
                        bool validIP;
                        bool validMask;


                        try
                        {
                            if (userEntryTxt.Text.Contains(" "))
                            {
                                //split user input string into ipa and ipm
                                string[] customIP = userEntryTxt.Text.Split(sep.ToCharArray());


                                validIP = IPAddress.TryParse(customIP[0], out IPAddress ip);
                                validMask = IPAddress.TryParse(customIP[1], out IPAddress mask);


                                if (!validIP || !validMask)
                                {
                                    UpdateStatusLbl("Not a Valid IP Address! Try Again");
                                }
                                else
                                {
                                    Process p = new Process();
                                    ProcessStartInfo info = new ProcessStartInfo
                                    {
                                        FileName = "netsh.exe",
                                        Arguments = String.Format("interface ipv4 set address name=\"{0}\" static {1} {2}", _adapter, ip, mask),
                                        CreateNoWindow = true,
                                        UseShellExecute = true,
                                        Verb = "runas",
                                        RedirectStandardOutput = false,
                                    };
                                    
                                    ProcessRequest(p);
                                    UpdateAdapterInfo();
                                }
                            }
                            else if (userEntryTxt.Text.Contains("/"))
                            {
                                sep = @"/";
                                string[] customIP = userEntryTxt.Text.Split(sep.ToCharArray());

                                validIP = IPAddress.TryParse(customIP[0], out IPAddress ip);
                                bool validMaskBits = int.TryParse(customIP[1], out int maskBits);

                                if (validMaskBits)
                                {
                                    string mask = null;

                                    switch (maskBits)
                                    {
                                        case 8:
                                            mask = "255.0.0.0";
                                            break;
                                        case 16:
                                            mask = "255.255.0.0";
                                            break;
                                        case 22:
                                            mask = "255.255.252.0";
                                            break;
                                        case 23:
                                            mask = "255.255.254.0";
                                            break;
                                        case 24:
                                            mask = "255.255.255.0";
                                            break;

                                        default:
                                            mask = null;
                                            UpdateStatusLbl("Invalid Maskbits! This app only supports '/8', '/16', '/22', '/23', or '/24'");
                                            break;
                                    }

                                    if (!string.IsNullOrEmpty(mask))
                                    {
                                        Process p = new Process();
                                        ProcessStartInfo info = new ProcessStartInfo
                                        {
                                            FileName = "netsh.exe",
                                            Arguments = String.Format("interface ipv4 set address name=\"{0}\" static {1} {2}", _adapter, ip, mask),
                                            CreateNoWindow = true,
                                            UseShellExecute = true,
                                            Verb = "runas",
                                            RedirectStandardOutput = false,
                                        };

                                        ProcessRequest(p);
                                        UpdateAdapterInfo();
                                    }
                                }
                                else
                                {
                                    UpdateStatusLbl("Invalid Maskbits! This app only supports '/8', '/16', '/22', '/23', or '/24'");
                                }
                            }
                            else
                            {
                                UpdateStatusLbl("Invalid Entry! Try Again. (OnKeyDown)");
                            }
                        }

                        catch (IndexOutOfRangeException)
                        {
                            UpdateStatusLbl("You must enter an IPAddress followed by a single space, then a Subnet Mask");
                        }
                        catch (Exception)
                        {
                            UpdateStatusLbl("Invalid! Try Again");
                        }
                    }
                }
            }));
        }

        private void OnUserEntryTextChanged(object sender, TextChangedEventArgs e)
        {          
            Console.WriteLine($"UserEntryText: {userEntryTxt.Text}");
        }

        private void OnKeyDownInMainWindowHandler(object sender, KeyEventArgs e)
        {
            Console.WriteLine($"KeyDownMainWindow: {e.Key}");

            if (e.Key == Key.Escape)
            {
                System.Windows.Application.Current.Shutdown();   
            }
        }

        private void ipaTxt_TextChanged(object sender, EventArgs e)
        {
            if ((string)ipaTxt.Content == String.Empty)
            {
                this.Dispatcher.Invoke(() => 
                {
                    ipaTxt.Content = "awaiting adapter...";
                });
                
            }
        }

        private void SetIpaText(string text)
        {
            this.Dispatcher.Invoke(() => 
            {
                ipaTxt.Content = text;
            });
            
        }

        private long SpeedCalc(NetworkInterface nic)
        {
            return nic.Speed;
        }

        private void SetSpeed()
        { 
            string speedSuffix = string.Empty;
            decimal div = 0;

            if (Math.Round(speed  / 1000000d, 0) >= 999) 
            {
                speedSuffix = "Gbps";
                div = 1000;

            }
            else
            {
                speedSuffix = "Mbps";
                div = 1;
            }

            this.Dispatcher.Invoke(() =>
            {
                Console.WriteLine($"{Math.Round(speed  / (1000000 * div), 0)} {speedSuffix}");
                speedTxt.Content = String.Format($"{Math.Round(speed  / (1000000 * div), 0)} {speedSuffix}");
            });


        }
        #endregion

    }
}
