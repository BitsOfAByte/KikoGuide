namespace KikoGuide.UI.ImGuiFullComponents.DutyList
{
    using ImGuiNET;
    using KikoGuide.Localization;
    using KikoGuide.Types;
    using KikoGuide.Managers;
    using KikoGuide.UI.ImGuiBasicComponents;
    using System;
    using System.Linq;
    using System.Collections.Generic;

    /// <summary>
    ///     A component for displaying a list of duties.
    /// </summary>
    public static class DutyListComponent
    {
        private static DutyListPresenter _presenter = new DutyListPresenter();

        /// <summary>
        ///     Draws the duty list.
        /// </summary>
        /// <param name="dutyPool"> The duty pool to draw information for. </param>
        /// <param name="onDutySelect"> The action to call when a duty is selected. </param>
        /// <param name="filter"> The filter to use when drawing the duty list, empty to show all. </param>
        /// <param name="dutyType"> The duty type to show listings for, or null to show all. </param>
        public static void Draw(List<Duty> dutyPool, Action<Duty> onDutySelected, string filter = "", int? dutyType = null)
        {
            try
            {
                List<Duty> dutyList;
                if (dutyType != null) dutyList = dutyPool.Where(duty => duty.Type == dutyType).ToList();
                else dutyList = dutyPool;

                // If there are no duties found, display a message and return.
                if (dutyList.Count() == 0)
                {
                    ImGui.TextDisabled(TStrings.DutyListNoneFound);
                    return;
                }

                // Create a table for each duty, containing its level and name.
                if (ImGui.BeginTable("DutyList", 2, ImGuiTableFlags.ScrollY))
                {
                    ImGui.TableSetupScrollFreeze(0, 1);
                    ImGui.TableSetupColumn(TStrings.Level, ImGuiTableColumnFlags.WidthFixed, 45);
                    ImGui.TableSetupColumn(TStrings.Duty);
                    ImGui.TableHeadersRow();

                    // Fetch all duties for this duty type and draw them.
                    foreach (var duty in dutyList)
                    {
                        // Do not show the duty if it isn't unlocked or isn't part of the filter.
                        if (!DutyManager.IsUnlocked(duty)) continue;
                        if (!duty.Name.ToLower().Contains(filter.ToLower())) continue;

                        // Ad the level and duty name to the list.
                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                        ImGui.Text(duty.Level.ToString());
                        ImGui.TableNextColumn();

                        // If this duty does not have any data or is unsupported, draw it as such and move on.
                        if (!_presenter.HasDutyData(duty)) { NoDataDuty(duty.GetCanonicalName()); continue; }
                        if (!duty.IsSupported()) { UnsupportedDuty(duty.GetCanonicalName()); continue; }

                        // Draw a selectable text for this duty and trigger the onDutySelected event when clicked.
                        if (ImGui.Selectable(duty.GetCanonicalName(), false, ImGuiSelectableFlags.AllowDoubleClick)) onDutySelected(duty);

                        // If the player is inside this duty, add some text next to it.
                        if (duty == _presenter.GetPlayerDuty()) Badges.Custom(Colours.Green, TStrings.InDuty);
                    }

                    ImGui.EndTable();
                }
            }
            catch (Exception e) { ImGui.TextColored(Colours.Error, $"Component Exception: {e.Message}"); }
        }


        /// <summary>
        ///     Draws an unsupported duty in the list.
        /// </summary>
        private static void UnsupportedDuty(string name)
        {
            ImGui.TextDisabled(name);
            Badges.Questionmark(TStrings.DutyListNeedsUpdate);
        }

        /// <summary>
        ///     Draws a duty with no data.
        /// </summary>
        private static void NoDataDuty(string name)
        {
            ImGui.TextColored(Colours.Red, name);
            Badges.Questionmark(TStrings.DutyListNoGuide(name));
        }
    }
}