﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SimioApiHelper.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "15.9.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("c:\\(test)\\SimioHeadlessTest")]
        public string HeadlessSystemFolder {
            get {
                return ((string)(this["HeadlessSystemFolder"]));
            }
            set {
                this["HeadlessSystemFolder"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("c:\\program files (x86)\\simio")]
        public string SimioInstallationFolder {
            get {
                return ((string)(this["SimioInstallationFolder"]));
            }
            set {
                this["SimioInstallationFolder"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string HeadlessRunModel {
            get {
                return ((string)(this["HeadlessRunModel"]));
            }
            set {
                this["HeadlessRunModel"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string HeadlessRunExperiment {
            get {
                return ((string)(this["HeadlessRunExperiment"]));
            }
            set {
                this["HeadlessRunExperiment"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string HeadlessBuilderTargetFolder {
            get {
                return ((string)(this["HeadlessBuilderTargetFolder"]));
            }
            set {
                this["HeadlessBuilderTargetFolder"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string HeadlessRunSimioProjectFile {
            get {
                return ((string)(this["HeadlessRunSimioProjectFile"]));
            }
            set {
                this["HeadlessRunSimioProjectFile"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string HeadlessRunSimioProjectsFolder {
            get {
                return ((string)(this["HeadlessRunSimioProjectsFolder"]));
            }
            set {
                this["HeadlessRunSimioProjectsFolder"] = value;
            }
        }
    }
}
