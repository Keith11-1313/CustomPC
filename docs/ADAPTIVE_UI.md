# Adaptive desktop UI

CustomPC remains a Windows Forms application with focused modal editors and one hosted workspace. The redesign uses desktop command grouping and actual layout reflow instead of shrinking fixed coordinates.

## Visual direction

- Canvas: #F7F8FA; product/data surfaces: #FFFFFF; navigation/text: #1B2725; primary action: #147750; selected surface: #E6F3ED; border: #DEE4DF.
- Righteous is reserved for the CustomPC brand. Segoe UI is used for application headings, working controls and data, with distinct title, value, body and caption sizes. Labels identify fields; icon-only rail buttons retain accessible names and tooltips.
- Main shell: expanded navigation at 1180 effective pixels and above; a compact rail below it. Navigation scrolls independently on short windows. Overview scrolls vertically while navigation stays in place.
- Data pages: title and description, commands, wrapping filter groups, then a single data surface. Refresh stays at the upper right. Page headers and command spacing are compact, with restrained secondary buttons and emerald primary actions. Commands preserve role permissions.
- Catalog: category rail on wide pages, wrapping category buttons on narrow pages. Cards fill available columns and change to compact horizontal rows in short or narrow product areas. Search waits 180 ms after typing to avoid rebuilding photographs on every keystroke.
- Builder: individual component cards with photos, selectors and Details actions. Checkout moves below the component list at narrower widths. The list scrolls independently.
- Editors: labelled field groups reflow from two columns to one; field area scrolls and Save/Cancel remain in a separate action bar. Category-specific fields are removed from layout rather than disabled.
- Admin keeps analytics. Staff uses summary cards, its current queue and operational links, without charts. Customer and Guest get build/catalog actions. Guest has no personal orders.

## Implementation map

WorkspacePage and WorkspacePage.Designer define the shared list workspace. EditorSheet owns editor field reflow. DashboardForm.Layout, CatalogForm.Layout, LoginForm.Layout and BuilderForm.Layout define screen-specific composition. Existing form Designer files still own the editable controls and the business handlers remain in their original forms.

## Research used

- Microsoft [responsive design techniques](https://learn.microsoft.com/en-us/windows/apps/design/layout/responsive-design): reposition, resize and reflow content based on usable space.
- Microsoft [NavigationView](https://learn.microsoft.com/en-us/windows/apps/develop/ui/controls/navigationview): consistent navigation with expanded and compact modes.
- Microsoft [desktop productivity design](https://learn.microsoft.com/en-us/windows/apps/get-started/line-of-business/design-for-lob): grouped commands, appropriate density and adapting details panes rather than shrinking everything.

These are design principles adapted to WinForms, not a claim that the application uses WinUI or its native materials. Native Designer, accessibility tools, printer and actual OS DPI checks must be recorded separately from bitmap renders.

Typography reference: Microsoft Windows typography guidance (https://learn.microsoft.com/en-us/windows/apps/design/signature-experiences/typography). Segoe UI is used for WinForms/GDI compatibility; this is not a claim of native WinUI controls.
