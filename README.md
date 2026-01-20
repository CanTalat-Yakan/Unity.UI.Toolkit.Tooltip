# Unity Essentials

This module is part of the Unity Essentials ecosystem and follows the same lightweight, editor-first approach.
Unity Essentials is a lightweight, modular set of editor utilities and helpers that streamline Unity development. It focuses on clean, dependency-free tools that work well together.

All utilities are under the `UnityEssentials` namespace.

```csharp
using UnityEssentials;
```

## Installation

Install the Unity Essentials entry package via Unity's Package Manager, then install modules from the Tools menu.

- Add the entry package (via Git URL)
    - Window → Package Manager
    - "+" → "Add package from git URL…"
    - Paste: `https://github.com/CanTalat-Yakan/UnityEssentials.git`

- Install or update Unity Essentials packages
    - Tools → Install & Update UnityEssentials
    - Install all or select individual modules; run again anytime to update

---

# UI Toolkit Tooltip

> Quick overview: Zero‑setup hover tooltips for UI Toolkit. Uses the built‑in `VisualElement.tooltip` property, auto‑injects a USS, and renders a clamped floating label near the hovered element.

A lightweight helper that automatically attaches to every `UIDocument` after scene load. It injects a tooltip stylesheet and creates a single floating `Label` (class `tooltip-label`). When you hover any element with a non‑empty `tooltip` property, the helper shows the label near that element, clamps it to the panel bounds, and hides it when not applicable - no custom editor code needed.

![screenshot](Documentation/Screenshot.png)

## Features
- Auto‑attach to all `UIDocument`s
  - Added at runtime via `RuntimeInitializeOnLoadMethod(AfterSceneLoad)`; no manual setup needed
- Works with UI Toolkit’s built‑in `tooltip`
  - Set `element.tooltip` in code or via UI Builder; the helper finds the first ancestor with a non‑empty tooltip
- Styling via USS
  - Loads `Resources/UnityEssentials_USS_Tooltip.uss` and adds it to the document
  - Tooltip label uses class `tooltip-label` (absolute, non‑picking)
- Smart positioning
  - Positions below/right of the hovered element with a small offset (25, 5)
  - Clamps to the panel bounds to avoid going off‑screen
- Efficient updates
  - Skips work if the mouse hasn’t moved and the tooltip is already visible
  - Ignores the tooltip label itself when hit‑testing (so it never interferes with picking)
- Multiple documents supported
  - Each helper only shows tooltips for elements under its own `UIDocument`

## Requirements
- Unity Editor 6000.0+
- At least one `UIDocument` in your scene
- Dependencies (Unity Essentials modules)
  - UI Toolkit Extensions: provides `UIDocument.AddStyleSheet(...)`
  - Utilities – ResourceLoader: provides `ResourceLoader.TryGet<StyleSheet>(..)`, used to load the USS
- Resources
  - `Resources/UnityEssentials_USS_Tooltip.uss` (included in this package)

Tip: Customize the look by editing `UnityEssentials_USS_Tooltip.uss` and styling the `.tooltip-label` class.

## Usage
1) Add a `UIDocument`
- Ensure your scene contains one or more `UIDocument`s; the helper attaches automatically on play

2) Set tooltips on elements
- In UI Builder: select any element and fill the Tooltip field
- From code:
```csharp
var root = GetComponent<UIDocument>().rootVisualElement;
var button = root.Q<Button>("MyButton");
button.tooltip = "Click to start";
```

3) Run your scene
- Hover elements with tooltips and observe the floating label; it’s rendered as a child of the document root and stays within bounds

### Styling
- Edit `Resources/UnityEssentials_USS_Tooltip.uss`
- The tooltip label carries the class `tooltip-label`; adjust font, padding, background, border, and transitions as needed

## How It Works
- Initialization
  - On load, finds all `UIDocument`s and adds `TooltipHelper` to those without one
  - On `Awake`, loads the tooltip USS via `ResourceLoader` and calls `document.AddStyleSheet(…)`
  - Creates a single `Label`, adds class `tooltip-label`, sets `position: Absolute`, and adds it under the document root
- Update loop
  - Uses `Input.mousePosition` and converts to panel space; picks the element under the pointer and walks up to find a non‑empty `tooltip`
  - Ensures the element is a descendant of this document’s root (so multiple docs don’t cross‑display)
  - Shows/hides the label based on content, updates text only when it changes
  - Computes position from the hovered element’s `worldBound` plus a small offset; clamps against `panel.visualTree.layout` size

## Notes and Limitations
- Motion‑based updates
  - Position is refreshed primarily when the mouse moves; if the target element animates while the mouse is still, the label may not reposition until the next mouse movement
- Coordinate system
  - Y is flipped from screen to panel coordinates; helper uses `RuntimePanelUtils.ScreenToPanel`
- Z‑order
  - The label is added last to the document root and uses absolute positioning; if you have special z‑index rules, adjust USS accordingly
- Customization
  - Class name and offset are fixed in this component; style the `.tooltip-label` class to change visuals and spacing

## Files in This Package
- `Runtime/TooltipHelper.cs` – Auto‑attach helper; USS injection; picking and label placement
- `Runtime/UnityEssentials.UIToolkitTooltip.asmdef` – Runtime assembly definition
- `Resources/UnityEssentials_USS_Tooltip.uss` – Default tooltip styles
- `package.json` – Package manifest metadata

## Tags
unity, ui toolkit, tooltip, hover, uidocument, uxml, uss, visualelement, runtime
