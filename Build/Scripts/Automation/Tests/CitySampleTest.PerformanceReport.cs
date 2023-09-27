// Copyright Epic Games, Inc. All Rights Reserved.

using AutomationTool;
using EpicGames.Core;
using Gauntlet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnrealBuildBase;
using UnrealBuildTool;
using Log = EpicGames.Core.Log;
using Microsoft.Extensions.Logging;

using static AutomationTool.CommandUtils;

namespace CitySampleTest
{
	/// <summary>
	/// CI testing
	/// </summary>
	public class PerformanceReport : AutoTest
	{
		string ReportCacheDirRoot_BuildMachine => @"\\epicgames.net\Root\Builds\UE5\CitySample\PerfCache";
		string ReportCacheDirRoot_LocalMachine => Path.Combine(Unreal.RootDirectory.FullName, "GauntletTemp", "PerfCache");

		private DirectoryInfo TempPerfCSVDir => new DirectoryInfo(Path.Combine(Unreal.RootDirectory.FullName, "GauntletTemp", "PerfReportCSVs"));
		private string OriginalBuildName = null;


		public PerformanceReport(UnrealTestContext InContext)
			: base(InContext)
		{
			// We need to save off the build name as if this is a preflight that suffix will be stripped
			// after GetConfiguration is called. This will cause a mismatch in CreateReport.
			OriginalBuildName = Globals.Params.ParseValue("BuildName", InContext.BuildInfo.BuildName);
			Logger.LogInformation("Setting OriginalBuildName to {OriginalBuildName}", OriginalBuildName);
		}

		public override CitySampleTestConfig GetConfiguration()
		{
			CitySampleTestConfig Config = base.GetConfiguration();
			UnrealTestRole ClientRole = Config.RequireRole(UnrealTargetRole.Client);
			ClientRole.CommandLineParams.Add("csvGpuStats");
			ClientRole.CommandLineParams.AddOrAppendParamValue("execcmds", "t.FPSChart.DoCSVProfile 1");
			if(Context.Constraint.Platform == UnrealTargetPlatform.Win64)
			{
				ClientRole.CommandLineParams.AddOrAppendParamValue("execcmds", "csv.TargetFrameRateOverride 60");
			}
			ClientRole.CommandLineParams.Add("CitySampleTest.FPSChart");

			// Add CSV metadata
			List<string> CsvMetadata = new List<string>
			{
				"testname=CitySample",
				"gauntletTestType=AutoTest",
				"gauntletSubTest=Performance",
				"testBuildIsPreflight=" + (ReportGenUtils.IsTestingPreflightBuild(OriginalBuildName) ? "1" : "0"),
				"testBuildVersion=" + OriginalBuildName
			};
			ClientRole.CommandLineParams.Add("csvMetadata", "\"" + String.Join(",", CsvMetadata) + "\"");

			return Config;
		}

		protected override void InitHandledErrors()
        {
			base.InitHandledErrors();
		}

		/// <summary>
		/// Produces a detailed csv report using PerfReportTool.
		/// Also, stores perf data in the perf cache, and generates a historic report using the data the cache contains.
		/// </summary>
		private void GenerateLocalPerfReport(UnrealTargetPlatform Platform, string ArtifactPath)
		{
			var ReportCacheDir = Automation.IsBuildMachine
				? ReportCacheDirRoot_BuildMachine
				: ReportCacheDirRoot_LocalMachine;

			var ToolPath = FileReference.Combine(Unreal.EngineDirectory, "Binaries", "DotNET", "CsvTools", "PerfreportTool.exe");
			if (!FileReference.Exists(ToolPath))
			{
				Logger.LogError("Failed to find perf report utility at this path: \"{ToolPath}\".", ToolPath);
				return;
			}

			var ReportConfigDir = Path.Combine(Unreal.RootDirectory.FullName, "Samples", "Showcases", "CitySample", "Build", "Scripts", "PerfReport");
			var ReportPath = Path.Combine(ArtifactPath, "Reports", "Performance");

			// Csv files may have been output in one of two places.
			// Check both...
			var CsvsPaths = new[]
			{
				Path.Combine(ArtifactPath, "Client", "Profiling", "FPSChartStats"),
				Path.Combine(ArtifactPath, "Client", "Settings", "CitySample", "Saved", "Profiling", "FPSChartStats")
			};

			var DiscoveredCsvs = new List<string>();
			foreach (var CsvsPath in CsvsPaths)
			{
				if (Directory.Exists(CsvsPath))
				{
					DiscoveredCsvs.AddRange(
						from CsvFile in Directory.GetFiles(CsvsPath, "*.csv", SearchOption.AllDirectories)
						where CsvFile.Contains("csvprofile", StringComparison.InvariantCultureIgnoreCase)
						select CsvFile);
				}
			}

			if (DiscoveredCsvs.Count == 0)
			{
				Logger.LogError("Test completed successfully but no csv profiling results were found. Searched paths were:\r\n  {Paths}", string.Join("\r\n  ", CsvsPaths.Select(s => $"\"{s}\"")));
				return;
			}

			// Find the newest csv file and get its directory
			// (PerfReportTool will only output cached data in -csvdir mode)
			var NewestFile =
				(from CsvFile in DiscoveredCsvs
				 let Timestamp = File.GetCreationTimeUtc(CsvFile)
				 orderby Timestamp descending
				 select CsvFile).First();
			var NewestDir = Path.GetDirectoryName(NewestFile);

			Logger.LogInformation("Using perf report cache directory \"{ReportCacheDir}\".", ReportCacheDir);
			Logger.LogInformation("Using perf report output directory \"{ReportPath}\".", ReportPath);
			Logger.LogInformation("Using csv results directory \"{NewestDir}\". Generating historic perf report data...", NewestDir);

			// Make sure the cache and output directories exist
			if (!Directory.Exists(ReportCacheDir))
			{
				try { Directory.CreateDirectory(ReportCacheDir); }
				catch (Exception Ex)
				{
					Logger.LogError("Failed to create perf report cache directory \"{ReportCacheDir}\". {Ex}", ReportCacheDir, Ex);
					return;
				}
			}
			if (!Directory.Exists(ReportPath))
			{
				try { Directory.CreateDirectory(ReportPath); }
				catch (Exception Ex)
				{
					Logger.LogError("Failed to create perf report output directory \"{ReportPath}\". {Ex}", ReportPath, Ex);
					return;
				}
			}

			// Win64 is actually called "Windows" in csv profiles
			var PlatformNameFilter = Platform == UnrealTargetPlatform.Win64 ? "Windows" : $"{Platform}";

			// Produce the detailed report, and update the perf cache
			CommandUtils.RunAndLog(ToolPath.FullName, $"-csvdir \"{NewestDir}\" -o \"{ReportPath}\" -reportxmlbasedir \"{ReportConfigDir}\" -summaryTableCache \"{ReportCacheDir}\" -searchpattern csvprofile* -metadatafilter platform=\"{PlatformNameFilter}\"", out int ErrorCode);
			if (ErrorCode != 0)
			{
				Logger.LogError("PerfReportTool returned error code \"{ErrorCode}\" while generating detailed report.", ErrorCode);
			}

			// Now generate the all-time historic summary report
			HistoricReport("HistoricReport_AllTime", new[]
			{
				$"platform={PlatformNameFilter}"
			});

			// 14 days historic report
			HistoricReport($"HistoricReport_14Days", new[]
			{
				$"platform={PlatformNameFilter}",
				$"starttimestamp>={DateTimeOffset.Now.ToUnixTimeSeconds() - (14 * 60L * 60L * 24L)}"
			});

			// 7 days historic report
			HistoricReport($"HistoricReport_7Days", new[]
			{
				$"platform={PlatformNameFilter}",
				$"starttimestamp>={DateTimeOffset.Now.ToUnixTimeSeconds() - (7 * 60L * 60L * 24L)}"
			});

			void HistoricReport(string Name, IEnumerable<string> Filter)
			{
				var Args = new[]
				{
					$"-summarytablecachein \"{ReportCacheDir}\"",
					$"-summaryTableFilename \"{Name}.html\"",
					$"-reportxmlbasedir \"{ReportConfigDir}\"",
					$"-o \"{ReportPath}\"",
					$"-metadatafilter \"{string.Join(" and ", Filter)}\"",
					"-summaryTable autoPerfReportStandard",
					"-condensedSummaryTable autoPerfReportStandard",
					"-emailtable",
					"-recurse"
				};

				var ArgStr = string.Join(" ", Args);

				CommandUtils.RunAndLog(ToolPath.FullName, ArgStr, out ErrorCode);
				if (ErrorCode != 0)
				{
					Logger.LogError("PerfReportTool returned error code \"{ErrorCode}\" while generating historic report.", ErrorCode);
				}
			}
		}

		public override bool StartTest(int Pass, int InNumPasses)
		{
			if (Pass == 0 && TempPerfCSVDir.Exists)
			{
				TempPerfCSVDir.Delete(recursive: true);
			}
			return base.StartTest(Pass, InNumPasses);
		}

		public override ITestReport CreateReport(TestResult Result, UnrealTestContext Context, UnrealBuildSource Build, IEnumerable<UnrealRoleResult> Artifacts, string ArtifactPath)
		{
			if (Result == TestResult.Passed)
			{
				// Our artifacts from each iteration such as the client log will be overwritten by subsequent iterations so we need to copy them out to a temp dir
				// to preserve them until we're ready to make our report on the final iteration.
				CopyPerfFilesToTempDir(ArtifactPath);

				if (GetCurrentPass() < (GetNumPasses() - 1))
				{
					Logger.LogInformation($"Skipping Csv report generator until final pass. On pass {GetCurrentPass() + 1} of {GetNumPasses()}.");
					return base.CreateReport(Result, Context, Build, Artifacts, ArtifactPath);
				}

				// Local report generation is an example of how to use the PerfReportTool.
				if (!Globals.Params.ParseParam("NoLocalReports"))
				{
					// NOTE: This does not currently work with long paths due to the CsvTools not properly supporting them.
					Logger.LogInformation("Generating performance reports using PerfReportTool.");
					GenerateLocalPerfReport(Context.GetRoleContext(UnrealTargetRole.Client).Platform, ArtifactPath);
				}

				if (Globals.Params.ParseParam("PerfReportServer") &&
					!Globals.Params.ParseParam("SkipPerfReportServer"))
				{
					Dictionary<string, dynamic> CommonDataSourceFields = new Dictionary<string, dynamic>
					{
						{ "HordeJobUrl", Globals.Params.ParseValue("JobDetails", null) }
					};

					Logger.LogInformation("Creating perf server importer with build name {BuildName}", OriginalBuildName);
					string DataSourceName = "Automation.CitySample.Performance";
					string ImportDirOverride = Globals.Params.ParseValue("PerfReportServerImportDir", null);
					ICsvImporter Importer = ReportGenUtils.CreatePerfReportServerImporter(DataSourceName, OriginalBuildName, CommandUtils.IsBuildMachine, ImportDirOverride, CommonDataSourceFields);
					if (Importer != null)
					{
						// Recursively grab all the csv files we copied to the temp dir and convert them to binary.
						List<FileInfo> AllBinaryCsvFiles = ReportGenUtils.CollectAndConvertCsvFilesToBinary(TempPerfCSVDir.FullName);
						if (AllBinaryCsvFiles.Count == 0)
						{
							throw new AutomationException($"No Csv files found in {TempPerfCSVDir}");
						}

						// The corresponding log for each csv sits in the same subdirectory as the csv file itself.
						IEnumerable<CsvImportEntry> ImportEntries = AllBinaryCsvFiles
							.Select(CsvFile => new CsvImportEntry(CsvFile.FullName, Path.Combine(CsvFile.Directory.FullName, "ClientOutput.log")));

						// Create the import batch
						Importer.Import(ImportEntries);
					}

					// Cleanup the temp dir
					TempPerfCSVDir.Delete(recursive: true);
				}
			}
			else
			{
				Logger.LogWarning("Skipping performance report generation because the perf report test failed.");
			}

			return base.CreateReport(Result, Context, Build, Artifacts, ArtifactPath);
		}

		private void CopyPerfFilesToTempDir(string ArtifactPath)
		{
			if (!TempPerfCSVDir.Exists)
			{
				Logger.LogInformation("Creating temp perf csv dir: {TempPerfCSVDir}", TempPerfCSVDir);
				TempPerfCSVDir.Create();
			}

			string ClientArtifactDir = Path.Combine(ArtifactPath, "Client");
			string ClientLogPath = Path.Combine(ClientArtifactDir, "ClientOutput.log");

			// The FPSChartStats folder can vary in location depending on the platform, so use the helper to find the best match.
			string FPSChartsPath = PathUtils.FindRelevantPath(ClientArtifactDir, "Profiling", "FPSChartStats");
			if (string.IsNullOrEmpty(FPSChartsPath))
			{
				Logger.LogWarning("Failed to find FPSCharts folder in {ClientArtifactDir}", ClientArtifactDir);
				return;
			}

			// CsvTools use .NET Framework 4.8 currently so we must explicitly create long paths for them to work.
			FPSChartsPath = PathUtils.MakeLongPath(FPSChartsPath);

			// Grab all the csv files that have valid metadata.
			// We don't want to convert to binary in place as the legacy reports require the raw csv.
			List<FileInfo> CsvFiles = ReportGenUtils.CollectValidCsvFiles(FPSChartsPath);
			if (CsvFiles.Count > 0)
			{
				// We only want to copy the latest file as the other will have already been copied when this was run for those iterations.
				CsvFiles.SortBy(Info => Info.LastWriteTimeUtc);
				FileInfo LatestCsvFile = CsvFiles.Last();

				// Create a subdir for each pass as we want to store the csv and log together in the same dir to make it easier to find them later.
				string PassDir = Path.Combine(TempPerfCSVDir.FullName, $"PerfCsv_Pass_{GetCurrentPass()}");
				Directory.CreateDirectory(PassDir);

				FileInfo LogFileInfo = new FileInfo(ClientLogPath);
				if (LogFileInfo.Exists)
				{
					string LogDestPath = Path.Combine(PassDir, LogFileInfo.Name);
					Logger.LogInformation("Copying Log {ClientLogPath} To {LogDest}", ClientLogPath, LogDestPath);
					LogFileInfo.CopyTo(LogDestPath);
				}
				else
				{
					Logger.LogWarning("No log file was found at {ClientLogPath}", ClientLogPath);
				}

				string CsvDestPath = Path.Combine(PassDir, LatestCsvFile.Name);
				Logger.LogInformation("Copying Csv {CsvPath} To {CsvDestPath}", LatestCsvFile.FullName, CsvDestPath);
				LatestCsvFile.CopyTo(CsvDestPath);
			}
			else
			{
				Logger.LogWarning("No valid csv files found in {FPSChartsPath}", FPSChartsPath);
			}
		}
	}

	//
	// Horrible hack to repeat the CitySample perf tests 3 times...
	// There is no way to pass "-repeat=N" to Gauntlet via the standard engine build scripts, nor is
	// it possible to override the number of iterations per-test via the GetConfiguration() function.
	//
	// In theory we can pass the "CitySampleTest.PerformanceReport" test name to Gauntlet 3 times via Horde scripts,
	// but the standard build scripts will attempt to define 3 nodes all with the same name, which won't work.
	//
	// These three classes allow us to run 3 copies of the PerformanceReport test, but ensures they all have 
	// different names to fit into the build script / Gauntlet structure.
	//

	public class PerformanceReport_1 : PerformanceReport
	{
		public PerformanceReport_1(UnrealTestContext InContext) : base(InContext) { }
	}

	public class PerformanceReport_2 : PerformanceReport
	{
		public PerformanceReport_2(UnrealTestContext InContext) : base(InContext) { }
	}

	public class PerformanceReport_3 : PerformanceReport
	{
		public PerformanceReport_3(UnrealTestContext InContext) : base(InContext) { }
	}
}
