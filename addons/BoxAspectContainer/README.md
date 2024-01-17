# BoxAspectContainer

BoxAspectContainer is a Godot plugin implementing a custom Container class. It is similar to the built-in BoxContainer class, but it provides many more configuration options, and it allows preserving the aspect ratio of child controls when scaling them to fit.

This addon can also be found in the [Godot Asset Library](https://godotengine.org/asset-library/asset/2518).

![Example project screenshot](https://github.com/pineapplemachine/Godot-BoxAspectContainer/blob/master/example/Screenshots/Screenshot.png?raw=true)

In this documentation, the **Flow** axis of the BoxAspectContainer refers to the axis that child controls are laid out in a line on, i.e. the X axis when the container's Vertical flag is false or the Y axis when it's true. The **Fit** axis refers to the opposite axis, upon which controls are aligned or sized to fit, i.e. the Y axis when the container's Vertical flag is false or the X axis when it's true.

## Usage

To use BoxAspectContainer in your Godot project, clone or download this repository and copy the contents of its `addons/` subdirectory into your Godot project's own `addons/` subdirectory.

Alternatively, you can use Godot's AssetLib tool to search for the BoxAspectContainer asset and add it to your project via the editor.

![AssetLib search screenshot](https://github.com/pineapplemachine/Godot-BoxAspectContainer/blob/master/example/Screenshots/AssetLib.png?raw=true)

Note that this container class is implemented in C#. This means that you will need to use Godot with C# enabled (i.e. using the Mono build of the engine). It also means, due to current limitations of the C# API, that the class and its properties will not have descriptions in the editor. You can refer to this readme or to the commented source code in `addons/BoxAspectContainer/BoxAspectContainer.cs` for detailed descriptions of the class and its attributes.

## Properties

**FlowAlignment:**
Determines how controls are aligned in the container
along its flow axis. Begin places them at the left or
top, end places them at the right or bottom, and center
places them at the center.

**Vertical:**
Determines whether child controls flow in a horizontal or
vertical line within the container.

**Reverse:**
When set, child controls are placed in reverse order.

**OverrideRotation:**
When set, child controls will have their rotations
all overridden and set to 0.
Note that child controls with non-zero rotation may
have unusual or unintended layouts within the container.

**OverrideScale:**
When set, child controls will have their scale vectors
all overridden and set to (1, 1).
Note that child controls with non-default scale may
have unusual or unintended layouts within the container.

**BoxContainerThemeSeparation:**
When set, adds the theme's "separation" value for the
BoxContainer class to the separation between controls in
the container.

**ControlSeparationPixels:**
Amount of extra space to add in between each child Control,
in pixels.

**ControlSeparationProportional:**
Amount of extra space to add in between each child Control,
as a proportion of the container's fit axis (height when
horizontal, width when vertical).

**ShrinkToFit:**
When this flag is set, and the contents of the container
exceed its width (when horizontal) or its height
(when vertical), the contents will all be scaled down and
made to fit.

**ExpandToFit:**
When this flag is set, and the contents of the container
are less than its width (when horizontal) or its height
(when vertical), the contents will all be scaled up in
order to take up the full space.

**LayoutHiddenControls:**
When set, controls with their Visible flag set to false
will still take up space within the container and
influence the layout of the visible controls.

**InnerMarginPixels:**
Adds an inner margin to the container, in between its
borders and the contained child controls, measured in
pixels.

**InnerMarginProportional:**
Adds an inner margin to the container, in between its
borders and the contained child controls,
as a proportion of the container's fit axis (height when
horizontal, width when vertical).

## Child container sizing

The BoxAspectContainer lays out controls differently depending on their horizontal and vertical container sizing flags.

### Container fit axis

The following flags are checked in the provided order of precedence, meaning that Fill behavior takes precedence over Expand behavior, which takes precedence over ShrinkBegin behavior, and so on.

If the **Fill** flag is set along the container's fit axis, then the control is scaled up or down to fit, while preserving the aspect ratio indicated by its minimum size values.

If the **Expand** flag is set along the container's fit axis, then the control is scaled up or down only on that axis, and its minimum size is used as-is on the flow axis.

If the **ShrinkBegin** flag is set along the container's fit axis, then the control will be aligned to the top or the left of the container, and it will be scaled down to fit if it's larger than the container on its fit axis, while maintaining aspect ratio.

If the **ShrinkEnd** flag is set along the container's fit axis, then the control will be aligned to the bottom or the right of the container, and it will be scaled down to fit if it's larger than the container on its fit axis, while maintaining aspect ratio.

If the **ShrinkCenter** flag is set along the container's fit axis, then the control will be aligned to the center of the container, and it will be scaled down to fit if it's larger than the container on its fit axis, while maintaining aspect ratio.

### Container flow axis

If the **Expand** flag is set, then the container will distribute any unused space along its flow axis among all such flagged child controls, weighted by those controls' stretch ratio. When the **Fill** flag is also set, the control will be stretched on the flow axis to take up that space. Otherwise, the space will be added as margin.
