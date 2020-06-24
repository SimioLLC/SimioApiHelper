using System;
using Microsoft.Deployment.WindowsInstaller;
using WixSharp;
using WixSharp.CommonTasks;
using WixSharp.Controls;

//#error "DON'T FORGET to install NuGet package 'WixSharp' and remove this `#error` statement."
// NuGet console: Install-Package WixSharp
// NuGet Manager UI: browse tabb

namespace SimioApiHelper_Setup
{
    public class Program
    {
        static void Main()
        {
            Feature binaries = new Feature("ApiHelper Binaries");
            Feature docs = new Feature("ApiHelper Documentation");
            Feature serverInstall = new Feature("ApiHelper Installer");
            Feature dataFiles = new Feature("ApiHelper Example Data");
            Feature helpFiles = new Feature("ApiHelper Help");


            var project = new ManagedProject("Simio Api Helper",
                             new Dir(@"%ProgramFiles%\Simio LLC\Simio API Helper",
                        new File(new Id("SimioApiHelperInstall_exe"), binaries, @"ApiHelperFiles\SimioApiHelper.exe",
                            new FileShortcut(binaries, "SimioApiHelper", @"%ProgramMenu%\Simio LLC\SimioApiHelper"),
                            new FileShortcut(binaries, "SimioApiHelper", @"%Desktop%")), // PortalManagerFile
                        new File(binaries, @"ApiHelperFiles\SimioApiHelper.exe.config"),
                        new File(binaries, @"ApiHelperFiles\HeadlessLibrary.dll"),
                        new File(binaries, @"ApiHelperFiles\SimioAPI.dll"),
                        new File(binaries, @"ApiHelperFiles\SimioAPI.Extensions.dll"),
                        new File(binaries, @"ApiHelperFiles\SimioEnums.dll"),
                        new File(binaries, @"ApiHelperFiles\SimioAPI.Graphics.dll"),
                        new File(binaries, @"ApiHelperFiles\SimioReplicationRunnerContracts.dll"),
                        new File(binaries, @"ApiHelperFiles\SimioTypes.dll"),
                        new File(binaries, @"ApiHelperFiles\IconLib.dll"),
                        new File(binaries, @"ApiHelperFiles\MathNet.Numerics.dll"),
                        new File(binaries, @"ApiHelperFiles\NewtonSoft.Json.dll"),
                        new File(binaries, @"ApiHelperFiles\QlmLicenseLib.dll"),
                        new File(binaries, @"ApiHelperFiles\ServiceModelEx.dll"),
                        new File(binaries, @"ApiHelperFiles\SimioDLL.dll")
                        ),

                    new Dir(@"PersonalFolder\SimioApiHelper\",
                        new File(dataFiles, @"DataFiles\ExperimentTest.spfx"),
                        new File(dataFiles, @"DataFiles\SchedulingDiscretePartProduction.spfx")), // Samples

                    new Dir(@"PersonalFolder\SimioApiHelper\Help",
                            new File(helpFiles, @"HelpFiles\Simio Api Note - Simio API Helper.pdf")),

                    new Dir("%Startup%",
                        new ExeFileShortcut(binaries, "SimioApiHelper", "[INSTALLDIR]SimioApiHelper.exe", "SimioApiHelperInstall.msi"))

                    // new Dir(@"%ProgramMenu%\Simio LLC\SimioPortal",
                    //    new ExeFileShortcut(binaries, "Uninstall PortalManager", "[System64Folder]msiexec.exe", "/x [ProductCode]"))

                    ); // project


            project.GUID = new Guid("40711452-172C-44E8-931F-BDD327C7242B");

            project.BannerImage = @"Images\ApiHelperBanner.png";
            project.LicenceFile = @"Resources\SimioEULA.rtf";
            project.ControlPanelInfo.ProductIcon = @"Images\AddRemoveProgramsIcon.ico";
            project.ControlPanelInfo.Readme = "Assistance with common Simio API situations.";

            project.BuildMsi();
        }

        [CustomAction]
        public static ActionResult ShowCustomDialog(Session session)
        {
            return WixCLRDialog.ShowAsMsiDialog(new CustomDialog(session));
        }
    }
}