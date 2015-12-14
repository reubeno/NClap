using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("NClap")]
[assembly: AssemblyDescription(".NET Command Line Argument Parser")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Microsoft Corporation")]
[assembly: AssemblyProduct("NClap")]
[assembly: AssemblyCopyright("Copyright © Microsoft Corporation. All right reserved.")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: AssemblyVersion("1.4.0.*")]
[assembly: AssemblyFileVersion("1.4.0")]

[assembly: InternalsVisibleTo("NClap.Repl, PublicKey=00240000048000009400000006020000002400005253413100040000010001004d089a4ac1afb2e8cf8090691f6eba11a2dc12e19604de336beac607870c7217b95b3b973afb82313be1d222b3adc1d6de9f735b39ebaee5d80f5044dfaef2cb058c3c0fe96790922c4c56458eb0e18d1a88c0146f5c56de3d49d693b1bdd8e08c4f61851dd7037e00236e0c11666e6cf34c146b7dd46e2e9321410ef0360683")]
[assembly: InternalsVisibleTo("NClap.Tests, PublicKey=00240000048000009400000006020000002400005253413100040000010001004d089a4ac1afb2e8cf8090691f6eba11a2dc12e19604de336beac607870c7217b95b3b973afb82313be1d222b3adc1d6de9f735b39ebaee5d80f5044dfaef2cb058c3c0fe96790922c4c56458eb0e18d1a88c0146f5c56de3d49d693b1bdd8e08c4f61851dd7037e00236e0c11666e6cf34c146b7dd46e2e9321410ef0360683")]

[assembly: CLSCompliant(true)]
[assembly: ComVisible(false)]
[assembly: Guid("ce1a820d-79f3-410d-b869-884dea01fbe6")]
[assembly: NeutralResourcesLanguage("en-us")]

[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "NClap.Parser", Justification = "We will fix this later")]
[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "NClap.Metadata", Justification = "We will fix this later")]
[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "NClap.Types", Justification = "We will fix this later")]

[assembly: SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "flags", Scope = "member", Target = "NClap.Metadata.PositionalArgumentAttribute.#.ctor(NClap.Metadata.ArgumentFlags)", Justification = "We will fix this if we come up with a better name")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "flags", Scope = "member", Target = "NClap.Metadata.ArgumentAttribute.#.ctor(NClap.Metadata.ArgumentFlags)", Justification = "We will fix this if we come up with a better name")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Flags", Scope = "type", Target = "NClap.Metadata.ArgumentFlags", Justification = "We will fix this if we come up with a better name")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Flags", Scope = "member", Target = "NClap.Metadata.ArgumentBaseAttribute.#Flags", Justification = "We will fix this if we come up with a better name")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "flags", Scope = "member", Target = "NClap.Metadata.ArgumentBaseAttribute.#.ctor(NClap.Metadata.ArgumentFlags)", Justification = "We will fix this if we come up with a better name")]

[assembly: SuppressMessage("Microsoft.Usage", "CA2243:AttributeStringLiteralsShouldParseCorrectly", Justification = "It should be okay to append text to an informational version.")]

[assembly: SuppressMessage("Microsoft.Design", "CA1018:MarkAttributesWithAttributeUsage", Scope = "type", Target = "NClap.Metadata.ArgumentAttribute", Justification = "Redundant attribute specification is undesired")]
[assembly: SuppressMessage("Microsoft.Design", "CA1018:MarkAttributesWithAttributeUsage", Scope = "type", Target = "NClap.Metadata.PositionalArgumentAttribute", Justification = "Redundant attribute specification is undesired")]
[assembly: SuppressMessage("Microsoft.Design", "CA1018:MarkAttributesWithAttributeUsage", Scope = "type", Target = "NClap.Metadata.MustBeGreaterThanAttribute", Justification = "Redundant attribute specification is undesired")]
[assembly: SuppressMessage("Microsoft.Design", "CA1018:MarkAttributesWithAttributeUsage", Scope = "type", Target = "NClap.Metadata.MustBeGreaterThanOrEqualToAttribute", Justification = "Redundant attribute specification is undesired")]
[assembly: SuppressMessage("Microsoft.Design", "CA1018:MarkAttributesWithAttributeUsage", Scope = "type", Target = "NClap.Metadata.MustBeLessThanAttribute", Justification = "Redundant attribute specification is undesired")]
[assembly: SuppressMessage("Microsoft.Design", "CA1018:MarkAttributesWithAttributeUsage", Scope = "type", Target = "NClap.Metadata.MustBeLessThanOrEqualToAttribute", Justification = "Redundant attribute specification is undesired")]
[assembly: SuppressMessage("Microsoft.Design", "CA1018:MarkAttributesWithAttributeUsage", Scope = "type", Target = "NClap.Metadata.MustExistAttribute", Justification = "Redundant attribute specification is undesired")]
[assembly: SuppressMessage("Microsoft.Design", "CA1018:MarkAttributesWithAttributeUsage", Scope = "type", Target = "NClap.Metadata.MustMatchRegexAttribute", Justification = "Redundant attribute specification is undesired")]
[assembly: SuppressMessage("Microsoft.Design", "CA1018:MarkAttributesWithAttributeUsage", Scope = "type", Target = "NClap.Metadata.MustNotBeAttribute", Justification = "Redundant attribute specification is undesired")]
[assembly: SuppressMessage("Microsoft.Design", "CA1018:MarkAttributesWithAttributeUsage", Scope = "type", Target = "NClap.Metadata.MustNotBeEmptyAttribute", Justification = "Redundant attribute specification is undesired")]
[assembly: SuppressMessage("Microsoft.Design", "CA1018:MarkAttributesWithAttributeUsage", Scope = "type", Target = "NClap.Metadata.MustNotExistAttribute", Justification = "Redundant attribute specification is undesired")]
[assembly: SuppressMessage("Microsoft.Design", "CA1018:MarkAttributesWithAttributeUsage", Scope = "type", Target = "NClap.Metadata.MustNotMatchRegexAttribute", Justification = "Redundant attribute specification is undesired")]
