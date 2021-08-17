using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WixSharp;

namespace SimEngineControllerInstall
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Feature binaries = new Feature("SimEngineController Binaries");

                Feature docs = new Feature("SimEngineController Documentation");
                Feature serverInstall = new Feature("SimEngineController Installer");
                Feature dataFiles = new Feature("SimEngineController Example Data");
                Feature helpFiles = new Feature("SimEngineController Help");

                var project = new ManagedProject("SimEngineController",
                    new Dir(@"%ProgramFiles%\Simio LLC\SimEngineController",

                        new File(new Id("SimEngineControllerInstall_exe"), binaries, @"SimEngineControllerFiles\SimEngineController.exe",
                            new FileShortcut(binaries, "SimEngineController", @"%ProgramMenu%\Simio LLC\SimEngineController"),
                            new FileShortcut(binaries, "SimEngineController", @"%Desktop%")), // PortalManagerFile

                        
                        new File(binaries, @"SimEngineControllerFiles\SimEngineController.exe.config"),
                        ////new File(binaries, @"SimEngineControllerFiles\MathnetNumerics.dll"),
                        new File(binaries, @"SimEngineControllerFiles\QlmLicenseLib.dll"),
                        ////new File(binaries, @"SimEngineControllerFiles\ServiceModel.dll"),
                        new File(binaries, @"SimEngineControllerFiles\SimEngineInterfaceHelpers.dll"),
                        new File(binaries, @"SimEngineControllerFiles\SimEngineLibrary.dll"),
                        new File(binaries, @"SimEngineControllerFiles\SimioAPI.dll"),
                        new File(binaries, @"SimEngineControllerFiles\SimioAPI.Extensions.dll"),
                        new File(binaries, @"SimEngineControllerFiles\SimioAPI.Graphics.dll"),
                        new File(binaries, @"SimEngineControllerFiles\SimioDLL.dll"),
                        new File(binaries, @"SimEngineControllerFiles\SimioEnums.dll"),
                        new File(binaries, @"SimEngineControllerFiles\SimioReplicationRunnerContracts.dll"),
                        new File(binaries, @"SimEngineControllerFiles\SimioTypes.dll")

                        ),


                    new Dir(@"%Personal%\SimEngineTest",
                        new Dir(dataFiles, @"Extensions\"),
                        new Dir(dataFiles, @"Projects\"),
                        new Dir(dataFiles, @"Requests\")),

                    new Dir(@"%AppDataFolder%\SimEngineController\Help",
                            new File(helpFiles, @"HelpFiles\SimEngineController.pdf"))

                    ////,new Dir("%Startup%",
                    ////    new ExeFileShortcut(binaries, "SimEngineController", "[INSTALLDIR]SimEngineController.exe", "SimEngineControllerInstall.msi"))

                    // new Dir(@"%ProgramMenu%\Simio LLC\SimioPortal",
                    //    new ExeFileShortcut(binaries, "Uninstall PortalManager", "[System64Folder]msiexec.exe", "/x [ProductCode]"))

                    ); // project

                project.GUID = new Guid("402DCABA-FAF5-48F9-810C-19DC91CA8C4A"); // SimEngineController 01Apr2021

                project.BannerImage = @"Images\SimEngineBanner.png";
                project.LicenceFile = @"Resources\SimioEULA.rtf";
                project.ControlPanelInfo.ProductIcon = @"Images\AddRemoveProgramsIcon.ico";
                project.ControlPanelInfo.Readme = "Controller for the bare SimEngine";

                project.BuildMsi();



            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Wupsy. Err={ex}");
            }
        }
    }
}
