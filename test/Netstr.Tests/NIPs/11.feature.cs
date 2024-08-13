﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (https://www.specflow.org/).
//      SpecFlow Version:3.9.0.0
//      SpecFlow Generator Version:3.9.0.0
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace Netstr.Tests.NIPs
{
    using TechTalk.SpecFlow;
    using System;
    using System.Linq;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "3.9.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public partial class NIP_11Feature : object, Xunit.IClassFixture<NIP_11Feature.FixtureData>, System.IDisposable
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
        private static string[] featureTags = ((string[])(null));
        
        private Xunit.Abstractions.ITestOutputHelper _testOutputHelper;
        
#line 1 "11.feature"
#line hidden
        
        public NIP_11Feature(NIP_11Feature.FixtureData fixtureData, Netstr_Tests_XUnitAssemblyFixture assemblyFixture, Xunit.Abstractions.ITestOutputHelper testOutputHelper)
        {
            this._testOutputHelper = testOutputHelper;
            this.TestInitialize();
        }
        
        public static void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "NIPs", "NIP-11", "\tRelays may provide server metadata to clients to inform them of capabilities, ad" +
                    "ministrative contacts, and various server attributes.\r\n\tThis is made available a" +
                    "s a JSON document over HTTP, on the same URI as the relay\'s websocket.", ProgrammingLanguage.CSharp, featureTags);
            testRunner.OnFeatureStart(featureInfo);
        }
        
        public static void FeatureTearDown()
        {
            testRunner.OnFeatureEnd();
            testRunner = null;
        }
        
        public void TestInitialize()
        {
        }
        
        public void TestTearDown()
        {
            testRunner.OnScenarioEnd();
        }
        
        public void ScenarioInitialize(TechTalk.SpecFlow.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioInitialize(scenarioInfo);
            testRunner.ScenarioContext.ScenarioContainer.RegisterInstanceAs<Xunit.Abstractions.ITestOutputHelper>(_testOutputHelper);
        }
        
        public void ScenarioStart()
        {
            testRunner.OnScenarioStart();
        }
        
        public void ScenarioCleanup()
        {
            testRunner.CollectScenarioErrors();
        }
        
        public virtual void FeatureBackground()
        {
#line 5
#line hidden
#line 6
 testRunner.Given("a relay is running", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
            TechTalk.SpecFlow.Table table42 = new TechTalk.SpecFlow.Table(new string[] {
                        "PublicKey",
                        "PrivateKey"});
            table42.AddRow(new string[] {
                        "5758137ec7f38f3d6c3ef103e28cd9312652285dab3497fe5e5f6c5c0ef45e75",
                        "nsec12y4pgafw6kpcqjtfyrdyxtcupnddj5kdft768kdl55wzq50ervpqauqnw4"});
#line 7
 testRunner.And("Alice is connected to relay", ((string)(null)), table42, "And ");
#line hidden
        }
        
        void System.IDisposable.Dispose()
        {
            this.TestTearDown();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Relay sends an information document")]
        [Xunit.TraitAttribute("FeatureTitle", "NIP-11")]
        [Xunit.TraitAttribute("Description", "Relay sends an information document")]
        public void RelaySendsAnInformationDocument()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Relay sends an information document", "\tGET HTTP request to the websockets endpoint with a application/nostr+json Accept" +
                    " header should\r\n\tproduce a json Relay Information Document", tagsOfScenario, argumentsOfScenario, featureTags);
#line 11
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 5
this.FeatureBackground();
#line hidden
                TechTalk.SpecFlow.Table table43 = new TechTalk.SpecFlow.Table(new string[] {
                            "Header",
                            "Value"});
                table43.AddRow(new string[] {
                            "Accept",
                            "application/nostr+json"});
#line 14
 testRunner.When("Alice sends a GET HTTP request to its websockets endpoint", ((string)(null)), table43, "When ");
#line hidden
                TechTalk.SpecFlow.Table table44 = new TechTalk.SpecFlow.Table(new string[] {
                            "Header",
                            "Value"});
                table44.AddRow(new string[] {
                            "Access-Control-Allow-Origin",
                            "*"});
#line 17
 testRunner.Then("Alice receives a response with headers", ((string)(null)), table44, "Then ");
#line hidden
                TechTalk.SpecFlow.Table table45 = new TechTalk.SpecFlow.Table(new string[] {
                            "Field",
                            "Type"});
                table45.AddRow(new string[] {
                            "name",
                            "string"});
                table45.AddRow(new string[] {
                            "description",
                            "string"});
                table45.AddRow(new string[] {
                            "contact",
                            "string"});
                table45.AddRow(new string[] {
                            "pubkey",
                            "string"});
                table45.AddRow(new string[] {
                            "software",
                            "string"});
                table45.AddRow(new string[] {
                            "version",
                            "string"});
                table45.AddRow(new string[] {
                            "supported_nips",
                            "int[]"});
#line 20
 testRunner.And("Alice receives a response with json content", ((string)(null)), table45, "And ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "3.9.0.0")]
        [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
        public class FixtureData : System.IDisposable
        {
            
            public FixtureData()
            {
                NIP_11Feature.FeatureSetup();
            }
            
            void System.IDisposable.Dispose()
            {
                NIP_11Feature.FeatureTearDown();
            }
        }
    }
}
#pragma warning restore
#endregion
