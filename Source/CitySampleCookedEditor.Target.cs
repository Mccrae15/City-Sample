// Copyright Epic Games, Inc. All Rights Reserved.

using UnrealBuildTool;
using System.Collections.Generic;

[SupportedConfigurations(UnrealTargetConfiguration.Debug, UnrealTargetConfiguration.Development, UnrealTargetConfiguration.Test, UnrealTargetConfiguration.Shipping)]
public class CitySampleCookedEditorTarget : CitySampleEditorTarget
{
	public CitySampleCookedEditorTarget(TargetInfo Target) : base(Target)
	{
		SetDefaultsForCookedEditor(bIsCookedCooker: false, bIsForExternalUse: true);
	}
}
