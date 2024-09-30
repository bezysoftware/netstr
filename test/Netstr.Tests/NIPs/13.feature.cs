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
    public partial class NIP_13Feature : object, Xunit.IClassFixture<NIP_13Feature.FixtureData>, System.IDisposable
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
        private static string[] featureTags = ((string[])(null));
        
        private Xunit.Abstractions.ITestOutputHelper _testOutputHelper;
        
#line 1 "13.feature"
#line hidden
        
        public NIP_13Feature(NIP_13Feature.FixtureData fixtureData, Netstr_Tests_XUnitAssemblyFixture assemblyFixture, Xunit.Abstractions.ITestOutputHelper testOutputHelper)
        {
            this._testOutputHelper = testOutputHelper;
            this.TestInitialize();
        }
        
        public static void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "NIPs", "NIP-13", "\t Proof of Work (PoW) is a way to add a proof of computational work to a note.\r\n\t" +
                    " This proof can be used as a means of spam deterrence.", ProgrammingLanguage.CSharp, featureTags);
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
            TechTalk.SpecFlow.Table table72 = new TechTalk.SpecFlow.Table(new string[] {
                        "Key",
                        "Value"});
            table72.AddRow(new string[] {
                        "MinPowDifficulty",
                        "20"});
#line 6
 testRunner.Given("a relay is running with options", ((string)(null)), table72, "Given ");
#line hidden
            TechTalk.SpecFlow.Table table73 = new TechTalk.SpecFlow.Table(new string[] {
                        "PublicKey",
                        "PrivateKey"});
            table73.AddRow(new string[] {
                        "5758137ec7f38f3d6c3ef103e28cd9312652285dab3497fe5e5f6c5c0ef45e75",
                        "512a14752ed58380496920da432f1c0cdad952cd4afda3d9bfa51c2051f91b02"});
#line 9
 testRunner.And("Alice is connected to relay", ((string)(null)), table73, "And ");
#line hidden
        }
        
        void System.IDisposable.Dispose()
        {
            this.TestTearDown();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Messages with low difficulty and those off target are rejected, those with high a" +
            "nd on target difficulty accepted")]
        [Xunit.TraitAttribute("FeatureTitle", "NIP-13")]
        [Xunit.TraitAttribute("Description", "Messages with low difficulty and those off target are rejected, those with high a" +
            "nd on target difficulty accepted")]
        public void MessagesWithLowDifficultyAndThoseOffTargetAreRejectedThoseWithHighAndOnTargetDifficultyAccepted()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Messages with low difficulty and those off target are rejected, those with high a" +
                    "nd on target difficulty accepted", "\t1) Low diff\r\n\t2) High diff but doesn\'t match target\r\n\t3) High diff\r\n\t4) High dif" +
                    "f matching target", tagsOfScenario, argumentsOfScenario, featureTags);
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
#line 5
this.FeatureBackground();
#line hidden
                TechTalk.SpecFlow.Table table74 = new TechTalk.SpecFlow.Table(new string[] {
                            "Id",
                            "Content",
                            "Tags",
                            "Kind",
                            "CreatedAt"});
                table74.AddRow(new string[] {
                            "00387d3bb57ceab60effbefffcaecff27614c60c75d7b36b01caa71249e3ca3c",
                            "Hello",
                            "[[\"nonce\", \"cc2e9737-e4f5-48d2-8c55-1461aeca3c87\"]]",
                            "1",
                            "1722337838"});
                table74.AddRow(new string[] {
                            "0000017cb9da5d1295c5d9e902055c25280ae95ea6767ad89a02f928742b703d",
                            "Hello",
                            "[[\"nonce\", \"84fe8193-f35e-4d9e-9871-b509caaa6412\", \"5\"]]",
                            "1",
                            "1722337838"});
                table74.AddRow(new string[] {
                            "00000ed0cf8d67d9cb4f5b211ad9c8daea5b7bbf7721e345070d98a91cc289ff",
                            "Hello",
                            "[[\"nonce\", \"49c7c782-8f45-4dbb-adac-5ebc71c3363c\"]]",
                            "1",
                            "1722337838"});
                table74.AddRow(new string[] {
                            "000005e3b3172e58be368ed6b51b7ecf96a3d32b1107496bf6d786f8084aa17f",
                            "Hello",
                            "[[\"nonce\", \"045b7487-e889-4179-9d52-ce46beffef66\", \"21\"]]",
                            "1",
                            "1722337838"});
#line 18
 testRunner.When("Alice publishes events", ((string)(null)), table74, "When ");
#line hidden
                TechTalk.SpecFlow.Table table75 = new TechTalk.SpecFlow.Table(new string[] {
                            "Type",
                            "Id",
                            "Success"});
                table75.AddRow(new string[] {
                            "OK",
                            "00387d3bb57ceab60effbefffcaecff27614c60c75d7b36b01caa71249e3ca3c",
                            "false"});
                table75.AddRow(new string[] {
                            "OK",
                            "0000017cb9da5d1295c5d9e902055c25280ae95ea6767ad89a02f928742b703d",
                            "false"});
                table75.AddRow(new string[] {
                            "OK",
                            "00000ed0cf8d67d9cb4f5b211ad9c8daea5b7bbf7721e345070d98a91cc289ff",
                            "true"});
                table75.AddRow(new string[] {
                            "OK",
                            "000005e3b3172e58be368ed6b51b7ecf96a3d32b1107496bf6d786f8084aa17f",
                            "true"});
#line 24
 testRunner.Then("Alice receives messages", ((string)(null)), table75, "Then ");
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
                NIP_13Feature.FeatureSetup();
            }
            
            void System.IDisposable.Dispose()
            {
                NIP_13Feature.FeatureTearDown();
            }
        }
    }
}
#pragma warning restore
#endregion
