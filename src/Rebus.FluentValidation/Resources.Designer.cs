﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Rebus.FluentValidation {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Rebus.FluentValidation.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The message type &apos;{0}&apos; is already configured to be handled by &apos;{1}&apos;..
        /// </summary>
        internal static string ArgumentNullException_MessageTypeAlreadyConfigured {
            get {
                return ResourceManager.GetString("ArgumentNullException_MessageTypeAlreadyConfigured", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The &apos;{0}&apos; method does not exist..
        /// </summary>
        internal static string InvalidOperationException_WrapMethodDoesNotExist {
            get {
                return ResourceManager.GetString("InvalidOperationException_WrapMethodDoesNotExist", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Could not wrap {0} in ValidationFailedWrapper&lt;&gt;.
        /// </summary>
        internal static string RebusApplicationException_CouldNotWrapMessage {
            get {
                return ResourceManager.GetString("RebusApplicationException_CouldNotWrapMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Message {0} failed to validate.
        ///{1}.
        /// </summary>
        internal static string ValidationFailed {
            get {
                return ResourceManager.GetString("ValidationFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Validation -&gt; {0} {1} is configured to be dropped..
        /// </summary>
        internal static string ValidationFailed_Drop {
            get {
                return ResourceManager.GetString("ValidationFailed_Drop", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Validation -&gt; {0} {1} is configured to be moved to error queue..
        /// </summary>
        internal static string ValidationFailed_MovingToErrorQueue {
            get {
                return ResourceManager.GetString("ValidationFailed_MovingToErrorQueue", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Validation -&gt; {0} {1} is configured to pass through..
        /// </summary>
        internal static string ValidationFailed_PassThrough {
            get {
                return ResourceManager.GetString("ValidationFailed_PassThrough", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Validation -&gt; {0} {1} is configured to be wrapped as {2}..
        /// </summary>
        internal static string ValidationFailed_WrapAsValidationFailed {
            get {
                return ResourceManager.GetString("ValidationFailed_WrapAsValidationFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Message {0} successfully validate..
        /// </summary>
        internal static string ValidationSucceeded {
            get {
                return ResourceManager.GetString("ValidationSucceeded", resourceCulture);
            }
        }
    }
}
