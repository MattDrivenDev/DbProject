﻿---------------------------------------------------------------------
-- $DbName$ - Release Script
---------------------------------------------------------------------
-- Version:					$Version$
-- Generated date:			$ReleaseDate$
-- Generated on:			$ReleaseMachine$
-- Generated by:			$ReleaseUser$
--
-- IMPORTANT! 
-- Before executing this script, you should really take a backup of 
-- the target database.
---------------------------------------------------------------------

SET IMPLICIT_TRANSACTIONS, NUMERIC_ROUNDABORT OFF;
SET ANSI_NULLS, ANSI_PADDING, ANSI_WARNINGS, ARITHABORT, CONCAT_NULL_YIELDS_NULL, NOCOUNT, QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;

$foreach_script_begin$
---------------------------------------------------------------------
-- $script_name$
---------------------------------------------------------------------
GO

$script_content$

$foreach_script_end$

SET NOEXEC OFF;