// This file is used by Code Analysis to maintain SuppressMessage 
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given 
// a specific target and scoped to a namespace, type, member, etc.
//
// To add a suppression to this file, right-click the message in the 
// Code Analysis results, point to "Suppress Message", and click 
// "In Suppression File".
// You do not need to add suppressions to this file manually.

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "DigitalRise.Animation")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "DigitalRise.Linq")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "DigitalRise.Mathematics")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "DigitalRise.Text")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "DigitalRise.Undo")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "System.Collections.Generic")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "DigitalRise.Collections.ObjectModel")]

// Parallel Tasks (Warnings on Xbox 360)
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2140:TransparentMethodsMustNotReferenceCriticalCodeFxCopRule", Scope = "member", Target = "DigitalRise.Threading.WorkItem.#.cctor()")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2140:TransparentMethodsMustNotReferenceCriticalCodeFxCopRule", Scope = "member", Target = "DigitalRise.Threading.Worker.#.cctor()")]

// dotNetZip
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "DigitalRise.Ionic.Zlib")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Scope = "namespace", Target = "DigitalRise.Ionic.Zlib")]
