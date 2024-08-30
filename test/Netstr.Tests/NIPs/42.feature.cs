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
    public partial class NIP_42Feature : object, Xunit.IClassFixture<NIP_42Feature.FixtureData>, System.IDisposable
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
        private static string[] featureTags = ((string[])(null));
        
        private Xunit.Abstractions.ITestOutputHelper _testOutputHelper;
        
#line 1 "42.feature"
#line hidden
        
        public NIP_42Feature(NIP_42Feature.FixtureData fixtureData, Netstr_Tests_XUnitAssemblyFixture assemblyFixture, Xunit.Abstractions.ITestOutputHelper testOutputHelper)
        {
            this._testOutputHelper = testOutputHelper;
            this.TestInitialize();
        }
        
        public static void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "NIPs", "NIP-42", "\tDefines a way for clients to authenticate to relays by signing an ephemeral even" +
                    "t.", ProgrammingLanguage.CSharp, featureTags);
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
 testRunner.Given("a relay is running with AUTH required", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
            TechTalk.SpecFlow.Table table78 = new TechTalk.SpecFlow.Table(new string[] {
                        "PublicKey",
                        "PrivateKey"});
            table78.AddRow(new string[] {
                        "5758137ec7f38f3d6c3ef103e28cd9312652285dab3497fe5e5f6c5c0ef45e75",
                        "512a14752ed58380496920da432f1c0cdad952cd4afda3d9bfa51c2051f91b02"});
#line 6
 testRunner.And("Alice is connected to relay", ((string)(null)), table78, "And ");
#line hidden
        }
        
        void System.IDisposable.Dispose()
        {
            this.TestTearDown();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Not authenticated client cannot publish or subscribe")]
        [Xunit.TraitAttribute("FeatureTitle", "NIP-42")]
        [Xunit.TraitAttribute("Description", "Not authenticated client cannot publish or subscribe")]
        public void NotAuthenticatedClientCannotPublishOrSubscribe()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Not authenticated client cannot publish or subscribe", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 10
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
                TechTalk.SpecFlow.Table table79 = new TechTalk.SpecFlow.Table(new string[] {
                            "Kinds"});
                table79.AddRow(new string[] {
                            "1"});
#line 11
 testRunner.When("Alice sends a subscription request abcd", ((string)(null)), table79, "When ");
#line hidden
                TechTalk.SpecFlow.Table table80 = new TechTalk.SpecFlow.Table(new string[] {
                            "Id",
                            "Content",
                            "Kind",
                            "Tags",
                            "CreatedAt"});
                table80.AddRow(new string[] {
                            "8ed8cc390eaf6db9e0ae8f3bf720a80d81ae49f95f953a9a4e26a72dc7f4a2c5",
                            "Hello",
                            "1",
                            "",
                            "1722337838"});
#line 14
 testRunner.And("Alice publishes events", ((string)(null)), table80, "And ");
#line hidden
                TechTalk.SpecFlow.Table table81 = new TechTalk.SpecFlow.Table(new string[] {
                            "Type",
                            "Id",
                            "Success"});
                table81.AddRow(new string[] {
                            "AUTH",
                            "*",
                            ""});
                table81.AddRow(new string[] {
                            "CLOSED",
                            "abcd",
                            ""});
                table81.AddRow(new string[] {
                            "OK",
                            "8ed8cc390eaf6db9e0ae8f3bf720a80d81ae49f95f953a9a4e26a72dc7f4a2c5",
                            "false"});
#line 17
 testRunner.Then("Alice receives messages", ((string)(null)), table81, "Then ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Authenticated client can publish and subscribe")]
        [Xunit.TraitAttribute("FeatureTitle", "NIP-42")]
        [Xunit.TraitAttribute("Description", "Authenticated client can publish and subscribe")]
        public void AuthenticatedClientCanPublishAndSubscribe()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Authenticated client can publish and subscribe", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 23
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
#line 24
 testRunner.When("Alice publishes an AUTH event for the challenge sent by relay", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
                TechTalk.SpecFlow.Table table82 = new TechTalk.SpecFlow.Table(new string[] {
                            "Kinds"});
                table82.AddRow(new string[] {
                            "2"});
#line 25
 testRunner.And("Alice sends a subscription request abcd", ((string)(null)), table82, "And ");
#line hidden
                TechTalk.SpecFlow.Table table83 = new TechTalk.SpecFlow.Table(new string[] {
                            "Id",
                            "Content",
                            "Kind",
                            "Tags",
                            "CreatedAt"});
                table83.AddRow(new string[] {
                            "8ed8cc390eaf6db9e0ae8f3bf720a80d81ae49f95f953a9a4e26a72dc7f4a2c5",
                            "Hello",
                            "1",
                            "",
                            "1722337838"});
#line 28
 testRunner.And("Alice publishes events", ((string)(null)), table83, "And ");
#line hidden
                TechTalk.SpecFlow.Table table84 = new TechTalk.SpecFlow.Table(new string[] {
                            "Type",
                            "Id",
                            "Success"});
                table84.AddRow(new string[] {
                            "AUTH",
                            "*",
                            ""});
                table84.AddRow(new string[] {
                            "OK",
                            "*",
                            "true"});
                table84.AddRow(new string[] {
                            "EOSE",
                            "abcd",
                            ""});
                table84.AddRow(new string[] {
                            "OK",
                            "8ed8cc390eaf6db9e0ae8f3bf720a80d81ae49f95f953a9a4e26a72dc7f4a2c5",
                            "true"});
#line 31
 testRunner.Then("Alice receives messages", ((string)(null)), table84, "Then ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Client stays unauthenticated when invalid challenge is used")]
        [Xunit.TraitAttribute("FeatureTitle", "NIP-42")]
        [Xunit.TraitAttribute("Description", "Client stays unauthenticated when invalid challenge is used")]
        public void ClientStaysUnauthenticatedWhenInvalidChallengeIsUsed()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Client stays unauthenticated when invalid challenge is used", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 38
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
#line 39
 testRunner.When("Alice publishes an AUTH event with invalid challenge", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
                TechTalk.SpecFlow.Table table85 = new TechTalk.SpecFlow.Table(new string[] {
                            "Kinds"});
                table85.AddRow(new string[] {
                            "1"});
#line 40
 testRunner.When("Alice sends a subscription request abcd", ((string)(null)), table85, "When ");
#line hidden
                TechTalk.SpecFlow.Table table86 = new TechTalk.SpecFlow.Table(new string[] {
                            "Id",
                            "Content",
                            "Kind",
                            "Tags",
                            "CreatedAt"});
                table86.AddRow(new string[] {
                            "8ed8cc390eaf6db9e0ae8f3bf720a80d81ae49f95f953a9a4e26a72dc7f4a2c5",
                            "Hello",
                            "1",
                            "",
                            "1722337838"});
#line 43
 testRunner.And("Alice publishes events", ((string)(null)), table86, "And ");
#line hidden
                TechTalk.SpecFlow.Table table87 = new TechTalk.SpecFlow.Table(new string[] {
                            "Type",
                            "Id",
                            "Success"});
                table87.AddRow(new string[] {
                            "AUTH",
                            "*",
                            ""});
                table87.AddRow(new string[] {
                            "OK",
                            "*",
                            "false"});
                table87.AddRow(new string[] {
                            "CLOSED",
                            "abcd",
                            ""});
                table87.AddRow(new string[] {
                            "OK",
                            "8ed8cc390eaf6db9e0ae8f3bf720a80d81ae49f95f953a9a4e26a72dc7f4a2c5",
                            "false"});
#line 46
 testRunner.Then("Alice receives messages", ((string)(null)), table87, "Then ");
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
                NIP_42Feature.FeatureSetup();
            }
            
            void System.IDisposable.Dispose()
            {
                NIP_42Feature.FeatureTearDown();
            }
        }
    }
}
#pragma warning restore
#endregion