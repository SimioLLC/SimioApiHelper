using HeadlessLibrary;
using LoggertonHelpers;
using SimioAPI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SimioApiHelper
{
    public partial class FormMain : Form
    {
        private bool IsLoaded = false;

        HeadlessContext HeadlessRunContext { get; set; }

        FileSystemWatcher FileWatcher { get; set; }

        public FormMain()
        {
            InitializeComponent();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            try
            {
                Logit(EnumLogFlags.Information, $"Begin Machine={Environment.MachineName}");

                comboSimioLocation.DataSource = DLLHelpers.GetSimioApiLocations();
                comboFindSimioExtensionLocations.DataSource = DLLHelpers.GetSimioApiLocations();

                RefreshForm();

            }
            catch (Exception ex)
            {
                ExceptionLog($"Err={ex}");
            }
            finally
            {
                timerLogs.Enabled = true;
                IsLoaded = true;
            }

        }

        private void RefreshForm()
        {
            try
            {
                RefreshTabDashboard();
                RefreshTabHeadlessBuilder(true);
                RefreshTabHeadlessRun();
                RefreshTabSettings();
            }
            catch (Exception ex)
            {
                Alert($"Refresh Error={ex}");
            }

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
                string assemblyPath = comboDllFile.Text;
                textAssemblyLoadInfo.Clear();
                bool showSimioOnly = checkSimioOnly.Checked;
                textAssemblyLoadInfo.Text = LoadAssembly(assemblyPath, showSimioOnly);

            }
            catch (Exception ex)
            {
                ExceptionLog($"Marker={marker} Err={ex}");
            }
        }

        /// <summary>
        /// Load the assembly of the selected DLL file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private string LoadAssembly( string assemblyFile, bool showSimioOnly )
        {
            if (!IsLoaded)
                return "$(File={assemblyFile}Not Loaded)";

            string marker = "begin";
            try
            {
                StringBuilder sb = new StringBuilder();
                marker = $"Loading File={assemblyFile}";
                sb.AppendLine($"********** {marker}");
                Logit(EnumLogFlags.Information, marker);
                Assembly myAssembly = Assembly.LoadFrom(assemblyFile);

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
                            if (showSimioOnly)
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

                return sb.ToString();
            }
            catch (Exception ex)
            {
                ExceptionLog($"Marker={marker} Err={ex}");
                return "Error Occurred.";
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
                List<string> dllFiles = DLLHelpers.GetDllFiles(comboSimioLocation.Text, textDllHelperExcludeFilter.Text);
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
                                            case "IChangeoverMatrices":
                                                {
                                                    sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                                                }
                                                break;
                                            case "ISimioCollection`1":
                                                {
                                                    sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                                                }
                                                break;
                                            case "INamedMutableSimioCollection`1":
                                                {
                                                    sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                                                }
                                                break;
                                            case "IMutableSimioCollection`1":
                                                {
                                                    sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                                                }
                                                break;
                                            case "INamedSimioCollection`1":
                                                {
                                                    sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                                                }
                                                break;
                                            case "IPropertyObject`1":
                                                {
                                                    sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                                                }
                                                break;
                                            case "IIntelligentObject":
                                                {
                                                    sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                                                }
                                                break;
                                            case "IIntelligentObject`1":
                                                {
                                                    sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                                                }
                                                break;
                                            case "IIntelligentObjectRuntimeData":
                                                {
                                                    sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                                                }
                                                break;
                                            case "IElementObject":
                                                {
                                                    sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                                                }
                                                break;
                                            case "IElementObject`1":
                                                {
                                                    sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                                                }
                                                break;
                                            case "IElementData":
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
                                            case "IAgentRuntimeData":
                                                {
                                                    sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                                                }
                                                break;
                                            case "IRuntimeLog":
                                                {
                                                    sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                                                }
                                                break;
                                            case "IRuntimeLogRecord":
                                                {
                                                    sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                                                }
                                                break;
                                            case "IRuntimeLogData":
                                                {
                                                    sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                                                }
                                                break;
                                            case "IRuntimeLog`1":
                                                {
                                                    sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                                                }
                                                break;
                                            case "IUnitizedPropertyDefinition":
                                                {
                                                    sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                                                }
                                                break;
                                            case "IObjectInstanceReferencePropertyDefinition":
                                                {
                                                    sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                                                }
                                                break;
                                            case "IProperty":
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
                                            case "IRealPropertyDefinition":
                                                {
                                                    sb.AppendLine(AddInfo(myType.Name, anyType.Name));
                                                }
                                                break;

                                            default:
                                                {
                                                    string xx = $"Type={anyType.Name}";
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

        /// <summary>
        /// Location Simio Property Types
        /// </summary>
        /// <param name="myType"></param>
        /// <param name="anyType"></param>
        /// <returns></returns>
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

        private void RefreshTabHeadlessRun()
        {
            try
            {
                textHeadlessRunFilesLocation.Text = Properties.Settings.Default.HeadlessSystemFolder;

                comboHeadlessRunExecutableToRun.DataSource = Directory.GetFiles(textHeadlessRunFilesLocation.Text, "*.EXE").ToList();

                textHeadlessRunProjectFile.Text = Properties.Settings.Default.HeadlessRunSimioProjectFile;

                comboHeadlessRunModels.Text = Properties.Settings.Default.HeadlessRunModel;
                comboHeadlessRunExperiments.Text = Properties.Settings.Default.HeadlessRunExperiment;

                HeadlessRunContext = new HeadlessContext(textHeadlessRunFilesLocation.Text);


            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Err={ex}");
            }
        }

        private void SaveTabHeadlessRun()
        {
            try
            {
                textHeadlessRunFilesLocation.Text = Properties.Settings.Default.HeadlessSystemFolder;

                comboHeadlessRunExecutableToRun.DataSource = Directory.GetFiles(textHeadlessRunFilesLocation.Text, "*.EXE").ToList();

                textHeadlessRunProjectFile.Text = Properties.Settings.Default.HeadlessRunSimioProjectFile;

                comboHeadlessRunModels.Text = Properties.Settings.Default.HeadlessRunModel;
                comboHeadlessRunExperiments.Text = Properties.Settings.Default.HeadlessRunExperiment;

            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Err={ex}");
            }

        }

        private void RefreshTabSettings()
        {
            try
            {
                propertyGrid1.SelectedObject = Properties.Settings.Default;

            }
            catch (Exception ex)
            {
                Logit($"Err={ex}");
            }
        }

        /// <summary>
        /// Put the selected files (DLLs, etc) except for those ignored into the checkbox list.
        /// </summary>
        /// <param name="includeUserFiles"></param>
        private void RefreshTabHeadlessBuilder( bool includeUserFiles )
        {
            try
            {
                textHeadlessBuildLocation.Text = SimioApiHelper.Properties.Settings.Default.HeadlessSystemFolder;
                textSimioInstallationFolder.Text = SimioApiHelper.Properties.Settings.Default.SimioInstallationFolder;

                buttonBuildHeadlessSystem.Enabled = false;

                if ( Directory.Exists(textHeadlessBuildLocation.Text) 
                    && Directory.Exists(textSimioInstallationFolder.Text))
                {
                    buttonBuildHeadlessSystem.Enabled = true;
                }

                if ( Directory.Exists(textSimioInstallationFolder.Text))
                {

                    List<string> ignorePatterns = new List<string>
                    {
                        "^LumenWorks",
                        "^DevExpress",
                        "^SPG",
                        "^WW",
                        "^Smart",
                        "^SharpDX",
                        "^Windows7",
                        "^NDepend",
                        "^NewtonSoft",
                        "^Ogre",
                        "^PlexityHide",
                        "^PSTaskDialog"
                    };

                    RefreshChecklistForTargets(textSimioInstallationFolder.Text, includeUserFiles, ignorePatterns);
                }

            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Err={ex}");
            }
        }

        /// <summary>
        /// Copy a list of files from source to target.
        /// The list can be full paths, but the filename is extracted.
        /// Copies are done with overwrite.
        /// </summary>
        /// <param name="sourceFolder"></param>
        /// <param name="targetFolder"></param>
        /// <param name="filepathList"></param>
        private void CopyList( string sourceFolder, string targetFolder, List<string> filepathList )
        {
            foreach (string filepath in filepathList)
            {
                string filename = Path.GetFileName(filepath);
                string sourcePath = Path.Combine(sourceFolder, filename);
                string targetPath = Path.Combine(targetFolder, filename);

                try
                {
                    File.Copy(filepath, targetPath, true);
                }
                catch (Exception ex)
                {
                    Logit($"Source={sourcePath} Target={targetPath} Err={ex.Message}");
                }
            } // foreach file

        }

        /// <summary>
        /// Rebuild the checklist for files to be included in the target folder.
        /// This includes 
        /// 1. files at the top of the SimioInstallPath (except those matching the ignore pattern)
        /// 2. DLLs under UserExtensions
        /// 3. Optionally DLLs under Documents > SimioUserExtensions.
        /// Log any duplicated names.
        /// </summary>
        /// <param name="simioInstallPath"></param>
        /// <param name="ignorePatternList"></param>
        /// <param name="includeUserExtension">Add files from person Documents > SimioUserExtensions</param>

        private void RefreshChecklistForTargets(string simioInstallPath, bool includeUserExtension, List<string> ignorePatternList )
        {
            string marker = "Begin";
            try
            {
                checklistSelectedFiles.Items.Clear();

                if ( !Directory.Exists(simioInstallPath) )
                {
                    Logit($"Warning: SimioInstallPath={simioInstallPath} cannot be found.");
                    return;
                }

                marker = $"Getting top-level files from {simioInstallPath}";
                string[] files = Directory.GetFiles(simioInstallPath, "*.DLL", SearchOption.TopDirectoryOnly);

                List<string> includedFiles = new List<string>();
                foreach ( string file in files)
                {
                    includedFiles.Add(file);
                }

                string dllPath = Path.Combine(simioInstallPath, "UserExtensions");
                marker = $"Getting files from UserExternsions folder={dllPath}";
                string[] folders = Directory.GetDirectories(dllPath);
                foreach ( string folder in folders)
                {
                    foreach ( string dllFile in Directory.GetFiles(folder, "*.dll", SearchOption.TopDirectoryOnly))
                    {
                        includedFiles.Add(dllFile);
                    }
                }

                // Filter and add to the checklist control
                List<string> filteredFiles = new List<string>();
                foreach ( string file in includedFiles )
                {
                    string fn = Path.GetFileName(file);
                    foreach ( string pattern in ignorePatternList )
                    {
                        if ( Regex.IsMatch(fn, pattern, RegexOptions.IgnoreCase))
                        {
                            goto GetNextFile;
                        }
                    }

                    filteredFiles.Add(file);

                GetNextFile:;
                } // foreach file

                // Check for user extensions
                if ( includeUserExtension )
                {
                    string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    marker = $"Getting DLL files from User's Userextensions={docPath}";
                    docPath = Path.Combine(docPath, "SimioUserExtensions");
                    if ( Directory.Exists(docPath))
                    {
                        foreach ( string dllFile in Directory.GetFiles(docPath, "*.dll") )
                        {
                            filteredFiles.Add(dllFile);
                        }
                    }
                }

                // All the files are now collected, so add them to the checklist, noting any duplicates that were found.
                Dictionary<string, string> filenameDict = new Dictionary<string, string>();
                int duplicateCount = 0;
                foreach (string filepath in filteredFiles)
                {
                    string fn = Path.GetFileName(filepath);
                    if (filenameDict.TryGetValue(fn, out string foundPath))
                    {
                        duplicateCount++;
                        Logit($"Warning: Duplicate Name={fn} File1={foundPath}, so File2={filepath} not added.");
                    }
                    else
                    {
                        filenameDict.Add(fn, filepath);
                        checklistSelectedFiles.Items.Add(filepath, true);
                    }
                }

                // Add the roaming licens
                checklistSelectedFiles.Items.Add(Path.Combine(simioInstallPath, "SimioRoam.lic"), true);

                if (duplicateCount > 0)
                    Alert($"Warning: While searching UserExtension files, there were {duplicateCount} duplicates. See the log for details.");

            }
            catch (Exception ex)
            {
                Alert($"SimioInstall={simioInstallPath}. Marker={marker} Err={ex.Message}");
            }
        }

        /// <summary>
        /// Refresh the dashboard, which shows settings and any errors within the settings.
        /// </summary>
        private void RefreshTabDashboard()
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
                Alert($"RefreshDashboard: Err={ex}");
                return;
            }
        }

        private void Alert(EnumLogFlags flags, string msg)
        {
            Loggerton.Instance.LogIt(flags, msg);
            MessageBox.Show(msg);
        }
        private void Alert(string msg)
        {
            Loggerton.Instance.LogIt(EnumLogFlags.Error, msg);
            MessageBox.Show(msg);
        }

        private void Logit(string msg)
        {
            SetStatusLabel(msg);
            Loggerton.Instance.LogIt(EnumLogFlags.Error, msg);
        }
        private void Logit(EnumLogFlags flags, string msg)
        {
            SetStatusLabel(msg);
            Loggerton.Instance.LogIt(flags, msg);
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
                Alert(EnumLogFlags.Error, $"Combo Error={ex}");
            }
        }


        /// <summary>
        /// Select and load a Simio Project.
        /// Done showing a 'wait' cursor, as the load can take a while.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonHeadlessRunSelectAndLoadProjectFile_Click(object sender, EventArgs e)
        {

            string marker = "Select the project";

            if ( HeadlessRunContext == null )
            {
                Alert($"HeadlessContext is null.");
                return;
            }

            try
            {
                string projectFile = HeadlessHelpers.GetProjectFile();
                textHeadlessRunProjectFile.Text = projectFile;

                Cursor.Current = Cursors.WaitCursor;
                if ( !HeadlessRunContext.LoadProject(projectFile, out string explanation))
                {
                    Alert(explanation);
                    return;
                }

                var project = HeadlessRunContext.CurrentProject;
                marker = $"Selected and loaded Project={project.Name} with {project.Models.Count} models.";
                Logit($"Info: {marker}");

                comboHeadlessRunModels.DataSource = project.Models.ToList();
                comboHeadlessRunModels.DisplayMember = "Name";

                SetStateTabHeadlessRun("ProjectLoaded");

            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Cannot Get/Load. Marker={marker} Err={ex.Message}");
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void buttonHeadlessRun_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                if (!HeadlessRunContext.RunModelPlan(out string explanation))
                {
                    Alert(explanation);
                }
                else
                {
                    Alert(EnumLogFlags.Information, $"Model={textHeadlessRunProjectFile.Text} ran the Plan successfully. Check the logs for more information.");
                }
            }
            catch (Exception ex)
            {
                Alert($"Err={ex.Message}");
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
                string extensionsPath = textHeadlessRunFilesLocation.Text;

                if (!HeadlessRunContext.RunModelExperiment( out string explanation))
                {
                    Alert(explanation);
                }
                else
                {
                    Alert(EnumLogFlags.Information, $"Model={HeadlessRunContext.CurrentModel.Name} Experiment={HeadlessRunContext.CurrentExperiment.Name} was run. Check the logs for more information.");
                }
            }
            catch (Exception ex)
            {
                Alert($"Err={ex.Message}");
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }

        }

        private void comboSimioLocation_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Choose a folder for the source of DLLs (e.g. ProgramFiles) and gather up
        /// all the files (except those in the exclude list). This includes the
        /// DLLs in the UserExtension folders. Optionally also gather up the
        /// DLLs in the user's SimioUserExtensions folder that lives under MyDocuments.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonSelectSimioInstallationFolder_Click(object sender, EventArgs e)
        {
            try
            {
                FolderBrowserDialog dialog = new FolderBrowserDialog();
                dialog.SelectedPath = Properties.Settings.Default.SimioInstallationFolder;
                dialog.ShowNewFolderButton = false;

                DialogResult result = dialog.ShowDialog();
                if (result != DialogResult.OK)
                    return;

                if ( !Directory.Exists(dialog.SelectedPath))
                {
                    Alert($"Folder={dialog.SelectedPath} does not exist.");
                    return;
                }

                textSimioInstallationFolder.Text = dialog.SelectedPath;
                Properties.Settings.Default.SimioInstallationFolder = dialog.SelectedPath;
                Properties.Settings.Default.Save();

                // Actions upon selection
                RefreshTabHeadlessBuilder( cbHeadlessBuildUsersFiles.Checked );

            }
            catch (Exception ex)
            {
                Alert($"Error selecting Simio installation location={ex}");
            }

        }

        private void buttonSelectHeadlessFolder_Click(object sender, EventArgs e)
        {
            try
            {
                FolderBrowserDialog dialog = new FolderBrowserDialog();
                dialog.SelectedPath = Properties.Settings.Default.HeadlessSystemFolder;
                dialog.ShowNewFolderButton = true;

                DialogResult result = dialog.ShowDialog();
                if (result != DialogResult.OK)
                    return;

                if ( !Directory.Exists(dialog.SelectedPath))
                {
                    Alert($"Folder={dialog.SelectedPath} does not exist.");
                    return;
                }

                textHeadlessBuildLocation.Text = dialog.SelectedPath;
                Properties.Settings.Default.HeadlessSystemFolder = dialog.SelectedPath;
                Properties.Settings.Default.Save();

                // Actions upon selection

            }
            catch (Exception ex)
            {
                Alert($"Error Selecting Headless Folder={ex.Message}");
            }

        }



        private void buttonBuildHeadlessSystem_Click(object sender, EventArgs e)
        {

            Cursor.Current = Cursors.WaitCursor;
            try
            {
                string simioFolder = textSimioInstallationFolder.Text;
                string buildFolder = textHeadlessBuildLocation.Text;

                if (!Directory.Exists(simioFolder))
                {
                    Alert($"Folder={simioFolder} does not exist.");
                    return;
                }

                if (!Directory.Exists(buildFolder))
                {
                    Alert($"Folder={buildFolder} does not exist.");
                    return;
                }


                // Copy minimal headless files
                List<string> filesToMove = new List<string>();
                foreach ( var item in checklistSelectedFiles.CheckedItems)
                {
                    filesToMove.Add(item as string);
                }

                Logit($"Info: Harvesting {filesToMove.Count} files.");
                CopyList(simioFolder, buildFolder, filesToMove);

                // Copy over optional DLLs

                // Create the minimal Build exe and batch

            }
            catch (Exception ex)
            {
                Alert($"Error Building Headless System={ex.Message}");
            }
            finally
            {
                Cursor.Current = Cursors.Default;
                Alert($"Info: Harvest is complete.");
            }
        }

        private void SetStateTabHeadlessRun(string state)
        {

            switch (state.ToUpper())
            {
                case "NULL":
                    {
                        comboHeadlessRunModels.Enabled = false;
                        comboHeadlessRunExperiments.Enabled = false;
                        buttonHeadlessRunExperiment.Enabled = false;
                        buttonHeadlessRunPlan.Enabled = false;
                        buttonHeadlessRunRiskAnalysis.Enabled = false;
                        buttonHeadlessRunSelectProjectFile.Enabled = false;
                    }
                    break;
                case "SETEXTENSIONS":
                    {
                        buttonHeadlessRunSelectProjectFile.Enabled = true;
                        comboHeadlessRunModels.Enabled = false;
                        comboHeadlessRunExperiments.Enabled = false;
                        buttonHeadlessRunExperiment.Enabled = false;
                        buttonHeadlessRunPlan.Enabled = false;
                        buttonHeadlessRunRiskAnalysis.Enabled = false;
                    }
                    break;
                case "PROJECTLOADED":
                    {
                        buttonHeadlessRunSelectProjectFile.Enabled = true;
                        comboHeadlessRunModels.Enabled = true;
                        comboHeadlessRunExperiments.Enabled = false;
                        buttonHeadlessRunExperiment.Enabled = false;
                        buttonHeadlessRunPlan.Enabled = false;
                        buttonHeadlessRunRiskAnalysis.Enabled = false;
                    }
                    break;
                case "MODELLOADED":
                    {
                        buttonHeadlessRunSelectProjectFile.Enabled = true;
                        comboHeadlessRunModels.Enabled = true;
                        comboHeadlessRunExperiments.Enabled = true;
                        buttonHeadlessRunExperiment.Enabled = false;
                        buttonHeadlessRunPlan.Enabled = true;
                        buttonHeadlessRunRiskAnalysis.Enabled = true;
                    }
                    break;
                case "EXPERIMENTLOADED":
                    {
                        buttonHeadlessRunSelectProjectFile.Enabled = true;
                        comboHeadlessRunModels.Enabled = true;
                        comboHeadlessRunExperiments.Enabled = true;
                        buttonHeadlessRunExperiment.Enabled = true;
                        buttonHeadlessRunPlan.Enabled = true;
                        buttonHeadlessRunRiskAnalysis.Enabled = true;
                    }
                    break;
                default:
                    {
                        Alert($"Unknown State={state}");
                    }
                    break;
            }

        }

        private void buttonChangeHeadlessLocation_Click(object sender, EventArgs e)
        {
            try
            {
                FolderBrowserDialog dialog = new FolderBrowserDialog();
                dialog.SelectedPath = Properties.Settings.Default.HeadlessSystemFolder;
                dialog.ShowNewFolderButton = true;

                DialogResult result = dialog.ShowDialog();
                if (result != DialogResult.OK)
                    return;

                SetStateTabHeadlessRun("Null");

                if (!Directory.Exists(dialog.SelectedPath))
                {
                    Alert($"Folder={dialog.SelectedPath} does not exist.");
                    return;
                }

                HeadlessRunContext = new HeadlessContext(dialog.SelectedPath);
                if ( HeadlessRunContext == null )
                {
                    Alert($"Cannot SetExtensions to={dialog.SelectedPath}");
                    return;
                }

                textHeadlessRunFilesLocation.Text = HeadlessRunContext.ExtensionsPath;

                // Put the executables in the combo dropdown, and make the first one the default.
                List<string> exeFiles = Directory.GetFiles(textHeadlessRunFilesLocation.Text, "*.exe").ToList();
                comboHeadlessRunExecutableToRun.DataSource = exeFiles;
                if (exeFiles.Any())
                    comboHeadlessRunExecutableToRun.Text = Path.GetFileName(exeFiles[0]);
                
                Properties.Settings.Default.HeadlessSystemFolder = dialog.SelectedPath;
                Properties.Settings.Default.Save();

                // Actions upon selection
                SetStateTabHeadlessRun("SetExtensions");

            }
            catch (Exception ex)
            {
                Alert($"Error Selecting Headless Folder={ex.Message}");
            }

            
        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RefreshForm();
        }

        private void comboHeadlessRunModels_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void comboHeadlessRunModels_SelectedValueChanged(object sender, EventArgs e)
        {
            var model = (IModel)(sender as ComboBox).SelectedItem;
            if (model != null)
            {
                //comboHeadlessRunExperiments.Items.Clear();
                comboHeadlessRunExperiments.DataSource = model.Experiments.ToList();
                comboHeadlessRunExperiments.DisplayMember = "Name";
            }
            HeadlessRunContext.CurrentModel = model;
            SetStateTabHeadlessRun("ModelLoaded");

        }

        /// <summary>
        /// Save the project file back to disk.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonSaveProject_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            string marker = "Begin";
            try
            {
                string projectPath = textHeadlessRunProjectFile.Text;

                if ( !HeadlessRunContext.SaveProject( projectPath, out string explanation) )
                { 
                    Alert(explanation);
                    return;
                }
                else
                {
                    Alert($"Project saved");
                }

            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Cannot Get/Load. Marker={marker} Err={ex.Message}");
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }

        }

        private void comboHeadlessRunExperiments_SelectedIndexChanged(object sender, EventArgs e)
        {
            var experiment = (sender as ComboBox).SelectedItem as IExperiment;
            if (experiment != null)
            {
                HeadlessRunContext.CurrentExperiment = experiment;
                SetStateTabHeadlessRun("ExperimentLoaded");
            }

        }

        private void buttonHeadlessRunRiskAnalysis_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                if (!HeadlessRunContext.RunModelRiskAnalysis(out string explanation))
                {
                    Alert(explanation);
                }
                else
                {
                    Alert(EnumLogFlags.Information, $"Model={textHeadlessRunProjectFile.Text} ran the RiskAnalysis successfully. Check the logs for more information.");
                }
            }
            catch (Exception ex)
            {
                Alert($"Err={ex.Message}");
                throw;
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }

        }

        private void buttonFileWatcherStart_Click(object sender, EventArgs e)
        {
            FileWatcher = new FileSystemWatcher();
            FileWatcher.Path = textFilewatcherPath.Text;
            FileWatcher.Filter = textFilewatcherFilter.Text;
            FileWatcher.IncludeSubdirectories = true;

            if (cbFwNotifyLastAccess.Checked)
                FileWatcher.NotifyFilter |= NotifyFilters.LastAccess;
            if (cbFwNotifyLastWrite.Checked)
                FileWatcher.NotifyFilter |= NotifyFilters.LastWrite;

            FileWatcher.Changed += OnChanged;
            FileWatcher.EnableRaisingEvents = true;

            buttonFileWatcherStart.Enabled = false;
            buttonFileWatcherStop.Enabled = true;
        }

        private void OnChanged(object fsw, FileSystemEventArgs fsea)
        {
            string msg = $"{DateTime.Now.ToString("HH:mm:ss.ff")}: {fsea.ChangeType.ToString()} {fsea.FullPath}\n";
            if (!cbFwPauseLogging.Checked)
            {
                //textFileWatcherLog.Invoke((MethodInvoker)(() => textFileWatcherLog.Text = msg));
                this.UIThread(() => this.textFileWatcherLog.AppendText(msg));
            }

        }

        private void FileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void buttonFileWatcherStop_Click(object sender, EventArgs e)
        {

            FileWatcher.Changed -= OnChanged;
            FileWatcher.Dispose();

            buttonFileWatcherStart.Enabled = true;
            buttonFileWatcherStop.Enabled = false;

        }

        private void textFileWatcherLog_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            textFileWatcherLog.Clear();
        }

        private void buttonFilewatcherSelect_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();

            DialogResult result = dialog.ShowDialog();
            if ( result == DialogResult.OK)
            {
                textFilewatcherPath.Text = dialog.SelectedPath;
            }
        }

        private void buttonHeadlessBuildAddExe_Click(object sender, EventArgs e)
        {
            string sourceFilepath = "";
            string targetFilepath = "";
            try
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Filter = "Exe File (*.exe)|*.exe";

                DialogResult result = dialog.ShowDialog();
                if (result != DialogResult.OK)
                    return;

                sourceFilepath = dialog.FileName;
                targetFilepath = Path.Combine(textHeadlessRunFilesLocation.Text, Path.GetFileName(sourceFilepath));

                if ( File.Exists(targetFilepath))
                {
                    result = MessageBox.Show($"File exists. Overwrite {targetFilepath}?");
                    if (result != DialogResult.OK)
                        return;

                    File.Delete(targetFilepath);
                }

                File.Copy(sourceFilepath, targetFilepath);

            }
            catch (Exception ex)
            {
                Alert($"Source={sourceFilepath} Target={targetFilepath} Err={ex.Message}");
            }
        }

        private void buttonTabSettingsSave_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Save();
            Properties.Settings.Default.Reload();
            this.RefreshForm();
        }

        private void buttonHeadlessRunExecutable_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(comboHeadlessRunExecutableToRun.Text))
                {
                    Alert($"Please select an executable and try again.");
                    return;
                }

                string folder = textHeadlessRunFilesLocation.Text;
                string exeName = comboHeadlessRunExecutableToRun.Text;
                string exePath = Path.Combine(folder, exeName);
                if ( !File.Exists(exePath) )
                {
                    Alert($"Cannot find Executable={exePath}");
                    return;
                }

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    CreateNoWindow = false,
                    UseShellExecute = false,
                    FileName = exePath,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                using (Process exeProcess = Process.Start(startInfo))
                {
                    exeProcess.WaitForExit();
                }

            }
            catch (Exception ex)
            {
                Alert($"Error={ex}");
            }
            

        }

        private void comboHeadlessRunExecutableToRun_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
