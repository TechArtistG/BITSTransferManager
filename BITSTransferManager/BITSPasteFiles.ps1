<#
===== BITSPaste BITS-Transfer Tool =====
Author: Geordie Moffatt

This script has two main functions:
1. To add itself as a context menu option in File explorer
2. To initialte a BITS-Trasnfer job using the clipboard as a file source and a supplied destination (supplied from explorer via context menu)

=== Installation ===
To install the contect menu:
1. Make sure this script is in a perminant local location on you system.  Advised location is %AppData%\Microsoft\BITSPaste.  
2. Call:
	./BITSPasteFiles.ps1 setup

=== Usage ===
After installation, you will have a "Paste Files using BITS" option in the context menu when right clicking on or inside folders.  Copy files as usual then use this command to paste them.

=== TODO ===
 - Format-FileSize function fails on very large numbers, need to use long and refactor format code

#>


Add-Type -Assembly PresentationCore, PresentationFramework

function asAdminWithSTAAndParams([string[]]$argList)
{
    #[string]$cmdPath = $args[0]
    $joinArgs = "-sta -NoProfile -ExecutionPolicy Bypass -File "
    foreach ($arg in $argList)
    {
        $joinArgs += "`"$arg`" "
    }

    Write-Host $joinArgs
    if (!([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator"))
    { 
        Start-Process powershell.exe $joinArgs -Verb RunAs -Wait
    }
}



Function Format-FileSize() {
    Param ([int]$size)
    If     ($size -gt 1TB) {[string]::Format("{0:0.00} TB", $size / 1TB)}
    ElseIf ($size -gt 1GB) {[string]::Format("{0:0.00} GB", $size / 1GB)}
    ElseIf ($size -gt 1MB) {[string]::Format("{0:0.00} MB", $size / 1MB)}
    ElseIf ($size -gt 1KB) {[string]::Format("{0:0.00} kB", $size / 1KB)}
    ElseIf ($size -gt 0)   {[string]::Format("{0:0.00} B", $size)}
    Else                   {""}
}

function writeMessage
{
    [string]$txt = $args[0]
    [int]$type = $args[1] # 0 = info, 1 = success, 2 = warning, 3 = error
    $FGColor = "White"
    $pre = "MESSAGE"

    if($type -eq 0)
    {
        $FGColor = "White"
        $pre = " MESSAGE "       
    }
    elseif($type -eq 1)
    {
        $FGColor = "Green"
        $pre = " SUCCESS " 
    }
    elseif($type -eq 2) 
    {
        $FGColor = "Yellow"
        $pre = " WARNING " 
    }
    elseif($type -eq 3) 
    {
        $FGColor = "Red"
        $pre = "  ERROR  "
    }
    elseif($type -eq 4) 
    {
        $FGColor = "Magenta"
        $pre = "  PROMPT  "
    }

    Write-Host ($pre + ">") -ForegroundColor Black -BackgroundColor $FGColor -NoNewline
    Write-Host (" " + $txt) -ForegroundColor $FGColor 
}


function BITS-PasteFiles
{
	param (
        [string]$dstPath
    )

	$dstPath = [Management.Automation.WildcardPattern]::Escape($dstPath)

	if ([Windows.Clipboard]::ContainsFileDropList())
	{
		$clipItems =[Windows.Clipboard]::GetFileDropList()

		if($clipItems.Count -gt 0)
		{
			$sources = @()
			$destinations = @()
			$errorFiles = @()
			$errorDetected = $false
			# We've detected valid files in clipboard, setup BITS Transfer
			foreach ($item in $clipItems)
			{
				$escapedItem = [Management.Automation.WildcardPattern]::Escape($item)
				if(Test-Path $escapedItem)
				{
					# Path exists
					if(Test-Path $escapedItem -PathType leaf)
					{
						# Path is File						
						$sources += $escapedItem
						$destinations += $dstPath
					}
					else
					{
						# path is Folder
						# Use robocopy to create folder structure
						$curDestPath = $dstPath + "\" + (Split-Path $escapedItem -Leaf)
						robocopy $escapedItem $curDestPath /e /xf *.* | Out-Null

						# Add current folder
						$sources += "$($escapedItem)\*.*"
						$destinations += $curDestPath

						# Get child folders
						Get-ChildItem -Path $escapedItem -Directory *.* -Recurse | foreach { $spath = $_.FullName.Remove(0,$escapedItem.Length+1); $sources += "$item\$spath\*.*"; $destinations += "$curDestPath\$spath" }					
					}
				}
			}
			
			if($errorDetected -eq $true)
			{
				[System.Windows.MessageBox]::Show("File paths containing square brackets were detected.  BITS transfer does not support file paths containing wild card charaters.","Error",0,"Error")
				writeMessage "BITS Job was canceled" 3
				writeMessage "Bad file paths:" 3
				foreach ($item in $errorFiles)
				{
					Write-Host $item
				}
				read-host "Press ENTER to close window..."
				exit
			}

			# RetryTimeout 1 day
			$job = Start-BitsTransfer -Asynchronous -Source $sources -Destination $destinations -DisplayName "Clipboard job" -RetryTimeout 8640 -RetryInterval 60

			# Possible State values: Queued,    Connecting,    Transferring,    Suspended,    Error,    TransientError,    Transferred,    Acknowledged,    Canceled
			$BPS = 0
			$lastTransferedBytes = 0

			$JobTotalSizeStr = Format-FileSize $job.BytesTotal

			while (($job.JobState -eq "Queued") -or 
				($job.JobState -eq "Connecting") -or
				($job.JobState -eq "Transferring") -or
				($job.JobState -eq "Suspended") -or
				($job.JobState -eq "TransientError") -or
				($job.JobState -eq "Acknowledged"))
			{
				$BPS = $job.BytesTransferred - $lastTransferedBytes
				$BPSStr = Format-FileSize $BPS
				$BytesTransferredStr = Format-FileSize $job.BytesTransferred
				$pct = [int](($job.BytesTransferred*100) / $job.BytesTotal)
				Write-Progress -Activity "BITS Copy Job" -Status ("$($job.JobState) $pct% | Transient Error Count: $($job.TransientErrorCount) | $($job.FilesTransferred)/$($job.FilesTotal) Files | $BytesTransferredStr of $JobTotalSizeStr | $BPSStr/sec") -PercentComplete $pct 
				$lastTransferedBytes = $job.BytesTransferred 

				sleep 1;
			}

			Write-Progress -Activity "BITS Copy Job" -Completed

			Switch($job.JobState)
			{
				"Transferred" {
					writeMessage "Transfer Completed Successfully!" 1
					Complete-BitsTransfer -BitsJob $job
				}
				"Error" {
					writeMessage "Errors occurred while transfering files:" 3
					$job | Format-List
					read-host "Press ENTER to close window..."
				}
				"Canceled" {
					writeMessage "BITS Job was canceled" 3
					read-host "Press ENTER to close window..."
				}
			}
		}
	}
	else 
	{
		writeMessage "No files/folders detected in Clipboard" 2
	}
}

function addExplorerFolderContextCommand
{
	# Add Explorer context command
	param (
        [string]$keyName,
		[string]$cmdDisplayName,
		[string]$cmd
    )

	$keyRoot = "Registry::HKEY_CLASSES_ROOT\Directory\Background\shell"

	# Check if key already exists
	$keyPath = $keyRoot + "\" + $keyName

	if(Test-Path $keyPath)
	{
		# Update
		Set-Item -Path $keyPath -Value $cmdDisplayName
		Set-Item -Path "$keyPath\command" -Value $cmd
	}
	else
	{
		# create
		New-Item -Path $keyRoot -Name $keyName
		Set-Item -Path $keyPath -Value $cmdDisplayName

		New-Item -Path $keyPath -Name "command"
		Set-Item -Path "$keyPath\command" -Value $cmd
	}

}

function setBITSPolicyValues()
{
	# Setting Files per BITS Job limit since default is 200
	$BITSPolicyKeyRoot = "Registry::HKEY_LOCAL_MACHINE\Software\Policies\Microsoft\Windows\BITS"
	
	# Check if key already exists
	$MaxFilesPerJobKeyName = "MaxFilesPerJob"
	$MaxFilesPerJobKey = "$BITSPolicyKeyRoot\$MaxFilesPerJobKeyName"
	$maxFilesPerJob = 100000

	New-ItemProperty -Path $BITSPolicyKeyRoot -Name $MaxFilesPerJobKeyName -Value $maxFilesPerJob 
	
}

if($args.Length -eq 1)
{
	if($args[0] -eq "setup")
	{
		$currentPrincipal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
		if($currentPrincipal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator))
		{
			addExplorerFolderContextCommand -keyName "BITSPaste" -cmdDisplayName "Paste Files using BITS" -cmd ('Powershell -File "' + $PSCommandPath + '" "%V"')
			setBITSPolicyValues	
		}
		else
		{
			asAdminWithSTAAndParams @($PSCommandPath, "setup")
		}
	}
	elseif($args[0] -eq "test")
	{
		writeMessage "BITS Paste File Powershell script was called." 0	
		writeMessage $args[1] 0
		read-host "Press ENTER to close window..."
	}
	else
	{
		BITS-PasteFiles -dstPath $args[0]
	}
}

