﻿#NoEnv  ; Recommended for performance and compatibility with future AutoHotkey releases.
; #Warn  ; Enable warnings to assist with detecting common errors.
SendMode Input  ; Recommended for new scripts due to its superior speed and reliability.
SetWorkingDir %A_ScriptDir%  ; Ensures a consistent starting directory.

Sleep 50
Send {LAlt Down}
Sleep 50
Send {F4 Down}
Sleep 50
Send {F4 Up}
Sleep 50
Send {LAlt Up}
Sleep 50