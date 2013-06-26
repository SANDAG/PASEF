/* Filename:    Pasef.cs
 * Program:     Pasef
 * Version:     7 SR13
 * Programmers: Terry Beckhelm
 *              Daniel Flyte (C# revision)
 * Description: This application performs demographic characteristics forecasting.
 * Methods:     
 *              Main()
 *              doPasefWork()
 *              doPDHHWork()
 *              doEnrollment()
 *              
 *              derivePop()
 *              extractBasePop()
 *              extractControlPop()
 *              extractCtList()
 *              extractDefmPop()
 *              extractSpecialCtList()
 *              extractSRAChgShare()
 *              
 *              loadCtTable()
 *              loadMGRATable()
 *              loadMGRATabTable()
 *              loadSRATable()
 *              processParams()
 *              writeToStatus()
 *              
 *              PDHH procs
 *              AdjustRegionwideDistribution()
 *              AdjustSeedHHS()
 *              AdjustSeedHHSMGRA()
 *              AllocateTOMGRAS()
 *              BuildCTHHDetail()
 *              BuildSeed()
 *              BuildSeedMGRA()
 *              ControlToLocal()
 *              DescendingSortMulti()
 *              DOFinishChecks()
 *              ExtractCTHHData
 * 
 * Database:    pila
 *                  Databases: pila.sr13
 *                  Tables: indexed by scenario and year
 *						  
 *              Tables: 
 *                input:
 *                  concep.dbo.ct10Kids
 *                  concep.dbo.distribution_popest_HHworkers_region
 *                  distribution_HH_wo_children
 *                  error_factors_HHS
 *                  mgrabase: control pop by MGRA by year and scenario
 *                  pasee_special_pop_tracts: special pop cts          
 *                  pasef_mgra_tab:  tabular MGRA forecasts for year YYYY serve as base year data for year YYYY
 *                  pasef_ct:   base year detailed data
 *                  pasef_sra_chgshr: change in ethnic sex share by sra (add 1.0) and apply as multiplier
 *                  pop_defm: detailed defm pop data by year and scenario
                    xref_mgra_sr13: master cross-reference
 * 
 *                output:
 *                  pasef_ct: ct level pop output, year YYYY, scenario AA
 *                  pasef_sra: sra level pop output, year YYYY, scenario AA
 *                  pasef_mgra: normalized MGRA forecasts for year YYYY, scenario AA
 *                  pasef_mgra_tab:  tabular MGRA forecasts for year YYYY, scenario AA
 *                  pasef_HHdetail_mgra
 *                  pasef_HHdetail_ct

 *   
 *                      
 * Revision History
 * STR             Date       By    Description
 * --------------------------------------------------------------------------
 *                 06/07/98   tb    Initial coding
 *                 09/03/03   df    C# revision
 *                 01/13/05   tb    changes for Series 11 MGRAs
 *                 12/17/10   tb    changes for Final Series 12 with PILA server
 *                 07/23/12   tb    changes for SR13
 *                 
 * --------------------------------------------------------------------------
 */

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;


namespace Sandag.TechSvcs.RegionalModels
{
  public class Pasef : System.Windows.Forms.Form
  {
    #region Fields

    private Configuration config;
    private KeyValueConfigurationCollection appSettings;
    private ConnectionStringSettingsCollection connectionStrings;

    // Constant instance variables
    public int MAX_CHAR;      // Ethnicity, sex, age combo
    public int MAX_AGE_GROUPS;       /* Max number of columns in update calls */
    
    public int MAX_CTS_IN_SRA;       // Max number of CTs in any SRA
    public int NUM_AGE;        // Number of five-year age groups
    public int NUM_CTS;        // Number of actual CTs
    public int NUM_MGRAS;    // Number of MGRAs

    public int NUM_HHXS;     // number of hh by size categories
    public int NUM_HHWORKERS;      // number of hh by workers categories
    public int MAX_MGRAS_IN_CTS;  //maximum number of mgras in any ct

    public int NUM_ETH;         /* Number of ethnic groups- 0 stores total */
    public int NUM_SEX;         /* Number of sex groups- 0 stores total */
    
    public int NUM_SRA;        // Number of actual SRAs
    public int NUM_MIX;

    public TABLENAMES TN = new TABLENAMES();
    public string networkPath;

    private int ct, MGRA, sra, year,num_special, scenarioID, pyear;
    private int eth, sex, age, popa;
    private int spopa, nspopa;
    
    private StreamWriter swSRA, swCT;
    
    private int[] ctList, ctCounts, masterVector, MGRAIndex;
    private int[,] MGRAIDs, MGRAPop;
    private int[,,,] masterESA;   
      // List of SRA IDs
    private int[] sraList = { 1, 2, 3, 4, 5, 6, 10, 11, 12, 13, 14, 15, 16, 
                              17, 20, 21, 22, 30, 31, 32, 33, 34, 35, 36, 37, 
                              38, 39, 40, 41, 42, 43, 50, 51, 52, 53, 54, 55, 
                              60, 61, 62, 63 };
    private Master[] c, s, m;
    private Master reg;

    // Special pop definitions.  Some of these are new for series 10.
    private static SpecialMaster[] specialCT = new SpecialMaster[40]; 
        
    // Intermediate calculations filenames
    private string sra_debug;
    private string ct_debug;
    private string sra_ascii,ct_ascii,mgra_ascii,mgra_tab_ascii;
    private string mgra_debug;
    private System.Windows.Forms.Label lblSelectScenario;
    private System.Windows.Forms.Label lblSelectYr;
    private System.Data.SqlClient.SqlConnection sqlConnection;
    private System.Data.SqlClient.SqlCommand sqlCommand;
    private System.Windows.Forms.Button btnRun;
    private System.Windows.Forms.TextBox txtStatus;
    private System.Windows.Forms.ComboBox cboYear;
    private System.Windows.Forms.Button btnExit;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label1;
    private CheckBox doCharacteristics;
    private CheckBox doDetailedHH;
    private CheckBox doEnroll;
    private System.Windows.Forms.ComboBox cboScenarioID;

    #endregion Fields
    //private IContainer components;

    private delegate void WriteDelegate( string str );
    // Constructor
	public Pasef()
	{
      InitializeComponent();
      writeToStatusBox( "Awaiting user input..." );
    }

		#region Windows Form Designer generated code
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		
  
    /// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Pasef));
            this.lblSelectScenario = new System.Windows.Forms.Label();
            this.lblSelectYr = new System.Windows.Forms.Label();
            this.sqlConnection = new System.Data.SqlClient.SqlConnection();
            this.sqlCommand = new System.Data.SqlClient.SqlCommand();
            this.txtStatus = new System.Windows.Forms.TextBox();
            this.btnExit = new System.Windows.Forms.Button();
            this.btnRun = new System.Windows.Forms.Button();
            this.cboYear = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.cboScenarioID = new System.Windows.Forms.ComboBox();
            this.doCharacteristics = new System.Windows.Forms.CheckBox();
            this.doDetailedHH = new System.Windows.Forms.CheckBox();
            this.doEnroll = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // lblSelectScenario
            // 
            this.lblSelectScenario.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSelectScenario.Location = new System.Drawing.Point(160, 96);
            this.lblSelectScenario.Name = "lblSelectScenario";
            this.lblSelectScenario.Size = new System.Drawing.Size(128, 16);
            this.lblSelectScenario.TabIndex = 100;
            this.lblSelectScenario.Text = "Scenario";
            // 
            // lblSelectYr
            // 
            this.lblSelectYr.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSelectYr.Location = new System.Drawing.Point(32, 96);
            this.lblSelectYr.Name = "lblSelectYr";
            this.lblSelectYr.Size = new System.Drawing.Size(120, 16);
            this.lblSelectYr.TabIndex = 25;
            this.lblSelectYr.Text = "Forecast Year";
            // 
            // sqlConnection
            // 
            this.sqlConnection.ConnectionString = "workstation id=TBE;packet size=4096;user = forecast;password = forecast;data sour" +
    "ce=PILA\\SdgIntDb;persist security info=False;initial catalog=sr13";
            this.sqlConnection.FireInfoMessageEventOnUserErrors = false;
            this.sqlConnection.InfoMessage += new System.Data.SqlClient.SqlInfoMessageEventHandler(this.sqlConnection_InfoMessage);
            // 
            // sqlCommand
            // 
            this.sqlCommand.Connection = this.sqlConnection;
            // 
            // txtStatus
            // 
            this.txtStatus.Location = new System.Drawing.Point(28, 341);
            this.txtStatus.Multiline = true;
            this.txtStatus.Name = "txtStatus";
            this.txtStatus.ReadOnly = true;
            this.txtStatus.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtStatus.Size = new System.Drawing.Size(360, 72);
            this.txtStatus.TabIndex = 28;
            this.txtStatus.TabStop = false;
            // 
            // btnExit
            // 
            this.btnExit.BackColor = System.Drawing.Color.Red;
            this.btnExit.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnExit.Location = new System.Drawing.Point(182, 433);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(96, 40);
            this.btnExit.TabIndex = 8;
            this.btnExit.Text = "Exit";
            this.btnExit.UseVisualStyleBackColor = false;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // btnRun
            // 
            this.btnRun.BackColor = System.Drawing.Color.LightGreen;
            this.btnRun.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRun.Location = new System.Drawing.Point(62, 433);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(96, 40);
            this.btnRun.TabIndex = 7;
            this.btnRun.Text = "Run";
            this.btnRun.UseVisualStyleBackColor = false;
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
            // 
            // cboYear
            // 
            this.cboYear.Items.AddRange(new object[] {
            "2020",
            "2035",
            "2050"});
            this.cboYear.Location = new System.Drawing.Point(32, 120);
            this.cboYear.Name = "cboYear";
            this.cboYear.Size = new System.Drawing.Size(64, 21);
            this.cboYear.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Garamond", 21.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.Navy;
            this.label2.Location = new System.Drawing.Point(8, 8);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(315, 32);
            this.label2.TabIndex = 101;
            this.label2.Text = "PASEF SR13";
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(16, 56);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(366, 23);
            this.label1.TabIndex = 102;
            this.label1.Text = "Forecast Demographic Characteristics";
            // 
            // cboScenarioID
            // 
            this.cboScenarioID.ItemHeight = 13;
            this.cboScenarioID.Items.AddRange(new object[] {
            "0 - EP"});
            this.cboScenarioID.Location = new System.Drawing.Point(160, 120);
            this.cboScenarioID.Name = "cboScenarioID";
            this.cboScenarioID.Size = new System.Drawing.Size(72, 21);
            this.cboScenarioID.TabIndex = 104;
            // 
            // doCharacteristics
            // 
            this.doCharacteristics.AutoSize = true;
            this.doCharacteristics.Checked = true;
            this.doCharacteristics.CheckState = System.Windows.Forms.CheckState.Checked;
            this.doCharacteristics.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.doCharacteristics.Location = new System.Drawing.Point(28, 160);
            this.doCharacteristics.Name = "doCharacteristics";
            this.doCharacteristics.Size = new System.Drawing.Size(217, 24);
            this.doCharacteristics.TabIndex = 105;
            this.doCharacteristics.Text = "Do Pop Characteristics ";
            this.doCharacteristics.UseVisualStyleBackColor = true;
            // 
            // doDetailedHH
            // 
            this.doDetailedHH.AutoSize = true;
            this.doDetailedHH.Checked = true;
            this.doDetailedHH.CheckState = System.Windows.Forms.CheckState.Checked;
            this.doDetailedHH.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.doDetailedHH.Location = new System.Drawing.Point(28, 204);
            this.doDetailedHH.Name = "doDetailedHH";
            this.doDetailedHH.Size = new System.Drawing.Size(164, 24);
            this.doDetailedHH.TabIndex = 106;
            this.doDetailedHH.Text = "Do Detailed HH  ";
            this.doDetailedHH.UseVisualStyleBackColor = true;
            // 
            // doEnroll
            // 
            this.doEnroll.AutoSize = true;
            this.doEnroll.Checked = true;
            this.doEnroll.CheckState = System.Windows.Forms.CheckState.Checked;
            this.doEnroll.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.doEnroll.Location = new System.Drawing.Point(28, 251);
            this.doEnroll.Name = "doEnroll";
            this.doEnroll.Size = new System.Drawing.Size(147, 24);
            this.doEnroll.TabIndex = 107;
            this.doEnroll.Text = "Do Enrollment ";
            this.doEnroll.UseVisualStyleBackColor = true;
            // 
            // Pasef
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(394, 495);
            this.Controls.Add(this.doEnroll);
            this.Controls.Add(this.doDetailedHH);
            this.Controls.Add(this.doCharacteristics);
            this.Controls.Add(this.cboScenarioID);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cboYear);
            this.Controls.Add(this.btnRun);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.txtStatus);
            this.Controls.Add(this.lblSelectYr);
            this.Controls.Add(this.lblSelectScenario);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Pasef";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Pasef Version 7 SR13 ";
            this.Load += new System.EventHandler(this.Pasef_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

    }
		#endregion
		[STAThread]


    /*****************************************************************************/
    /* method Main() */
    /// <summary>
    /// The main entry point for the application
    /// </summary>
      
    /* Revision History
    * 
    * STR             Date       By    Description
    * --------------------------------------------------------------------------
    *                 06/07/98   tb    Initial coding
    *                 03/06/02   tb    Changes for series 10 data sets
    *                 09/03/03   df    C# revision
    * --------------------------------------------------------------------------
    */
	  static void Main() 
	  {
		  Application.Run( new Pasef() );
	  }

    /*****************************************************************************/

    /* method btnOK_Click() */
    /// <summary>
    /// Method to begin processing form entry.
    /// </summary>
  
    /* Revision History
    * 
    * STR             Date       By    Description
    * --------------------------------------------------------------------------
    *                 09/03/03   df    Initial coding
    * --------------------------------------------------------------------------
    */
    private void btnRun_Click( object sender, System.EventArgs e )
    {
      MethodInvoker mi = new MethodInvoker( doPasefWork );
      if( !processParams(ref year, ref scenarioID) )
          return;
      mi.BeginInvoke( null, null );
    }

    /*****************************************************************************/

    /* method doPasefWork() */
    /// <summary>
    /// Method to begin processing form entry.
    /// </summary>
      
    /* Revision History
    * 
    * STR             Date       By    Description
    * --------------------------------------------------------------------------
    *                 09/03/03   df    C# revision
    * --------------------------------------------------------------------------
    */
    private void doPasefWork()
    {
       
      try
      {
        config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        appSettings = config.AppSettings.Settings;
        connectionStrings = config.ConnectionStrings.ConnectionStrings;

        MAX_CHAR = int.Parse(appSettings["MAX_CHAR"].Value);      // Ethnicity, sex, age combo
        MAX_AGE_GROUPS = int.Parse(appSettings["MAX_AGE_GROUPS"].Value);       /* Max number of columns in update calls */
        MAX_MGRAS_IN_CTS = int.Parse(appSettings["MAX_MGRAS_IN_CT"].Value);      // Max MGRAs in CT
        MAX_CTS_IN_SRA = int.Parse(appSettings["MAX_CTS_IN_SRA"].Value);      // Max number of CTs in any SRA
        NUM_AGE = int.Parse(appSettings["NUM_AGE"].Value);       // Number of five-year age groups
        NUM_CTS = int.Parse(appSettings["NUM_CTS"].Value);       // Number of actual CTs
        NUM_HHWORKERS = int.Parse(appSettings["NUM_HHWORKERS"].Value);
        NUM_HHXS = int.Parse(appSettings["NUM_HHXS"].Value);
        NUM_MGRAS = int.Parse(appSettings["NUM_MGRAS"].Value);   // Number of MGRAs
        
        NUM_ETH = int.Parse(appSettings["NUM_ETH"].Value);         /* Number of ethnic groups- 0 stores total */
        NUM_SEX = int.Parse(appSettings["NUM_SEX"].Value);         /* Number of sex groups- 0 stores total */
        NUM_SRA = int.Parse(appSettings["NUM_SRA"].Value);       // Number of actual SRAs
        NUM_MIX = (NUM_ETH - 1) * (NUM_SEX - 1);

        // Set the table names.
        TN.basePopTable = String.Format(appSettings["basePopTable"].Value);
        TN.ctTable = String.Format(appSettings["ctTable"].Value);
        TN.sraTable = String.Format(appSettings["sraTable"].Value);
        TN.MGRABASE = String.Format(appSettings["MGRABASE"].Value);
        TN.pasefMGRA = String.Format(appSettings["pasefMGRA"].Value);
        TN.pasefMGRATab = String.Format(appSettings["pasefMGRATab"].Value);
        TN.xref = String.Format(appSettings["xref"].Value);
        TN.defmPop = String.Format(appSettings["defmPop"].Value);
        TN.pasef_update_tab_proc = String.Format(appSettings["pasef_update_tab_proc"].Value);
        TN.special_pop_tracts = String.Format(appSettings["special_pop_tracts"].Value);
        TN.sraShare = String.Format(appSettings["sraShare"].Value);
       
        TN.ct10Kids = String.Format(appSettings["ct10Kids"].Value);
        TN.distributionHHWorkersRegion = String.Format(appSettings["distributionHHWorkersRegion"].Value);
        TN.distributionHHWOC = String.Format(appSettings["distributionHHWOC"].Value);
        TN.errorFactorsHHSCT = String.Format(appSettings["errorFactorsHHSCT"].Value);
        TN.overridesHHSDetail = String.Format(appSettings["overridesHHSDetail"].Value);
        TN.regFcst = String.Format(appSettings["regFcst"].Value);
        TN.pasefHHDetailMGRA = String.Format(appSettings["pasefHHDetailMGRA"].Value);
        TN.pasefHHDetailCT = String.Format(appSettings["pasefHHDetailCT"].Value);

        TN.enrollment = String.Format(appSettings["enrollment"].Value);
      
        // Set output file names
        networkPath = String.Format(appSettings["networkPath"].Value);
        ct_debug = networkPath + String.Format(appSettings["ct_debug"].Value); 
        mgra_debug = networkPath + String.Format(appSettings["mgra_debug"].Value);
        sra_debug = networkPath + String.Format(appSettings["sra_debug"].Value);
        ct_ascii = networkPath + String.Format(appSettings["ct_ascii"].Value);
        mgra_ascii = networkPath + String.Format(appSettings["mgra_ascii"].Value);
        mgra_tab_ascii = networkPath + String.Format(appSettings["mgra_tab_ascii"].Value);
        sra_ascii = networkPath + String.Format(appSettings["sra_ascii"].Value);
        
        swCT = new StreamWriter( new FileStream( ct_debug,FileMode.Create ) );        
        swSRA = new StreamWriter( new FileStream( sra_debug,FileMode.Create ) );
        swCT.AutoFlush = swSRA.AutoFlush = true;

        // Initialize some other variables
        ctList = new int[NUM_CTS];
        ctCounts = new int[NUM_CTS];
        MGRAIndex = new int[NUM_CTS];
        masterESA = new int[NUM_CTS, NUM_ETH, NUM_SEX, NUM_AGE];
        masterVector = new int[MAX_CHAR];
        MGRAIDs = new int[NUM_CTS, MAX_MGRAS_IN_CTS];
        MGRAPop = new int[NUM_CTS, MAX_MGRAS_IN_CTS];
        c = new Master[NUM_CTS];
        s = new Master[NUM_SRA];
        m = new Master[NUM_MGRAS];
        reg = new Master();
      }
      catch( IOException io )
      {
        MessageBox.Show( io.ToString(), "IO Exception" );
      }

      try
      {
          if (doCharacteristics.Checked)
          {
              extractCTList();
              extractSpecialCtList();
              extractControlPop();
              extractDEFMPop();
              extractBasePop();
              extractSRAChgShare();
              derivePop(this, swSRA);

              writeToStatusBox("SRA Controlling Phase 1...");
              PasefUtils.sraBaseControls(this, swSRA, s, reg, year, sraList, sra_ascii, scenarioID);
              loadSRATable();

              for (int i = 0; i < NUM_SRA; i++)
                  PasefUtils.popTotals(this, s[i].estimated.nsPop, s[i].nsPop);

              writeToStatusBox("CT Controlling Phase 1...");
              PasefUtils.ctBaseControls(this, swCT, s, c, sraList, ctList, ct_ascii, scenarioID, year);
              loadCTTable();
              PasefUtils.MGRABaseControls(this, c, m, ctList, MGRAIDs, MGRAPop, masterVector, MGRAIndex, mgra_ascii, mgra_tab_ascii, mgra_debug, scenarioID, year);
              loadMGRATable();
              loadMGRATabTable();
              swCT.Close();
              swSRA.Close();
          } // end if

          if (doEnroll.Checked)
            doEnrollment();
          if (doDetailedHH.Checked)
            doPDHHWork();
          writeToStatusBox("Completed Pasef processing!");
      }
      catch (Exception e)
      {
          MessageBox.Show(e.ToString(), e.GetType().ToString());
      }

    }   // end doPasefWork()

    /*****************************************************************************/

    public void doPDHHWork()
    {
        CTMASTER[] ct = new CTMASTER[NUM_CTS];      //ct data class
        REG reg = new REG();
        int i, j;
        
        reg.hhXs = new int[NUM_HHXS];           // regional hh X size controls
        reg.hhXsAdjusted = new int[NUM_HHXS];   // regional controls adjusted for overrides
        reg.hhwocAdjusted = new int[2];
        reg.hhworkersAdjusted = new int[NUM_HHWORKERS];

        reg.hhwoc = new int[2];                   // regional total hh without children 0 = without, 1 = with
        reg.hhworkers = new int[NUM_HHWORKERS];  // regional total hh with workers by category, 0, 1, 2, 3+
        reg.hhworkersp = new double[NUM_HHWORKERS, NUM_HHWORKERS]; //distribution of workers categories by hh size

        for (i = 0; i < NUM_CTS; ++i)
        {
            ct[i] = new CTMASTER();
            ct[i].m = new MDHH[MAX_MGRAS_IN_CTS];
            for (j = 0; j < MAX_MGRAS_IN_CTS; ++j)
            {
                ct[i].m[j] = new MDHH();
                ct[i].m[j].hhworkers = new int[NUM_HHWORKERS];
                ct[i].m[j].hhwoc = new int[2];
                ct[i].m[j].hhXs = new int[NUM_HHXS];
                ct[i].m[j].hhXsc = new double[NUM_HHXS];                // ct hh x size category computed
                ct[i].m[j].hhXsp = new double[NUM_HHXS];                // mgra hh x size proportions derived with poisson function
                ct[i].m[j].hhXspa = new double[NUM_HHXS];
            }  // end for j

            ct[i].hhworkers = new int[NUM_HHWORKERS];             // mgra hh by workers rounded
            ct[i].hhwoc = new int[2];                                // mgra hh wo children rounded
            ct[i].hhXs = new int[NUM_HHXS];                       // mgra hh x size category rounded
            ct[i].hhXsc = new double[NUM_HHXS];                // ct hh x size category computed
            ct[i].hhXsp = new double[NUM_HHXS];                // mgra hh x size proportions derived with poisson function
            ct[i].hhXspa = new double[NUM_HHXS];
            ct[i].hhXso = new int[NUM_HHXS];    // hhs overrides for special ct;
            ct[i].hhXsop = new double[NUM_HHXS];   // hhs overrides expressed as % of total hh
            ct[i].hhXsef = new double[NUM_HHXS];   // ct level hh x size error factors from base data
            ct[i].hhXs4 = new int[4];             // hh X size summed to 4 categories hhs = 1, hhs = 2, hhs = 3, hhs = 4+
            ct[i].hhwocp = new double[4];     // hh wo children % 4 categories hhs = 1, hhs = 2, hhs = 3, hhs = 4+
            ct[i].hhwocc = new double[4];   // ct hh wo children computed
            ct[i].hhworkersp = new double[NUM_HHWORKERS];
            ct[i].hhworkersc = new double[NUM_HHWORKERS];     // ct hh by workers computed

        }  // end for i

        try
        {
            BuildCTHHDetail(ct,reg);
            writeToStatusBox("COMPLETED POPEST DETAILED HH RUN");

        } // end try

        catch (Exception exc)
        {
            MessageBox.Show(exc.ToString(), exc.GetType().ToString());
            return;
        } // end catch

    } // end doPDHHWork()

    //***************************************************************************

    /* method derivePop() */
    /// <summary>
    /// Method to compute the population variables.
    /// </summary>
      
    /* Revision History
    * 
    * STR             Date       By    Description
    * -------------------------------------------------------------------------
    *                 09/03/99   tb    Initial coding
    *                 03/06/02   tb    Changes for sr10 data sets - base year
    *                                  2000
    *                 09/03/03   df    C# revision
    * -------------------------------------------------------------------------
    */
    private void derivePop(Pasef p, StreamWriter sw)
    {
      int i;
      // Control the special pops.
      
      PasefUtils.controlCTSpop( this,c, swCT, specialCT, num_special );
        
      writeToStatusBox( "Building SRA special and non-special populations." );
      PasefUtils.deriveSRAPop( p, s, c, reg, swSRA, sraList );
      /* derive initial sra totals for sex and ethnicity */
      
      for (i = 0; i < NUM_SRA; ++i)
      {
        PasefUtils.popTotals(p,s[i].estimated.nsPop,s[i].nsPop);
        /* call intermediate print routine with indexes for sra and nspop flag */
        PasefUtils.printCalcAge(p,sw,s[i].nsPop,sraList[i]);
      }     /* end for i */

      /* derive the regional base data totals for sex and ethnicity */
      PasefUtils.popTotals(p,reg.estimated.nsPop,reg.nsPop);
      PasefUtils.popTotals(p,reg.estimated.pop,reg.pop);
      PasefUtils.popTotals(p,reg.estimated.sPop,reg.sPop);

    }   // end derivePop()

    /*****************************************************************************/

    /* method extractBasePop() */
    /// <summary>
    /// Method to extract base population and compute initial CT and SRA 
    /// population distribution.  Also compute special pops and non special pops
    /// initial distributions.
    /// </summary>
      
    /* Revision History
    * 
    * STR             Date       By    Description
    * -------------------------------------------------------------------------
    *                 09/03/99   tb    Initial coding
    *                 09/03/03   df    C# revision
    * -------------------------------------------------------------------------    */
    private void extractBasePop()
    {
      int ctID, sraIndex;

      int iter = 2;
      writeToStatusBox( "Extracting base population..." );
     
      System.Data.SqlClient.SqlDataReader rdr;
      sqlCommand.CommandText = String.Format(appSettings["selectBasePop"].Value, TN.basePopTable, scenarioID, pyear);
        
      try
      {
        sqlConnection.Open();
        rdr = sqlCommand.ExecuteReader();
        while( rdr.Read() )
        {
          iter = 2;
          ct = rdr.GetInt16( iter++ );
          sra = rdr.GetByte( iter++ );
          eth = rdr.GetByte( iter++ );
          sex = rdr.GetByte( iter++ );
          age = rdr.GetByte( iter++ );
          popa = rdr.GetInt32( iter++ );
          spopa = rdr.GetInt32( iter++ );
          nspopa = rdr.GetInt32( iter++);
          ctID = PasefUtils.getIndex( ct, ctList, NUM_CTS );
          
          sraIndex = PasefUtils.getIndex( sra, sraList, NUM_SRA );
          c[ctID].controlIndex = sraIndex;
          c[ctID].pop[eth, sex, age] = popa;
          c[ctID].pop[0,sex,age] += popa;
          c[ctID].pop[eth,0,age] += popa;
          c[ctID].pop[0,0,age] += popa;

          /* If this is a special pop CT, build special pop, add it to
          * SRA and region. */
          if( PasefUtils.inSpecialPop( ct ,specialCT,num_special) )
          {
            // Everything in a CT special pop category
            c[ctID].sPop[eth,sex,age] = spopa;
            c[ctID].sPop[0,sex,age] += spopa;
            c[ctID].sPop[eth,0,age] += spopa;
            c[ctID].sPop[0,0,age] += spopa;
            c[ctID].special = true;     // Mark this CT
          }   // end if

        }   // end while
        rdr.Close();
      }      
      catch( Exception e )
      {
        MessageBox.Show( e.ToString(), e.GetType().ToString() );
      }
      finally
      {
        sqlConnection.Close();
      }
      
    }     // End method extractBasePop()

    /*****************************************************************************/

    /* method extractControlPop() */
    /// <summary>
    /// Method to extract control pop data from MGRABase.
    /// </summary>
      
    /* Revision History
    * 
    * STR             Date       By    Description
    * --------------------------------------------------------------------------
    *                 06/07/99   tb    Initial coding
    *                 03/06/02   tb    Changes for sr10 data set
    *                                  New xref_MGRA_sr10, new ct00
    *                 09/04/03   df    C# revision
    * --------------------------------------------------------------------------
    */
    private void extractControlPop()
    {
      int ctID, sraIndex;
      // -----------------------------------------------------------------------
      writeToStatusBox( "Extracting MGRABase population..." );
     
      for (sraIndex = 0; sraIndex < NUM_SRA; ++sraIndex)
        s[sraIndex] = new Master();

      System.Data.SqlClient.SqlDataReader rdr;
      sqlCommand.CommandText = String.Format(appSettings["selectTotalPop"].Value, TN.MGRABASE, TN.xref, scenarioID, year);
      
      try
      {
        sqlConnection.Open();
        rdr = sqlCommand.ExecuteReader();
        while( rdr.Read() )
        {
          MGRA = rdr.GetInt32( 0 );
          
          m[MGRA-1] = new Master();
          sra = rdr.GetByte( 1 );
          ct = rdr.GetInt32( 2 );
          popa = rdr.GetInt32( 3 );
          sraIndex = PasefUtils.getIndex( sra, sraList, NUM_SRA );
          ctID = PasefUtils.getIndex( ct, ctList, NUM_CTS );
          
          c[ctID].control.popTotal += popa;
          // Is this a special pop CT?
          if( PasefUtils.inSpecialPop( ct ,specialCT,num_special) )
            c[ctID].control.sPopTotal += popa;
      
          s[sraIndex].control.popTotal += popa;
          reg.control.popTotal += popa;
          MGRAIDs[ctID, MGRAIndex[ctID]] = MGRA;
          MGRAPop[ctID, MGRAIndex[ctID]++] = popa;
        }   // end while
            rdr.Close();

      }   // end try
      catch( Exception e )
      {
          MessageBox.Show( e.ToString(), e.GetType().ToString() );
      }  // end catch
      finally
      {
        sqlConnection.Close();
      }

      // Compute control.nsPopTotal as residual from pop and sPop.
      swCT.WriteLine( "CT Controlled Pop" );
      for( int i = 0; i < NUM_CTS; i++ )
      {
        sraIndex = c[i].controlIndex;
        c[i].control.nsPopTotal += c[i].control.popTotal - c[i].control.sPopTotal;
        s[sraIndex].control.sPopTotal += c[i].control.sPopTotal;
        s[sraIndex].control.nsPopTotal += c[i].control.nsPopTotal;
        reg.control.sPopTotal += c[i].control.sPopTotal;
        reg.control.nsPopTotal += c[i].control.nsPopTotal;
        swCT.WriteLine( "{0,6}, {1,7}, {2,7}, {3,7}", c[i].id, c[i].control.popTotal, c[i].control.sPopTotal, c[i].control.nsPopTotal );
        swCT.Flush();
      }   // end for i
           
    }     // End method extractControlPop()

    /*****************************************************************************/

    /* method extractCTList() */
    /// <summary>
    /// Method to extract ctIDs and build list.
    /// </summary>
      
    /* Revision History
    * 
    * STR             Date       By    Description
    * -------------------------------------------------------------------------
    *                 06/07/99   tb    Initial coding
    *                 03/06/02   tb    Changes for sr10 data sets
    *                                  xref poassed as parm for queries, new
    *                                  ct00
    *                 09/04/03   df    C# revision
    * -------------------------------------------------------------------------    */
    private void extractCTList()
    {
      System.Data.SqlClient.SqlDataReader rdr;
      int sraIndex, ctID = 0;
      // --------------------------------------------------------------------

      writeToStatusBox( "Extracting CT list..." );
     
      sqlCommand.CommandText = String.Format(appSettings["selectCTList"].Value, TN.xref);
        
      try
      {
        sqlConnection.Open();
        rdr = sqlCommand.ExecuteReader();      
        /* SQL Data reading loop.  Read records and store into Master data structure array. */
        while( rdr.Read() )
        {
          ct = rdr.GetInt32( 0 );
          sra = rdr.GetByte( 1 );
          //ctList[ctID] = new int();
          ctList[ctID] = ct;
          sraIndex = PasefUtils.getIndex( sra, sraList, NUM_SRA );
          c[ctID] = new Master();
          c[ctID].controlIndex = sraIndex;
          c[ctID++].id = ct;
        }   // end while
        rdr.Close();
      }   // end try

      catch( Exception e )
      {
          MessageBox.Show( e.ToString(), e.GetType().ToString() );
      }
      finally
      {
        sqlConnection.Close();
      }
    }     // End method extractCTList()

    /*****************************************************************************/

    /* method extractDefmPop() */
    /// <summary>
    /// Method to extract defm detailed sex, ethnic data.
    /// </summary>
      
    /* Revision History
    * 
    * STR             Date       By    Description
    * -------------------------------------------------------------------------
    *                 09/03/99   tb    Initial coding
    *                 09/03/03   df    C# revision
    * -------------------------------------------------------------------------    */
    private void extractDEFMPop()
    {
      int i,j,k,yy,age,sc;
      int [] defmData = new int[NUM_MIX];
      // --------------------------------------------------------------------

      writeToStatusBox( "Extracting DEFM detailed population..." );
      
      System.Data.SqlClient.SqlDataReader rdr;
      sqlCommand.CommandText = String.Format(appSettings["selectDEFMPop"].Value, TN.defmPop, scenarioID, year);
     
      try
      {
        for (i = 0; i < NUM_ETH; ++i)
           for (j = 0; j < NUM_SEX; ++j)
              for (k = 0; k < NUM_AGE; ++k)
                 reg.pop[i,j,k] = new int();
                  
        sqlConnection.Open();
        rdr = sqlCommand.ExecuteReader();
        while( rdr.Read() )
        {
          sc = rdr.GetByte(0);
          yy = rdr.GetInt32( 1 );
          age = rdr.GetByte( 2);
          for (i = 0; i < 16; ++i)
          {
              defmData[i] = rdr.GetInt32(i+3);
          }  // end for i
          
          reg.pop[1,1,age] = defmData[0];     /* Hisp M */
          reg.pop[1,2,age] = defmData[1];     /* Hisp F */
          reg.pop[2,1,age] = defmData[2];     /* NHW M */
          reg.pop[2,2,age] = defmData[3];     /* NWH F */
          reg.pop[3,1,age] = defmData[4];     /* NHB M */
          reg.pop[3,2,age] = defmData[5];     /* NHB F */
          reg.pop[4,1,age] = defmData[6];     /* NHi M */
          reg.pop[4,2,age] = defmData[7];     /* NHi F */
          reg.pop[5,1,age] = defmData[8];     /* NHa m */
          reg.pop[5,2,age] = defmData[9];     /* NHa F */
          reg.pop[6,1,age] = defmData[10];     /* NHh m */
          reg.pop[6,2,age] = defmData[11];     /* NHh F */
          reg.pop[7,1,age] = defmData[12];     /* NHo m */
          reg.pop[7,2,age] = defmData[13];     /* NHo F */
          reg.pop[8,1,age] = defmData[14];     /* NH2 m */
          reg.pop[8,2,age] = defmData[15];     /* NH2 F */

        }   // end while
        rdr.Close();
      }     // End try

      catch( Exception e )
      {
        MessageBox.Show( e.ToString(), e.GetType().ToString() );
        Close();
      }
      finally
      {
        sqlConnection.Close();
      }

      // Get regional totals by ethnicity and sex.
      for(k = 0; k < NUM_AGE; k++ )
      {
        for(j = 1; j < NUM_ETH; j++ )
        {
          for(i = 1; i < NUM_SEX; i++ )
          {
            reg.pop[0,i,k] += reg.pop[j,i,k];
            reg.pop[j,0,k] += reg.pop[j,i,k];
            reg.pop[0,0,k] += reg.pop[j,i,k];
          }   // end for i
        }   // end for j
      }   // end for k
    }     // End method extractDefmPop()

    // **************************************************************************

    /* extractSpecialCTList() */
    /// <summary>
    /// Populate  special pop array
    /// </summary>
    /// <param name="ct_list">Census Tract/SRA lookup table</param>
    //	Revision History
    //   Date       By   Description
    //   ------------------------------------------------------------------
    //   09/16/03   tb   started initial coding
    //   ------------------------------------------------------------------

    public void extractSpecialCtList()
    {
      System.Data.SqlClient.SqlDataReader rdr;
      
      //-------------------------------------------------------------------------
	    num_special = 0;
          
      writeToStatusBox("EXTRACTING SPECIAL TRACT LIST AND PARAMETERS");
      sqlCommand.CommandText = String.Format(appSettings["selectSimple"].Value, TN.special_pop_tracts);
     
      try
      {
        this.sqlConnection.Open();	
        rdr = this.sqlCommand.ExecuteReader();
        while (rdr.Read())
        {
          specialCT[num_special] = new SpecialMaster();
          specialCT[num_special].ctID = rdr.GetInt32(0);
          specialCT[num_special].baseCode = (int)rdr.GetByte(1);
          specialCT[num_special++].sraID = (int)rdr.GetByte(2);
        }     // end while
        rdr.Close();
      }     // end try
      catch (Exception e)
      {
        MessageBox.Show(e.ToString(),e.GetType().ToString());
      }   // end catch
      finally
      {
        sqlConnection.Close();
      }
    }     //end extractSpecialCtList()

    /*****************************************************************************/

    /* method extractSRAChgShare() */
    /// <summary>
    /// Method to extract SRA ethnic sex share changes.
    /// </summary>
      
    /* Revision History
    * 
    * STR             Date       By    Description
    * -------------------------------------------------------------------------
    *                 09/16/99   tb    Initial coding
    *                 09/03/03   df    C# revision
    * -------------------------------------------------------------------------
    */
    private void extractSRAChgShare()
    {
      int sraIndex,i;
      double [] share = new Double[NUM_MIX];
      // ---------------------------------------------------------------------
      // for series 10 final, the sra share changes are for 10 years - we need to 1/2 them
      // for our 5-year increments
      writeToStatusBox( "Extracting SRA share changes..." );
     
      System.Data.SqlClient.SqlDataReader rdr;
      sqlCommand.CommandText = String.Format(appSettings["selectSimple"].Value, TN.sraShare);
     
      try
      { 
        sqlConnection.Open();
        rdr = sqlCommand.ExecuteReader();
        while( rdr.Read() )
        {
          sra = rdr.GetByte( 0 );
          sraIndex = PasefUtils.getIndex( sra, sraList, NUM_SRA );
          s[sraIndex].chgShare = new double[9,3];
          for (i = 0;i < NUM_MIX; ++i)
              share[i] = rdr.GetDouble(i+1)/2;
          for (i = 0;i < 8; ++i)
          {
            s[sraIndex].chgShare[i+1,1] = share[i*2];
            s[sraIndex].chgShare[i+1,2] = share[i*2+1];
          }     // end for i

        }   // end while
        rdr.Close();
      }   // end try
      catch( Exception e )
      {
        MessageBox.Show( e.ToString(), e.GetType().ToString() );
      }
      finally
      {
        sqlConnection.Close();
      }
    }     // End method extractSRAChgShare()

    /*****************************************************************************/

    /* method loadCTTable() */
    /// <summary>
    /// Method to load the intermediate debug table.
    /// </summary>
      
    /* Revision History
    * 
    * STR             Date       By    Description
    * -------------------------------------------------------------------------
    *                 08/26/99   tb    Initial coding
    *                 09/03/03   df    C# revision
    * ------------------------------------------------------------------------- 
    */
    private void loadCTTable()
    {
      writeToStatusBox( "Deleting from " + TN.ctTable + "..." );

      sqlCommand.CommandText = String.Format(appSettings["deleteFrom"].Value, TN.ctTable, scenarioID, year);
      
      try
      { 
        sqlConnection.Open();
        sqlCommand.ExecuteNonQuery(); 
      }
      catch( SqlException s )
      {
        MessageBox.Show( s.ToString(), s.GetType().ToString() );
      }
      finally
      {
        sqlConnection.Close();
      }
      
      writeToStatusBox( "Loading " + TN.ctTable + "..." );   
      sqlCommand.CommandText = String.Format(appSettings["bulkInsert"].Value,TN.ctTable,ct_ascii);
     
      try
      {
        sqlConnection.Open();
        sqlCommand.ExecuteNonQuery(); 
      }
      catch( SqlException s )
      {
        MessageBox.Show( s.ToString(), s.GetType().ToString() );
        Close();
      }
      finally
      {
        sqlConnection.Close();
      }
    }     // End method loadCTTable()

    /*****************************************************************************/

    /* method loadMGRATable() */
    /// <summary>
    /// Method to load the final MGRA table.
    /// </summary>
      
    /* Revision History
    * 
    * STR             Date       By    Description
    * -------------------------------------------------------------------------
    *                 09/02/99   tb    Initial coding
    *                 09/08/03   df    C# revision
    * ------------------------------------------------------------------------- 
    */
    private void loadMGRATable()
    {
      writeToStatusBox( "Deleting from table " + TN.pasefMGRA );
      sqlCommand.CommandText = String.Format(appSettings["deleteFrom"].Value, TN.pasefMGRA, scenarioID, year);
     
      try
      {
        sqlConnection.Open();
        sqlCommand.ExecuteNonQuery(); 
      }
      catch(Exception e )
      {
        MessageBox.Show( e.ToString(), e.GetType().ToString());
      }
      finally
      {
        sqlConnection.Close();
      }
      writeToStatusBox( "Bulk loading " + TN.pasefMGRA );
      sqlCommand.CommandTimeout = 240;
      sqlCommand.CommandText = String.Format(appSettings["bulkInsert"].Value, TN.pasefMGRA, mgra_ascii);
     
      try
      {
        sqlConnection.Open();
        sqlCommand.ExecuteNonQuery(); 
      }
      catch(Exception e )
      {
        MessageBox.Show( e.ToString(), e.GetType().ToString());
      }
      finally
      {
        sqlConnection.Close();
      }
    }     // End method loadMGRATable()

    /*****************************************************************************/

    /* method loadMGRATabTable() */
    /// <summary>
    /// Method to load the final MGRA tab table.
    /// </summary>
      
    /* Revision History
    * 
    * STR             Date       By    Description
    * -------------------------------------------------------------------------
    *                 09/03/99   tb    Initial coding
    *                 09/08/03   df    C# revision
    * ------------------------------------------------------------------------- 
    */
    private void loadMGRATabTable()
    {
      writeToStatusBox( "Deleting from " + TN.pasefMGRATab );
      sqlCommand.CommandText = String.Format(appSettings["deleteFrom"].Value, TN.pasefMGRATab,scenarioID,year);
     
      try
      {
        sqlConnection.Open();
        sqlCommand.ExecuteNonQuery(); 
      }
      catch( Exception e )
      {
        MessageBox.Show( e.ToString(), e.GetType().ToString() );

      }
      finally
      {
        sqlConnection.Close();
      }

      writeToStatusBox( "Bulk loading " + TN.pasefMGRATab );
      sqlCommand.CommandText = String.Format(appSettings["bulkInsert"].Value, TN.pasefMGRATab, mgra_tab_ascii);
    
      try
      {
        sqlConnection.Open();
        sqlCommand.ExecuteNonQuery(); 
      }
      catch( Exception e )
      {
        MessageBox.Show( e.ToString(), e.GetType().ToString() );
      }
      finally
      {
        sqlConnection.Close();
      }

         
      writeToStatusBox( "updating totals on " + TN.pasefMGRATab );
      sqlCommand.CommandText = "execute " + TN.pasef_update_tab_proc + " " + scenarioID + "," + year;
      try
      {
        sqlConnection.Open();
        sqlCommand.ExecuteNonQuery(); 
      }
      catch(Exception e )
      {
        MessageBox.Show( e.ToString(), e.GetType().ToString());
      }
      finally
      {
        sqlConnection.Close();
      }
     
    }     // End method loadMGRATabTable()

    /*****************************************************************************/

    /* method loadSRATable() */
    /// <summary>
    /// Method to load the intermediate SRA table.
    /// </summary>
      
    /* Revision History
    * 
    * STR             Date       By    Description
    * -------------------------------------------------------------------------
    *                 09/08/99   tb    Initial coding
    *                 09/03/03   df    C# revision
    * ------------------------------------------------------------------------- 
    */
    private void loadSRATable()
    {
      writeToStatusBox( "Deleting from " + TN.sraTable );
      sqlCommand.CommandText = String.Format(appSettings["deleteFrom"].Value, TN.sraTable,scenarioID,year);
     
      try
      {
        sqlConnection.Open();
        sqlCommand.ExecuteNonQuery(); 
      }
      catch( SqlException s )
      {
          MessageBox.Show( s.ToString(), "SQL Exception" );
      }
      finally
      {
        sqlConnection.Close();
      }

      writeToStatusBox( "Loading SRA table..." );
      sqlCommand.CommandText = String.Format(appSettings["bulkInsert"].Value, TN.sraTable, sra_ascii);
      
      try
      {
        sqlConnection.Open();
        sqlCommand.ExecuteNonQuery(); 
      }
      catch( SqlException s )
      {
        MessageBox.Show( s.ToString(), "SQL Exception" );
      }
      finally
      {
        sqlConnection.Close();
      }

      writeToStatusBox( "Updating " + TN.sraTable );
      sqlCommand.CommandText = String.Format(appSettings["updateSRATAble"].Value, TN.sraTable, scenarioID, year);
      
      try
      {
        sqlConnection.Open();
        sqlCommand.ExecuteNonQuery();
      }
      catch (SqlException s)
      {
        MessageBox.Show(s.ToString(), "SQL Exception");

      }
      finally
      {
        sqlConnection.Close();
      }
    }     // End method loadSRATable()

    /*****************************************************************************/

    /* method processParams() */
    /// <summary>
    /// Method to process form input parameters and build table names.
    /// </summary>
      
    /* Revision History
    * 
    * STR             Date       By    Description
    * -------------------------------------------------------------------------
    *                 05/07/97   tb    Initial coding
    *                 09/03/03   df    C# revision
    * -------------------------------------------------------------------------
    */
    private bool processParams(ref int year, ref int scenarioID)
    {
      int incrementSwitch, indexer;
      int[] years = {2020, 2035, 2050};
      int[] incrementSwitches = {8,15,15};

      // ----------------------------------------------------------------------
      indexer = cboYear.SelectedIndex;
      year = years[indexer];
      incrementSwitch = incrementSwitches[indexer];
      pyear = year - incrementSwitch;
      
      scenarioID = cboScenarioID.SelectedIndex;              

      return true;
    }     // End method processParams()

    /*****************************************************************************/

    /* method writeToStatusBox() */
    /// <summary>
    /// Method to append a string on a new line to the status box.
    /// </summary>
    
    /* Revision History
    * 
    * STR             Date       By    Description
    * --------------------------------------------------------------------------
    *                07/28/03   df     Initial coding
    * --------------------------------------------------------------------------
    */
    public void writeToStatusBox( string status )
    {
      /* If we are running this method from primary thread, no marshalling is
          * needed. */
      if( !txtStatus.InvokeRequired )
      {
        // Append to the string (whose last character is already newLine)
        txtStatus.Text += status + Environment.NewLine;
        // Move the caret to the end of the string, and then scroll to it.
        txtStatus.Select( txtStatus.Text.Length, 0 );
        txtStatus.ScrollToCaret();
        Refresh();
      }   // end if
      /* Invoked from another thread.  Show progress asynchronously via a new delegate. */
      else
      {
        WriteDelegate write = new WriteDelegate( writeToStatusBox );
        Invoke( write, new object[] {status} );
      }   // end else
    }     // End method writeToStatusBox()

    // ****************************************************************************


    private void btnExit_Click(object sender, System.EventArgs e)
    {
      Close();
    }

    private void menuItem2_Click(object sender, System.EventArgs e)
    {
      Close();
    }

    private void Pasef_Load(object sender, System.EventArgs e)
    {
    
    }

    private void sqlConnection_InfoMessage(object sender, SqlInfoMessageEventArgs e)
    {

    }

    #region CTHHDetail

    //procedures
     /*             AdjustRegionwideDistribution()
     *              AdjustSeedHHS()
     *              AdjustSeedHHSMGRA()
     *              AllocateTOMGRAS()
     *              BuildCTHHDetail()
     *              BuildSeed()
     *              BuildSeedMGRA()
     *              ControlToLocal()
     *              DescendingSortMulti()
     *              DOFinishChecks()
      *             ExtractCTHHData()
     */
    //-----------------------------------------------------------------------------------------------

    // AdjustRegionwideDistribution()
    //  Adjust the estimates for regionwide distributions
    //      1.  Derive regionwide factors 
    //      2.  Apply regionwide factors
    //      3.  reset old_rowtotals
    //      4.  Get row and col tots
    //      5.  compute diffratio for each ct

    //   Revision History
    //   Date       By   Description
    //   ------------------------------------------------------------------

    //   06/08/14   tbe  New procedure for deriving CT detailed HH data
    //   ------------------------------------------------------------------

    public bool AdjustRegionwideDistribution(int modelswitch, StreamWriter foutw, REG reg, CTMASTER[] ct, int[] rowtotals,
                             int[] old_rowtotals, int[] coltotals, double[] rowdiffratio, int dimj)
    {
        string str = "";
        int i;
        int j;
        int[] passer = new int[dimj];
        bool doanother = false;

        double[] regfac = new double[dimj];
        // --------------------------------------------------------------------------------------------

        foutw.WriteLine("ADJUST FOR REGIONWIDE DISTRIBUTION");
        str = "REGIONWIDE FACTORS ,";
        // adjust for regionwide distributions

        // derive regionwide factor for indicated variable (modelswitch)
        // 1 - HHS regfac[j] = reg.hhXsAdjusted[j]/ coltotals[j]
        // 2 - HHWOC regfac[j] = reg.hhwoc[j]/coltotals[j]
        // 3 - HHWORKERS regfac[j] = reg.hhworkers[j]/coltotals[j]

        for (j = 0; j < dimj; ++j)
        {
            regfac[j] = 0;
            if (coltotals[j] > 0)
            {
                if (modelswitch == 1)
                    regfac[j] = (double)reg.hhXs[j] / (double)coltotals[j];
                else if (modelswitch == 2)
                    regfac[j] = (double)reg.hhwoc[j] / (double)coltotals[j];
                else
                    regfac[j] = (double)reg.hhworkers[j] / (double)coltotals[j];
            } // end if

            coltotals[j] = 0;     // reset col controls
            str += regfac[j] + ",";
        }  // end for j

        foutw.WriteLine(str);

        // apply regionwide factors to each cell
        for (i = 0; i < NUM_CTS; ++i)
        {
            if (ct[i].HHSovr)
                continue;
            Array.Clear(passer, 0, passer.Length);
            rowdiffratio[i] = 0;
            rowtotals[i] = 0;
            str = i + "," + ct[i].ctid + ",";
            for (j = 0; j < dimj; ++j)
            {
                if (modelswitch == 1)
                {
                    ct[i].hhXsc[j] = (double)ct[i].hhXs[j] * regfac[j];
                    ct[i].hhXs[j] = (int)(ct[i].hhXsc[j] + .5);
                    rowtotals[i] += ct[i].hhXs[j];
                    coltotals[j] += ct[i].hhXs[j];
                    passer[j] = ct[i].hhXs[j];

                }  // end if
                else if (modelswitch == 2)
                {
                    ct[i].hhwocc[j] = (double)ct[i].hhwoc[j] * regfac[j];
                    ct[i].hhwoc[j] = (int)(ct[i].hhwocc[j] + .5);
                    rowtotals[i] += ct[i].hhwoc[j];
                    coltotals[j] += ct[i].hhwoc[j];
                    passer[j] = ct[i].hhwoc[j];
                }   // end else if
                else
                {
                    ct[i].hhworkersc[j] = (double)ct[i].hhworkersc[j] * regfac[j];
                    ct[i].hhworkers[j] = (int)(ct[i].hhworkersc[j] + .5);
                    rowtotals[i] += ct[i].hhworkers[j];
                    coltotals[j] += ct[i].hhworkers[j];
                    passer[j] = ct[i].hhworkers[j];
                }   // end else

            }   // end for j

            // derive row diff ratio
            if (rowtotals[i] > 0)
                rowdiffratio[i] = (double)old_rowtotals[i] / (double)rowtotals[i];
            else
                rowdiffratio[i] = 0;
            doanother = false;
            if (rowdiffratio[i] < .99 || rowdiffratio[i] > 1.01)
                doanother = true;

            old_rowtotals[i] = rowtotals[i];
            WriteCTData(foutw, ct[i].ctid, passer, rowtotals[i], rowdiffratio[i], i, dimj);
        }   // end for i

        str = "COLUMN TOTALS,";
        for (j = 0; j < dimj; ++j)
        {
            str += coltotals[j] + ",";
            coltotals[j] = 0;          // zero col controls
        }   // end for j

        foutw.WriteLine(str);
        foutw.WriteLine("END OF ADJUST FOR REGIONWIDE DISTRIBUTION");
        foutw.Flush();
        return doanother;

    }  // end procedure AdjustRegionwideDistribution()

    //******************************************************************************************************

    // AdjustSeedHHS()
    //  Adjust the estimates for hhp 

    //   Revision History
    //   Date       By   Description
    //   ------------------------------------------------------------------

    //   07/08/11   tbe  New procedure for deriving CT detailed HH data
    //   ------------------------------------------------------------------

    public void AdjustSeedHHS(int[] passer, int hhp, int hh, int dimj)
    {
        int[] start = new int[dimj];
        int[] adjmax = new int[dimj];
        int j;
        int minimplied = 0, maximplied = 0;
        int subtotal = 0;
        int which = 0;
        int iter = 0;
        //-------------------------------------------------------------------------------------------------------

        for (j = 0; j < dimj - 1; ++j)  // use hhs1 - hhs6 for temp value
        {
            minimplied += (j + 1) * passer[j];
            maximplied += (j + 1) * passer[j];
        }   // end for j

        // add the hhs7 value
        minimplied += dimj * passer[dimj - 1];
        maximplied += 10 * passer[dimj - 1];

        // check actual hhp in range of min and max

        if (hhp < minimplied)
            which = 1;
        else if (hhp > maximplied)
            which = 2;

        switch (which)
        {

            case 1:     // actual hhp is less than min implied - reduce the implied
                for (j = 1; j < dimj; ++j)
                {
                    start[j] = passer[j];
                    adjmax[j] = (int)(.90 * passer[j] + .5);
                }  // end for j
                iter = 0;

                while (minimplied > hhp && iter < 1000)
                {
                    minimplied = 0;
                    subtotal = 0;
                    for (j = 1; j < dimj; ++j)
                    {
                        if (passer[j] > adjmax[j])
                            --passer[j];

                        minimplied += passer[j] * (j + 1);
                        subtotal += passer[j];
                    }  // end for j
                    passer[0] = hh - subtotal;
                    minimplied += passer[0];
                    ++iter;
                }  // end while
                if (iter >= 1000 && minimplied < hhp)
                    MessageBox.Show("AdjSeed didn't converge on minimplied adjustment");

                break;

            case 2:    // actual hhp is greater than the max implied - increase the hhs categories
                iter = 0;
                while (maximplied < hhp && iter < 1000)
                {
                    maximplied = 0;
                    subtotal = 0;
                    for (j = 1; j < dimj; ++j)
                    {
                        ++passer[j];

                        maximplied += passer[j] * (j + 1);
                        subtotal += passer[j];
                    }  // end for j
                    passer[0] = hh - subtotal;
                    maximplied += passer[0];
                    ++iter;
                }  // end while
                if (iter >= 1000 && minimplied < hhp)
                    MessageBox.Show("AdjSeed didn't converge on minimplied adjustment");
                break;
            default:
                return;
        }  // end swich

    }  //  End procedure AdjustSeedHHS()   

    //***********************************************************************************************************

    // AdjustSeedHHSMGRA()
    //  Adjust the estimates for hhp 

    //   Revision History
    //   Date       By   Description
    //   ------------------------------------------------------------------

    //   07/08/11   tbe  New procedure for deriving CT detailed HH data
    //   ------------------------------------------------------------------

    public void AdjustSeedHHSMGRA(int[] passer, int hhp, int hh, int dimj)
    {
        int[] start = new int[dimj];
        int[] adjmax = new int[dimj];
        int[] savepasser = new int[dimj];
        int j;
        int minimplied = 0, maximplied = 0;
        int subtotal = 0;
        int which = 0;
        int iter = 0;
        int urange = 0;
        int lrange = 0;
        //-------------------------------------------------------------------------------------------------------
        lrange = (int)((double)hhp * .95);
        urange = (int)((double)hhp * 1.05);

        for (j = 0; j < dimj; ++j)
            savepasser[j] = passer[j];
        for (j = 0; j < dimj - 1; ++j)  // use hhs1 - hhs6 for temp value
        {
            minimplied += (j + 1) * passer[j];
            maximplied += (j + 1) * passer[j];
        }   // end for j

        // add the hhs7 value
        minimplied += dimj * passer[dimj - 1];
        maximplied += 10 * passer[dimj - 1];

        // check actual hhp in range of min and max

        if (minimplied > urange)
            which = 1;
        else if (maximplied < lrange)
            which = 2;
        else
            which = 0;

        switch (which)
        {

            case 1:     // min implied exceeds 105 % of hhp - reduce the implied
                for (j = 1; j < dimj; ++j)
                {
                    start[j] = passer[j];
                    adjmax[j] = (int)(.9 * passer[j]);
                }  // end for j
                iter = 0;

                int k = 0;
                while (minimplied > urange && iter < 1000)
                {
                    ++k;
                    if (k == 7)
                        k = 0;
                    minimplied = 0;
                    subtotal = 0;
                    if (passer[k] > adjmax[k] && passer[k] > 0)
                        --passer[k];
                    for (j = 1; j < 7; ++j)
                    {
                        minimplied += passer[j] * (j + 1);
                        subtotal += passer[j];
                    }  // end for j
                    passer[0] = hh - subtotal;
                    minimplied += passer[0];
                    ++iter;
                }  // end while
                if (iter >= 1000 && minimplied < hhp)
                    MessageBox.Show("AdjSeed didn't converge on minimplied adjustment");

                break;

            case 2:    // max implied is less than 95% of hhp - increase the hhs categories
                iter = 0;
                k = 0;
                while (maximplied < lrange && iter < 1000)
                {
                    maximplied = 0;
                    // move each bin up 1 and recompute
                    for (j = 0; j < 6; ++j)
                    {
                        if (passer[j] > 0)
                        {
                            --passer[j];
                            ++passer[j + 1];
                        }  // end if

                    }  // end for j

                    for (j = 0; j < 7; ++j)
                    {
                        maximplied += passer[j] * (j + 1);
                    }  // end for j

                    if (passer[0] < 0)
                        MessageBox.Show("In Adjseed case 2, adjustment yields < 0");

                    ++iter;
                }  // end while
                if (iter >= 1000 && minimplied < hhp)
                    MessageBox.Show("AdjSeed didn't converge on minimplied adjustment");
                break;
            default:
                return;
        }  // end swich

    }  //  End procedure AdjustSeedHHSMGRA()   

    //***********************************************************************************************************

    // AllocateToMGRAS()
    // Allocate the popest CT detailed HH data to mgras using Pachinko
    //   Revision History
    //   Date       By   Description
    //   ------------------------------------------------------------------

    //   06/21/11   tbe  New procedure for deriving CT detailed HH data
    //   ------------------------------------------------------------------
    public void AllocateToMGRAS(CTMASTER[] ct, int dimi)
    {
        int i, j, k, mcount, target, ret, pcounter;
        int[] passer = new int[7];     // maximum number of elements to be passed to Pachinko
        int[] master1 = new int[NUM_HHXS];
        int[] savemaster1 = new int[NUM_HHXS];
        int[] master2 = new int[2];
        int[] master3 = new int[NUM_HHWORKERS];
        int[] tmgrahh = new int[MAX_MGRAS_IN_CTS];
        int[,] tmgrahhs = new int[MAX_MGRAS_IN_CTS, 7];
        int[] torig_index = new int[MAX_MGRAS_IN_CTS];
        string str = "", str1 = "";
        FileStream foutm;
        //-----------------------------------------------------------------------------------------
        try
        {
            foutm = new FileStream(networkPath + "pdmgra", FileMode.Create);
        }
        catch (FileNotFoundException exc)
        {
            MessageBox.Show(exc.Message + " Error Opening Output File");
            return;
        }
        //assign a wrapper for writing strings to ascii
        StreamWriter foutmw = new StreamWriter(foutm);

        // seed the mgras for HHXs
        BuildSeedMGRAS(ct);

        for (i = 0; i < dimi; ++i)
        {
            Array.Clear(tmgrahhs, 0, tmgrahhs.Length);
            Array.Clear(tmgrahh, 0, tmgrahh.Length);
            Array.Clear(torig_index, 0, torig_index.Length);
            Array.Clear(savemaster1, 0, savemaster1.Length);

            // number of nmgra in ct[i]
            mcount = ct[i].num_mgras;
            pcounter = 0;
            writeToStatusBox("  Processing CT " + (i + 1).ToString() + " ID = " + ct[i].ctid + " Number of MGRAS = " + mcount);
            str1 = i + "," + ct[i].ctid + ",";
            int tstop = 0;
            if (i == 248)
                tstop = 1;

            // start with hhxs  
            // here is the issue here.  we are doing a 2-dim controlling, but hhp is the third simension that must be considered
            // because the pachinko essentially assigns hh to size categories based on the ct distribution, it is likely that there will be 
            // mgras with implied hhp (hhxs * size) that doesn't closely match hhp coming from popest.
            // therefore we need to do some adjustments
            //  1. for mgras with only 1 hh and there are about 600 of them, we have to assign the hhxs according to the hhp
            //  2. then we have to subtract that assignment from the ct controls and exclude that mgra from the pachinko

            // for each ct, the first adjustment is done here when we build the master1 array
            for (j = 0; j < NUM_HHXS; ++j)
            {
                str1 += ct[i].hhXs[j] + ",";
                master1[j] = ct[i].hhXs[j];
            } // end for j

            // adjust the master1 for mgras with hh = 1
            int hi;
            for (k = 0; k < mcount; ++k)
            {
                if (ct[i].m[k].hh == 1)
                {
                    hi = ct[i].m[k].hhp - 1;
                    if (hi >= 7)
                        hi = 6;
                    if (hi >= 0)
                    {
                        ct[i].m[k].hhXs[hi] = 1;
                        if (master1[hi] > 0)
                            master1[hi] -= 1;
                        ct[i].m[k].hhis1 = true;
                    }   // end if
                } // end if
            }  // end for k

            // now store the remaining master before we go to pachinko
            for (k = 0; k < 7; ++k)
                savemaster1[k] = master1[k];

            for (j = 0; j < 2; ++j)
            {
                str1 += ct[i].hhwoc[j] + ",";
                master2[j] = ct[i].hhwoc[j];
            }   // end for j

            for (j = 0; j < NUM_HHWORKERS; ++j)
            {
                str1 += ct[i].hhworkers[j] + ",";
                master3[j] = ct[i].hhworkers[j];
            }   // end for j

            //foutmw.WriteLine(str1);
            //foutmw.Flush();

            for (j = 0; j < mcount; ++j)
            {
                Array.Clear(passer, 0, passer.Length);

                if (ct[i].m[j].hh > 0)
                {
                    // start with HHS   - for hhxs we are only going to process mgras where hh > 1

                    if (!ct[i].m[j].hhis1)
                    {

                        // replace the values in the temp mgra array before final update - remember, these are only mgras where hh > 1
                        for (k = 0; k < 7; ++k)
                        {
                            tmgrahhs[pcounter, k] = ct[i].m[j].hhXs[k];
                        }  // end for k
                        torig_index[pcounter] = j;
                        tmgrahh[pcounter++] = ct[i].m[j].hh;  // store the mgra hh and increment the temp counter

                    }  // end if

                    // now do hh wo children
                    Array.Clear(passer, 0, passer.Length);

                    target = ct[i].m[j].hh;
                    ret = PasefUtils.PachinkoWithMasterDecrement(target, master2, passer, 2);
                    if (ret >= 40000)
                    {
                        MessageBox.Show("Pachinko did not resolve in 40000 iterations for CT " + ct[i].ctid.ToString() + " mgra " + ct[i].m[j].mgraid.ToString());
                    }     /* end if */

                    for (k = 0; k < 2; ++k)
                    {
                        ct[i].m[j].hhwoc[k] = passer[k];
                    }   // end for k

                    Array.Clear(passer, 0, passer.Length);

                    // now do workers

                    target = ct[i].m[j].hh;
                    ret = PasefUtils.PachinkoWithMasterDecrement(target, master3, passer, NUM_HHWORKERS);
                    if (ret >= 40000)
                    {
                        MessageBox.Show("Pachinko did not resolve in 40000 iterations for CT " + ct[i].ctid.ToString() + " mgra " + ct[i].m[j].mgraid.ToString());
                    }     /* end if */

                    for (k = 0; k < NUM_HHWORKERS; ++k)
                    {
                        ct[i].m[j].hhworkers[k] = passer[k];
                    }   // end for k               

                }  // end if

            }   // end for j

            // now recontrol the adjusted mgra hhxs data using update
            // tmgrahhs stores the hhs data for the mgras with hh > 1
            // tmgrahh stores the mgra hh totals
            // master1 stores the adjusted ct totals
            // pcounter is the m=number of remaining rows
            if (pcounter > 0)
            {
                PasefUpdate.update(pcounter, 7, tmgrahhs, tmgrahh, savemaster1);

                // at this point the tmgrahhs array should have been readjusted so that the ct hhxs col sums were right and the row sums mgra hh were right
                // replace the revised mgra data back to the ct.m structure
                for (k = 0; k < pcounter; ++k)
                {
                    j = torig_index[k];
                    for (int l = 0; l < 7; ++l)
                        ct[i].m[j].hhXs[l] = tmgrahhs[k, l];
                }   // end for k
            }  // end if

            int hhp_implied;
            // have to do writes down here after final update controlling for hhsx
            for (j = 0; j < mcount; ++j)
            {
                hhp_implied = 0;
                str = scenarioID + "," + year + "," + ct[i].ctid + ",";
                str += ct[i].m[j].mgraid + ",";
                if (ct[i].m[j].hh > 0)
                {

                    for (k = 0; k < NUM_HHXS; ++k)
                    {
                        str += ct[i].m[j].hhXs[k] + ",";
                        hhp_implied += (int)((double)ct[i].m[j].hhXs[k] * (double)(k + 1));
                    }  //end for k
                    if (ct[i].m[j].hh > 0)
                        ct[i].m[j].hhs = (double)hhp_implied / (double)ct[i].m[j].hh;
                    for (k = 0; k < 2; ++k)
                    {
                        str += ct[i].m[j].hhwoc[k] + ",";
                    }   // end for k
                    for (k = 0; k < NUM_HHWORKERS; ++k)
                    {
                        str += ct[i].m[j].hhworkers[k] + ",";
                    }   // end for k
                    str += ct[i].m[j].hh + "," + ct[i].m[j].hhs + "," + hhp_implied;
                }  // end if
                else
                {
                    str += "0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0";
                }  // end else
                foutmw.WriteLine(str);
                foutmw.Flush();
            }  // end for j

        }  // end for i                  
        foutmw.Flush();
        foutmw.Close();

    }// end procedure AllocateTOMGRAS()

    //***************************************************************************************************

    // BuildCTHHDetail()
    // Computes Detailed CT-level data for popsyn
    // extracts hh and hhp from popest_mgra data set, aggregating to CT, computes hhs

    //   Revision History
    //   Date       By   Description
    //   ------------------------------------------------------------------

    //   06/08/11   tbe  New procedure for deriving CT detailed HH data
    //   07/08/11   tbe  added code add overrides for some special, namely mil, tracts and
    //                   exclude them from controlling
    //   ------------------------------------------------------------------

    public void BuildCTHHDetail(CTMASTER[] ct, REG reg)
    {

        int i, j;
        int[,] matrx = new int[NUM_CTS, NUM_HHXS];  //matrix to store 2 dimensional ct, hhs array for finish routines

       
        bool doanother = true;
        int[] rowtotals = new int[NUM_CTS];
        int[] old_rowtotals = new int[NUM_CTS];
        int[] nurowt = new int[NUM_CTS];

        int[] coltotals = new int[NUM_HHXS];

        double[] tempfac = new double[NUM_HHXS], rowdiffratio = new double[NUM_CTS];

        FileStream fout;		//file stream class

        //-----------------------------------------------------------------------
       
        ExtractCTHHData(ct, reg,scenarioID,year);
        // open output file
        try
        {
            fout = new FileStream(networkPath + "pct", FileMode.Create);
        }
        catch (FileNotFoundException exc)
        {
            MessageBox.Show(exc.Message + " Error Opening Output File");
            return;
        }
        //assign a wrapper for writing strings to ascii
        StreamWriter foutw = new StreamWriter(fout);

        // build the seed value for hhXs
        writeToStatusBox("Building Seed for HH Size");
        BuildSeed(1, foutw, reg, ct, rowtotals, old_rowtotals, coltotals, NUM_HHXS);

        // at this point we should have the first rows (CT) derived for the first iteration
        writeToStatusBox("Controlling for HH Size");
        while (doanother)
        {
            doanother = AdjustRegionwideDistribution(1, foutw, reg, ct, rowtotals, old_rowtotals, coltotals, rowdiffratio, NUM_HHXS);

            doanother = ControlToLocal(1, foutw, ct, rowtotals, old_rowtotals, coltotals, rowdiffratio, NUM_HHXS);

        }   // end while

        // logic for finish routines for hhXs
        writeToStatusBox("Finish Routines for HH Size");
        DoFinishChecks(1, foutw, ct, reg, rowtotals, coltotals, NUM_HHXS);

        //---------------------------------------------------------------------------------------------------
        // build the seed value for hhwoc
        writeToStatusBox("Building Seed for HH w kids");
        BuildSeed(2, foutw, reg, ct, rowtotals, old_rowtotals, coltotals, 2);
        // At this point we have the initial cut for the hh wo kids array
        // the next step is to control using pachinko to get the distribution finished, 
        // this pachinko uses an upper bound that constrains the hh w kids*factor = kids

        // get the row and col totals (row totals should be ok)
        Array.Clear(rowtotals, 0, rowtotals.Length);
        Array.Clear(coltotals, 0, coltotals.Length);
        writeToStatusBox("Controlling for HH w Kids");
        for (i = 0; i < NUM_CTS; ++i)
        {
            for (j = 0; j < 2; ++j)
            {
                rowtotals[i] += ct[i].hhwoc[j];
                coltotals[j] += ct[i].hhwoc[j];
            }   // end for j
        }  // end for i

        writeToStatusBox("Finish Routines for HH w Kids");
        DoFinishChecks(2, foutw, ct, reg, rowtotals, coltotals, 2);

        //---------------------------------------------------------------------------------------------------
        // build the seed value for hhworkers
        writeToStatusBox("Building Seed for HH Workers");
        BuildSeed(3, foutw, reg, ct, rowtotals, old_rowtotals, coltotals, NUM_HHWORKERS);

        doanother = true;
        writeToStatusBox("Controlling Seed for HH Workers");
        while (doanother)
        {
            doanother = AdjustRegionwideDistribution(3, foutw, reg, ct, rowtotals, old_rowtotals, coltotals, rowdiffratio, NUM_HHWORKERS);

            doanother = ControlToLocal(3, foutw, ct, rowtotals, old_rowtotals, coltotals, rowdiffratio, NUM_HHWORKERS);

        }   // end while
        writeToStatusBox("Finish Routines for HH Workers");
        DoFinishChecks(3, foutw, ct, reg, rowtotals, coltotals, NUM_HHWORKERS);

        writeToStatusBox("Writing CT data");
        BulkLoadDHH(ct, year);

        // now allocate ct data to mgras
        writeToStatusBox("Allocating to MGRAs");
        AllocateToMGRAS(ct, NUM_CTS);

        writeToStatusBox("Writing MGRA data");
        BulkLoadMGRADHH();

        fout.Flush();
        fout.Close();

    }  // end procedure BuildCTHHDetail()

    //**********************************************************************************************************

    //BuildSeed()
    //  Derives the CT HH  Seed - based on the value of modelswitch
    //  modelswitch = 1 -> hhXs
    //      1.  Apply poisson
    //      2.  Apply error factor
    //      3.  Normalize to 1
    //      4.  Get roww and col tots

    //  modelswitch = 2 -> hhwoc

    //  modelswitch = 3 -> hhworkers

    // parameters
    // modelswitch - decides which cse we are doing
    // foutw - output streamwriter
    // reg - regional control data class
    // ct - CT data class
    // rowtotals - computed row totals CTs
    // coltotals - computed column totals data categories
    // dimj - dimension of data categories
    //        NUM_HHXS    - hhxs categories
    //        2           - hh w-wo categories
    //        NUM_HHWORKERS - hh workers categories

    //   Revision History
    //   Date       By   Description
    //   ------------------------------------------------------------------

    //   06/08/14   tbe  New procedure for deriving CT detailed HH data
    //   ------------------------------------------------------------------

    public void BuildSeed(int modelswitch, StreamWriter foutw, REG reg, CTMASTER[] ct, int[] rowtotals,
                          int[] old_rowtotals, int[] coltotals, int dimj)
    {
        string str = "", strs = "";
        int i;
        int j;
        int grandtotc = 0, grandtotr = 0;
        int[] passer = new int[dimj];
        double[] rowdiffratio = new double[NUM_CTS];
        double lambda;
        double fact, factot, factot1;
        FileStream foutp;
        //-------------------------------------------------------------------------------------------

        // open output file
        try
        {
            foutp = new FileStream(networkPath + "pctp", FileMode.Create);
        }
        catch (FileNotFoundException exc)
        {
            MessageBox.Show(exc.Message + " Error Opening Output File");
            return;
        }
        //assign a wrapper for writing strings to ascii
        StreamWriter foutpw = new StreamWriter(foutp);

        // label this iteration

        if (modelswitch == 1)
            strs = "CT HHXS SEED";
        else if (modelswitch == 2)
            strs = "CT HHWOC SEED";
        else
            strs = "CT HHWORKERS SEED";

        foutw.WriteLine(strs);
        foutw.Flush();

        // if modelswitch == 1 - HHXS
        // Build initial CT hhs by category using ct distributions and poisson formula
        // lambda = hhs -1
        // n = hhs category ( 0 - 6)
        // poisson formula = (lambda^n * exp(-lambda))(n!) 

        // if modelswitch == 2 - HHWOC
        // build initial CT hhwoc by multiplying ct[i].hhXs4[j] * hh wo children factors

        for (i = 0; i < NUM_CTS; ++i)
        {
            str = i + 1 + "," + ct[i].ctid + ",";
            rowtotals[i] = 0;
            Array.Clear(passer, 0, passer.Length);
            Array.Clear(rowdiffratio, 0, rowdiffratio.Length);

            switch (modelswitch)
            {
                case 1:     //hhXs
                    {
                        if (ct[i].hh == 0)
                            break;
                        if (ct[i].HHSovr)       // is this ct overriden - apply overrides % to hh
                        {
                            for (j = 0; j < dimj; ++j)
                            {
                                passer[j] = ct[i].hhXs[j];
                            }  // end for

                        }   // end if
                        else
                        {
                            if (ct[i].hhs == 1)
                                lambda = 1;
                            else if (ct[i].hhs == 0)
                                lambda = 0;
                            else
                                lambda = ct[i].hhs - 1;
                            fact = 1;
                            factot = 0;
                            factot1 = 0;
                            for (j = 0; j < dimj; ++j)
                            {
                                fact *= j;
                                if (fact == 0)
                                    fact = 1;
                                // use the poisson distribution to get the initial ct proportions
                               
                                ct[i].hhXsp[j] = (Math.Pow(lambda, (double)j) * Math.Exp(-lambda)) / fact;
                                str += ct[i].hhXsp[j] + ",";

                                // apply the base year error factors (derivedfrom 2000 Census)
                                ct[i].hhXspa[j] = ct[i].hhXsp[j] * ct[i].hhXsef[j];
                                factot += ct[i].hhXspa[j];
                            }   // end for j
                            foutpw.WriteLine(str);
                            foutpw.Flush();

                            // normalize to 1.0 for distributions
                            for (j = 0; j < dimj - 1; ++j)  // do the first 6
                            {
                                ct[i].hhXspa[j] /= factot;
                                factot1 += ct[i].hhXspa[j];
                            }   // end for j

                            ct[i].hhXspa[dimj - 1] = 1 - factot1; // fill the last as residual
                            if (ct[i].hhXspa[dimj - 1] < 0)   // constrain to 0
                                ct[i].hhXspa[dimj - 1] = 0;

                            for (j = 0; j < dimj; ++j)
                            {
                                // now get the first ct (row) estimate as proportion * hh
                                                            
                                ct[i].hhXsc[j] = ct[i].hhXspa[j] * ct[i].hh;    //derive floating point value
                                ct[i].hhXs[j] = (int)(ct[i].hhXsc[j] + .5);   //round up
                                rowtotals[i] += ct[i].hhXs[j];
                                passer[j] = ct[i].hhXs[j];
                            }   // end for j

                            // at this point we have hhs1 - hhs7 initial seed estimates.  We need to do validity checks on the initial distribution
                            // we'll estimate a minimum and maximum implied hhp from this distribution.  if the estimated minimum is greater than the 
                            // actual hhp, adjust the hhs categories down.  This process has several steps.  the first is to assign some kind of threshold
                            // for reducing the categories.  We'll start with 10%  that is we'll only reduce a category by up to 10% of its starting value
                            // this may change after we review the results.  This precludes emiminating the hh in a category before the implied minimum hhp gets to the
                            // actual hhp.  It's entirely possible that the rounding exercises will undo some of this. The algorithm actually works from high to low,
                            // hhs7 , hhs6, hhs5 etc, reducing by 1.  HHS1 is computed as the residual of HH - Sum(HHS2 - HHs7).  The implied max and min are recomputed
                            // and once the implied min gets under the actual. the process is stopped.
                            // for the minumim we compute min = 1*HHS1 + 2*HHS2 +...+7*HHS7;  the max is developed similarly, except that HHS7 is multiplied by 10 (a suggested
                            // starting point.  the census uses 20

                            // if the actual hhp is higher than the maximum implied, increas the count in each category, starting with HHS2 going to HHS7, computing HHS1 as
                            // a residual as outlined above.

                            // restore the adjusted values
                            AdjustSeedHHS(passer, ct[i].hhp, ct[i].hh, dimj);
                            for (j = 0; j < dimj; ++j)
                            {
                                ct[i].hhXs[j] = passer[j];
                                rowtotals[i] += ct[i].hhXs[j];
                            }   // end for j
                        }   // end else not overriden

                        // sum the hhXs into 4 categories
                        for (j = 0; j < 3; ++j)
                            ct[i].hhXs4[j] = ct[i].hhXs[j];
                        ct[i].hhXs4[3] = ct[i].hhXs[3] + ct[i].hhXs[4] + ct[i].hhXs[5] + ct[i].hhXs[6];

                    }  // end case 1
                    break;

                case 2:
                    {
                        // HHWOC - new methodology coded 08/15/2012
                        // start with # kids from popest bu CT
                        // compute HHWOC as hh1 * hhwocp1 + hh2 * hhwocp2 + hh3 * hhwocp3 + (hh4+hh5+hh6+hh7) * hhwocp4
                        // reset cases where kids = 0  if kids = 0, hhwoc = 0
                        // process overrides hhwoc = round(hh * override%)
                        // set hhwc = hh - hhwoc
                        // if hhwc > kids, hhwc = kids
                        // hhwoc = hh - hhwc
                        // control to region
                        //---------------------------------------------------------------------------------------------------

                        if (ct[i].hh == 0)   // skip all of this if there are no HH
                            break;

                        ct[i].hhwoc[1] = 0;  // 1 element is hh w kids this will be deretmined as a residual
                        ct[i].hhwoc[0] = 0;

                        ct[i].hhwoc[0] = (int)(ct[i].hhXs4[0] + (double)ct[i].hhXs4[1] * ct[i].hhwocp[1] +
                                        (double)ct[i].hhXs4[2] * ct[i].hhwocp[2] + (double)ct[i].hhXs4[3] * ct[i].hhwocp[3]);

                        ct[i].hhwoc[1] = ct[i].hh - ct[i].hhwoc[0];
                        if (ct[i].hhwoc[1] > ct[i].kids)   // constrain hhwc to kids
                        {
                            ct[i].hhwoc[1] = ct[i].kids;   // reset
                            ct[i].hhwoc[0] = ct[i].hh - ct[i].hhwoc[1];  // recompute hhwoc
                        }  // end if

                        for (j = 0; j < dimj; ++j)
                        {
                            rowtotals[i] += ct[i].hhwoc[j];
                            passer[j] = ct[i].hhwoc[j];
                        }  // end for j

                    }  // end case 2
                    break;

                case 3:   //HHWORKERS
                    {
                        if (ct[i].hh == 0)
                            break;
                        for (int k = 0; k < 4; ++k)
                        {

                            for (j = 0; j < dimj; ++j)
                            {
                                // notice we're multiplying the col values (%workers) * rows (HHS)
                                ct[i].hhworkersc[k] = (double)ct[i].hhXs4[j] * reg.hhworkersp[j, k];
                                ct[i].hhworkers[k] = (int)(ct[i].hhworkersc[k] + .5);  //round up

                            }   // end for j 
                            rowtotals[i] += ct[i].hhworkers[k];
                            passer[k] = ct[i].hhworkers[k];
                        }   // end for k
                    }  // end case 3
                    break;

            }  // end switch

            // write these data to ascii

            WriteCTData(foutw, ct[i].ctid, passer, rowtotals[i], rowdiffratio[i], i, dimj);
            grandtotr += rowtotals[i];
            // assign old rowtotals = rowtotals
            old_rowtotals[i] = rowtotals[i];
        }  // end for i
        foutpw.Flush();
        foutpw.Close();

        // get the row and column totals
        // get column totals
        str = "COLUMN TOTALS ";
        for (j = 0; j < dimj; ++j)
        {
            coltotals[j] = 0;
            for (i = 0; i < NUM_CTS; ++i)
            {
                if (modelswitch == 1)
                    coltotals[j] += ct[i].hhXs[j];
                else if (modelswitch == 2)
                    coltotals[j] += ct[i].hhwoc[j];
                else
                    coltotals[j] += ct[i].hhworkers[j];
            }  // end for i

            grandtotc += coltotals[j];
            str += coltotals[j] + ",";
        }   // end for j

        foutw.WriteLine(str);
        if (modelswitch == 1)
            str = "REG HH SIZE CONTROLS, ";
        else if (modelswitch == 2)
            str = "REG HH WO CHILDREN CONTROLS,";
        else
            str = "REG HH WORKERS CONTROLS,";

        for (j = 0; j < dimj; ++j)
        {
            if (modelswitch == 1)
                str += reg.hhXsAdjusted[j] + ",";
            else if (modelswitch == 2)
                str += reg.hhwoc[j] + ",";
            else if (modelswitch == 3)
                str += reg.hhworkers[j] + ",";
        } // end for j;

        foutw.WriteLine(str);

        foutw.WriteLine("END " + strs + " - grandtotc = " + grandtotc + " grandtotr = " + grandtotr);
        foutw.WriteLine("");
        foutw.Flush();

    }  // end procedure BuildSeed()

    //***************************************************************************************************

    //BuildSeedMGRA()
    //  Derives the MGRA HH  Seed - based on the value of modelswitch
    //  modelswitch = 1 -> hhXs
    //      1.  Apply poisson
    //      2.  Apply error factor
    //      3.  Normalize to 1
    //      4.  Get roww and col tots

    // parameters
    // modelswitch - decides which cse we are doing
    // foutw - output streamwriter
    // reg - regional control data class
    // ct - CT data class
    // rowtotals - computed row totals CTs
    // coltotals - computed column totals data categories
    // dimj - dimension of data categories
    //        NUM_HHXS    - hhxs categories
    //        2           - hh w-wo categories
    //        NUM_HHWORKERS - hh workers categories

    //   Revision History
    //   Date       By   Description
    //   ------------------------------------------------------------------

    //   04/01/12   tbe  New procedure for deriving MGRA detailed HHS data
    //   ------------------------------------------------------------------

    public void BuildSeedMGRAS(CTMASTER[] ct)
    {
        int i;
        int j, k;
        int mcount;
        double lambda;
        double fact, factot, factot1;
        //-------------------------------------------------------------------------------------------

        for (i = 0; i < NUM_CTS; ++i)
        {
            mcount = ct[i].num_mgras;
            for (j = 0; j < mcount; ++j)
            {
                if (ct[i].m[j].hh == 0)
                    break;
                if (ct[i].m[j].hhs == 1)
                    lambda = 1;
                else if (ct[i].m[j].hhs == 0)
                    lambda = 0;
                else
                    lambda = ct[i].m[j].hhs - 1;
                fact = 1;
                factot = 0;
                factot1 = 0;
                for (k = 0; k < 7; ++k)
                {
                    fact *= k;
                    if (fact == 0)
                        fact = 1;
                    // use the poisson distribution to get the initial ct proportions
                    try
                    {
                        ct[i].m[j].hhXspa[k] = (Math.Pow(lambda, (double)k) * Math.Exp(-lambda)) / fact;
                    }
                    catch (Exception exc)
                    {
                        MessageBox.Show(exc.ToString(), exc.GetType().ToString());

                    }  // end catch

                    factot += ct[i].m[j].hhXspa[k];
                }   // end for k

                // normalize to 1.0 for distributions
                for (k = 0; k < 6; ++k)  // do the first 6
                {
                    ct[i].m[j].hhXspa[k] /= factot;
                    factot1 += ct[i].m[j].hhXspa[k];
                }   // end for k

                ct[i].m[j].hhXspa[6] = 1 - factot1; // fill the last as residual
                if (ct[i].m[j].hhXspa[6] < 0)   // constrain to 0
                    ct[i].m[j].hhXspa[6] = 0;

                for (k = 0; k < 7; ++k)
                {
                    // now get the first ct (row) estimate as proportion * hh
                    ct[i].m[j].hhXsc[k] = ct[i].m[j].hhXspa[k] * ct[i].m[j].hh;    //derive floating point value
                    ct[i].m[j].hhXs[k] = (int)(ct[i].m[j].hhXsc[k] + .5);   //round up

                }   // end for k

            }  // end for j
        }  // end for i

    }  // end procedure BuildSeedMGRA()

    //***************************************************************************************************

    // Control to Local()
    // 
    //      1.  Apply diffratio
    //      2.  reset old_rowtotals
    //      3.  Get row and col tots

    //   Revision History
    //   Date       By   Description
    //   ------------------------------------------------------------------

    //   06/08/14   tbe  New procedure for deriving CT detailed HH data
    //   ------------------------------------------------------------------

    public bool ControlToLocal(int modelswitch, StreamWriter foutw, CTMASTER[] ct, int[] rowtotals,
                             int[] old_rowtotals, int[] coltotals, double[] rowdiffratio, int dimj)
    {
        int i, j;
        int[] passer = new int[dimj];
        string str = "";
        bool doanother = true;

        // control to local
        foutw.WriteLine("CONTROL TO LOCAL");
        for (i = 0; i < NUM_CTS; ++i)
        {

            str = ct[i].ctid + ",";
            rowtotals[i] = 0;
            if (ct[i].HHSovr)
                continue;
            for (j = 0; j < dimj; ++j)
            {
                if (modelswitch == 1)
                {
                    ct[i].hhXsc[j] *= rowdiffratio[j];
                    ct[i].hhXs[j] = (int)(ct[i].hhXs[j] + .5);
                    rowtotals[i] += ct[i].hhXs[j];
                    coltotals[j] += ct[i].hhXs[j];
                    str += ct[i].hhXs[j] + ",";
                }   // end if
                else if (modelswitch == 2)
                {
                    ct[i].hhwocc[j] *= rowdiffratio[j];
                    ct[i].hhwoc[j] = (int)(ct[i].hhwocc[j] + .5);
                    rowtotals[i] += ct[i].hhwoc[j];
                    coltotals[j] += ct[i].hhwoc[j];
                    str += ct[i].hhwoc[j] + ",";
                } // end else if
                else
                {
                    ct[i].hhworkersc[j] *= rowdiffratio[j];
                    ct[i].hhworkers[j] = (int)(ct[i].hhworkersc[j] + .5);
                    rowtotals[i] += ct[i].hhworkers[j];
                    coltotals[j] += ct[i].hhworkers[j];
                    str += ct[i].hhworkers[j] + ",";
                }
            }  // end for j

            str += rowtotals[i] + ",";

            if (rowtotals[i] > 0)
                rowdiffratio[i] = (double)old_rowtotals[i] / (double)rowtotals[i];
            else
                rowdiffratio[i] = 0;
            doanother = false;

            str += rowdiffratio[i];
            foutw.WriteLine(str);
            old_rowtotals[i] = rowtotals[i];
        }   // end for i

        str = "COLUMN TOTALS" + ",";
        for (j = 0; j < dimj; ++j)
            str += coltotals[j] + ",";
        foutw.WriteLine(str);
        foutw.WriteLine("END OF CONTROL TO LOCAL");
        foutw.Flush();
        // check for rowdiffratio in range

        for (i = 0; i < NUM_CTS; ++i)
        {
            if (ct[i].HHSovr)
                continue;
            if ((rowdiffratio[i] < .99 || rowdiffratio[i] > 1.01) && rowdiffratio[i] != 0)
            {
                doanother = true;
                break;
            }  // end if
        }  // end for i

        return doanother;
    }   // end procedure ControlToLocal()

    //******************************************************************************************************

    /*  DesscendingSortMulti() */

    /// Sort a small list of several vars (2+) in ascending order 

    //   Revision History
    //   Date       By   Description
    //   ------------------------------------------------------------------
    //   06/05/02   tb   initial coding

    //   ------------------------------------------------------------------

    public static void DescendingSortMulti(int[] indexer, int[] datacontrol, int[,] dataother, int num_rows, int num_cols)
    {
        int i, j, k;
        int tempindexer;
        int tempdatacontrol;
        int[] tempdataother = new int[num_cols];
        for (i = 0; i < num_rows; ++i)
        {
            tempdatacontrol = datacontrol[i];
            tempindexer = indexer[i];

            for (k = 0; k < num_cols; ++k)
                tempdataother[k] = dataother[i, k];

            for (j = i - 1; j >= 0 && datacontrol[j] < tempdatacontrol; j--)
            {
                datacontrol[j + 1] = datacontrol[j];
                indexer[j + 1] = indexer[j];
                for (k = 0; k < num_cols; ++k)
                    dataother[j + 1, k] = dataother[j, k];
            } // end for

            datacontrol[j + 1] = tempdatacontrol;
            indexer[j + 1] = tempindexer;
            for (k = 0; k < num_cols; ++k)
                dataother[j + 1, k] = tempdataother[k];
        }  // end for i

    } // end procedure DescendingSortMulti

    //***************************************************************************************************************

    // DoFinishChecks()
    // build temp parms for calling finish routines
    //   Revision History
    //   Date       By   Description
    //   ------------------------------------------------------------------

    //   06/08/14   tbe  New procedure for deriving CT detailed HH data
    //   ------------------------------------------------------------------

    public void DoFinishChecks(int modelswitch, StreamWriter foutw, CTMASTER[] ct, REG reg, int[] rowtotals, int[] coltotals, int dimj)
    {
        int i, j;
        bool call_finish1, call_finish2;
        int[] indexer = new int[NUM_CTS];
        int[] dcontrol = new int[NUM_CTS];
        int[,] dother = new int[NUM_CTS, 7];
        int[,] matrx = new int[NUM_CTS, dimj];
        int[] nurowt = new int[NUM_CTS], check1 = new int[dimj];
        int[] ovr = new int[NUM_CTS];
        int[] save1b = new int[NUM_CTS];  // these are temp arrays used to store values before and after controlling
        int[] save2b = new int[NUM_CTS];
        int[] save1a = new int[NUM_CTS];
        int[] save2a = new int[NUM_CTS];
        string str = "";
        //-----------------------------------------------------------------------------------------

        call_finish1 = false;
        call_finish2 = false;
        Array.Clear(ovr, 0, ovr.Length);

        for (i = 0; i < NUM_CTS; ++i)
        {
            if (ct[i].HHSovr && modelswitch == 1)
            {
                ovr[i] = 1;
                nurowt[i] = 0;
            }
            else
                nurowt[i] = ct[i].hh;

            if (!ct[i].HHSovr && !call_finish1 && (rowtotals[i] != ct[i].hh))
                call_finish1 = true;

            for (j = 0; j < dimj; ++j)
            {
                if (modelswitch == 1)
                {
                    if (!ct[i].HHSovr)
                        matrx[i, j] = ct[i].hhXs[j];
                    else
                        matrx[i, j] = 0;
                }   // end if

                else if (modelswitch == 2)
                {
                    
                    matrx[i, j] = ct[i].hhwoc[j];
                }  // end else if
                else
                    matrx[i, j] = ct[i].hhworkers[j];
            }  // end for j
        }   // end for i

        // sort the data before sending to the finish routines;  
        // this uses 3 arrays
        // the first array indexer carries the original index in the ct array
        // the second array dcontrol carries the controlling variable for the finish checks, hh
        // the third array carries the actual data being sorted

        for (i = 0; i < NUM_CTS; ++i)
        {
            indexer[i] = i;
            if (modelswitch == 2)   // for hh w kids sort them on descending value of hh with kids; 
                dcontrol[i] = ct[i].hhwoc[1];
            for (j = 0; j < dimj; ++j)
                dother[i, j] = matrx[i, j];
        }  // end for i

        if (modelswitch == 2)
            DescendingSortMulti(indexer, dcontrol, dother, NUM_CTS, dimj);

        //check for finish1
        for (j = 0; j < dimj; ++j)
        {
            if (modelswitch == 1)
                check1[j] = reg.hhXsAdjusted[j];
            else if (modelswitch == 2)
                check1[j] = reg.hhwocAdjusted[j];
            else
                check1[j] = reg.hhworkers[j];
        }  // end for j

        if (call_finish1)
        {
            PasefUtils.ff1(dother, nurowt, check1, NUM_CTS, dimj);
        }  // end if

        // get new row and col totals
        Array.Clear(rowtotals, 0, rowtotals.Length);
        Array.Clear(coltotals, 0, coltotals.Length);

        for (i = 0; i < NUM_CTS; ++i)
        {
            for (j = 0; j < dimj; ++j)
            {
                rowtotals[i] += dother[i, j];
                coltotals[j] += dother[i, j];
            }   // end for j
        }   // end for i

        // check for finish2
        for (j = 0; j < dimj; ++j)
        {

            if (!call_finish2 && coltotals[j] != check1[j])
            {
                call_finish2 = true;
                break;
            }  // end if

        }   // end for i

        // store the hh w kids before finish routines
        if (modelswitch == 2)
        {
            for (i = 0; i < NUM_CTS; ++i)
            {
                save1b[i] = dother[i, 0];
                save2b[i] = dother[i, 1];
            }   // end for i
        }  // end if

        if (call_finish2)
            PasefUtils.ff2(dother, check1, NUM_CTS, dimj);

        // store the hh w kids before finish routines
        if (modelswitch == 2)
        {
            for (i = 0; i < NUM_CTS; ++i)
            {
                save1a[i] = dother[i, 0];
                save2a[i] = dother[i, 1];
            }   // end for i
        }  // end if
        //back from finish routines
        // restore the order of the finished data
        if (modelswitch == 2)
        {
            for (i = 0; i < NUM_CTS; ++i)
            {
                int inew = indexer[i];
                for (j = 0; j < dimj; ++j)
                    matrx[inew, j] = dother[i, j];
            }  // end for j
        }   // end if
        else
        {
            for (i = 0; i < NUM_CTS; ++i)
            {
                for (j = 0; j < dimj; ++j)
                    matrx[i, j] = dother[i, j];
            }  // end for j
        }  // end else

        // get new row and col totals
        Array.Clear(rowtotals, 0, rowtotals.Length);
        Array.Clear(coltotals, 0, coltotals.Length);
        for (i = 0; i < NUM_CTS; ++i)
        {
            for (j = 0; j < dimj; ++j)
            {
                rowtotals[i] += matrx[i, j];
                coltotals[j] += matrx[i, j];
            }   // end for j
        }   // end for i

        foutw.WriteLine("AFTER FINISH ROUTINES");

        // replace matrix data in ct data class
        for (i = 0; i < NUM_CTS; ++i)
        {
            if (ct[i].HHSovr && ct[i].HHWOCovr)
                continue;
            str = ct[i].ctid.ToString() + ",";
            for (j = 0; j < dimj; ++j)
            {
                if (modelswitch == 1)
                {
                    if (!ct[i].HHSovr)
                        ct[i].hhXs[j] = matrx[i, j];
                }  // end if
                else if (modelswitch == 2)
                {
                    if (!ct[i].HHWOCovr)
                        ct[i].hhwoc[j] = matrx[i, j];
                }  // end else if
                else
                    ct[i].hhworkers[j] = matrx[i, j];

                str += matrx[i, j] + ",";

            }   // end for j
            str += rowtotals[i].ToString();
            foutw.WriteLine(str);
        }   // end for i
        str = " COLUMN TOTALS, ";
        for (j = 0; j < dimj; ++j)
            str += coltotals[j] + ",";
        foutw.WriteLine(str);

        str = "REG HHS CONTROLS, ";
        for (j = 0; j < dimj; ++j)
        {
            if (modelswitch == 1)
                str += reg.hhXsAdjusted[j] + ",";
            else if (modelswitch == 2)
                str += reg.hhwoc[j] + ",";
            else
                str += reg.hhworkers[j] + ",";
        }  // ed for j

        foutw.WriteLine(str);

        foutw.Flush();

    }   // end DoFinishChecks()

    //***********************************************************************************

    //ExtractCTHHData()

    // populate ct array with hh data from popest_mgra
    // db tables
    // the hh and hhp data for the CT come from the mgra estimates table      

    // the regional %distributions of workers by HHS are in the form of a 2-dimensional matrix
    // the matrix is dimensioned (row) HHS by col #workers

    //	Revision History
    //   Date       By   Description
    //   ------------------------------------------------------------------
    //   06/09/11   tb   started initial coding
    //   ------------------------------------------------------------------
    public void ExtractCTHHData(CTMASTER[] ct, REG reg, int scenarioID, int year)
    {
        System.Data.SqlClient.SqlDataReader rdr;
        int i = 0, j = 0, increm = 0;
        int index;
        int[] tempreg = new int[NUM_HHXS];
        //----------------------------------------------------------------

        writeToStatusBox("Filling CT Arrays");
        for (i = 0; i < NUM_HHXS; ++i)
        {
            if (i < NUM_HHWORKERS)
                reg.hhworkers[i] = new int();
            if (i < 2)
            {
                reg.hhwoc[i] = new int();
                reg.hhwocAdjusted[i] = new int();
            }
            reg.hhXs[i] = new int();
        }  // end for i

        // fill regional HH controls
        sqlCommand.CommandText = String.Format(appSettings["selectHHS"].Value, TN.regFcst, year);

        try
        {
            sqlConnection.Open();
            rdr = sqlCommand.ExecuteReader();
            while (rdr.Read())
            {
                increm = 0;
                for (i = 0; i < NUM_HHXS; ++i)
                {
                    reg.hhXs[i] = rdr.GetInt32(increm++); // skip estimates_year in mapping query results
                }   // end for i
                reg.hh = rdr.GetInt32(increm++);
                reg.hhp = rdr.GetInt32(increm++);
                reg.hhwoc[1] = rdr.GetInt32(increm++);
                reg.hhwoc[0] = rdr.GetInt32(increm++);
                reg.hhwocAdjusted[0] = reg.hhwoc[0];
                reg.hhwocAdjusted[1] = reg.hhwoc[1];

                for (i = 0; i < NUM_HHWORKERS; ++i)
                {
                    reg.hhworkers[i] = rdr.GetInt32(increm++);
                }   // end for

            }   // end while
            rdr.Close();

        }  // end try
        catch (Exception exc)
        {
            MessageBox.Show(exc.ToString(), exc.GetType().ToString());

        }  // end catch
        finally
        {
            sqlConnection.Close();
        }

        // fill regional HH X workers
        // this is a 2-dimension array - fills by row each row represents the  distribution of workers in HHSize (row)

        sqlCommand.CommandText = String.Format(appSettings["selectAllWhere1"].Value, TN.distributionHHWorkersRegion, year);
        try
        {
            sqlConnection.Open();
            rdr = sqlCommand.ExecuteReader();
            while (rdr.Read())
            {
                int row = rdr.GetInt32(1) - 1;   // skip estimates_year
                for (j = 0; j < NUM_HHWORKERS; ++j)
                {
                    reg.hhworkersp[row, j] = rdr.GetDouble(j + 2);
                }   // end for i

            }   // end while
            rdr.Close();

        }  // end try
        catch (Exception exc)
        {
            MessageBox.Show(exc.ToString(), exc.GetType().ToString());

        }  // end catch
        finally
        {
            sqlConnection.Close();
        }
        // get HH and HHP from mgra data table
        // fill regional HH and HHP
        this.sqlCommand.CommandText = String.Format(appSettings["selectPDHH1"].Value, TN.MGRABASE, year);
        try
        {
            sqlConnection.Open();
            rdr = sqlCommand.ExecuteReader();
            while (rdr.Read())
            {
                reg.hh = rdr.GetInt32(0);
                reg.hhp = rdr.GetInt32(1);
            }   // end while
            rdr.Close();

        }  // end try
        catch (Exception exc)
        {
            MessageBox.Show(exc.ToString(), exc.GetType().ToString());

        }  // end catch
        finally
        {
            sqlConnection.Close();
        }

        // fill ct HH from mgra data
        sqlCommand.CommandText = String.Format(appSettings["selectPDHH2"].Value, TN.MGRABASE, TN.xref, year);

        try
        {
            sqlConnection.Open();
            rdr = sqlCommand.ExecuteReader();
            i = 0;
            while (rdr.Read())
            {
                //ct[i] = new CTMASTER();

                ct[i].ctid = rdr.GetInt32(0);
                ct[i].hh = rdr.GetInt32(1);
                ct[i].hhp = rdr.GetInt32(2);
                if (ct[i].hh > 0)
                    ct[i].hhs = (double)(ct[i].hhp) / (double)(ct[i].hh);
                else
                    ct[i].hhs = 0;
                ++i;
                if (i >= NUM_CTS)
                    break;
            }   // end while
            rdr.Close();

        }  // end try
        catch (Exception exc)
        {
            MessageBox.Show(exc.ToString(), exc.GetType().ToString());

        }  // end catch
        finally
        {
            sqlConnection.Close();
        }

        // get ct error factors
        this.sqlCommand.CommandText = String.Format(appSettings["selectPDHH3"].Value, TN.errorFactorsHHSCT, year);
        try
        {
            sqlConnection.Open();
            rdr = sqlCommand.ExecuteReader();
            while (rdr.Read())
            {
                // skip 1st index which has year
                i = rdr.GetInt32(1);
                index = GetCTIndex(ct, i);
                ct[index].hhXsef = new double[NUM_HHXS];
                for (j = 0; j < NUM_HHXS; ++j)
                {
                    //ct[index].hhXsef[j] = new double();
                    ct[index].hhXsef[j] = rdr.GetDouble(2 + j);
                }   // end for j
            }   // end while
            rdr.Close();

        }  // end try
        catch (Exception exc)
        {
            MessageBox.Show(exc.ToString(), exc.GetType().ToString());

        }  // end catch
        finally
        {
            sqlConnection.Close();
        }

        // get ct hhwoc %
        sqlCommand.CommandText = String.Format(appSettings["selectPDHH3"].Value, TN.distributionHHWOC, year);

        try
        {
            sqlConnection.Open();
            rdr = sqlCommand.ExecuteReader();
            while (rdr.Read())
            {
                // skip first index popest year
                i = rdr.GetInt32(1);
                index = GetCTIndex(ct, i);
                ct[index].hhwocp = new double[4];
                if (index == 9999)
                {
                    MessageBox.Show("Bad CT Index on ct = " + i);
                }
                for (j = 0; j < 4; ++j)
                    ct[index].hhwocp[j] = rdr.GetDouble(2 + j);

            }   // end while
            rdr.Close();

        }  // end try
        catch (Exception exc)
        {
            MessageBox.Show(exc.ToString(), exc.GetType().ToString());

        }  // end catch
        finally
        {
            sqlConnection.Close();
        }

        // get ct overrides
        sqlCommand.CommandText = String.Format(appSettings["selectPDHH3"].Value, TN.overridesHHSDetail, year);
        Array.Clear(tempreg, 0, tempreg.Length);
        try
        {
            sqlConnection.Open();
            rdr = sqlCommand.ExecuteReader();
            while (rdr.Read())
            {
                // skip first index year
                i = rdr.GetInt32(1);
                index = GetCTIndex(ct, i);
                if (index == 9999)
                {
                    MessageBox.Show("Bad CT Index on ct = " + i);
                } // end if
                ct[index].HHSovr = true;
                ct[index].hhXsop = new double[NUM_HHXS];
                ct[index].hhXs = new int[NUM_HHXS];
                int temp1 = 0;
                for (j = 1; j < NUM_HHXS; ++j)  // skip first bin, cause it gets computed as a residual
                {
                    ct[index].hhXsop[j] = rdr.GetDouble(2 + j);   // the overrides is a float %
                    ct[index].hhXs[j] = (int)(ct[index].hhXsop[j] * (double)ct[index].hh);  // apply the % to total hh
                    temp1 += ct[index].hhXs[j];  // save the cumulative hh
                    tempreg[j] += ct[index].hhXs[j]; //here is the adjustment to the regional total
                }   // end for j

                ct[index].hhXs[0] = ct[index].hh - temp1;  // derive the first bin as a residual
                if (ct[index].hhXs[0] < 0)  //constrain to 0
                    ct[index].hhXs[0] = 0;
                tempreg[0] += ct[index].hhXs[0];  // add to regional adjustment

            }   // end while

        }  // end try
        catch (Exception exc)
        {
            MessageBox.Show(exc.ToString(), exc.GetType().ToString());

        }  // end catch
        finally
        {
            sqlConnection.Close();
        }

        // adjust the regional controls for the sum of the overrides
        for (j = 0; j < NUM_HHXS; ++j)
            reg.hhXsAdjusted[j] = reg.hhXs[j] - tempreg[j];

        // get ratio kids/hh fro hh with kids from 2010 census
        this.sqlCommand.CommandText = String.Format(appSettings["selectPDHH4"].Value, TN.ct10Kids);

        try
        {
            sqlConnection.Open();
            rdr = sqlCommand.ExecuteReader();
            while (rdr.Read())
            {

                i = rdr.GetInt32(0);
                index = GetCTIndex(ct, i);
                if (index == 9999)
                {
                    MessageBox.Show("Bad CT Index on ct = " + i);
                }
                try
                {
                    ct[index].kids_hh_wkids = rdr.GetDouble(1);
                }
                catch (Exception exc)
                {
                    MessageBox.Show(exc.ToString(), exc.GetType().ToString());

                }  // end catch
            }   // end while
            rdr.Close();

        }  // end try
        catch (Exception exc)
        {
            MessageBox.Show(exc.ToString(), exc.GetType().ToString());

        }  // end catch
        finally
        {
            sqlConnection.Close();
        }
        // get total children 0 = 17
        this.sqlCommand.CommandText = String.Format(appSettings["selectPDHH5"].Value, TN.ctTable, scenarioID, year);
        try
        {
            sqlConnection.Open();
            rdr = sqlCommand.ExecuteReader();
            while (rdr.Read())
            {

                i = rdr.GetInt32(0);
                index = GetCTIndex(ct, i);
                if (index == 9999)
                {
                    MessageBox.Show("Bad CT Index on ct = " + i);
                }
                ct[index].kids = rdr.GetInt32(1);
            }   // end while
            rdr.Close();

        }  // end try
        catch (Exception exc)
        {
            MessageBox.Show(exc.ToString(), exc.GetType().ToString());

        }  // end catch
        finally
        {
            sqlConnection.Close();
        }

        // fill the ct mgra list
        for (i = 0; i < NUM_CTS; ++i)
        {
            int counter = 0;
            ct[i].num_mgras = 0;
            //ct[i].m = new MDHH[MAX_MGRAS_IN_CTS];
            this.sqlCommand.CommandText = String.Format(appSettings["selectPDHH6"].Value, TN.MGRABASE, TN.xref, ct[i].ctid, year);

            try
            {
                sqlConnection.Open();
                rdr = sqlCommand.ExecuteReader();
                while (rdr.Read())
                {
                    //ct[i].m[counter] = new MDHH();
                    ct[i].m[counter].mgraid = rdr.GetInt32(0);
                    ct[i].m[counter].hh = rdr.GetInt32(1);
                    ct[i].m[counter].hhp = rdr.GetInt32(2);
                    if (ct[i].m[counter].hh > 0)
                        ct[i].m[counter].hhs = (double)ct[i].m[counter].hhp / (double)ct[i].m[counter].hh;
                    ++counter;
                }   // end while
                rdr.Close();
                ct[i].num_mgras = counter;
            }  // end try
            catch (Exception exc)
            {
                MessageBox.Show(exc.ToString(), exc.GetType().ToString());

            }  // end catch
            finally
            {
                sqlConnection.Close();
            }  // end finally
        }  // end for i

    } // end procedure ExtractCTHHDATA

    //*********************************************************************************************
    #endregion

    # region doEnrollment()

    //**************************************************************************************************
    // doEnrollment
    // compute enrollment for MGRA by 5 categories
    // procedure reads prior year enrollment by mgra and factors to current year totals

    public void doEnrollment()
    {
        System.Data.SqlClient.SqlDataReader rdr;
        int[,] enp = new int[NUM_MGRAS, 5];    // mgra enrollment prior year
        int[,] en = new int[NUM_MGRAS, 5];     // mgra enrollment current year (factored)
        int[] enc = new int[5];                // enrollment control totals
        int[] passer = new int[NUM_MGRAS];     // temp storage for factored values
        int mgraid;
        int i,j;

        // truncate the enrollment table for current year
        sqlCommand.CommandText = String.Format(appSettings["deleteFrom"].Value, TN.enrollment, scenarioID,year);
        try
        {
            sqlConnection.Open();
            sqlCommand.ExecuteNonQuery();
        }
        catch (Exception exc)
        {
            MessageBox.Show(exc.ToString(), exc.GetType().ToString());

        }
        finally
        {
            sqlConnection.Close();
        }

        // get forecast enrollment
        writeToStatusBox("Building Enrollment for MGRAs");
        for (i= 0; i < NUM_MGRAS; ++i)
        {
           for (j = 0; j < 5; ++j)
           {
               enp[i,j] = new int();
               en[i,j] = new int();
           } // end for j
        }  // end for i
           
        sqlCommand.CommandText = String.Format(appSettings["selectAllWhere2"].Value, TN.enrollment,scenarioID,pyear);  
       // get enrollment distribution from prior year
        try
        {
            sqlConnection.Open();
            rdr = sqlCommand.ExecuteReader();
            /* SQL Data reading loop.  Read records and store into Master data structure array. */
            while (rdr.Read())
            {
                mgraid = rdr.GetInt32(2) - 1;
                for (j = 0; j < 5; ++j)
                    enp[mgraid, j] = rdr.GetInt32(3 + j);
            }   // end while
            rdr.Close();
        }   // end try

        catch (Exception e)
        {
            MessageBox.Show(e.ToString(), e.GetType().ToString());
        }
        finally
        {
            sqlConnection.Close();
        }

        sqlCommand.CommandText = String.Format(appSettings["selectEN1"].Value, TN.regFcst, scenarioID,year);
        // get enrollment distribution from prior year
        try
        {
            sqlConnection.Open();
            rdr = sqlCommand.ExecuteReader();
            /* SQL Data reading loop.  Read records and store into Master data structure array. */
            while (rdr.Read())
            {
                for (j = 0; j < 5; ++j)
                    enc[j] = rdr.GetInt32(j);
            }   // end while
            rdr.Close();
        }   // end try

        catch (Exception e)
        {
            MessageBox.Show(e.ToString(), e.GetType().ToString());
        }
        finally
        {
            sqlConnection.Close();
        }

        // factor the mgra data to new totals
        for (j = 0; j < 5; ++j)   // for each category
        {
            writeToStatusBox("Processing enrollment for caregory " + j + 1);
            // get existing total
            int tot = 0;
            int newtot = 0;
            Array.Clear(passer, 0, passer.Length);
            for (i = 0; i < NUM_MGRAS; ++i)
            {
                tot += enp[i, j]; 
            }  // end for i

            // factor the 
            for (i = 0; i < NUM_MGRAS; ++i)
            {
                passer[i] = (int)((double)enp[i, j] / (double) tot * (double)enc[j]);
                newtot += passer[i];
            }   // end for i

            // send to roundoff routine
            if (newtot != enc[j])
                PasefUtils.pachinko(NUM_MGRAS, NUM_MGRAS, enc[j], passer);

            // restore the factored and rounded data to the curent enrollment
            for (i = 0; i < NUM_MGRAS; ++i)
                en[i,j] = passer[i];

        }  // end for j

        // insert the new mgra data

        for (i = 0; i < NUM_MGRAS; ++i)
        {
            if (i % 1000 == 0)
                writeToStatusBox("Inserting record " + i);
            string text = " values (" + scenarioID + "," + year + "," + (i+1) + ",";
            for (j = 0; j < 4; ++j)
            {
                text += en[i, j] + ",";
            }  // end for j
            text += en[i, 4] + ")";
            sqlCommand.CommandText = String.Format(appSettings["insertInto"].Value, TN.enrollment, text);
            try
            {
                sqlConnection.Open();
                sqlCommand.ExecuteNonQuery();

            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.ToString(), exc.GetType().ToString());
            }
            finally
            {
                sqlConnection.Close();
            }
        }   // end for i
    }

    // end doEnrollment()

    //****************************************************************************************************

    #endregion doEnrollment()


    #region Miscellaneous utilities

    // procedures

    //   GetCTIndex() - determine the index of the ct with ctid
    //   WritemgraData() - Write the controlled data to ASCII for bulk loading 
    //   WriteCTData() - write ct data to ASCII
    //   writeToStatusBox - display status text

    //---------------------------------------------------------------------------------------------------------

    //GetCTIndex()

    //   Revision History
    //   Date       By   Description
    //   ------------------------------------------------------------------
    //   06/09/11   tb   initial coding

    //   ------------------------------------------------------------------

    public Int32 GetCTIndex(CTMASTER[] ct, int i)
    {
        int j;
        int ret = 9999;
        for (j = 0; j < NUM_CTS; ++j)
        {
            if (ct[j].ctid == i)
            {
                ret = j;
                break;
            }   // end if
        } // end for j
        return ret;
    } // end procedure GetCTIndex

    //**************************************************************************************       

    //  WriteCTData() 
    // Write the ct data to ASCII 

    //   Revision History
    //   Date       By   Description
    //   ------------------------------------------------------------------
    //   06/12/11   tb   initial coding

    //   ------------------------------------------------------------------
    public void WriteCTData(StreamWriter foutt, int ctid, int[] passer, int rowtotal, double diffratio, int ii, int dimj)
    {
        string str = (ii + 1) + "," + ctid.ToString() + ",";
        for (int k = 0; k < dimj; k++)
        {
            str += passer[k] + ",";

        } // end for k    
        str += rowtotal + "," + diffratio;
        try
        {
            foutt.WriteLine(str);
            foutt.Flush();
        }
        catch (IOException exc)      //exceptions here
        {
            MessageBox.Show(exc.Message + " File Write Error");
            return;
        }

    } // end procedure WriteCTData

    
    #endregion

    #region SQL command procedures

    // procedures
    //    BulkLoadMGRADHH() - Bulk loads ASCII to popest MGRA     
    //    BulkLoadDHH() - run sql commands truncate and reload detailed hh data
    //------------------------------------------------------------------------------------------------

    /*  BulkLoadMGRADHH() */
    /// Bulk loads ASCII to popest MGRA

    //   Revision History
    //   Date       By   Description
    //   ------------------------------------------------------------------
    //   06/05/02   tb   initial coding

    //   ------------------------------------------------------------------
    public void BulkLoadMGRADHH()
    {
        string fo;

        fo = networkPath + "pdmgra";

        try
        {
            sqlConnection.Open();
            writeToStatusBox("TRUNCATING DETAILED POPEST MGRA HH TABLE");
            sqlCommand.CommandText= String.Format(appSettings["deleteFrom"].Value, TN.pasefHHDetailMGRA,scenarioID,year);
            sqlCommand.ExecuteNonQuery();

            writeToStatusBox("BULK LOADING DETAILED POPEST MGRA HH TABLE");
            sqlCommand.CommandTimeout = 180;
            sqlCommand.CommandText= String.Format(appSettings["bulkInsert"].Value, TN.pasefHHDetailMGRA, fo);
            sqlCommand.ExecuteNonQuery();

        }
        catch (Exception exc)
        {
            MessageBox.Show(exc.ToString(), exc.GetType().ToString());

        }
        finally
        {
            sqlConnection.Close();
        }
    } // end procedure BulkLoadMGRADHH()       

    //**********************************************************************************************

    /*  bulkLoadDHH() */
    /// Run SQL commands to truncate and reload detailed popest HH data

    //   Revision History
    //   Date       By   Description
    //   ------------------------------------------------------------------
    //   06/18/11   tb   initial coding

    //   ------------------------------------------------------------------

    public void BulkLoadDHH(CTMASTER[] ct, int year)
    {
        int i;
        int j;

        writeToStatusBox("LOADING CT DETAILED HH DATA");
        
        try
        {
            sqlConnection.Open();
            sqlCommand.CommandText= String.Format(appSettings["deleteFrom"].Value, TN.pasefHHDetailCT, scenarioID, year);
            sqlCommand.ExecuteNonQuery();

        }    // end try
        catch (Exception exc)
        {
            MessageBox.Show(exc.ToString(), exc.GetType().ToString());
        }  // end catch
        finally
        {
            sqlConnection.Close();
        }  // end finally

        
        for (i = 0; i < NUM_CTS; ++i)
        {
            string tex = "values (" + scenarioID + "," + year + "," + ct[i].ctid + ",";

            for (j = 0; j < NUM_HHXS; ++j)
                tex += ct[i].hhXs[j] + ",";
            for (j = 0; j < 2; ++j)
                tex += ct[i].hhwoc[j] + ",";
            for (j = 0; j < NUM_HHWORKERS - 1; ++j)
                tex += ct[i].hhworkers[j] + ",";
            tex += ct[i].hhworkers[NUM_HHWORKERS - 1] + ")";
            sqlCommand.CommandText= String.Format(appSettings["insertInto"].Value, TN.pasefHHDetailCT, tex);

            try
            {
                sqlConnection.Open();
               sqlCommand.ExecuteNonQuery();

            }    // end try
            catch (Exception exc)
            {
                MessageBox.Show(exc.ToString(), exc.GetType().ToString());
            }  // end catch
            finally
            {
                sqlConnection.Close();
            }  // end finally

        }  // end for
    }   // end procedure BulkLoadDHH()

      //**********************************************************************************************************************

    #endregion
       
  }     // End class Pasef

   public class REG  //regional control for detailed HH data class
{
    public int hhp;   //regional hhp control
    public int hh;    // regional hh control
    public int[] hhXs = new int[7];           // regional hh X size controls
    public int[] hhXsAdjusted = new int[7];   // regional controls adjusted for overrides
    public int[] hhwocAdjusted = new int[2];
    public int[] hhworkersAdjusted = new int[4];

    public int[] hhwoc = new int[2];                   // regional total hh without children 0 = without, 1 = with
    public int[] hhworkers = new int[4];  // regional total hh with workers by category, 0, 1, 2, 3+
    public double[,] hhworkersp = new double[4,4]; //distribution of workers categories by hh size
} // end class

//****************************************************************************************

// detailed mgra hh data class

public class MDHH
{
    public int mgraid;
    public int[] hhworkers;             // mgra hh by workers rounded
    public int[] hhwoc;                                // mgra hh wo children rounded
    public int[] hhXs;                       // mgra hh x size category rounded
    public double[] hhXsc;                // ct hh x size category computed
    public double[] hhXsp;                // mgra hh x size proportions derived with poisson function
    public double[] hhXspa;               // mgra hh x size poisson proportions adjusted by error factor
    public int hh;                                                  // number of hh in mgra
    public int hhp;                                                 // hhp in mgra
    public int hhpc;                                                // implied hhp from hhxs
    public bool hhis1 = false;                                      // is this mgra hh = 1; used for controlling
    public double hhs;

}  // end class

//******************************************************************************************

public class CTMASTER  //CT-level detailed HH data class
{
    public int ctid;
    public int hhp;
    public int hhpc;                          // ct hhp reconstituted
    public int hh;                            // ct total hh
    public double hhs;                        // ct hhs
    public bool HHSovr;                          // whether of not this ct uses hhXs overrides
    public bool HHWOCovr;                        // whether or not this ct uses hhwoc overrides
    public bool HHWORKERSovr;                    // whether or not this ct uses hhworkers overrides
    public int[] hhXso = new int[7];    // hhs overrides for special ct;
    public double[] hhXsop = new double[7];   // hhs overrides expressed as % of total hh
    public double[] hhXsp = new double[7];    // ct hh x size proportions derived with poisson function
    public double[] hhXsef = new double[7];   // ct level hh x size error factors from base data
    public double[] hhXspa = new double[7];   // ct hh x size poisson proportions adjusted by error factor
    public double[] hhXsc = new double[7];    // ct hh x size category computed
    public int[] hhXs = new int[7];           // ct hh x size category rounded
    public int[] hhXs4 = new int[4];             // hh X size summed to 4 categories hhs = 1, hhs = 2, hhs = 3, hhs = 4+

    public double[] hhwocp = new double[4];     // hh wo children % 4 categories hhs = 1, hhs = 2, hhs = 3, hhs = 4+
    public double[] hhwocc = new double[4];    // ct hh wo children computed
    public int[] hhwoc = new int[2];            // ct hh wo children rounded

    public double[] hhworkersp = new double[4];
    public double[] hhworkersc= new double[4];     // ct hh by workers computed
    public int[] hhworkers = new int[4];            // ct hh by workers rounded

    public int num_mgras;
    public int kids;           // number of children 0 - 17
    public int kidsl;          // minimum number of children computed from hhwc * 1
    public int kidsu;          // maximum number of kids computed from hhwc * 3(or some other acceptable number like 2.5)
    public double kids_hh_wkids;  // number of kids/hh with kids from 2000 census

    public MDHH[] m;
}

  public class AS
  {
    private const int NUM_ETH = 9;         /* Number of ethnic groups- 0 stores 
                                           * total */
    private const int NUM_SEX = 3;         // Number of sex groups- 0 stores

    public int[,] pop, sPop, nsPop;
    public int popTotal, sPopTotal, nsPopTotal;

    public AS()
    {
      pop = new int[NUM_ETH,NUM_SEX];
      sPop = new int[NUM_ETH,NUM_SEX];
      nsPop = new int[NUM_ETH,NUM_SEX];
    }
  }

  public class SpecialMaster
  {
    public int ctID, baseCode, sraID;

    // Constructor
    public SpecialMaster( int id, int code, int sra )
    {
      ctID = id;
      baseCode = code;
      sraID = sra;
    }
      public SpecialMaster( )
      {
        ctID = 0;
        baseCode = 0;
        sraID = 0;
      }
  }  


  public class Master
  {
    private const int NUM_ETH = 9;         /* Number of ethnic groups- 0 stores total */
    private const int NUM_SEX = 3;         // Number of sex groups- 0 stores total
    private const int NUM_AGE = 20;        // Number of five-year age groups

    public bool special;           // CT marked as special pop?
    public int id;                 // Actual identifier ct90, sra, etc.
    public int controlIndex;       // Next highest geo ID
    public AS control;             // Control by age and sex
    public AS estimated;           // Estimated by age and sex
    
    public int[,,] pop;            // Computed pop
    public int[,,] sPop;           // Special pop
    public int[,,] nsPop;          // Non-special pop
    public double[,] chgShare;     /* Change in ethnic sex share.  Used at SRA 
                                    * only */
    // Constructor
    public Master()
    {
        pop = new int[NUM_ETH, NUM_SEX, NUM_AGE];
        sPop = new int[NUM_ETH, NUM_SEX, NUM_AGE];
        nsPop = new int[NUM_ETH, NUM_SEX, NUM_AGE];
        control = new AS();
        estimated = new AS();
    }
  }

  public class TABLENAMES
  {
      public string basePopTable;
      public string ctTable;
      public string ct10Kids; 
      public string defmPop;
      public string enrollment;
      public string sraTable;
      public string MGRABASE;
      public string pasefMGRA;
      public string pasefMGRATab;
      
     
      public string pasef_update_tab_proc;
      public string special_pop_tracts;
      public string sraShare;
     
      public string detailedPopTabCT;
      public string distributionHHWorkersRegion;
      public string distributionHHWOC;
      public string errorFactorsHHSCT;
      public string overridesHHSDetail;
     
      public string pasefHHDetailMGRA;
      public string pasefHHDetailCT;
      public string regFcst;
      public string xref;  
  }
}
