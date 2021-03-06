﻿/*
==========================================================================
This file is part of Headquarters for DCS World (HQ4DCS), a mission generator for
Eagle Dynamics' DCS World flight simulator.

HQ4DCS was created by Ambroise Garel (@akaAgar).
You can find more information about the project on its GitHub page,
https://akaAgar.github.io/headquarters-for-dcs

HQ4DCS is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

HQ4DCS is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with HQ4DCS. If not, see https://www.gnu.org/licenses/
==========================================================================
*/

using Headquarters4DCS.Mission;
using Headquarters4DCS.Miz;
using Headquarters4DCS.Template;
using Headquarters4DCS.Tools;
using System;
using System.IO;
using System.Windows.Forms;

namespace Headquarters4DCS.Forms
{
    /// <summary>
    /// The main form of the application
    /// </summary>
    public partial class FormMain : Form
    {
        /// <summary>
        /// Mission template.
        /// </summary>
        public MissionTemplate Template { get; private set; } = null;

        /// <summary>
        /// Generated mission.
        /// </summary>
        private DCSMission Mission = null;

        /// <summary>
        /// File path to the last saved mission template. Null is mission wasn't saved yet.
        /// </summary>
        private string LastSaveFilePath = null;

        /// <summary>
        /// The main class of the application.
        /// </summary>
        private readonly HQ4DCS HQ;

        /// <summary>
        /// Constructor
        /// </summary>
        public FormMain(HQ4DCS hq)
        {
            InitializeComponent();
            HQ = hq;
            Template = new MissionTemplate();
        }

        /// <summary>
        /// Load all icons from the embedded ressources. Called once when forms is created.
        /// </summary>
        private void LoadIcons()
        {
            Icon = GUITools.GetIconFromResource("Icon.ico");

            MenuFileNew.Image = GUITools.GetImageFromResource("Icons.new.png");
            MenuFileOpen.Image = GUITools.GetImageFromResource("Icons.load.png");
            MenuFileSave.Image = GUITools.GetImageFromResource("Icons.save.png");
            MenuFileSaveAs.Image = GUITools.GetImageFromResource("Icons.saveAs.png");
            MenuFileExit.Image = GUITools.GetImageFromResource("Icons.exit.png");

            MenuMissionGenerate.Image = GUITools.GetImageFromResource("Icons.refresh.png");
            MenuMissionExportMIZ.Image = GUITools.GetImageFromResource("Icons.exportMIZ.png");
            MenuMissionExportBriefing.Image = GUITools.GetImageFromResource("Icons.exportBriefing.png");

            ToolStripButtonFileNew.Image = MenuFileNew.Image;
            ToolStripButtonFileOpen.Image = MenuFileOpen.Image;
            ToolStripButtonFileSave.Image = MenuFileSave.Image;
            ToolStripButtonFileSaveAs.Image = MenuFileSaveAs.Image;

            ToolStripButtonMissionGenerate.Image = MenuMissionGenerate.Image;
            ToolStripButtonMissionExportMIZ.Image = MenuMissionExportMIZ.Image;
            ToolStripButtonMissionExportBriefing.Image = MenuMissionExportBriefing.Image;
        }

        /// <summary>
        /// Form load event.
        /// </summary>
        /// <param name="sender">The form.</param>
        /// <param name="e">Event arguments.</param>
        private void Event_FormLoad(object sender, EventArgs e)
        {
            LoadIcons();
            TemplatePropertyGrid.SelectedObject = Template;
            GenerateMission();
        }

        /// <summary>
        /// Event raised when the form is closed.
        /// </summary>
        /// <param name="sender">The form.</param>
        /// <param name="e">Event parameters</param>
        private void Event_FormClosed(object sender, FormClosedEventArgs e) { DestroyMission(); }

        /// <summary>
        /// Event raised when any menu or toolbar item is clicked, apart from the "new mission" theater subitems.
        /// (These are handled by Event_MenuFileNewDropDownItemClicked)
        /// </summary>
        /// <param name="sender">Sender control.</param>
        /// <param name="e">Event arguments.</param>
        private void Event_MenuClick(object sender, EventArgs e)
        {
            string senderName = ((ToolStripItem)sender).Name;

            switch (senderName)
            {
                case "MenuFileNew":
                case "ToolStripButtonFileNew":
                    Template.Clear();
                    return;
                case "MenuFileOpen":
                case "ToolStripButtonFileOpen":
                    string fileToLoad = GUITools.ShowOpenFileDialog("hqt", HQTools.PATH_TEMPLATES, "HQ4DCS mission templates");
                    if (fileToLoad == null) return;
                    Template.LoadFromFile(fileToLoad);
                    LastSaveFilePath = fileToLoad;
                    UpdateFormTitle();
                    return;
                case "MenuFileSave":
                case "ToolStripButtonFileSave":
                    if (string.IsNullOrEmpty(LastSaveFilePath)) { Event_MenuClick(MenuFileSaveAs, e); return; }
                    Template.SaveToFile(LastSaveFilePath);
                    return;
                case "MenuFileSaveAs":
                case "ToolStripButtonFileSaveAs":
                    string fileToSave = GUITools.ShowSaveFileDialog(
                        "hqt", HQTools.PATH_TEMPLATES,
                        string.IsNullOrEmpty(LastSaveFilePath) ? "NewMission.hqt" : Path.GetFileName(LastSaveFilePath),
                        "HQ4DCS mission templates");
                    if (fileToSave == null) return;
                    Template.SaveToFile(fileToSave);
                    LastSaveFilePath = fileToSave;
                    UpdateFormTitle();
                    return;
                case "MenuFileExit":
                    Close();
                    return;
                case "MenuMissionGenerate":
                case "ToolStripButtonMissionGenerate":
                    GenerateMission();
                    return;
                case "MenuToolsRadioMessageGenerator":
                    ShowDevToolWarningMessage();
                    using (FormRadioMessageGenerator rmgForm = new FormRadioMessageGenerator()) { rmgForm.ShowDialog(); }
                    return;

                case "MenuMissionExportMIZ":
                case "ToolStripButtonMissionExportMIZ":
                    if (Mission == null) return;

                    string defaultFileName = HQTools.RemoveInvalidFileNameCharacters(Mission.BriefingName ?? "");
                    if (string.IsNullOrEmpty(defaultFileName)) defaultFileName = "New mission";

#if DEBUG
                    using (MizExporter mizExporter = new MizExporter())
                    {
                        if (mizExporter.CreateMizFile(Mission, $"{HQTools.PATH_DEBUG}DebugMission.miz"))
                            MessageBox.Show($"Mission exported to (DebugOutput)\\DebugMission.miz", "Debug export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        else
                            MessageBox.Show($"Failed to export mission to (DebugOutput)\\DebugMission.miz", "Debug export", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
#else
                    string mizFilePath = GUITools.ShowSaveFileDialog(
                            "miz", HQTools.GetDCSMissionPath(),
                            defaultFileName, "DCS World mission files");

                        if (!string.IsNullOrEmpty(mizFilePath))
                        {
                            using (MizExporter mizExporter = new MizExporter())
                            { mizExporter.CreateMizFile(Mission, mizFilePath); }
                        }
#endif
                    return;

                case "MenuMissionExportBriefingHTML":
                case "ToolStripButtonMissionExportBriefingHTML":
                    ExportBriefing(BriefingExportFileFormat.Html);
                    return;

                case "MenuMissionExportBriefingJPG":
                case "ToolStripButtonMissionExportBriefingJPG":
                    ExportBriefing(BriefingExportFileFormat.Jpg);
                    return;

                case "MenuMissionExportBriefingPNG":
                case "ToolStripButtonMissionExportBriefingPNG":
                    ExportBriefing(BriefingExportFileFormat.Png);
                    return;
            }
        }

        /// <summary>
        /// Event raised when the form is closing.
        /// </summary>
        /// <param name="sender">This form.</param>
        /// <param name="e">Form closing arguments.</param>
        private void Event_MainFormClosing(object sender, FormClosingEventArgs e)
        {
            // If not a debug build, show a confirmation message before closing.
#if !DEBUG
            e.Cancel =
                (MessageBox.Show(
                    "Are you sure you want to close HQ4DCS?\r\nUnsaved changes will be lost.",
                    "Exit?",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes);
#endif
        }

        /// <summary>
        /// Event raised when the mission template is changed in the property grid. Generates a new mission.
        /// </summary>
        /// <param name="s">The changed value</param>
        /// <param name="e">Property event</param>
        private void Event_TemplatePropertyGridPropertyValueChanged(object source, PropertyValueChangedEventArgs e)
        {
            GenerateMission();
        }

        /// <summary>
        /// Destroys the generated mission, if a mission was generated.
        /// </summary>
        private void DestroyMission()
        {
            if (Mission == null) return;

            Mission.Dispose();
            Mission = null;
        }

        /// <summary>
        /// Display a "warning: this is a development tool" message box the first time the method is called.
        /// </summary>
        private void ShowDevToolWarningMessage()
        {
#if !DEBUG
            if (DevToolWarningAlreadyShown) return;
            DevToolWarningAlreadyShown = true;

            MessageBox.Show(
                "The tool you're about to use is intended for HQ4DCS developers and modders. If you just want to create missions and do not intend to add new theaters, missions and the like to the HQ4DCS library, you shouldn't need it.",
                "Development tools", MessageBoxButtons.OK, MessageBoxIcon.Information);
#endif
        }

        /// <summary>
        /// Updates the title of the form. Called when a mission is created, loaded or saved, to change the displayed file name.
        /// </summary>
        private void UpdateFormTitle()
        {
            Text =
                $"Headquarters {HQ4DCS.HQ4DCS_VERSION_STRING} for DCS World {HQ4DCS.DCSWORLD_TARGETED_VERSION} - " +
                (string.IsNullOrEmpty(LastSaveFilePath) ? "New mission" : Path.GetFileName(LastSaveFilePath));
        }

        /// <summary>
        /// Generates a new mission from the mission template.
        /// </summary>
        private void GenerateMission()
        {
            SetExportMenuButtonsEnabledState(false);

            DestroyMission();
            Mission = HQ.Generator.Generate(Template, out string errorMessage);

            BriefingWebBrowser.Navigate("about:blank");
            BriefingWebBrowser.Document.OpenNew(false);
            if (Mission == null)
                BriefingWebBrowser.Document.Write($"<html><head></head><body><h2>Failed to generate mission</h2><p><strong>ERROR: </strong>{errorMessage}</p></body>");
            else
                BriefingWebBrowser.Document.Write(Mission.BriefingHTML);
            BriefingWebBrowser.Refresh();

            SetExportMenuButtonsEnabledState(Mission != null);
        }

        /// <summary>
        /// Exports the mission briefing to an HTML or image file.
        /// </summary>
        /// <param name="fileFormat">The file format to export to.</param>
        private void ExportBriefing(BriefingExportFileFormat fileFormat)
        {
            if (Mission == null) return; // No mission has been generated, nothing to export.

            string defaultFileName = HQTools.RemoveInvalidFileNameCharacters(Mission.BriefingName ?? "");
            if (string.IsNullOrEmpty(defaultFileName)) defaultFileName = "NewMission";

            string briefingFilePath = GUITools.ShowSaveFileDialog(
                fileFormat.ToString().ToLowerInvariant(), HQTools.GetDCSMissionPath(),
                defaultFileName, $"{fileFormat.ToString().ToUpperInvariant()} files");

            if (briefingFilePath == null) return;

            bool result;

            using (HTMLExporter briefingExporter = new HTMLExporter())
            {
                switch (fileFormat)
                {
                    default: return;
                    case BriefingExportFileFormat.Html: result = briefingExporter.ExportToHTML(briefingFilePath, Mission.BriefingHTML); break;
                    case BriefingExportFileFormat.Jpg: result = briefingExporter.ExportToJPEG(briefingFilePath, Mission.BriefingHTML); break;
                    case BriefingExportFileFormat.Png: result = briefingExporter.ExportToPNG(briefingFilePath, Mission.BriefingHTML); break;
                }
            }

            if (!result)
                MessageBox.Show("Failed to export briefing", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Changes the enabled state of all mission/briefing export MenuItems and ToolStripButtons.
        /// </summary>
        /// <param name="enabled">Should the buttons be enabled?</param>
        private void SetExportMenuButtonsEnabledState(bool enabled)
        {
            MenuMissionExportMIZ.Enabled = enabled;
            ToolStripButtonMissionExportMIZ.Enabled = enabled;

            MenuMissionExportBriefing.Enabled = enabled;
            ToolStripButtonMissionExportBriefing.Enabled = enabled;
            MenuMissionExportBriefingHTML.Enabled = enabled;
            ToolStripButtonMissionExportBriefingHTML.Enabled = enabled;
            MenuMissionExportBriefingJPG.Enabled = enabled;
            ToolStripButtonMissionExportBriefingJPG.Enabled = enabled;
            MenuMissionExportBriefingPNG.Enabled = enabled;
            ToolStripButtonMissionExportBriefingPNG.Enabled = enabled;
        }
    }
}
