﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using WocketsApplication.Controls;
using WocketsApplication.Controls.Alpha;
using System.Threading;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;
using  Microsoft.Win32;
using Wockets.Kernel.Types;
using Wockets.IPC;
using Wockets.Utils;
using Wockets.Kernel;
using Wockets.Data.Annotation;
using Wockets.Data.Plotters;
//using OpenNETCF.GDIPlus;
using OpenNETCF.Windows.Forms;
using WocketsApplication.Controls.Utils;

namespace WocketsApplication
{
    public partial class Form1 : Form
    {

        //private const int NUMBER_BUTTONS=9;

        //--- Primary Screen Dimentions ---
        // P3300: 240x320
        // Diamond Touch: 480x640
        // Diamond Touch 2: 480x800
        private int SCREEN_RESOLUTION_X = Screen.PrimaryScreen.Bounds.Width; 
        private int SCREEN_RESOLUTION_Y = Screen.PrimaryScreen.Bounds.Height; 

        private ClickableAlphaPanel[] panels= new ClickableAlphaPanel[ControlID.NUMBER_PANELS];
        private int[] slidingPanels = new int[2] { 0, 1 };
        private int[] numberButtons = new int[ControlID.NUMBER_PANELS];
        private int currentPanel = 0;
        private int currentPanelIndex = 0;
        private Rectangle clientArea;
        public bool pushed = false;
        private AlphaContainer _alphaManager;
        private Thread slidingThread;
       // private ClickableAlphaPanel[] buttonPanels = new ClickableAlphaPanel[9];
        //private Bitmap[] _buttonBackBuffers = new Bitmap[9];

 

        private Thread kListenerThread;


        delegate void UpdatewWocketsListCallback();
        private bool disposed = false;

        public void UpdatewWocketsList()
        {
            if (!disposed)
            {
                // InvokeRequired required compares the thread ID of the
                // calling thread to the thread ID of the creating thread.
                // If these threads are different, it returns true.
                if (wocketsList.InvokeRequired)
                {
                    UpdatewWocketsListCallback d = new UpdatewWocketsListCallback(UpdatewWocketsList);
                    this.Invoke(d, new object[] { });
                }
                else
                {

                    wocketsList.Controls.Clear();                   
                    RegistryKey rk = Registry.LocalMachine.OpenSubKey(Core.REGISTRY_DISCOVERED_SENSORS_PATH); 

                    //BUG crashing
                    string[] sensors = rk.GetSubKeyNames();
                    rk.Close();
                    if (sensors.Length > 0)
                    {
                        wocketsList._Status = "";
                        for (int i = 0; (i < sensors.Length); i++)
                        {

                            rk = Registry.LocalMachine.OpenSubKey(Core.REGISTRY_DISCOVERED_SENSORS_PATH + "\\" + sensors[i]); ;
                            WocketListItem wi = new WocketListItem((string)rk.GetValue("Name"), (string)rk.GetValue("MacAddress"),i+1);
                            rk.Close();
                            wi.Index = i;
                            wi.Name = wi.Index.ToString();
                            wi.Location = new Point(0, wi.Height * i);
                            wi.Click += new EventHandler(wocketClickHandler);
                            wocketsList.Controls.Add(wi);
                        }
                        wocketsList._Status = "";
                    }
                    else
                    {
                        wocketsList._Status = "No Wockets Found...";                        
                    }
                    wocketsList.Refresh();
                }
            }

        }

        //private bool wocketsConnected = false;

        private void KernelListener()
        {
            NamedEvents namedEvent = new NamedEvents();
            while (true)
            {
                //ensures prior synchronization
                namedEvent.Receive(Core._KernelGuid);


                RegistryKey rk = Registry.LocalMachine.CreateSubKey(Core.REGISTRY_REGISTERED_APPLICATIONS_PATH +
                                                                         "\\{" + Core._KernelGuid + "}");
                string response = (string)rk.GetValue("Message");
                rk.Close();

              if (response == ApplicationResponse.DISCOVERY_COMPLETED.ToString())
                {
                    UpdatewWocketsList();
                }
                else if (response == ApplicationResponse.CONNECT_SUCCESS.ToString())
                {
                   Core._Connected= true;
                   //plotterTimer.Enabled = true;
                   UpdatePlotter();                                    
                }
                else if (response == ApplicationResponse.DISCONNECT_SUCCESS.ToString())
                {
                    Core._Connected = false;                   
                }
            
                namedEvent.Reset();
            }
        }

  
        delegate void UpdatePlotterCallback();
        public void UpdatePlotter()
        {

            if (!disposed)
            {
                // InvokeRequired required compares the thread ID of the
                // calling thread to the thread ID of the creating thread.
                // If these threads are different, it returns true.
                if (this.plotterPanel.InvokeRequired)
                {
                    UpdatePlotterCallback d = new UpdatePlotterCallback(UpdatePlotter);
                    this.Invoke(d, new object[] { });

                }
                else
                {

                    if (plotter != null)
                    {
                        plotterTimer.Enabled = false;
                        plotter.Dispose();
                    }
                    plotter = new WocketsScalablePlotter(plotterPanel, selectedWockets.Count);
                    plotterPanel.Visible = true;
                    plotterTimer.Enabled = true;
                        
                }
            }

        }

        public Form1()
        {                    
            
            RegistryKey rk = Registry.LocalMachine.OpenSubKey("Software\\MIT\\Wockets", true);
            if (rk == null)
            {
                if (MessageBox.Show("Thanks for installing the wockets\nThe setup will continue. Are you ready?", "",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.No)
                    Application.Exit();                
            }

  


            ScreenUtils.ShowTaskBar(false);
            ScreenUtils.ShowTrayBar(false);
            InitializeComponent();
            InitializeInterface();

            //all commands should be sent after initializing interface

            wocketsList._Status = "Refresh wockets...";
            //this.slidingThread = new Thread(new ThreadStart(timeAnimation_Tick));
            _alphaManager = new AlphaContainer(this);
             this.Refresh();    
        }






        //private Color[] colors = new Color[ControlID.NUMBER_PANELS] { Color.White, Color.Black, Color.Red, Color.Green, Color.FromArgb(245, 219, 186), Color.FromArgb(245, 219, 186) };
        private Bitmap[] _backBuffers = new Bitmap[ControlID.NUMBER_PANELS];

        //private BluetoothPanel pp;
        private Transitions currentTransition;

        public void AddButton(int panelID, int buttonID, string pressedFilename,string unpressedFilename, int x, int y,int size, string unpressedText,ButtonType type)
        {            
            this.panels[panelID]._UnpressedButtonControls[buttonID] = new AlphaPictureBox();
            this.panels[panelID]._UnpressedButtonControls[buttonID].Name = buttonID.ToString();
            this.panels[panelID]._UnpressedButtonControls[buttonID].Size = new Size(size, size);
            this.panels[panelID]._UnpressedButtonControls[buttonID].Image = AlphaImage.CreateFromFile(Constants.PATH + unpressedFilename);
            this.panels[panelID]._UnpressedButtonControls[buttonID].Visible = true;

            this.panels[panelID]._UnpressedButtonControls[buttonID].Location = new Point(x, y);
            this.panels[panelID]._UnpressedButtonControls[buttonID].Click += new EventHandler(clickHandler);
            if (unpressedText != null)
            {
                this.panels[panelID]._ButtonText[buttonID] = new AlphaLabel();
                
                this.panels[panelID]._ButtonText[buttonID].Text = unpressedText;
                this.panels[panelID]._ButtonText[buttonID].ForeColor = Color.FromArgb(205, 183, 158);
                this.panels[panelID]._ButtonText[buttonID].Allign = StringAlignment.Center;
                this.panels[panelID]._ButtonText[buttonID].Visible = true;
                this.panels[panelID]._ButtonText[buttonID].Font = new Font(FontFamily.GenericSerif,9.0f,FontStyle.Regular);
                this.panels[panelID]._ButtonText[buttonID].Size = new Size(128, 30);
                this.panels[panelID]._ButtonText[buttonID].Location = new Point(x , y + size + 2);
            }
            

            this.panels[panelID]._PressedButtonControls[buttonID] = new AlphaPictureBox();
            this.panels[panelID]._PressedButtonControls[buttonID].Name = buttonID.ToString();
            this.panels[panelID]._PressedButtonControls[buttonID].Size = new Size(128, 30);
            this.panels[panelID]._PressedButtonControls[buttonID].Image = AlphaImage.CreateFromFile(Constants.PATH + pressedFilename);
            this.panels[panelID]._PressedButtonControls[buttonID].Visible = false;
            this.panels[panelID]._PressedButtonControls[buttonID].Location = new Point(x, y);      
            this.panels[panelID]._PressedButtonControls[buttonID].Click += new EventHandler(clickHandler);
  
            this.panels[panelID]._ButtonType[buttonID] = type;

            if (type == ButtonType.Alternating)
            {
                this.panels[panelID]._PressedButtonControls[buttonID].Enabled=false;
            }
        } 


        WocketSlidingList wocketsList = null;
        private Panel bluetoothPanel;
        private Label bluetoothName;
        /*private Label bluetoothMac;
        private Label bluetoothPIN;
        private ComboBox bluetoothTP;
        private ComboBox bluetoothSM;
        private WocketListItem currentWi;*/
        ArrayList selectedWockets = new ArrayList();
        WocketsScalablePlotter plotter=null;
        private Panel plotterPanel;




        private System.Windows.Forms.ListView annotationProtocolsList;
        private AnnotationProtocolList aProtocols;
        private Button startAnnnotationButton;
        private Label annotationLabel;
        public void InitializeInterface()
        {
            //GdiplusStartupInput input = new GdiplusStartupInput();
            //GdiplusStartupOutput output;
            //GpStatusPlus stat = NativeMethods.GdiplusStartup(out token, input, out output);

            currentTransition = Transitions.LEFT_TO_RIGHT;

            Constants.PATH = System.IO.Path.GetDirectoryName(
               System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase)+"\\NeededFiles\\";


            this.AutoScroll = false;
            this.numberButtons[ControlID.HOME_PANEL] = ControlID.HOME_PANEL_BUTTON_COUNT;
            this.numberButtons[ControlID.ABOUT_PANEL] = ControlID.ABOUT_PANEL_BUTTON_COUNT;
            this.numberButtons[ControlID.SETTINGS_PANEL] = ControlID.SETTINGS_PANEL_BUTTON_COUNT;
            this.numberButtons[ControlID.WOCKETS_PANEL] = ControlID.WOCKETS_PANEL_BUTTON_COUNT;
            this.numberButtons[ControlID.WOCKETS_CONFIGURATION_PANEL] = ControlID.WOCKETS_CONFIGURATION_PANEL_BUTTON_COUNT;
            this.numberButtons[ControlID.PLOTTER_PANEL] = ControlID.PLOTTER_PANEL_BUTTON_COUNT;
            this.numberButtons[ControlID.ANNOTATION_PROTCOLS_PANEL] = ControlID.ANNOTATION_PROTOCOLS_PANEL_BUTTON_COUNT;
            this.numberButtons[ControlID.ANNOTATION_BUTTON_PANEL] = ControlID.ANNOTATION_BUTTON_PANEL_BUTTON_COUNT;
           
            for (int i = 0; (i < ControlID.NUMBER_PANELS); i++)
           {

               this.panels[i] = new ClickableAlphaPanel(this.numberButtons[i]);
               this.panels[i].Size = new Size(SCREEN_RESOLUTION_X, SCREEN_RESOLUTION_Y);
                this.panels[i].MouseDown += new MouseEventHandler(owner_MouseDown);
                this.panels[i].MouseUp += new MouseEventHandler(owner_MouseUp);                
                //this.panels[i].BackColor=colors[i];
                this.panels[i].Dock = DockStyle.Fill;
                this.panels[i]._Backbuffer = new Bitmap(SCREEN_RESOLUTION_X, SCREEN_RESOLUTION_Y, PixelFormat.Format32bppRgb);
                this.Controls.Add(this.panels[i]);
            }

            //setup backgrounds
            this.panels[ControlID.HOME_PANEL]._Background = new Bitmap(Constants.PATH + "Backgrounds\\DottedBlack.png");
            this.panels[ControlID.HOME_PANEL]._BackgroundFile = Constants.PATH + "Backgrounds\\DottedBlack.png";
            this.panels[ControlID.ABOUT_PANEL]._Background = (Bitmap)this.panels[ControlID.HOME_PANEL]._Background.Clone();
            this.panels[ControlID.ABOUT_PANEL]._BackgroundFile = Constants.PATH + "Backgrounds\\DottedBlack.png";
            this.panels[ControlID.SETTINGS_PANEL]._Background = (Bitmap)this.panels[ControlID.HOME_PANEL]._Background.Clone();
            this.panels[ControlID.SETTINGS_PANEL]._BackgroundFile = Constants.PATH + "Backgrounds\\DottedBlack.png";
            //this.panels[ControlID.WOCKETS_PANEL]._Background = new Bitmap(Constants.PATH + "Backgrounds\\DottedBlack.png");
            //this.panels[ControlID.WOCKETS_PANEL]._BackgroundFile = Constants.PATH + "Backgrounds\\DottedBlack.png";
           // this.panels[ControlID.WOCKETS_CONFIGURATION_PANEL]._Background = new Bitmap(Constants.PATH + "Backgrounds\\DottedBlack.png");
            //this.panels[ControlID.WOCKETS_CONFIGURATION_PANEL]._BackgroundFile = Constants.PATH + "Backgrounds\\DottedBlack.png";
            //this.panels[ControlID.PLOTTER_PANEL]._Background = new Bitmap(Constants.PATH + "Backgrounds\\DottedBlack.png");
            //this.panels[ControlID.PLOTTER_PANEL]._BackgroundFile = Constants.PATH + "Backgrounds\\DottedBlack.png";
            //this.panels[ControlID.ANNOTATION_PROTCOLS_PANEL]._Background = new Bitmap(Constants.PATH + "Backgrounds\\DottedBlack.png");
            //this.panels[ControlID.ANNOTATION_PROTCOLS_PANEL]._BackgroundFile = Constants.PATH + "Backgrounds\\DottedBlack.png";
            this.panels[ControlID.ANNOTATION_PROTCOLS_PANEL].BackColor = Color.FromArgb(250, 237, 221);
            this.panels[ControlID.ANNOTATION_PROTCOLS_PANEL]._ClearCanvas = true;
            this.panels[ControlID.ANNOTATION_BUTTON_PANEL].BackColor = Color.FromArgb(250, 237, 221);
            this.panels[ControlID.ANNOTATION_BUTTON_PANEL]._ClearCanvas = true;

            //Main Page
            //Home Screen Bottom  Buttons
            AddButton(ControlID.HOME_PANEL, ControlID.SETTINGS_BUTTON, "Buttons\\SettingsPressed.png", "Buttons\\SettingsUnpressed.png", 0, this.Height - 130, 128, null,ButtonType.Fixed);
            AddButton(ControlID.HOME_PANEL, ControlID.MINIMIZE_BUTTON, "Buttons\\MinimizePressed.png", "Buttons\\MinimizeUnpressed.png", 160, this.Height - 130, 128,  null, ButtonType.Fixed);
            AddButton(ControlID.HOME_PANEL, ControlID.RESET_BUTTON, "Buttons\\TurnOffPressed.png", "Buttons\\TurnOffUnpressed.png", 310, this.Height - 130, 128,  null, ButtonType.Fixed);

            //Home Screen Buttons
            AddButton(ControlID.HOME_PANEL, ControlID.LINE_CHART_BUTTON, "Buttons\\LineChartPressed.png", "Buttons\\LineChartUnpressed.png", 0, 0, 128, "Plot", ButtonType.Fixed);
            AddButton(ControlID.HOME_PANEL, ControlID.BATTERY_BUTTON, "Buttons\\BatteryPressed.png", "Buttons\\BatteryUnpressed.png", 160, 0, 128,  "Power", ButtonType.Fixed);
            AddButton(ControlID.HOME_PANEL, ControlID.GREEN_POWER_BUTTON, "Buttons\\SavePowerPressed-128.png", "Buttons\\SavePowerUnpressed-128.png", 310, 0, 128,  "Go Green", ButtonType.Fixed);


            AddButton(ControlID.HOME_PANEL, ControlID.CONNECT_BUTTON, "Buttons\\ConnectPressed-128.png", "Buttons\\ConnectUnpressed-128.png", 0, 160, 128,  "Connect", ButtonType.Fixed);
            AddButton(ControlID.HOME_PANEL, ControlID.DISCONNECT_BUTTON, "Buttons\\DisconnectPressed-128.png", "Buttons\\DisconnectUnpressed-128.png", 160, 160, 128,  "Disconnect", ButtonType.Fixed);

            AddButton(ControlID.HOME_PANEL, ControlID.KERNEL_BUTTON, "Buttons\\StopKernelUnpressed-128.png", "Buttons\\StartKernelUnpressed-128.png", 310, 160, 128, "Start Kernel", ButtonType.Alternating);
            AddButton(ControlID.HOME_PANEL, ControlID.ANNOTATION_BUTTON, "Buttons\\AnnotatePressed.png", "Buttons\\AnnotateUnpressed.png", 0, 320, 128, "Annotate", ButtonType.Fixed);
            //AddButton(ControlID.HOME_PANEL, ControlID.STOP_KERNEL_BUTTON, "Buttons\\StopKernelPressed-128.png", "Buttons\\StopKernelUnpressed-128.png", 0, 310, 128, "Stop", "Stop", ButtonType.Fixed);

           // gauge.Visible = true;
           // gauge.Location = new Point(200, 200);
            //this.panels[ControlID.HOME_PANEL].Controls.Add(gauge);

            //gg.Size = new Size(400, 400);
            //this.panels[ControlID.HOME_PANEL].Controls.Add(gg);
            
            //AddButton(ControlID.HOME_PANEL, ControlID.ANNOTATE_BUTTON, "Buttons\\AnnotatePressed.png", "Buttons\\AnnotateUnpressed.png", 0, 160, 128);
            //AddButton(ControlID.HOME_PANEL, ControlID.STATISTICS_BUTTON, "Buttons\\StatisticsPressed.png", "Buttons\\StatisticsUnpressed.png", 310, 0, 128);
            //AddButton(ControlID.HOME_PANEL, ControlID.QUALITY_BUTTON, "Buttons\\SignalQualityPressed.png", "Buttons\\SignalQualityUnpressed.png", 0, 160, 128);
            
            //AddButton(ControlID.HOME_PANEL, ControlID.HEALTH_BUTTON, "Buttons\\HeartPressed.png", "Buttons\\HeartUnpressed.png", 310, 160, 128);




            //Annotation Bottom  Buttons
            AddButton(ControlID.ANNOTATION_PROTCOLS_PANEL, ControlID.ANNOTATION_BACK_BUTTON, "Buttons\\BackPressed.png", "Buttons\\BackUnpressed.png", 310, this.Height - 130, 128, null, ButtonType.Fixed);
            AddButton(ControlID.ANNOTATION_BUTTON_PANEL, ControlID.ANNOTATION_BUTTON_BACK_BUTTON, "Buttons\\BackPressed.png", "Buttons\\BackUnpressed.png", 310, this.Height - 130, 128, null, ButtonType.Fixed);

            //Settings Bottom  Buttons
            AddButton(ControlID.SETTINGS_PANEL, ControlID.BACK_BUTTON, "Buttons\\BackPressed.png", "Buttons\\BackUnpressed.png", 310, this.Height - 130, 128, null, ButtonType.Fixed);

            //Settings Buttons
            AddButton(ControlID.SETTINGS_PANEL, ControlID.BLUETOOTH_BUTTON, "Buttons\\BluetoothPressed.png", "Buttons\\BluetoothUnpressed.png", 0, 0, 128,  "Wockets", ButtonType.Fixed);
            AddButton(ControlID.SETTINGS_PANEL, ControlID.SOUND_BUTTON, "Buttons\\SoundPressed.png", "Buttons\\SoundUnpressed.png", 160, 0, 128, "Sound",  ButtonType.Fixed);

            //Wockets Screen

            AddButton(ControlID.WOCKETS_PANEL, ControlID.WOCKETS_BACK_BUTTON, "Buttons\\Back48Pressed.png", "Buttons\\Back48Unpressed.png", 400, this.Height - 48, 48, null, ButtonType.Fixed);
            AddButton(ControlID.WOCKETS_PANEL, ControlID.WOCKETS_UP_BUTTON, "Buttons\\Up48Pressed.png", "Buttons\\Up48Unpressed.png", 250, this.Height - 48, 48,  null, ButtonType.Fixed);
            AddButton(ControlID.WOCKETS_PANEL, ControlID.WOCKETS_DOWN_BUTTON, "Buttons\\Down48Pressed.png", "Buttons\\Down48Unpressed.png", 180, this.Height - 48, 48, null, ButtonType.Fixed);
            AddButton(ControlID.WOCKETS_PANEL, ControlID.WOCKETS_RELOAD_BUTTON, "Buttons\\BluetoothReloadPressed-48.png", "Buttons\\BluetoothReloadUnpressed-48.png", 20, this.Height - 48, 48, null,  ButtonType.Fixed);
            //AddButton(ControlID.WOCKETS_PANEL, ControlID.WOCKETS_SAVE_BUTTON, "Buttons\\SavePressed-64.png", "Buttons\\SaveUnpressed-64.png", 100, this.Height - 64, 64);

            wocketsList = new WocketSlidingList();                                         
            wocketsList.Size = new Size(Screen.PrimaryScreen.WorkingArea.Width, Screen.PrimaryScreen.WorkingArea.Height);            
            wocketsList.Location = new Point(0, 0);        
            this.panels[ControlID.WOCKETS_PANEL].Controls.Add(wocketsList);
            wocketsList.BringToFront();                     


            //Wockets Configuration Panel

            AddButton(ControlID.WOCKETS_CONFIGURATION_PANEL, ControlID.WOCKETS_CONFIGURATIONS_BLUETOOTH_BUTTON, "Buttons\\BluetoothUnpressed-64.png", "Buttons\\BluetoothPressed-64.png", 0, this.Height - 64, 64, null, ButtonType.Fixed);
            AddButton(ControlID.WOCKETS_CONFIGURATION_PANEL, ControlID.WOCKETS_CONFIGURATIONS_COMMAND_BUTTON, "Buttons\\CommandPressed-64.png", "Buttons\\CommandUnpressed-64.png", 80, this.Height - 64, 64, null, ButtonType.Fixed);
            AddButton(ControlID.WOCKETS_CONFIGURATION_PANEL, ControlID.WOCKETS_CONFIGURATIONS_TIMERS_BUTTON, "Buttons\\TimerPressed-64.png", "Buttons\\TimerUnpressed-64.png", 160, this.Height - 64, 64, null, ButtonType.Fixed);
            AddButton(ControlID.WOCKETS_CONFIGURATION_PANEL, ControlID.WOCKETS_CONFIGURATIONS_STATUS_BUTTON, "Buttons\\StatusPressed-64.png", "Buttons\\StatusUnpressed-64.png", 240, this.Height - 64, 64, null, ButtonType.Fixed);
            AddButton(ControlID.WOCKETS_CONFIGURATION_PANEL, ControlID.WOCKETS_CONFIGURATIONS_INFORMATION_BUTTON, "Buttons\\InformationPressed-64.png", "Buttons\\InformationUnpressed-64.png", 320, this.Height - 64, 64, null, ButtonType.Fixed);
            AddButton(ControlID.WOCKETS_CONFIGURATION_PANEL, ControlID.WOCKETS_CONFIGURATIONS_BACK_BUTTON, "Buttons\\Back64Pressed.png", "Buttons\\Back64Unpressed.png", 400, this.Height - 64, 64, null,  ButtonType.Fixed);
            bluetoothPanel = new Panel();
            bluetoothPanel.Size = new Size(Screen.PrimaryScreen.WorkingArea.Width, Screen.PrimaryScreen.WorkingArea.Height);
            bluetoothPanel.Visible = true;
            bluetoothPanel.BackColor = Color.FromArgb(245, 219, 186);
            bluetoothName = new Label();
            bluetoothName.Location = new Point(10, 10);
            bluetoothName.Size = new Size(250, 40);
            bluetoothName.Font = new Font(FontFamily.GenericSansSerif, 14.0f, System.Drawing.FontStyle.Underline | System.Drawing.FontStyle.Bold);
            bluetoothPanel.Controls.Add(bluetoothName);
            this.panels[ControlID.WOCKETS_CONFIGURATION_PANEL].Controls.Add(bluetoothPanel);            

            //Plotter Panel
            AddButton(ControlID.PLOTTER_PANEL, ControlID.WOCKETS_BACK_BUTTON, "Buttons\\Back48Pressed.png", "Buttons\\Back48Unpressed.png", 400, this.Height - 48, 48, null, ButtonType.Fixed);
            plotterPanel = new Panel();
            plotterPanel.Size = new Size(Screen.PrimaryScreen.WorkingArea.Width, Screen.PrimaryScreen.WorkingArea.Height);
            plotterPanel.Visible = true;
            plotterPanel.BackColor = Color.FromArgb(250, 237, 221);//Color.FromArgb(245, 219, 186);
            plotterPanel.Paint += new PaintEventHandler(plotterPanel_Paint);
            plotterTimer = new System.Windows.Forms.Timer();
            plotterTimer.Interval = 50;
            plotterTimer.Tick += new EventHandler(plotterTimer_Tick);
           
            this.panels[ControlID.PLOTTER_PANEL].Controls.Add(plotterPanel);            


            
            //this.panels[ControlID.WOCKETS_CONFIGURATION_PANEL].Controls.Add(
            //add bluetooth panel
            //add timers panel
            //add status panel



            for (int i = 0; (i < ControlID.NUMBER_PANELS); i++)
            {
                //cache panels with drawn backgrounds
                //this._backBuffers[i] = new Bitmap(480, 640, PixelFormat.Format32bppRgb);
                if (this.panels[i]._Background != null)
                {
                    Graphics offscreen = Graphics.FromImage(this.panels[i]._Backbuffer);
                    offscreen.DrawImage(this.panels[i]._Background, 0, 0);
                }
                this.panels[i].Initialize();
            }
            
            //this.panels[currentPanel]._Backbuffer = this._backBuffers[currentPanel];                        
            this.panels[currentPanel].Location = new Point(0, 0);
            this.panels[currentPanel].Update();
            this.panels[currentPanel].Visible = true;

            this.Deactivate += new EventHandler(Form1_Deactivate);
            this.Activated += new EventHandler(Form1_Activated);

            #region Annotation GUI
            //Setup the annotation protcols list
            annotationProtocolsList = new ListView();
            annotationProtocolsList.Location = new System.Drawing.Point(72, 44);
            annotationProtocolsList.View = View.List;
            annotationProtocolsList.Name = "annotationProtocolsList";
            annotationProtocolsList.Size = new System.Drawing.Size(100, 100);
            annotationProtocolsList.TabIndex = 0;
            annotationProtocolsList.SelectedIndexChanged += new EventHandler(annotationProtocolsList_SelectedIndexChanged);
            //adjust top label size and location
            annotationLabel= new Label();
            annotationLabel.Width = (int)(Screen.PrimaryScreen.WorkingArea.Width * 0.90);
            annotationLabel.Height = (int)(Screen.PrimaryScreen.WorkingArea.Width * 0.15);
            annotationLabel.Location = new Point(2, 2);                
            //Load the activity protocols from the master directory
            this.aProtocols = new AnnotationProtocolList();
            this.aProtocols.FromXML(Constants.PATH + "Master\\ActivityProtocols.xml");
            string longest_label = "";
            for (int i = 0; (i < this.aProtocols.Count); i++)
            {
                annotationProtocolsList.Items.Add(new ListViewItem(this.aProtocols[i]._Name));
                if (longest_label.Length < this.aProtocols[i]._Name.Length)
                    longest_label = this.aProtocols[i]._Name;
            }

            //Listbox dynamic placement
            annotationProtocolsList.Width = (int)(Screen.PrimaryScreen.WorkingArea.Width * 0.90);
            annotationProtocolsList.Height = (int)(Screen.PrimaryScreen.WorkingArea.Height * 0.70);
            annotationProtocolsList.Font = new Font(GUIHelper.FONT_FAMILY, 14F, this.Font.Style);
            annotationProtocolsList.Location = new Point((int)(Screen.PrimaryScreen.WorkingArea.Width * 0.05), (int)annotationLabel.Location.Y + annotationLabel.Height + 2);
            this.panels[ControlID.ANNOTATION_PROTCOLS_PANEL].Controls.Add(annotationProtocolsList);

            //add annotation label
            annotationLabel.Size = new Size(Screen.PrimaryScreen.WorkingArea.Width, 50);
            annotationLabel.Text = "Choose a protocol";
            annotationLabel.BackColor = Color.FromArgb(250, 237, 221);
            annotationLabel.Font = new Font(FontFamily.GenericSerif, 14.0f, FontStyle.Bold);
            annotationLabel.Visible = true;
            annotationLabel.Location = new Point((int)(Screen.PrimaryScreen.WorkingArea.Width * 0.05), 10);
            this.panels[ControlID.ANNOTATION_PROTCOLS_PANEL].Controls.Add(annotationLabel);

            //add a button to start
            startAnnnotationButton = new Button();
            startAnnnotationButton.Size = new Size(400, 50);
            startAnnnotationButton.Text = "Begin Annotation";
            //startAnnnotationButton.BackColor = Color.FromArgb(250, 237, 221);
            startAnnnotationButton.Font = new Font(FontFamily.GenericSerif, 14.0f, FontStyle.Bold);
            startAnnnotationButton.Enabled = false;
            startAnnnotationButton.Visible = true;
            startAnnnotationButton.Click += new EventHandler(startAnnnotationButton_Click);
            startAnnnotationButton.Location = new Point(Screen.PrimaryScreen.WorkingArea.Width/2 - 200, annotationProtocolsList.Location.Y + annotationProtocolsList.Height+10);
            this.panels[ControlID.ANNOTATION_PROTCOLS_PANEL].Controls.Add(startAnnnotationButton);
            
            #endregion Annotation GUI
            this.panels[ControlID.ANNOTATION_BUTTON_PANEL].AutoScroll = true;

        }
        private Session annotatedSession;
        private int selectedActivityProtocol;
        private ArrayList activityButtons = new ArrayList();

        private const int BS_MULTILINE = 0x00002000;
        private const int GWL_STYLE = -16;

        [System.Runtime.InteropServices.DllImport("coredll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [System.Runtime.InteropServices.DllImport("coredll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        public static void MakeButtonMultiline(Button b)
        {
            IntPtr hwnd = b.Handle;
            int currentStyle = GetWindowLong(hwnd, GWL_STYLE);
            int newStyle = SetWindowLong(hwnd, GWL_STYLE, currentStyle | BS_MULTILINE);
        }

        public static void MakeLabelMultiline(Label b)
        {
            IntPtr hwnd = b.Handle;
            int currentStyle = GetWindowLong(hwnd, GWL_STYLE);
            int newStyle = SetWindowLong(hwnd, GWL_STYLE, currentStyle | BS_MULTILINE);
        }

        private String truncateText(String text)
        {

            int maxChars = 10;

            if (text.Length <= maxChars)
                return text;

            char[] delimiter = { ' ', '-', '/' };
            String[] tokens = text.Split(delimiter);

            if (tokens.Length == 1)
                return text;

            String final = "";
            String currentLine = "";


            foreach (String part in tokens)
            {
                String temp = part;
                if (temp.StartsWith("(") && temp.EndsWith(")")) temp = temp.Substring(1, temp.Length - 2);
                else if (temp.StartsWith("(")) temp = temp.Remove(0, 1);
                else if (temp.EndsWith(")")) temp = temp.Substring(0, temp.Length - 1);

                if (temp.Equals("with")) temp = "w/";
                else if (temp.Equals("without")) temp = "w/o";
                else if (temp.Equals("morning")) temp = "AM";
                else if (temp.Equals("night")) temp = "PM";
                else if (temp.Equals("a")) temp = "";

                if ((currentLine.Length + temp.Length) >= maxChars)
                {
                    final += currentLine + " \n";
                    currentLine = "";
                }

                currentLine += temp + " ";
            }
            final += currentLine;

            return final;
        }

        ArrayList selectedButtons = new ArrayList();
        char[] delimiter = { '_' };
        private Annotation currentRecord = null;
        private void activityButton_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            string[] name = button.Name.Split('_');
            int category = Convert.ToInt32(name[0]);
            int index = Convert.ToInt32(name[1]);


            System.Windows.Forms.Button[] activityList = (System.Windows.Forms.Button[])activityButtons[category];
            for (int j = 0; j < activityList.Length; j++)
            {
                if (activityList[j].BackColor == Color.DodgerBlue)
                {
                    activityList[j].BackColor = Color.SkyBlue;
                    selectedButtons.Remove(activityList[j]);
                }
                else if (index == j)
                {
                    activityList[j].BackColor = Color.DodgerBlue;
                    selectedButtons.Add(activityList[j]);
                }
            }

            if (this.currentRecord != null)
            {

                stopAnnotation();
                TextWriter tw = new StreamWriter(Core._StoragePath + "\\AnnotationIntervals.xml");
                tw.WriteLine(this.annotatedSession.ToXML());
                tw.Close();
            }
            if (selectedButtons.Count > 0)
            {
                startAnnotation();
            }
        }


        ArrayList records = new ArrayList();

        private void startAnnotation()
        {
            this.currentRecord = new Annotation();
            this.currentRecord._StartDate = DateTime.Now.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ssK");
            this.currentRecord._StartHour = DateTime.Now.Hour;
            this.currentRecord._StartMinute = DateTime.Now.Minute;
            this.currentRecord._StartSecond = DateTime.Now.Second;
            TimeSpan ts = (DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0));
            this.currentRecord._StartUnix = ts.TotalSeconds;

            //check all buttons values, store them and disable them
           /* if (((Panel)this.panels[ANNOTATE_LIST_PANEL]).Visible)
            {
                foreach (ComboBox combo in categoryDrops)
                {
                    int button_id = Convert.ToInt32(combo.Name);
                    ActivityList category = (ActivityList)this.annotatedSession.OverlappingActivityLists[button_id];
                    string current_label = (string)combo.SelectedItem;
                    this.currentRecord.Activities.Add(new Activity(current_label, category._Name));
                }
            }
            else*/
            if (this.panels[ControlID.ANNOTATION_BUTTON_PANEL].Visible)
            {
                for (int i = 0; i < selectedButtons.Count; i++)
                {
                    System.Windows.Forms.Button but = (System.Windows.Forms.Button)selectedButtons[i];
                    string[] name = but.Name.Split('_');
                    int category = Convert.ToInt32(name[0]);
                    int index = Convert.ToInt32(name[1]);
                    this.currentRecord.Activities.Add(new Activity(this.annotatedSession.OverlappingActivityLists[category][index]._Name, this.annotatedSession.OverlappingActivityLists[category]._Name));
                }
            }
            //this.wocketsController.currentRecord = this.currentRecord;
        }

        private void stopAnnotation()
        {
            this.currentRecord._EndDate = DateTime.Now.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ssK");
            this.currentRecord._EndHour = DateTime.Now.Hour;
            this.currentRecord._EndMinute = DateTime.Now.Minute;
            this.currentRecord._EndSecond = DateTime.Now.Second;
            TimeSpan ts = (DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0));
            this.currentRecord._EndUnix = ts.TotalSeconds;
            this.annotatedSession.Annotations.Add(this.currentRecord);
            this.currentRecord = null;
            //this.wocketsController.currentRecord = null;

        }

        void startAnnnotationButton_Click(object sender, EventArgs e)
        {
            if (Core._Connected)
            {
                this.selectedActivityProtocol = this.annotationProtocolsList.SelectedIndices[0];
                if (File.Exists(Constants.PATH + "ActivityProtocols\\" + this.aProtocols[this.selectedActivityProtocol]._FileName))
                {
                    File.Copy(Constants.PATH + "ActivityProtocols\\" + this.aProtocols[this.selectedActivityProtocol]._FileName,
                           Core._StoragePath + "\\ActivityLabelsRealtime.xml");
                    this.annotatedSession = new Session();
                    annotatedSession.FromXML(Core._StoragePath + "\\ActivityLabelsRealtime.xml");
                }
                //add buttons to the interface

                this.panels[ControlID.ANNOTATION_BUTTON_PANEL].Visible = true;
                this.panels[currentPanel].Visible = false;
                this.currentPanel = ControlID.ANNOTATION_BUTTON_PANEL;


                this.panels[ControlID.ANNOTATION_BUTTON_PANEL].AutoScroll = true;
                int max_buttons_per_row = 4;
                int act_button_width = 0;
                int act_button_height = 0;
                int numberOfButtons = 0;
                float fontSize = 7F;
                int act_button_x = 0;
                int act_button_y = 0;
                int marginHeight = 20;
                int screenWidth = this.panels[ControlID.ANNOTATION_BUTTON_PANEL].Width;
                int screenHeight = this.panels[ControlID.ANNOTATION_BUTTON_PANEL].Height;
                int scrollbarWidth = 28;

                for (int i = 0; (i < this.annotatedSession.OverlappingActivityLists.Count); i++)
                {
                    Activity[] acts = this.annotatedSession.OverlappingActivityLists[i].ToArray();
                    if (Platform.NativeMethods.GetPlatformType() == "PocketPC")
                    {
                        System.Windows.Forms.Button[] buttons = new System.Windows.Forms.Button[acts.Length];

                        for (int j = 0; j < buttons.Length; j++)
                        {
                            buttons[j] = new System.Windows.Forms.Button();
                            MakeButtonMultiline(buttons[j]);
                            buttons[j].Name = i + "_" + j;
                            buttons[j].Text = truncateText(acts[j]._Name);
                            buttons[j].Click += new EventHandler(this.activityButton_Click);                            
                            buttons[j].BackColor = Color.SkyBlue;
                            numberOfButtons += 1;
                        }
                        activityButtons.Add(buttons);
                    }
                }

                if (numberOfButtons > 12)
                {
                    screenWidth -= scrollbarWidth;
                    act_button_width = act_button_height = screenWidth / max_buttons_per_row;
                }
                else if ((numberOfButtons <= 12) && (numberOfButtons > 8))
                {
                    act_button_width = screenWidth / max_buttons_per_row;
                    act_button_height = act_button_width + (act_button_width / 3);
                }
                else if ((numberOfButtons <= 8) && (numberOfButtons > 3))
                {
                    int dBlockSize = (screenWidth - 2) / max_buttons_per_row;
                    max_buttons_per_row = 2;
                    act_button_width = dBlockSize * 2;
                    int s = (int)Math.Ceiling(numberOfButtons / 2.0);
                    act_button_height = ((dBlockSize * 4) + 22) / s;
                    fontSize = 12F;
                }
                else
                {
                    int dBlockSize = screenWidth / max_buttons_per_row;
                    max_buttons_per_row = 1;
                    act_button_width = screenWidth - 2;
                    act_button_height = (dBlockSize * 4) / numberOfButtons;
                    fontSize = 14F;
                }

                if (Platform.NativeMethods.GetPlatformType() == "PocketPC")
                {
                    for (int i = 0; i < activityButtons.Count; i++)
                    {
                        System.Windows.Forms.Button[] activityList = (System.Windows.Forms.Button[])activityButtons[i];
                        int buttonsOnRow = 0;
                        for (int j = 0; j < activityList.Length; j++)
                        {

                            activityList[j].Visible = true;
                            activityList[j].Width = act_button_width;
                            activityList[j].Height = act_button_height;
                            activityList[j].Location = new System.Drawing.Point(act_button_x, act_button_y);
                            activityList[j].Font = new System.Drawing.Font("Microsoft Sans Serif", fontSize, System.Drawing.FontStyle.Regular);
                            ((Panel)this.panels[ControlID.ANNOTATION_BUTTON_PANEL]).Controls.Add(activityList[j]);
                            buttonsOnRow += 1;

                            if (buttonsOnRow == activityList.Length) //completed a category
                            {
                                act_button_x = 0;
                                act_button_y += act_button_height + marginHeight;
                                buttonsOnRow = 0;
                            }
                            else if (buttonsOnRow == max_buttons_per_row) //completed a row within a category
                            {
                                act_button_x = 0;
                                act_button_y += act_button_height;
                                buttonsOnRow = 0;
                            }
                            else //added a button within a row
                                act_button_x += act_button_width;
                        }

                    }
                }              
            }
            else
                MessageBox.Show("Please connect to wockets first before annotating", "Confirm", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);            


        }

        void annotationProtocolsList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!startAnnnotationButton.Enabled)
                startAnnnotationButton.Enabled = true;
            //throw new NotImplementedException();
        }

        void plotterTimer_Tick(object sender, EventArgs e)
        {
            if (plotter != null)
            {
                if (backBuffer == null) // || (isResized))
                {
                    backBuffer = new Bitmap(plotterPanel.Width, (int)(plotterPanel.Height));
                }
                using (Graphics g = Graphics.FromImage(backBuffer))
                {

                    plotter.Draw(g);
                    g.Dispose();

                }
            }
        }

        private Bitmap backBuffer = null;
        private System.Windows.Forms.Timer plotterTimer=null;

        void plotterPanel_Paint(object sender, PaintEventArgs e)
        {
            if (plotterPanel.Visible)
            {
                if (backBuffer != null)
                    e.Graphics.DrawImage(backBuffer, 0, 0);
            }
        }

        void Form1_Activated(object sender, EventArgs e)
        {
            ScreenUtils.ShowTaskBar(false);
            ScreenUtils.ShowTrayBar(false);
        }

        void Form1_Deactivate(object sender, EventArgs e)
        {
            ScreenUtils.ShowTaskBar(true);        
        }


        void owner_MouseUp(object sender, MouseEventArgs e)
        {

            //if ((pushed)&& (clientArea.Contains(e.X, e.Y)))                   
            //    timeAnimation_Tick();            
            //this.pushed = false;
      
            if (this.panels[currentPanel]._UnpressedButtonControls != null)
            {
                for (int i = 0; (i < this.panels[currentPanel]._UnpressedButtonControls.Length); i++)
                {
                    if ((this.panels[currentPanel]._ButtonType[i]== ButtonType.Fixed) && (this.panels[currentPanel]._ButtonPressed[i]))
                    {
                        this.panels[currentPanel]._UnpressedButtonControls[i].Visible = true;
                        this.panels[currentPanel]._PressedButtonControls[i].Visible = false;
                        this.panels[currentPanel]._ButtonPressed[i] = false;
                    }
  
                }
            }
        }


        void owner_MouseDown(object sender, MouseEventArgs e)
        {
            Control p = (Control)sender;
            if (!p.Enabled)
                return;

            if ((!pushed)  && (this.slidingPanels.Contains(currentPanel)))
            {
                if (e.X < (Screen.PrimaryScreen.WorkingArea.Width / 3))
                {
                    this.currentTransition = Transitions.LEFT_TO_RIGHT;
                    this.pushed = true;
                    this.clientArea = new Rectangle((Screen.PrimaryScreen.WorkingArea.Width / 2), e.Y - (Screen.PrimaryScreen.WorkingArea.Height / 5), Screen.PrimaryScreen.WorkingArea.Width, (Screen.PrimaryScreen.WorkingArea.Height / 5) * 2);
                }
                else if (e.X > (Screen.PrimaryScreen.WorkingArea.Width * (2 / 3)))
                {
                    this.currentTransition = Transitions.RIGHT_TO_LEFT;
                    this.pushed = true;
                    this.clientArea = new Rectangle(0, e.Y - (Screen.PrimaryScreen.WorkingArea.Height / 5), (Screen.PrimaryScreen.WorkingArea.Width  / 2), (Screen.PrimaryScreen.WorkingArea.Height / 5) * 2);
                }
            }


            if (this.panels[currentPanel]._UnpressedButtonControls != null)
            {
                for (int i = 0; (i < this.panels[currentPanel]._UnpressedButtonControls.Length); i++)
                {

                    if (this.panels[currentPanel]._UnpressedButtonControls[i].HitTest(e.X, e.Y))
                    {
                        if ((this.panels[currentPanel]._ButtonType[i]== ButtonType.Fixed) && (!this.panels[currentPanel]._ButtonPressed[i]))
                        {
                            this.panels[currentPanel]._PressedButtonControls[i].Size = new Size(128, 128);
                                this.panels[currentPanel]._PressedButtonControls[i].Visible = true;
                                this.panels[currentPanel]._UnpressedButtonControls[i].Visible = false;
                                this.panels[currentPanel]._ButtonPressed[i] = true;
                                this.panels[currentPanel]._PressedButtonControls[i].Refresh();
                        }
                    }
                    else if ((this.panels[currentPanel]._ButtonType[i]== ButtonType.Fixed) && (this.panels[currentPanel]._ButtonPressed[i]))
                    {
                        this.panels[currentPanel]._UnpressedButtonControls[i].Size = new Size(128, 128);
                        this.panels[currentPanel]._UnpressedButtonControls[i].Visible = true;
                        this.panels[currentPanel]._PressedButtonControls[i].Visible = false;                       
                        this.panels[currentPanel]._ButtonPressed[i] = false;
                        this.panels[currentPanel]._UnpressedButtonControls[i].Refresh();

                    }
                }
            }
            this.Refresh();
            
        }


        private void wocketClickHandler(object sender, EventArgs e)
        {
            WocketListItem wi = (WocketListItem)sender;
            int name = Convert.ToInt32(wi.Name);
            if ( (wi.AddHitTest(wi.LastX, wi.LastY)) && !selectedWockets.Contains(wi))
            {
                selectedWockets.Add(wi);
                wi.BackColor = Color.FromArgb(205,183,158);
            }
            else if (wi.RemoveHitTest(wi.LastX, wi.LastY) && selectedWockets.Contains(wi))
            {
                selectedWockets.Remove(wi);
                wi.BackColor = Color.FromArgb(245, 219, 186);
            }
            else                        
            {
                bluetoothName.Text = wi._Name;                
                this.panels[ControlID.WOCKETS_CONFIGURATION_PANEL].Visible = true;
                this.panels[ControlID.WOCKETS_PANEL].Visible = false;
                currentPanel = ControlID.WOCKETS_CONFIGURATION_PANEL;
            }
        }
        public delegate void ClickHandler(object sender, EventArgs e);
        private double clickTime = 0;
        private void clickHandler(object sender, EventArgs e)
        {
            AlphaPictureBox p = (AlphaPictureBox)sender;



            int name = Convert.ToInt32(p.Name);
            if (currentPanel == ControlID.WOCKETS_PANEL)
            {
                if (name == ControlID.WOCKETS_BACK_BUTTON)
                {                    
                    this.panels[ControlID.SETTINGS_PANEL].Visible = true;
                    this.panels[ControlID.WOCKETS_PANEL].Visible = false;
                    currentPanel = ControlID.SETTINGS_PANEL;
                    ArrayList s = new ArrayList();
                    for (int i = 0; (i < selectedWockets.Count); i++)
                    {
                        s.Add(((WocketListItem)selectedWockets[i])._MacAddress);
                    }
                    Core.SetSensors(Core._KernelGuid, s);
                }
                else if (name == ControlID.WOCKETS_UP_BUTTON)
                    wocketsList.MoveDown();
                else if (name == ControlID.WOCKETS_DOWN_BUTTON)
                    wocketsList.MoveUp();
                else if (name == ControlID.WOCKETS_RELOAD_BUTTON)
                {
                    wocketsList._Status = "Searching for Wockets...";
                    wocketsList.Refresh();
                    if (Core._KernelGuid != null)
                        Core.Send(KernelCommand.DISCOVER, Core._KernelGuid);
                }
               /* else if (name == ControlID.WOCKETS_SAVE_BUTTON)
                {
                }*/
            }
            else if (currentPanel == ControlID.WOCKETS_CONFIGURATION_PANEL)
            {
                if (name == ControlID.WOCKETS_CONFIGURATIONS_BACK_BUTTON)
                {

                    this.panels[ControlID.WOCKETS_PANEL].Visible = true;
                    this.panels[ControlID.WOCKETS_CONFIGURATION_PANEL].Visible = false;
                    this.currentPanel = ControlID.WOCKETS_PANEL;
                }

            }

            else if (currentPanel == ControlID.HOME_PANEL)
            {
                if (name == ControlID.KERNEL_BUTTON)
                {
                    if (!this.panels[currentPanel]._ButtonPressed[ControlID.KERNEL_BUTTON])
                    {


                      
                        this.panels[currentPanel]._UnpressedButtonControls[ControlID.KERNEL_BUTTON].Enabled = false;
                        this.panels[currentPanel]._PressedButtonControls[ControlID.KERNEL_BUTTON].Size = new Size(128, 128);
                        //this.panels[currentPanel]._PressedButtonControls[ControlID.KERNEL_BUTTON].BringToFront();
                        this.panels[currentPanel]._PressedButtonControls[ControlID.KERNEL_BUTTON].Visible = true;
                        this.panels[currentPanel]._UnpressedButtonControls[ControlID.KERNEL_BUTTON].Visible = false;
                        this.panels[currentPanel]._ButtonText[ControlID.KERNEL_BUTTON].Text = "Stop Kernel";
                        this.panels[currentPanel]._ButtonPressed[ControlID.KERNEL_BUTTON] = true;

                        if (this.panels[currentPanel]._Background != null)
                        {
                            Graphics offscreen = Graphics.FromImage(this.panels[currentPanel]._Backbuffer);
                            offscreen.DrawImage(this.panels[currentPanel]._Background, 0, 0);
                        }



                        if (!Core._KernelStarted)
                            Core.Start();

                        Thread.Sleep(5000);
                        if (Core._KernelStarted)
                        {
                            if (!Core._Registered)
                            {
                                Core.Register();
                                if (Core._Registered)
                                {
                                    kListenerThread = new Thread(new ThreadStart(KernelListener));
                                    kListenerThread.Start();
                                }
                            }
                        }

                        this.panels[currentPanel]._PressedButtonControls[ControlID.KERNEL_BUTTON].Enabled = true;
                        clickTime = WocketsTimer.GetUnixTime();
                    }
                    else
                    {

                        if ((WocketsTimer.GetUnixTime() - clickTime) < 3000)
                            return;
                        if (MessageBox.Show("Are you sure you want to stop wockets kernel?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
                        {
                            this.panels[currentPanel]._PressedButtonControls[ControlID.KERNEL_BUTTON].Enabled = false;
                            this.panels[currentPanel]._UnpressedButtonControls[ControlID.KERNEL_BUTTON].Size = new Size(128, 128);
                            this.panels[currentPanel]._UnpressedButtonControls[ControlID.KERNEL_BUTTON].Visible = true;
                            //this.panels[currentPanel]._UnpressedButtonControls[ControlID.KERNEL_BUTTON].BringToFront();
                            this.panels[currentPanel]._PressedButtonControls[ControlID.KERNEL_BUTTON].Visible = false;
                            this.panels[currentPanel]._ButtonText[ControlID.KERNEL_BUTTON].Text = "Start Kernel";
                            this.panels[currentPanel]._ButtonPressed[ControlID.KERNEL_BUTTON] = false;

                            if (this.panels[currentPanel]._Background != null)
                            {
                                Graphics offscreen = Graphics.FromImage(this.panels[currentPanel]._Backbuffer);
                                offscreen.DrawImage(this.panels[currentPanel]._Background, 0, 0);
                            }


                            if (Core._KernelStarted)
                            {

                                Core._Connected = false;
                                Core._Registered = false;
                                selectedWockets.Clear();
                                Core.Terminate();

                                this.panels[currentPanel]._UnpressedButtonControls[ControlID.KERNEL_BUTTON].Visible = true;
                                this.panels[currentPanel]._PressedButtonControls[ControlID.KERNEL_BUTTON].Visible = false;
                                this.panels[currentPanel]._ButtonText[ControlID.KERNEL_BUTTON].Text = "Start Kernel";
                                this.panels[currentPanel]._ButtonPressed[ControlID.KERNEL_BUTTON] = false;
                            }
                            this.panels[currentPanel]._UnpressedButtonControls[ControlID.KERNEL_BUTTON].Enabled = true;

                            //this.panels[currentPanel].Refresh();

                        }
                    }

                }
                else if (name == ControlID.BATTERY_BUTTON)
                {
                    if (Core._Connected)
                        Core.SetSniff(Core._KernelGuid, SleepModes.NoSleep);
                }
                else if (name == ControlID.GREEN_POWER_BUTTON)
                {
                    if (Core._Connected)
                        Core.SetSniff(Core._KernelGuid, SleepModes.Sleep1Second);
                }
                else if (name == ControlID.RESET_BUTTON)
                {
                    if (MessageBox.Show("Are you sure you want to exit?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
                    {
                        if (Core._KernelGuid != null)
                            Core.Unregister(Core._KernelGuid);
                        ScreenUtils.ShowTaskBar(true);

                        //Terminate the kernel
                        if (Core._KernelGuid != null)
                            Core.Send(KernelCommand.TERMINATE, Core._KernelGuid);

                        Application.Exit();
                        System.Diagnostics.Process.GetCurrentProcess().Kill();
                    }
                }
                else if (name == ControlID.SETTINGS_BUTTON)
                {

                    this.panels[currentPanel].Visible = false;
                    this.panels[ControlID.SETTINGS_PANEL].Location = new Point(0, 0);
                    this.panels[ControlID.SETTINGS_PANEL].BringToFront();
                    this.panels[ControlID.SETTINGS_PANEL].Visible = true;
                    this.panels[ControlID.SETTINGS_PANEL].Dock = DockStyle.None;
                    this.currentPanel = ControlID.SETTINGS_PANEL;
                }
                else if (name == ControlID.CONNECT_BUTTON)
                {
                    if (Core._Registered)
                    {
                        if (!Core._Connected)
                            Core.Connect(Core._KernelGuid);
                        else
                            MessageBox.Show("Wockets Already Connected!");
                    }
                    else
                        MessageBox.Show("Application not registered!");
                }
                else if (name == ControlID.DISCONNECT_BUTTON)
                {
                    if (Core._Connected)
                    {
                        Core.Disconnect(Core._KernelGuid);
                        plotter = null;
                    }
                    else
                        MessageBox.Show("Wockets Already Disconnected!");
                }
                else if (name == ControlID.LINE_CHART_BUTTON)
                {
                    if (Core._Connected)
                    {                        
                        //plotterTimer.Enabled = true;
                        this.panels[currentPanel].Visible = false;
                        this.panels[ControlID.PLOTTER_PANEL].Location = new Point(0, 0);
                        this.panels[ControlID.PLOTTER_PANEL].BringToFront();
                        this.panels[ControlID.PLOTTER_PANEL].Visible = true;
                        this.panels[ControlID.PLOTTER_PANEL].Dock = DockStyle.None;
                        this.currentPanel = ControlID.PLOTTER_PANEL;

                        UpdatePlotter();

                    }
                    else
                        MessageBox.Show("Cannot plot without connecting wockets!");

                }
                else if (name == ControlID.ANNOTATION_BUTTON)
                {
                    this.panels[currentPanel].Visible = false;
                    this.panels[ControlID.ANNOTATION_PROTCOLS_PANEL].Location = new Point(0, 0);
                    this.panels[ControlID.ANNOTATION_PROTCOLS_PANEL].BringToFront();
                    this.panels[ControlID.ANNOTATION_PROTCOLS_PANEL].Visible = true;
                    this.panels[ControlID.ANNOTATION_PROTCOLS_PANEL].Dock = DockStyle.None;
                    this.currentPanel = ControlID.ANNOTATION_PROTCOLS_PANEL;
                }
            }
            else if (currentPanel == ControlID.SETTINGS_PANEL)
            {
                if (name == ControlID.BACK_BUTTON)
                {
                    this.panels[ControlID.HOME_PANEL].Visible = true;
                    this.panels[ControlID.SETTINGS_PANEL].Visible = false;
                    this.currentPanel = ControlID.HOME_PANEL;
                }
                else if (name == ControlID.BLUETOOTH_BUTTON)
                {
                    this.panels[ControlID.SETTINGS_PANEL].Visible = false;
                    this.panels[ControlID.WOCKETS_PANEL].Location = new Point(0, 0);
                    //this.panels[ControlID.WOCKETS_PANEL].BringToFront();                   
                    this.panels[ControlID.WOCKETS_PANEL].Visible = true;
                    this.panels[ControlID.WOCKETS_PANEL].Dock = DockStyle.None;
                    this.currentPanel = ControlID.WOCKETS_PANEL;
                    UpdatewWocketsList();
                }
            }
            else if (currentPanel == ControlID.PLOTTER_PANEL)
            {
                if (name == ControlID.PLOTTER_BACK_BUTTON)
                {
                    this.panels[ControlID.HOME_PANEL].Visible = true;
                    this.panels[ControlID.PLOTTER_PANEL].Visible = false;
                    this.currentPanel = ControlID.HOME_PANEL;
                    plotterTimer.Enabled = false;
                    plotterPanel.Visible = false;

                    //Core.SetSniff(wocketsKernelGuid, SleepModes.Sleep1Second);
                }
            }
            else if (currentPanel == ControlID.ANNOTATION_PROTCOLS_PANEL)
            {
                if (name == ControlID.ANNOTATION_BACK_BUTTON)
                {
                    this.panels[ControlID.HOME_PANEL].Visible = true;
                    this.panels[ControlID.ANNOTATION_PROTCOLS_PANEL].Visible = false;
                    this.currentPanel = ControlID.HOME_PANEL;                                        
                }
            }
            else if (currentPanel == ControlID.ANNOTATION_BUTTON_PANEL)
            {
                if (name == ControlID.ANNOTATION_BUTTON_BACK_BUTTON)
                {
                    this.panels[ControlID.HOME_PANEL].Visible = true;
                    this.panels[ControlID.ANNOTATION_BUTTON_PANEL].Visible = false;
                    this.currentPanel = ControlID.HOME_PANEL;
                }
            }
            this.Refresh();
        }

        int m = 0;
        private void timeAnimation_Tick()
        {
            int prevPanelIndex = currentPanelIndex;
            int prevPanel = slidingPanels[currentPanelIndex];
            currentPanelIndex++;
            currentPanelIndex = currentPanelIndex % slidingPanels.Length;
            currentPanel = slidingPanels[currentPanelIndex];
            if (this.currentTransition == Transitions.LEFT_TO_RIGHT)
            {
                this.panels[currentPanel].Location = new Point(0 - this.panels[currentPanel].Width, 0);
                this.panels[currentPanel].BringToFront();
                this.panels[currentPanel].Visible = true;

                this.panels[currentPanel].Dock = DockStyle.None;
                m = 0;

                for (int x = -480; (x <= 0); x += 100)
               // for (int x = Screen.PrimaryScreen.WorkingArea.Width; (x >=0 ); x -= 100)
                {
                    this.panels[currentPanel].Location = new Point(x, this.panels[currentPanel].Location.Y);
                    //this.panels[currentPanel]._Backbuffer = this._backBuffers[currentPanel];
                    this.panels[currentPanel].Update();
                }

                this.panels[currentPanel].Location = new Point(0, this.panels[currentPanel].Location.Y);
                this.panels[prevPanel].Visible = false;
            }
            else if (this.currentTransition == Transitions.RIGHT_TO_LEFT)
            {
                this.panels[currentPanel].Location = new Point(0 - this.panels[currentPanel].Width, 0);
                this.panels[currentPanel].BringToFront();
                this.panels[currentPanel].Visible = true;

                this.panels[currentPanel].Dock = DockStyle.None;
                m = 0;

                //for (int x = -480; (x <= 0); x += 100)
                for (int x = Screen.PrimaryScreen.WorkingArea.Width; (x >=0 ); x -= 100)
                {
                    this.panels[currentPanel].Location = new Point(x, this.panels[currentPanel].Location.Y);
                    //this.panels[currentPanel]._Backbuffer = this._backBuffers[currentPanel];
                    this.panels[currentPanel].Update();
                }

                this.panels[currentPanel].Location = new Point(0, this.panels[currentPanel].Location.Y);
                this.panels[prevPanel].Visible = false;
            }
        }


       /* protected override void OnPaintBackground(PaintEventArgs e)
        {
            // Prevent flicker, we will take care of the background in OnPaint()
        }*/

        protected override void OnPaint(PaintEventArgs e)
        {          
           // this.Invalidate();
            //SHFullScreen(this.Handle, SHFS_HIDETASKBAR | SHFS_HIDESIPBUTTON | SHFS_HIDESTARTICON);
            _alphaManager.OnPaint(e);
         
        }
    }
}