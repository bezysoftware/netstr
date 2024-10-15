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
    public partial class NIP_70Feature : object, Xunit.IClassFixture<NIP_70Feature.FixtureData>, System.IDisposable
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
        private static string[] featureTags = ((string[])(null));
        
        private Xunit.Abstractions.ITestOutputHelper _testOutputHelper;
        
#line 1 "70.feature"
#line hidden
        
        public NIP_70Feature(NIP_70Feature.FixtureData fixtureData, Netstr_Tests_XUnitAssemblyFixture assemblyFixture, Xunit.Abstractions.ITestOutputHelper testOutputHelper)
        {
            this._testOutputHelper = testOutputHelper;
            this.TestInitialize();
        }
        
        public static void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "NIPs", "NIP-70", "\tWhen the \"-\" tag is present, that means the event is \"protected\".\r\n\tA protected " +
                    "event is an event that can only be published to relays by its author.", ProgrammingLanguage.CSharp, featureTags);
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
 testRunner.Given("a relay is running with AUTH enabled", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
            TechTalk.SpecFlow.Table table136 = new TechTalk.SpecFlow.Table(new string[] {
                        "PublicKey",
                        "PrivateKey"});
            table136.AddRow(new string[] {
                        "5758137ec7f38f3d6c3ef103e28cd9312652285dab3497fe5e5f6c5c0ef45e75",
                        "512a14752ed58380496920da432f1c0cdad952cd4afda3d9bfa51c2051f91b02"});
#line 7
 testRunner.And("Alice is connected to relay", ((string)(null)), table136, "And ");
#line hidden
        }
        
        void System.IDisposable.Dispose()
        {
            this.TestTearDown();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Not authenticated client tries to publish protected event")]
        [Xunit.TraitAttribute("FeatureTitle", "NIP-70")]
        [Xunit.TraitAttribute("Description", "Not authenticated client tries to publish protected event")]
        public void NotAuthenticatedClientTriesToPublishProtectedEvent()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Not authenticated client tries to publish protected event", "\tAlice cannot publish protected events when she isn\'t authenticated", tagsOfScenario, argumentsOfScenario, featureTags);
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
                TechTalk.SpecFlow.Table table137 = new TechTalk.SpecFlow.Table(new string[] {
                            "Id",
                            "Content",
                            "Kind",
                            "Tags",
                            "CreatedAt"});
                table137.AddRow(new string[] {
                            "92f3f4bfb1c756108b242dc02169fa96bd53d5ac5331c6ac5e377045637e2cf5",
                            "Protected",
                            "1",
                            "[[ \"-\" ]]",
                            "1722337837"});
#line 13
 testRunner.When("Alice publishes an event", ((string)(null)), table137, "When ");
#line hidden
                TechTalk.SpecFlow.Table table138 = new TechTalk.SpecFlow.Table(new string[] {
                            "Type",
                            "Id",
                            "Success"});
                table138.AddRow(new string[] {
                            "AUTH",
                            "*",
                            ""});
                table138.AddRow(new string[] {
                            "OK",
                            "92f3f4bfb1c756108b242dc02169fa96bd53d5ac5331c6ac5e377045637e2cf5",
                            "false"});
#line 16
 testRunner.Then("Alice receives messages", ((string)(null)), table138, "Then ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Authenticated client publishes their protected event")]
        [Xunit.TraitAttribute("FeatureTitle", "NIP-70")]
        [Xunit.TraitAttribute("Description", "Authenticated client publishes their protected event")]
        public void AuthenticatedClientPublishesTheirProtectedEvent()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Authenticated client publishes their protected event", "\tOnce Alice authenticates she can publish protected events", tagsOfScenario, argumentsOfScenario, featureTags);
#line 21
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
#line 23
 testRunner.When("Alice publishes an AUTH event for the challenge sent by relay", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
                TechTalk.SpecFlow.Table table139 = new TechTalk.SpecFlow.Table(new string[] {
                            "Id",
                            "Content",
                            "Kind",
                            "Tags",
                            "CreatedAt"});
                table139.AddRow(new string[] {
                            "92f3f4bfb1c756108b242dc02169fa96bd53d5ac5331c6ac5e377045637e2cf5",
                            "Protected",
                            "1",
                            "[[ \"-\" ]]",
                            "1722337837"});
#line 24
 testRunner.When("Alice publishes an event", ((string)(null)), table139, "When ");
#line hidden
                TechTalk.SpecFlow.Table table140 = new TechTalk.SpecFlow.Table(new string[] {
                            "Type",
                            "Id",
                            "Success"});
                table140.AddRow(new string[] {
                            "AUTH",
                            "*",
                            ""});
                table140.AddRow(new string[] {
                            "OK",
                            "*",
                            "true"});
                table140.AddRow(new string[] {
                            "OK",
                            "92f3f4bfb1c756108b242dc02169fa96bd53d5ac5331c6ac5e377045637e2cf5",
                            "true"});
#line 27
 testRunner.Then("Alice receives messages", ((string)(null)), table140, "Then ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Authenticated client tries to publish someone else\'s protected event")]
        [Xunit.TraitAttribute("FeatureTitle", "NIP-70")]
        [Xunit.TraitAttribute("Description", "Authenticated client tries to publish someone else\'s protected event")]
        public void AuthenticatedClientTriesToPublishSomeoneElsesProtectedEvent()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Authenticated client tries to publish someone else\'s protected event", "\tThe event Alice tries to publish was signed by Bob, relay should reject it", tagsOfScenario, argumentsOfScenario, featureTags);
#line 33
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
#line 35
 testRunner.When("Alice publishes an AUTH event for the challenge sent by relay", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
                TechTalk.SpecFlow.Table table141 = new TechTalk.SpecFlow.Table(new string[] {
                            "Id",
                            "PublicKey",
                            "Content",
                            "Kind",
                            "Tags",
                            "CreatedAt"});
                table141.AddRow(new string[] {
                            "1c982ee8b0f2484815a4befbb26bb02d6b20b4b3a85bfe6568a3712f943aa940",
                            "5bc683a5d12133a96ac5502c15fe1c2287986cff7baf6283600360e6bb01f627",
                            "Protected",
                            "1",
                            "[[ \"-\" ]]",
                            "1722337837"});
#line 36
 testRunner.When("Alice publishes an event", ((string)(null)), table141, "When ");
#line hidden
                TechTalk.SpecFlow.Table table142 = new TechTalk.SpecFlow.Table(new string[] {
                            "Type",
                            "Id",
                            "Success"});
                table142.AddRow(new string[] {
                            "AUTH",
                            "*",
                            ""});
                table142.AddRow(new string[] {
                            "OK",
                            "*",
                            "true"});
                table142.AddRow(new string[] {
                            "OK",
                            "1c982ee8b0f2484815a4befbb26bb02d6b20b4b3a85bfe6568a3712f943aa940",
                            "false"});
#line 39
 testRunner.Then("Alice receives messages", ((string)(null)), table142, "Then ");
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
                NIP_70Feature.FeatureSetup();
            }
            
            void System.IDisposable.Dispose()
            {
                NIP_70Feature.FeatureTearDown();
            }
        }
    }
}
#pragma warning restore
#endregion
