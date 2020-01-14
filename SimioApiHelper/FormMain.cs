using SimioApiHelper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SimioHelper
{
    public partial class FormMain : Form
    {
        private bool IsLoaded = false;

        public FormMain()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Load the assembly of the selected DLL file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonLoadAssembly_Click(object sender, EventArgs e)
        {
            if (!IsLoaded)
                return;

            string marker = "begin";
            try
            {
                StringBuilder sb = new StringBuilder();
                marker = $"Loading File={comboDllFile.Text}";
                sb.AppendLine($"********** {marker}");
                logit(marker);
                Assembly myAssembly = Assembly.LoadFrom(comboDllFile.Text);

                marker = "Getting Types";
                try
                {
                    IEnumerable<Type> myTypes = myAssembly.GetLoadableTypes();

                    textAssemblyLoadInfo.Clear();
                    sb.AppendLine("********** Type Information:");
                    foreach (var myType in myTypes)
                    {
                        marker = $"MyType={myType.Name}";

                        string ss = $"  Name={myType.FullName}";
                        sb.AppendLine(ss);
                        foreach (Type anyType in myType.GetInterfaces())
                        {
                            string name = anyType.Name;
                            string fullName = anyType.FullName;
                            if (checkSimioOnly.Checked)
                            {
                                if (fullName != null && fullName.ToLower().Contains("simio"))
                                    sb.AppendLine($"    Type={anyType}");
                            }
                            else
                            {
                                sb.AppendLine($"    Type={anyType}");
                            }
                        }
                    } // foreach type
                }
                catch (Exception ex)
                {
                    sb.AppendLine($"Error(s) Occured. See exception log below.");
                    ExceptionLog($"Marker={marker} Err={ex}");
                }

                marker = "Getting Referenced Assemblies";
                try
                {
                    sb.AppendLine();
                    sb.AppendLine("********** Referenced Assemblies:");
                    int nn = 0;
                    foreach (AssemblyName refName in myAssembly.GetReferencedAssemblies().ToList())
                    {
                        sb.AppendLine($"{++nn}. {refName}");
                    }
                }
                catch (Exception ex)
                {
                    sb.AppendLine($"Error(s) Occured. See exception log below.");
                    ExceptionLog($"Marker={marker} Err={ex}");
                }

                textAssemblyLoadInfo.Text = sb.ToString();

            }
            catch (Exception ex)
            {
                ExceptionLog($"Marker={marker} Err={ex}");
            }
        }

        /// <summary>
        /// Write logs that occur via exceptions
        /// </summary>
        /// <param name="msg"></param>
        private void ExceptionLog(string msg)
        {
            textExceptions.Text = msg;
        }

        /// <summary>
        /// The simio location for files changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboSimioLocation_SelectedValueChanged(object sender, EventArgs e)
        {
            try
            {
                List<string> dllFiles = DLLHelpers.GetDllFiles(comboSimioLocation.Text);
                comboDllFile.DataSource = dllFiles;
            }
            catch (Exception ex)
            {
                ExceptionLog($"Err={ex}");
            }
        }

        /// <summary>
        /// The DLL choice changed. Write selected info about the DLL file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboDllFile_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                textAssemblyName.Text = DLLHelpers.GetDllInfo(comboDllFile.Text);
                textAssemblyLoadInfo.Text = "";
                textExceptions.Text = "";
            }
            catch (Exception ex)
            {
                ExceptionLog($"Err={ex}");
            }

        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            try
            {
                logit($"Begin Machine={Environment.MachineName}");

                comboSimioLocation.DataSource = DLLHelpers.GetSimioApiLocations();
                comboFindSimioExtensionLocations.DataSource = DLLHelpers.GetSimioApiLocations();
                timerLogs.Enabled = true;
            }
            catch (Exception ex)
            {
                ExceptionLog($"Err={ex}");
            }
            finally
            {
                IsLoaded = true;
            }

        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
            this.Close();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogAbout dialog = new DialogAbout();
            dialog.Show();
        }

        private void createIconToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogCreateIcon dialog = new DialogCreateIcon();
            dialog.ShowDialog();

        }

        /// <summary>
        /// Look through the DLLs for Steps and AddIns
        /// </summary>
        /// <param name="folder"></param>
        private string FindStepsAndAddIns(string folder)
        {
            string marker = "begin";

            if (!Directory.Exists(folder))
            {
                return $"Could not locate folder={folder}";
            }

            try
            {
                StringBuilder sb = new StringBuilder();

                marker = "Looking for Steps";

                string[] files = Directory.GetFiles(folder, "*.dll");
                foreach (string file in files)
                {
                    marker = "Loading File";
                    sb.AppendLine($"********** Loading File={file}");
                    Assembly myAssembly = Assembly.LoadFrom(file);

                    try
                    {
                        IEnumerable<Type> myTypes = myAssembly.GetLoadableTypes();

                        textAssemblyLoadInfo.Clear();
                        foreach (var myType in myTypes)
                        {
                            marker = $"MyType={myType.Name}";

                            var interfaces = myType.GetInterfaces();

                            if (interfaces != null)
                                foreach (Type anyType in myType.GetInterfaces())
                                {
                                    marker = $"MyType={myType.Name} Interface={anyType}";
                                    string fullname = anyType.FullName;


                                    if (!string.IsNullOrEmpty(fullname) && fullname.StartsWith("SimioAPI."))
                                    {
                                        switch (anyType.Name)
                                        {
                                            case "IModelHelperAddIn":
                                                {
                                                    sb.AppendLine(AddInfo(myType.Name, anyType.Name));

                                                }
                                                break;

                                            case "IStep":
                                                {

                                                    sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                                                }
                                                break;
                                            case "IStepDefinition":
                                                {

                                                    sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                                                }
                                                break;
                                            case "IElement":
                                                {
                                                    sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                                                }
                                                break;
                                            case "IElementDefinition":
                                                {
                                                    sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                                                }
                                                break;

                                            case "IDesignAddIn":
                                                {
                                                    sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                                                }
                                                break;
                                            case "IDesignAddInGuiDetails":
                                                {
                                                    sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                                                }
                                                break;

                                            case "IGridDataRecords":
                                                {
                                                    sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                                                }
                                                break;
                                            case "IGridDataRecord":
                                                {
                                                    sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                                                }
                                                break;
                                            case "IGridDataProvider":
                                                {
                                                    sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                                                }
                                                break;
                                            case "ITravelSteeringBehavior":
                                                {
                                                    sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                                                }
                                                break;
                                            case "ITravelSteeringBehaviorDefinition":
                                                {
                                                    sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                                                }
                                                break;

                                            case "IGridDataProviderWithFiles":
                                                {
                                                    sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                                                }
                                                break;

                                            case "IExperimentationAddInDefinition":
                                                {
                                                    sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                                                }
                                                break;

                                            case "IExperimentationAddIn":
                                                {
                                                    sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                                                }
                                                break;

                                            case "IExperimentationRunner":
                                                {
                                                    sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                                                }
                                                break;

                                            default:
                                                {
                                                    string xx = "";
                                                }
                                                break;

                                        } // switch
                                    }
                                } // foreach interface

                        } // foreach loadable type

                    }
                    catch (Exception ex)
                    {
                        sb.AppendLine($"Error(s) Occurred. Marker={marker} Err={ex}");
                        return sb.ToString();
                    }
                }

                return sb.ToString();
            }
            catch (Exception ex)
            {
                return $"Marker={marker} Err={ex}";
            }

        }


        private bool FindProperties(Type myType, Type anyType)
        {
            try
            {
                StringBuilder sb = new StringBuilder();

                switch (anyType.Name)
                {
                    case "INamedSimioCollection`1":
                        {
                            sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                        }
                        break;
                    case "INamedMutableSimioCollection`1":
                        {
                            sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                        }
                        break;
                    case "ISimioCollection`1":
                        {
                            sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                        }
                        break;
                    case "IMutableSimioCollection`1":
                        {
                            sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                        }
                        break;

                    case "IProperty":
                        {
                            sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                        }
                        break;
                    case "IPropertyObject":
                        {
                            sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                        }
                        break;
                    case "IPropertyDefinition":
                        {
                            sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                        }
                        break;
                    case "IObjectInstanceReferencePropertyDefinition":
                        {
                            sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                        }
                        break;
                    case "IObjectInstanceDefinitionPropertyDefinition":
                        {
                            sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                        }
                        break;
                    case "IUnitizedPropertyDefinition":
                        {
                            sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                        }
                        break;
                    case "IIntelligentObject":
                        {
                            sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                        }
                        break;
                    case "IIntelligentObjectRuntimeData":
                        {
                            sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                        }
                        break;
                    case "IAgentRuntimeData":
                        {
                            sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                        }
                        break;
                    case "IElementObject":
                        {
                            sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                        }
                        break;
                    case "IElementData":
                        {
                            sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                        }
                        break;
                    case "IState":
                        {
                            sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                        }
                        break;
                    case "IStateDefinition":
                        {
                            sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                        }
                        break;
                    case "IUnitizedStateDefinition":
                        {
                            sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                        }
                        break;
                    case "IExecutionContext":
                        {
                            sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                        }
                        break;
                    case "ITableColumn":
                        {
                            sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                        }
                        break;
                    case "IUnitizedTableColumn":
                        {
                            sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                        }
                        break;
                    case "IColorPropertyDefinition":
                        {
                            sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                        }
                        break;
                    case "IBooleanPropertyDefinition":
                        {
                            sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                        }
                        break;
                    case "IDateTimePropertyDefinition":
                        {
                            sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                        }
                        break;
                    case "IEnumPropertyDefinition":
                        {
                            sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                        }
                        break;
                    case "IEventReferencePropertyDefinition":
                        {
                            sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                        }
                        break;
                    case "IRealPropertyDefinition":
                        {
                            sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                        }
                        break;
                    case "IIntegerPropertyDefinition":
                        {
                            sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                        }
                        break;
                    case "IListReferencePropertyDefinition":
                        {
                            sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                        }
                        break;
                    case "IRateTableReferencePropertyDefinition":
                        {
                            sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                        }
                        break;
                    case "IDayPatternReferencePropertyDefinition":
                        {
                            sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                        }
                        break;
                    case "ITableReferencePropertyDefinition":
                        {
                            sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                        }
                        break;
                    case "IStateReferencePropertyDefinition":
                        {
                            sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                        }
                        break;
                    case "ISelectionRuleReferencePropertyDefinition":
                        {
                            sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                        }
                        break;
                    case "ISteeringBehaviorReferencePropertyDefinition":
                        {
                            sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                        }
                        break;
                    case "IChangeoverMatrixReferencePropertyDefinition":
                        {
                            sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                        }
                        break;
                    case "IExpressionPropertyDefinition":
                        {
                            sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                        }
                        break;
                    case "ISequenceNumberPropertyDefinition":
                        {
                            sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                        }
                        break;
                    case "ITaskDependencyPropertyDefinition":
                        {
                            sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                        }
                        break;
                    case "IElementReferencePropertyDefinition":
                        {
                            sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                        }
                        break;
                    case "INodeInstanceReferencePropertyDefinition":
                        {
                            sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                        }
                        break;
                    case "IEntityInstanceReferencePropertyDefinition":
                        {
                            sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                        }
                        break;
                    case "ITransporterInstanceReferencePropertyDefinition":
                        {
                            sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                        }
                        break;
                    case "IObjectInstanceListReferencePropertyDefinition":
                        {
                            sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                        }
                        break;
                    case "INodeInstanceListReferencePropertyDefinition":
                        {
                            sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                        }
                        break;
                    case "ITransporterInstanceListReferencePropertyDefinition":
                        {
                            sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                        }
                        break;
                    case "IObjectTypeReferencePropertyDefinition":
                        {
                            sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                        }
                        break;
                    case "ISequenceDestinationPropertyDefinition":
                        {
                            sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                        }
                        break;
                    case "ITableStateColumn":
                        {
                            sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                        }
                        break;

                    default:
                        {

                        }
                        break;

                };
                return true;
            }

            catch (Exception ex)
            {
                string xx = $"AnyType={anyType.Name} Err={ex}";
                return false;
            }
        }

        private string AddInfo(string name, string interfaceName)
        {
            return $"    {name}  Interface={interfaceName}";
        }

        private void buttonFindStepsAndAddIns_Click(object sender, EventArgs e)
        {
            if (!IsLoaded)
                return;

            string marker = "begin";
            try
            {
                string folder = comboFindSimioExtensionLocations.Text;

                string result = FindStepsAndAddIns(folder);

                textStepsAndAddIns.Text = result;
            }
            catch (Exception ex)
            {
                ExceptionLog($"Marker={marker} Err={ex}");
            }

        }

        /// <summary>
        /// Refresh the dashboard, which shows settings and any errors within the settings.
        /// </summary>
        public void RefreshTabDashboard()
        {
            string explanation = "";

            try
            {

                labelHwComputerName.Text = $"Computer Name: {PlatformHelpers.GetComputerName()}";
                labelHwPhysicalMemory.Text = $"Physical Memory: {PlatformHelpers.GetPhysicalMemory()}";
                labelHwCpuMaker.Text = $"CPU Maker: {PlatformHelpers.GetCpuManufacturer()}";
                labelHwCpuSpeed.Text = $"CPU Speed: {PlatformHelpers.GetCpuSpeedInGHz()}";

                labelCurrentDotNet.Text = $"Current .NET Version: {Environment.Version}";

                List<string> versionList = new List<string>();

                StringBuilder sb = new StringBuilder();

                sb.AppendLine(" .NET Versions 1 through 4:");
                if (!PlatformHelpers.GetDotNetVersion1Through4FromRegistry(versionList, out explanation))
                {
                    sb.AppendLine($"Error getting .NET Versions 1-4: {explanation}");
                }
                else
                {
                    foreach (string version in versionList)
                        sb.AppendLine(version);
                }

                sb.AppendLine("");
                sb.AppendLine(" .NET Versions after 4:");
                if (!PlatformHelpers.GetDotNetVersionAfter4FromRegistry(versionList, out explanation))
                {
                    sb.AppendLine($"Error getting .NET Versions after 4: {explanation}");
                }
                else
                {
                    foreach (string version in versionList)
                        sb.AppendLine(version);
                }

                textDotNetVersions.Text = sb.ToString();

            }
            catch (Exception ex)
            {
                alert($"RefreshDashboard: Err={ex}");
                return;
            }
        }

        private void alert(string msg)
        {
            logit(msg);
            MessageBox.Show(msg);
        }
        private void logit(string msg)
        {
            SetStatusLabel(msg);
            LoggertonHelpers.Loggerton.Instance.LogIt(LoggertonHelpers.EnumLogFlags.Error, msg);
        }

        private void SetStatusLabel(string msg)
        {
            labelStatus.Text = msg;
        }

        private void timerLogs_Tick(object sender, EventArgs e)
        {
            textLogs.Text = LoggertonHelpers.Loggerton.Instance.GetLogs(LoggertonHelpers.EnumLogFlags.All);

        }

        private void buttonDashboard_Click(object sender, EventArgs e)
        {
            RefreshTabDashboard();
        }

        private void comboFindSimioExtensionLocations_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                ComboBox combo = sender as ComboBox;

                string location = combo.SelectedItem as string;

            }
            catch (Exception ex)
            {
                alert($"Combo Error={ex}");
            }
        }

        /// <summary>
        /// Prompt the user for a Simio project file.
        /// </summary>
        /// <returns></returns>
        private string GetModelFile()
        {
            try
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Multiselect = false;
                dialog.Filter = "Simio Project|*.spfx";

                DialogResult result = dialog.ShowDialog();

                if (result != DialogResult.OK)
                    return string.Empty;

                return dialog.FileName;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Err={ex.Message}");
            }
        }

        private void buttonHeadlessSelectModel_Click(object sender, EventArgs e)
        {
            textHeadlessModelFile.Text = GetModelFile();
        }

        private void buttonHeadlessRun_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                if (!HeadlessHelpers.RunModel(textHeadlessModelFile.Text, "Model", cbHeadlessRunRiskAnalysis.Checked,
                    cbHeadlessSaveModelAfterRun.Checked,
                    cbHeadlessPublishPlanAfterRun.Checked, out string explanation))
                {
                    alert(explanation);
                }
                else
                {
                    alert($"Model={textHeadlessModelFile.Text} performed the actions successfully. Check the logs for more information.");
                }
            }
            catch (Exception ex)
            {
                alert($"Err={ex.Message}");
                throw;
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }

        }

        private void buttonRunExperiment_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                if (!HeadlessHelpers.RunExperiment( textHeadlessModelFile.Text, "Model", "Experiment1",
                    cbHeadlessSaveModelAfterRun.Checked,
                    out string explanation))
                {
                    alert(explanation);
                }
                else
                {
                    alert($"Model={textModelName.Text} Experiment={textExperimentName} performed the actions successfully. Check the logs for more information.");
                }
            }
            catch (Exception ex)
            {
                alert($"Err={ex.Message}");
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }

        }
    }
}
