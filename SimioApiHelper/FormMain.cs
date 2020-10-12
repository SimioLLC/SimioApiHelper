using SimEngineLibrary;
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

        SimEngineContext SimEngineRunContext { get; set; }

        FileSystemWatcher FileWatcher { get; set; }

        /// <summary>
        /// A DLLs list of the assemblies it is dependent upon
        /// </summary>
        List<AssemblyReference> DependencyList = new List<AssemblyReference>();

        public FormMain()
        {
            InitializeComponent();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            try
            {
                Logit(EnumLogFlags.Information, $"Begin Machine={Environment.MachineName}");

                string simEngineTestFolder = CheckSimEngineTestFolder();
                Logit($"Info: Test Folder Location={simEngineTestFolder}");

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
                RefreshTabSimEngineBuilder(true);
                RefreshTabSimEngineRun();
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

                textAssemblyLoadInfo.Text = LoadAssembly(assemblyPath, showSimioOnly, DependencyList);

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
        private string LoadAssembly( string assemblyFile, bool showSimioOnly, List<AssemblyReference> dependencyList )
        {
            if (!IsLoaded)
                return "$(File={assemblyFile}Not Loaded)";

            dependencyList.Clear();

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
                    
                    AssemblyName aName = AssemblyName.GetAssemblyName(myAssembly.Location);
                    AssemblyReference aReference = new AssemblyReference(aName, -1);

                    Dictionary<string, AssemblyReference> dependencyDict = new Dictionary<string, AssemblyReference>();
                    Stack<AssemblyReference> stack = new Stack<AssemblyReference>();
                    if ( !DLLHelpers.GetDependencies(aReference, dependencyDict, stack, out string explanation ))
                    {
                        ExceptionLog($"Marker={marker} Err={explanation}");
                        sb.AppendLine($" ** Error getting dependencies={explanation}");
                        Logit($"Error: Cannot get dependencies. Err={explanation}");
                    }
                    else
                    {
                        Logit($"Info: Found {dependencyDict.Count} dependices. Stack count={stack.Count}");
                        sb.AppendLine($"  There are {dependencyDict.Count} unique dependencies:");
                        sb.AppendLine($"");
                        int nn = 0;
                        
                        foreach ( AssemblyReference aRef in dependencyDict.Values )
                        {
                            sb.AppendLine($"{nn++} {aRef.Name} Full={aRef.FullName}");
                            if (string.IsNullOrEmpty(aRef.AssemblyPath))
                                sb.AppendLine($"    No assembly path (perhaps in GAC)");
                            else
                                sb.AppendLine($"    Path={aRef.AssemblyPath}");

                            dependencyList.Add(aRef);
                        }
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

        /// <summary>
        /// Check for the simEngine test folder.
        /// If found (or created) return the path and add to Settings.
        /// </summary>
        /// <returns></returns>
        private string CheckSimEngineTestFolder()
        {
            string marker = "Begin";
            try
            {
                string simEngineTestFolder = Properties.Settings.Default.SimEngineSystemFolder;

                // If there is no test folder, then try to create one under MyDocuments
                if (!Directory.Exists(simEngineTestFolder))
                {
                    string myDocsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    simEngineTestFolder = Path.Combine(myDocsFolder, "SimioAutomationTest");
                    marker = $"Attempting to create SimEngine Test Folder at={simEngineTestFolder}";
                    if (!Directory.Exists(simEngineTestFolder))
                    {
                        Logit($"Info: Creating new SimEngineSystemFolder={simEngineTestFolder}");
                        Directory.CreateDirectory(Path.Combine(myDocsFolder, "SimioAutomationTest"));
                    }

                    Properties.Settings.Default.SimEngineSystemFolder = simEngineTestFolder;
                    Logit($"Info: Set SimEngineSystemFolder to={simEngineTestFolder}");
                }
                return simEngineTestFolder;
            }
            catch (Exception ex)
            {
                Logit($"Marker={marker} Err={ex.Message}");
                return string.Empty;
            }
        }

        private void RefreshTabSimEngineRun()
        {
            string marker = "Begin";
            try
            {
                string simEngineTestFolder = CheckSimEngineTestFolder();

                textSimEngineRunFilesLocation.Text = simEngineTestFolder;

                comboSimEngineRunExecutableToRun.DataSource = Directory.GetFiles(textSimEngineRunFilesLocation.Text, "*.EXE").ToList();

                textSimEngineRunProjectFile.Text = Properties.Settings.Default.SimEngineRunSimioProjectFile;

                comboSimEngineRunModels.Text = Properties.Settings.Default.SimEngineRunModel;
                comboSimEngineRunExperiments.Text = Properties.Settings.Default.SimEngineRunExperiment;

                SimEngineRunContext = new SimEngineContext(textSimEngineRunFilesLocation.Text);


            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Marker={marker} Err={ex}");
            }
        }

        private void SaveTabSimEngineRun()
        {
            try
            {
                textSimEngineRunFilesLocation.Text = Properties.Settings.Default.SimEngineSystemFolder;

                comboSimEngineRunExecutableToRun.DataSource = Directory.GetFiles(textSimEngineRunFilesLocation.Text, "*.EXE").ToList();

                textSimEngineRunProjectFile.Text = Properties.Settings.Default.SimEngineRunSimioProjectFile;

                comboSimEngineRunModels.Text = Properties.Settings.Default.SimEngineRunModel;
                comboSimEngineRunExperiments.Text = Properties.Settings.Default.SimEngineRunExperiment;

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
        private void RefreshTabSimEngineBuilder( bool includeUserFiles )
        {
            try
            {
                textHarvestTargetFolder.Text = SimioApiHelper.Properties.Settings.Default.SimEngineSystemFolder;
                textHarvestSourceFolder.Text = SimioApiHelper.Properties.Settings.Default.SimioInstallationFolder;

                buttonBuildSimEngineSystem.Enabled = false;

                if ( Directory.Exists(textHarvestTargetFolder.Text) 
                    && Directory.Exists(textHarvestSourceFolder.Text))
                {
                    buttonBuildSimEngineSystem.Enabled = true;
                }

                if ( Directory.Exists(textHarvestSourceFolder.Text))
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

                    RefreshChecklistForTargets(textHarvestSourceFolder.Text, includeUserFiles, ignorePatterns);
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

                // Add the roaming license
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
            StatusStripBottom.Text = msg;
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
        private void buttonSimEngineRunSelectAndLoadProjectFile_Click(object sender, EventArgs e)
        {

            string marker = "Select the project";

            if ( SimEngineRunContext == null )
            {
                Alert($"SimEngineContext is null.");
                return;
            }

            try
            {
                string projectFile = SimEngineHelpers.GetProjectFile();
                textSimEngineRunProjectFile.Text = projectFile;

                Cursor.Current = Cursors.WaitCursor;
                if ( !SimEngineRunContext.LoadProject(projectFile, out string explanation))
                {
                    Alert(explanation);
                    return;
                }

                var project = SimEngineRunContext.CurrentProject;
                marker = $"Selected and loaded Project={project.Name} with {project.Models.Count} models.";
                Logit($"Info: {marker}");

                comboSimEngineRunModels.DataSource = project.Models.ToList();
                comboSimEngineRunModels.DisplayMember = "Name";

                List<string> errorList = new List<string>();
                foreach ( IModel model in project.Models)
                {
                    if ( !SimEngineRunContext.LoadModel(model.Name, out explanation))
                    {
                        Alert(explanation);
                        return;
                    }
                }

                SetStateTabSimEngineRun("ProjectLoaded");
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

        private void buttonSimEngineRun_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                if (!SimEngineRunContext.RunModelPlan(out string explanation))
                {
                    Alert(explanation);
                }
                else
                {
                    Alert(EnumLogFlags.Information, $"Model={textSimEngineRunProjectFile.Text} ran the Plan successfully. Check the logs for more information.");
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
                string extensionsPath = textSimEngineRunFilesLocation.Text;
                string resultsPath = textResultsPath.Text;

                if (!SimEngineRunContext.RunModelExperiment( resultsPath, out string explanation))
                {
                    Alert(explanation);
                }
                else
                {
                    Alert(EnumLogFlags.Information, $"Model={SimEngineRunContext.CurrentModel.Name} Experiment={SimEngineRunContext.CurrentExperiment.Name} was run. Check the logs for more information.");
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

                textHarvestSourceFolder.Text = dialog.SelectedPath;
                Properties.Settings.Default.SimioInstallationFolder = dialog.SelectedPath;
                Properties.Settings.Default.Save();

                // Actions upon selection
                RefreshTabSimEngineBuilder( cbSimEngineBuildUsersFiles.Checked );

            }
            catch (Exception ex)
            {
                Alert($"Error selecting Simio installation location={ex}");
            }

        }

        private void buttonSelectSimEngineFolder_Click(object sender, EventArgs e)
        {
            try
            {
                FolderBrowserDialog dialog = new FolderBrowserDialog();
                dialog.SelectedPath = Properties.Settings.Default.SimEngineSystemFolder;
                dialog.ShowNewFolderButton = true;

                DialogResult result = dialog.ShowDialog();
                if (result != DialogResult.OK)
                    return;

                if ( !Directory.Exists(dialog.SelectedPath))
                {
                    Alert($"Folder={dialog.SelectedPath} does not exist.");
                    return;
                }

                textHarvestTargetFolder.Text = dialog.SelectedPath;
                Properties.Settings.Default.SimEngineSystemFolder = dialog.SelectedPath;
                Properties.Settings.Default.Save();

                // Actions upon selection

            }
            catch (Exception ex)
            {
                Alert($"Error Selecting SimEngine Folder={ex.Message}");
            }

        }



        private void buttonBuildSimEngineSystem_Click(object sender, EventArgs e)
        {

            Cursor.Current = Cursors.WaitCursor;
            int fileCount = 0;
            try
            {
                string simioFolder = textHarvestSourceFolder.Text;
                string buildFolder = textHarvestTargetFolder.Text;

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


                // Copy minimal simEngine files
                List<string> filesToMove = new List<string>();
                foreach ( var item in checklistSelectedFiles.CheckedItems)
                {
                    filesToMove.Add(item as string);
                }

                fileCount = filesToMove.Count;
                Logit($"Info: Harvesting {fileCount} files.");
                CopyList(simioFolder, buildFolder, filesToMove);

            }
            catch (Exception ex)
            {
                Alert($"Error Building SimEngine System={ex.Message}");
            }
            finally
            {
                Cursor.Current = Cursors.Default;
                Alert($"Info: Harvest is complete. {fileCount} files copied.");
            }
        }

        private void SetStateTabSimEngineRun(string state)
        {

            switch (state.ToUpper())
            {
                case "NULL":
                    {
                        comboSimEngineRunModels.Enabled = false;
                        comboSimEngineRunExperiments.Enabled = false;
                        buttonSimEngineRunExperiment.Enabled = false;
                        buttonSimEngineRunPlan.Enabled = false;
                        buttonSimEngineRunRiskAnalysis.Enabled = false;
                        buttonSimEngineRunSelectProjectFile.Enabled = false;
                    }
                    break;
                case "SETEXTENSIONS":
                    {
                        buttonSimEngineRunSelectProjectFile.Enabled = true;
                        comboSimEngineRunModels.Enabled = false;
                        comboSimEngineRunExperiments.Enabled = false;
                        buttonSimEngineRunExperiment.Enabled = false;
                        buttonSimEngineRunPlan.Enabled = false;
                        buttonSimEngineRunRiskAnalysis.Enabled = false;
                    }
                    break;
                case "PROJECTLOADED":
                    {
                        buttonSimEngineRunSelectProjectFile.Enabled = true;
                        comboSimEngineRunModels.Enabled = true;
                        comboSimEngineRunExperiments.Enabled = false;
                        buttonSimEngineRunExperiment.Enabled = false;
                        buttonSimEngineRunPlan.Enabled = false;
                        buttonSimEngineRunRiskAnalysis.Enabled = false;
                    }
                    break;
                case "MODELLOADED":
                    {
                        buttonSimEngineRunSelectProjectFile.Enabled = true;
                        comboSimEngineRunModels.Enabled = true;
                        comboSimEngineRunExperiments.Enabled = true;
                        buttonSimEngineRunExperiment.Enabled = true;
                        buttonSimEngineRunPlan.Enabled = true;
                        buttonSimEngineRunRiskAnalysis.Enabled = true;
                    }
                    break;
                case "EXPERIMENTLOADED":
                    {
                        buttonSimEngineRunSelectProjectFile.Enabled = true;
                        comboSimEngineRunModels.Enabled = true;
                        comboSimEngineRunExperiments.Enabled = true;
                        buttonSimEngineRunExperiment.Enabled = true;
                        buttonSimEngineRunPlan.Enabled = true;
                        buttonSimEngineRunRiskAnalysis.Enabled = true;
                    }
                    break;
                default:
                    {
                        Alert($"Unknown State={state}");
                    }
                    break;
            }

        }

        /// <summary>
        /// Change the location of the EXE,DLLs, ... aka the SetExtensions location.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonChangeSimEngineLocation_Click(object sender, EventArgs e)
        {
            try
            {
                FolderBrowserDialog dialog = new FolderBrowserDialog();
                dialog.SelectedPath = Properties.Settings.Default.SimEngineSystemFolder;
                dialog.ShowNewFolderButton = true;

                DialogResult result = dialog.ShowDialog();
                if (result != DialogResult.OK)
                    return;

                SetStateTabSimEngineRun("Null");

                if (!Directory.Exists(dialog.SelectedPath))
                {
                    Alert($"Folder={dialog.SelectedPath} does not exist.");
                    return;
                }

                SimEngineRunContext = new SimEngineContext(dialog.SelectedPath);
                if ( SimEngineRunContext == null )
                {
                    Alert($"Cannot SetExtensions to={dialog.SelectedPath}");
                    return;
                }

                textSimEngineRunFilesLocation.Text = SimEngineRunContext.ExtensionsPath;

                // Put the executables in the combo dropdown, and make the first one the default.
                List<string> exeFiles = Directory.GetFiles(textSimEngineRunFilesLocation.Text, "*.exe").ToList();
                comboSimEngineRunExecutableToRun.DataSource = exeFiles;
                if (exeFiles.Any())
                    comboSimEngineRunExecutableToRun.Text = Path.GetFileName(exeFiles[0]);
                
                Properties.Settings.Default.SimEngineSystemFolder = dialog.SelectedPath;
                Properties.Settings.Default.Save();

                // Actions upon selection
                SetStateTabSimEngineRun("SetExtensions");

            }
            catch (Exception ex)
            {
                Alert($"Error Selecting SimEngine Folder={ex.Message}");
            }

            
        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RefreshForm();
        }

        private void comboSimEngineRunModels_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void comboSimEngineRunModels_SelectedValueChanged(object sender, EventArgs e)
        {
            var model = (IModel)(sender as ComboBox).SelectedItem;
            if (model != null)
            {
                if ( model.Experiments.Any())
                {
                    buttonSimEngineRunExperiment.Enabled = true;
                    comboSimEngineRunExperiments.DataSource = model.Experiments.ToList();
                    comboSimEngineRunExperiments.DisplayMember = "Name";
                }
                else
                {
                    buttonSimEngineRunExperiment.Enabled = false;
                    comboSimEngineRunExperiments.DataSource = null;
                    comboSimEngineRunExperiments.Text = "";
                }
            }

            SimEngineRunContext.CurrentModel = model;
            SetStateTabSimEngineRun("ModelLoaded");

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
                string projectPath = textSimEngineRunProjectFile.Text;

                if ( !SimEngineRunContext.SaveProject( projectPath, out string explanation) )
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

        private void comboSimEngineRunExperiments_SelectedIndexChanged(object sender, EventArgs e)
        {
            var experiment = (sender as ComboBox).SelectedItem as IExperiment;
            if (experiment != null)
            {
                SimEngineRunContext.CurrentExperiment = experiment;
                SetStateTabSimEngineRun("ExperimentLoaded");
            }

        }

        private void buttonSimEngineRunRiskAnalysis_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                if (!SimEngineRunContext.RunModelRiskAnalysis(out string explanation))
                {
                    Alert(explanation);
                }
                else
                {
                    Alert(EnumLogFlags.Information, $"Model={textSimEngineRunProjectFile.Text} ran the RiskAnalysis successfully. Check the logs for more information.");
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

        private void buttonSimEngineBuildAddExe_Click(object sender, EventArgs e)
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
                targetFilepath = Path.Combine(textSimEngineRunFilesLocation.Text, Path.GetFileName(sourceFilepath));

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

        private void buttonSimEngineRunExecutable_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(comboSimEngineRunExecutableToRun.Text))
                {
                    Alert($"Please select an executable and try again.");
                    return;
                }

                string folder = textSimEngineRunFilesLocation.Text;
                string exeName = comboSimEngineRunExecutableToRun.Text;
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

        private void comboSimEngineRunExecutableToRun_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private bool AddDependenciesToHarvestTarget(List<AssemblyReference> DependencyList, 
            string simioInstallFolder, string targetFolder, out string explanation)
        {
            explanation = "";
            try
            {
                if (!DependencyList.Any())
                {
                    explanation = $"Nothing in Dependency List";
                    return false;
                }

                if (!Directory.Exists(simioInstallFolder))
                {
                    explanation = $"Harvest source({simioInstallFolder} not found.";
                    return false;
                }
                if (!Directory.Exists(targetFolder))
                {
                    explanation = $"Harvest target({targetFolder} not found.";
                    return false;
                }

                foreach (AssemblyReference aRef in DependencyList)
                {
                    string filename = Path.GetFileName(aRef.AssemblyPath);

                    string targetPath = Path.Combine(targetFolder, filename);
                    if ( !File.Exists(targetPath))
                    {
                        string sourcePath = Path.Combine(simioInstallFolder, filename);
                        if (File.Exists(sourcePath))
                        {
                            File.Copy(sourcePath, targetPath, false);
                            Logit($"Info: Copied file from {sourcePath} to {targetPath}");
                        }
                        else
                            Logit($"Warning: Could not find source file={sourcePath}");
                    }

                }

                return true;
            }
            catch (Exception ex)
            {
                explanation = $"TargetFolder={targetFolder} Err={ex.Message}";
                return false;
            }
        }

        private void buttonAddDependentsToHarvest_Click(object sender, EventArgs e)
        {
            if (!AddDependenciesToHarvestTarget(DependencyList, textHarvestSourceFolder.Text, textHarvestTargetFolder.Text, out string explanation))
                Alert(explanation);
        }

        private void buttonResultsPath_Click(object sender, EventArgs e)
        {
            string marker = "Select the path";
            var project = SimEngineRunContext.CurrentProject;

            if ( project == null )
            {
                Alert($"No Current Project file selected");
            }

            try
            {
                string projectFile = textSimEngineRunProjectFile.Text;
                if (projectFile == null)
                {
                    Alert($"No Project file selected");
                }

                string name = Path.GetFileNameWithoutExtension(projectFile);
                string folder = Path.GetDirectoryName(projectFile);

                SaveFileDialog dialog = new SaveFileDialog();
                dialog.OverwritePrompt = true;
                dialog.FileName = Path.Combine(folder, name + ".csv");
                dialog.Filter = "CSV File (*.csv)|*.csv";

                DialogResult result = dialog.ShowDialog();

                if ( result == DialogResult.OK)
                {
                    textResultsPath.Text = dialog.FileName;
                }

            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Cannot Get Results Path. Marker={marker} Err={ex.Message}");
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }

        }
    }
}
