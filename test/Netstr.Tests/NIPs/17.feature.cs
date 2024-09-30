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
    public partial class NIP_17Feature : object, Xunit.IClassFixture<NIP_17Feature.FixtureData>, System.IDisposable
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
        private static string[] featureTags = ((string[])(null));
        
        private Xunit.Abstractions.ITestOutputHelper _testOutputHelper;
        
#line 1 "17.feature"
#line hidden
        
        public NIP_17Feature(NIP_17Feature.FixtureData fixtureData, Netstr_Tests_XUnitAssemblyFixture assemblyFixture, Xunit.Abstractions.ITestOutputHelper testOutputHelper)
        {
            this._testOutputHelper = testOutputHelper;
            this.TestInitialize();
        }
        
        public static void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "NIPs", "NIP-17", "\tThis NIP defines an encrypted direct messaging scheme using NIP-44 encryption an" +
                    "d NIP-59 seals and gift wraps.", ProgrammingLanguage.CSharp, featureTags);
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
#line 4
#line hidden
#line 5
 testRunner.Given("a relay is running with AUTH enabled", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
            TechTalk.SpecFlow.Table table76 = new TechTalk.SpecFlow.Table(new string[] {
                        "PublicKey",
                        "PrivateKey"});
            table76.AddRow(new string[] {
                        "5758137ec7f38f3d6c3ef103e28cd9312652285dab3497fe5e5f6c5c0ef45e75",
                        "512a14752ed58380496920da432f1c0cdad952cd4afda3d9bfa51c2051f91b02"});
#line 6
 testRunner.And("Alice is connected to relay", ((string)(null)), table76, "And ");
#line hidden
            TechTalk.SpecFlow.Table table77 = new TechTalk.SpecFlow.Table(new string[] {
                        "PublicKey",
                        "PrivateKey"});
            table77.AddRow(new string[] {
                        "5bc683a5d12133a96ac5502c15fe1c2287986cff7baf6283600360e6bb01f627",
                        "3551fc7617f76632e4542992c0bc01fecb224de639c4b6a1e0956946e8bb8a29"});
#line 9
 testRunner.And("Bob is connected to relay", ((string)(null)), table77, "And ");
#line hidden
        }
        
        void System.IDisposable.Dispose()
        {
            this.TestTearDown();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Not authenticated client tries to fetch kind 1059 events")]
        [Xunit.TraitAttribute("FeatureTitle", "NIP-17")]
        [Xunit.TraitAttribute("Description", "Not authenticated client tries to fetch kind 1059 events")]
        public void NotAuthenticatedClientTriesToFetchKind1059Events()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Not authenticated client tries to fetch kind 1059 events", "\tAlice can\'t fetch kind 1059 events when she isn\'t authenticated", tagsOfScenario, argumentsOfScenario, featureTags);
#line 13
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 4
this.FeatureBackground();
#line hidden
                TechTalk.SpecFlow.Table table78 = new TechTalk.SpecFlow.Table(new string[] {
                            "Authors",
                            "Kinds"});
                table78.AddRow(new string[] {
                            "",
                            "1,1059"});
                table78.AddRow(new string[] {
                            "5bc683a5d12133a96ac5502c15fe1c2287986cff7baf6283600360e6bb01f627",
                            ""});
#line 15
 testRunner.When("Alice sends a subscription request abcd", ((string)(null)), table78, "When ");
#line hidden
                TechTalk.SpecFlow.Table table79 = new TechTalk.SpecFlow.Table(new string[] {
                            "Type",
                            "Id"});
                table79.AddRow(new string[] {
                            "AUTH",
                            "*"});
                table79.AddRow(new string[] {
                            "CLOSED",
                            "abcd"});
#line 19
 testRunner.Then("Alice receives messages", ((string)(null)), table79, "Then ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Authenticated client tries to fetch kind 1059 events")]
        [Xunit.TraitAttribute("FeatureTitle", "NIP-17")]
        [Xunit.TraitAttribute("Description", "Authenticated client tries to fetch kind 1059 events")]
        public void AuthenticatedClientTriesToFetchKind1059Events()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Authenticated client tries to fetch kind 1059 events", "\tOnce Alice authenticates she can fetch their kind 1059 events, but no one else\'s" +
                    "", tagsOfScenario, argumentsOfScenario, featureTags);
#line 24
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 4
this.FeatureBackground();
#line hidden
#line 26
 testRunner.When("Alice publishes an AUTH event for the challenge sent by relay", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
                TechTalk.SpecFlow.Table table80 = new TechTalk.SpecFlow.Table(new string[] {
                            "Id",
                            "Content",
                            "Kind",
                            "Tags",
                            "CreatedAt"});
                table80.AddRow(new string[] {
                            "ff526515d15975c3839f027cd301ba49afca237fa0d84f53765e9c320a269d90",
                            "Secret",
                            "1059",
                            "[[\"p\",\"5758137ec7f38f3d6c3ef103e28cd9312652285dab3497fe5e5f6c5c0ef45e75\"]]",
                            "1722337838"});
                table80.AddRow(new string[] {
                            "fb90964eba126b74bc71bf31e9e198dc4fbdd79e3de4d4f02dacddbe8a6ac71c",
                            "Charlie\'s Secret",
                            "1059",
                            "[[\"p\",\"fe8d7a5726ea97ce6140f9fb06b1fe7d3259bcbf8de42c2a5d2ec9f8f0e2f614\"]]",
                            "1722337838"});
#line 27
 testRunner.And("Bob publishes events", ((string)(null)), table80, "And ");
#line hidden
                TechTalk.SpecFlow.Table table81 = new TechTalk.SpecFlow.Table(new string[] {
                            "Kinds"});
                table81.AddRow(new string[] {
                            "1059"});
#line 31
 testRunner.When("Alice sends a subscription request abcd", ((string)(null)), table81, "When ");
#line hidden
                TechTalk.SpecFlow.Table table82 = new TechTalk.SpecFlow.Table(new string[] {
                            "Id",
                            "Content",
                            "Kind",
                            "Tags",
                            "CreatedAt"});
                table82.AddRow(new string[] {
                            "03403b4d4c4fad3ff1f561f030dff80daa256c66a4a195e3eb58bce90b2457bd",
                            "Secret 2",
                            "1059",
                            "[[\"p\",\"5758137ec7f38f3d6c3ef103e28cd9312652285dab3497fe5e5f6c5c0ef45e75\"]]",
                            "1722337838"});
                table82.AddRow(new string[] {
                            "0e9391da7663a19e77d11966f57396a89a3a7bef1be1d045475e75be8eca246e",
                            "Charlie\'s Secret 2",
                            "1059",
                            "[[\"p\",\"fe8d7a5726ea97ce6140f9fb06b1fe7d3259bcbf8de42c2a5d2ec9f8f0e2f614\"]]",
                            "1722337838"});
#line 34
 testRunner.And("Bob publishes events", ((string)(null)), table82, "And ");
#line hidden
                TechTalk.SpecFlow.Table table83 = new TechTalk.SpecFlow.Table(new string[] {
                            "Type",
                            "Id",
                            "EventId",
                            "Success"});
                table83.AddRow(new string[] {
                            "AUTH",
                            "*",
                            "",
                            ""});
                table83.AddRow(new string[] {
                            "OK",
                            "*",
                            "",
                            "true"});
                table83.AddRow(new string[] {
                            "EVENT",
                            "abcd",
                            "ff526515d15975c3839f027cd301ba49afca237fa0d84f53765e9c320a269d90",
                            ""});
                table83.AddRow(new string[] {
                            "EOSE",
                            "abcd",
                            "",
                            ""});
                table83.AddRow(new string[] {
                            "EVENT",
                            "abcd",
                            "03403b4d4c4fad3ff1f561f030dff80daa256c66a4a195e3eb58bce90b2457bd",
                            ""});
#line 38
 testRunner.Then("Alice receives messages", ((string)(null)), table83, "Then ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Authenticated client tries to fetch kind 1059 events through other filters")]
        [Xunit.TraitAttribute("FeatureTitle", "NIP-17")]
        [Xunit.TraitAttribute("Description", "Authenticated client tries to fetch kind 1059 events through other filters")]
        public void AuthenticatedClientTriesToFetchKind1059EventsThroughOtherFilters()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Authenticated client tries to fetch kind 1059 events through other filters", "\tEven when using complex filters, authenticated client should still not receive s" +
                    "omeone else\'s kind 1059 events", tagsOfScenario, argumentsOfScenario, featureTags);
#line 46
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 4
this.FeatureBackground();
#line hidden
#line 48
 testRunner.When("Alice publishes an AUTH event for the challenge sent by relay", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
                TechTalk.SpecFlow.Table table84 = new TechTalk.SpecFlow.Table(new string[] {
                            "Id",
                            "Content",
                            "Kind",
                            "Tags",
                            "CreatedAt"});
                table84.AddRow(new string[] {
                            "ff526515d15975c3839f027cd301ba49afca237fa0d84f53765e9c320a269d90",
                            "Secret",
                            "1059",
                            "[[\"p\",\"5758137ec7f38f3d6c3ef103e28cd9312652285dab3497fe5e5f6c5c0ef45e75\"]]",
                            "1722337838"});
                table84.AddRow(new string[] {
                            "fb90964eba126b74bc71bf31e9e198dc4fbdd79e3de4d4f02dacddbe8a6ac71c",
                            "Charlie\'s Secret",
                            "1059",
                            "[[\"p\",\"fe8d7a5726ea97ce6140f9fb06b1fe7d3259bcbf8de42c2a5d2ec9f8f0e2f614\"]]",
                            "1722337838"});
#line 49
 testRunner.And("Bob publishes events", ((string)(null)), table84, "And ");
#line hidden
                TechTalk.SpecFlow.Table table85 = new TechTalk.SpecFlow.Table(new string[] {
                            "Ids",
                            "Authors",
                            "Kinds"});
                table85.AddRow(new string[] {
                            "",
                            "",
                            "1059"});
                table85.AddRow(new string[] {
                            "fb90964eba126b74bc71bf31e9e198dc4fbdd79e3de4d4f02dacddbe8a6ac71c",
                            "",
                            ""});
                table85.AddRow(new string[] {
                            "",
                            "fe8d7a5726ea97ce6140f9fb06b1fe7d3259bcbf8de42c2a5d2ec9f8f0e2f611059",
                            ""});
                table85.AddRow(new string[] {
                            "",
                            "fe8d7a5726ea97ce6140f9fb06b1fe7d3259bcbf8de42c2a5d2ec9f8f0e2f611059",
                            "1059"});
#line 53
 testRunner.When("Alice sends a subscription request abcd", ((string)(null)), table85, "When ");
#line hidden
                TechTalk.SpecFlow.Table table86 = new TechTalk.SpecFlow.Table(new string[] {
                            "Type",
                            "Id",
                            "EventId",
                            "Success"});
                table86.AddRow(new string[] {
                            "AUTH",
                            "*",
                            "",
                            ""});
                table86.AddRow(new string[] {
                            "OK",
                            "*",
                            "",
                            "true"});
                table86.AddRow(new string[] {
                            "EVENT",
                            "abcd",
                            "ff526515d15975c3839f027cd301ba49afca237fa0d84f53765e9c320a269d90",
                            ""});
                table86.AddRow(new string[] {
                            "EOSE",
                            "abcd",
                            "",
                            ""});
#line 59
 testRunner.Then("Alice receives messages", ((string)(null)), table86, "Then ");
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
                NIP_17Feature.FeatureSetup();
            }
            
            void System.IDisposable.Dispose()
            {
                NIP_17Feature.FeatureTearDown();
            }
        }
    }
}
#pragma warning restore
#endregion
