namespace Savonia.Measurements.Providers.MeasurerService
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.SAMIServiceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.SAMIServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // SAMIServiceProcessInstaller
            // 
            this.SAMIServiceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.SAMIServiceProcessInstaller.Password = null;
            this.SAMIServiceProcessInstaller.Username = null;
            // 
            // SAMIServiceInstaller
            // 
            this.SAMIServiceInstaller.Description = "SAMI Measurer Service can read and save measurements to Savonia SAMI system.";
            this.SAMIServiceInstaller.DisplayName = "SAMI Measurer Service";
            this.SAMIServiceInstaller.ServiceName = "SAMI Measurer Service";
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.SAMIServiceProcessInstaller,
            this.SAMIServiceInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller SAMIServiceProcessInstaller;
        private System.ServiceProcess.ServiceInstaller SAMIServiceInstaller;
    }
}