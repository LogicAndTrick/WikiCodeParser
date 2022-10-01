﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace LogicAndTrick.WikiCodeParser.Tests {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
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
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("LogicAndTrick.WikiCodeParser.Tests.Resources", typeof(Resources).Assembly);
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
        ///   Looks up a localized string similar to TWHL uses a custom text syntax that we call &lt;em&gt;WikiCode&lt;/em&gt;. It is a combination of Markdown and BBCode, with a few extra things to spice it up. If you&amp;#039;re familiar with either of these markup systems, you shouldn&amp;#039;t have too much trouble.
        ///&lt;h2 id=&quot;Basic_text_formatting&quot;&gt;Basic text formatting&lt;/h2&gt;
        ///There are two ways to do basic text formatting - the &lt;em&gt;Markdown&lt;/em&gt; way and the &lt;em&gt;BBCode&lt;/em&gt; way. Markdown style is usually faster to type, but BBCode is generally more flexible.
        ///&lt;table class=&quot;ta [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string TwhlFormattingWikiPageHtml {
            get {
                return ResourceManager.GetString("TwhlFormattingWikiPageHtml", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to [cat:Meta]
        ///
        ///TWHL uses a custom text syntax that we call /WikiCode/. It is a combination of Markdown and BBCode, with a few extra things to spice it up. If you&apos;re familiar with either of these markup systems, you shouldn&apos;t have too much trouble.
        ///
        ///== Basic text formatting
        ///
        ///There are two ways to do basic text formatting - the /Markdown/ way and the /BBCode/ way. Markdown style is usually faster to type, but BBCode is generally more flexible.
        ///
        ///|= Format | Markdown style | BBCode style
        ///|- *Bold text* |  [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string TwhlFormattingWikiPageInput {
            get {
                return ResourceManager.GetString("TwhlFormattingWikiPageInput", resourceCulture);
            }
        }
    }
}
