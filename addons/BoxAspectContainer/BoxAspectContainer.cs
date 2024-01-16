using System.Collections.Generic;
using System.Linq;
using Godot;

namespace PineappleAddon;

/**
 * Alternative to BoxContainer with more options and
 * functionality, including the option to preserve the
 * aspect ratio of child controls when scaling them to
 * fill the width or height of the container.
 */
[Tool]
public partial class BoxAspectContainer : Container {
    private BoxContainer.AlignmentMode flowAlignment = BoxContainer.AlignmentMode.Begin;
    private bool vertical = false;
    private bool reverse = false;
    private bool overrideRotation = false;
    private bool overrideScale = false;
    private bool boxContainerThemeSeparation = false;
    private float controlSeparationPixels = 0.0f;
    private float controlSeparationProportional = 0.0f;
    private bool shrinkToFit = false;
    private bool expandToFit = false;
    private bool layoutHiddenControls = false;
    private Vector2 innerMarginPixels = Vector2.Zero;
    private Vector2 innerMarginProportional = Vector2.Zero;
    
    /**
     * Determines how controls are aligned in the container
     * along its flow axis. Begin places them at the left or
     * top, end places them at the right or bottom, and center
     * places them at the center.
     */
    [Export]
    public BoxContainer.AlignmentMode FlowAlignment {
        get => this.flowAlignment;
        set {
            this.flowAlignment = value;
            this.Resort();
        }
    }
    
    /**
     * Determines whether child controls flow in a horizontal or
     * vertical line within the container.
     */
    [Export]
    public bool Vertical {
        get => this.vertical;
        set {
            this.vertical = value;
            this.Resort();
        }
    }
    
    /**
     * When set, child controls are placed in reverse order.
     */
    [Export]
    public bool Reverse {
        get => this.reverse;
        set {
            this.reverse = value;
            this.Resort();
        }
    }
    
    /**
     * When set, child controls will have their rotations
     * all overridden and set to 0.
     * Note that child controls with non-zero rotation may
     * have unusual or unintended layouts within the container.
     */
    [Export]
    public bool OverrideRotation {
        get => this.overrideRotation;
        set {
            this.overrideRotation = value;
            this.Resort();
        }
    }
    
    /**
     * When set, child controls will have their scale vectors
     * all overridden and set to (1, 1).
     * Note that child controls with non-default scale may
     * have unusual or unintended layouts within the container.
     */
    [Export]
    public bool OverrideScale {
        get => this.overrideScale;
        set {
            this.overrideScale = value;
            this.Resort();
        }
    }
    
    /**
     * When set, adds the theme's "separation" value for the
     * BoxContainer class to the separation between controls in
     * the container.
     */
    [Export]
    public bool BoxContainerThemeSeparation {
        get => this.boxContainerThemeSeparation;
        set {
            this.boxContainerThemeSeparation = value;
            this.Resort();
        }
    }
    
    /**
     * Amount of extra space to add in between each child Control,
     * in pixels.
     */
    [Export]
    public float ControlSeparationPixels {
        get => this.controlSeparationPixels;
        set {
            this.controlSeparationPixels = value;
            this.Resort();
        }
    }
    
    /**
     * Amount of extra space to add in between each child Control,
     * as a proportion of the container's fit axis (height when
     * horizontal, width when vertical).
     */
    [Export]
    public float ControlSeparationProportional {
        get => this.controlSeparationProportional;
        set {
            this.controlSeparationProportional = value;
            this.Resort();
        }
    }
    
    /**
     * When this flag is set, and the contents of the container
     * exceed its width (when horizontal) or its height
     * (when vertical), the contents will all be scaled down and
     * made to fit.
     */
    [Export]
    public bool ShrinkToFit {
        get => this.shrinkToFit;
        set {
            this.shrinkToFit = value;
            this.Resort();
        }
    }
    
    /**
     * When this flag is set, and the contents of the container
     * are less than its width (when horizontal) or its height
     * (when vertical), the contents will all be scaled up in
     * order to take up the full space.
     */
    [Export]
    public bool ExpandToFit {
        get => this.expandToFit;
        set {
            this.expandToFit = value;
            this.Resort();
        }
    }
    
    /**
     * When set, controls with their Visible flag set to false
     * will still take up space within the container and
     * influence the layout of the visible controls.
     */
    [Export]
    public bool LayoutHiddenControls {
        get => this.layoutHiddenControls;
        set {
            this.layoutHiddenControls = value;
            this.Resort();
        }
    }
    
    /**
     * Adds an inner margin to the container, in between its
     * borders and the contained child controls, measured in
     * pixels.
     */
    [Export]
    public Vector2 InnerMarginPixels {
        get => this.innerMarginPixels;
        set {
            this.innerMarginPixels = value;
            this.Resort();
        }
    }
    
    /**
     * Adds an inner margin to the container, in between its
     * borders and the contained child controls,
     * as a proportion of the container's fit axis (height when
     * horizontal, width when vertical).
     */
    [Export]
    public Vector2 InnerMarginProportional {
        get => this.innerMarginProportional;
        set {
            this.innerMarginProportional = value;
            this.Resort();
        }
    }
    
    public override void _Notification(int what) {
        if((long) what is
            Container.NotificationSortChildren or
            Control.NotificationLayoutDirectionChanged or
            Control.NotificationResized or
            Control.NotificationThemeChanged or
            CanvasItem.NotificationTransformChanged or
            Node.NotificationChildOrderChanged or
            Node.NotificationEnterTree or
            Node.NotificationReady or
            Node.NotificationTranslationChanged
        ) {
            this.Resort();
        }
    }
    
    /**
     * Enumerate child Control nodes.
     * 
     * This function excludes Controls with their Visible flag
     * set to false, unless the BoxAspectContainer's
     * LayoutHiddenControls flag is true.
     */
    public IEnumerable<Control> GetControls() {
        IEnumerable<Node> child_nodes = (
            this.Reverse ?
            ((IEnumerable<Node>) this.GetChildren()).Reverse() :
            this.GetChildren()
        );
        foreach(Node child_node in child_nodes) {
            if(child_node is Control child_control) {
                if(!child_control.Visible && !this.LayoutHiddenControls) {
                    continue;
                }
                yield return child_control;
            }
        }
    }
    
    /**
     * Assign positions and sizes to child controls, according to
     * the container's layout settings.
     */
    public void Resort() {
        Vector2 inner_margin = this.get_inner_margin();
        Vector2 this_size_less_margin = this.Size - (2 * inner_margin);
        float this_size_flow = this.get_flow_axis(this_size_less_margin);
        float this_size_fit = this.get_fit_axis(this_size_less_margin);
        float sep_size = this.get_separation_size(this_size_fit);
        float child_stretch_ratio_sum = 0.0f;
        float child_offset = 0.0f;
        bool first_child_control = true;
        // Initially scale and set position of each child control.
        foreach(Control child_control in this.GetControls()) {
            if(first_child_control) {
                first_child_control = false;
            }
            else {
                child_offset += sep_size;
            }
            if(this.OverrideRotation) {
                child_control.Rotation = 0;
            }
            if(this.OverrideScale) {
                child_control.Scale = Vector2.One;
            }
            child_control.Position = (
                inner_margin +
                this.get_flow_vector(child_offset)
            );
            this.fit_control(child_control, this_size_fit);
            child_offset += this.get_flow_axis(child_control.Size);
            if(this.get_control_size_flags_flow(child_control) is
                SizeFlags.Expand or SizeFlags.ExpandFill
            ) {
                child_stretch_ratio_sum += Mathf.Max(
                    0.0f, child_control.SizeFlagsStretchRatio
                );
            }
        }
        // Finalize layout when children fit within the container
        // along its flow axis.
        if(child_offset <= this_size_flow) {
            if(child_stretch_ratio_sum > 0.0f) {
                this.apply_flow_axis_expand(
                    child_offset,
                    this_size_flow,
                    child_stretch_ratio_sum
                );
            }
            else if(this.ExpandToFit) {
                this.scale_controls_to_fit(
                    child_offset,
                    this_size_flow,
                    this_size_fit
                );
            }
            else {
                this.apply_flow_axis_alignment(
                    child_offset,
                    this_size_flow
                );
            }
        }
        // Finalize layout when children exceeded the container
        // size along its flow axis.
        else {
            if(this.ShrinkToFit) {
                this.scale_controls_to_fit(
                    child_offset,
                    this_size_flow,
                    this_size_fit
                );
            }
            else {
                this.apply_flow_axis_alignment(
                    child_offset,
                    this_size_flow
                );
            }
        }
    }
    
    /**
     * Helper to get X or width when horizontal, Y or height
     * when vertical.
     * Child controls flow along this axis.
     */
    private float get_flow_axis(Vector2 vector) {
        return this.Vertical ? vector.Y : vector.X;
    }
    
    /**
     * Helper to get Y or height when horizontal, X or width
     * when vertical.
     * Child controls are aligned or scaled to fit the
     * container on this axis.
     */
    private float get_fit_axis(Vector2 vector) {
        return this.Vertical ? vector.X : vector.Y;
    }
    
    /**
     * Helper to assign X or width when horizontal, Y or height
     * when vertical.
     */
    private Vector2 set_flow_axis(Vector2 vector, float value) {
        return (
            this.Vertical ?
            new Vector2(vector.X, value) :
            new Vector2(value, vector.Y)
        );
    }
    
    /**
     * Helper to assign Y or height when horizontal, X or width
     * when vertical.
     */
    private Vector2 set_fit_axis(Vector2 vector, float value) {
        return (
            this.Vertical ?
            new Vector2(value, vector.Y) :
            new Vector2(vector.X, value)
        );
    }
    
    /**
     * Helper to scale X or width when horizontal, Y or height
     * when vertical.
     */
    private Vector2 scale_flow_axis(Vector2 vector, float value) {
        return (
            this.Vertical ?
            new Vector2(vector.X, vector.Y * value) :
            new Vector2(vector.X * value, vector.Y)
        );
    }
    
    /**
     * Helper to scale Y or height when horizontal, X or width
     * when vertical.
     */
    private Vector2 scale_fit_axis(Vector2 vector, float value) {
        return (
            this.Vertical ?
            new Vector2(vector.X * value, vector.Y) :
            new Vector2(vector.X, vector.Y * value)
        );
    }
    
    /**
     * Helper to get a horizontal when horizontal,
     * a vertical vector when vertical.
     */
    private Vector2 get_flow_vector() {
        return this.Vertical ? Vector2.Down : Vector2.Right;
    }
    
    /**
     * Helper to get a horizontal when horizontal,
     * a vertical vector when vertical.
     */
    private Vector2 get_flow_vector(float scale) {
        return (
            this.Vertical ?
            new Vector2(0.0f, scale) :
            new Vector2(scale, 0.0f)
        );
    }
    
    /**
     * Helper to get a vertical when horizontal,
     * a horizontal vector when vertical.
     */
    private Vector2 get_fit_vector() {
        return this.Vertical ? Vector2.Right : Vector2.Down;
    }
    
    /**
     * Helper to get a vertical when horizontal,
     * a horizontal vector when vertical.
     */
    private Vector2 get_fit_vector(float scale) {
        return (
            this.Vertical ?
            new Vector2(scale, 0.0f) :
            new Vector2(0.0f, scale)
        );
    }
    
    /**
     * Helper to get a control's SizeFlags along the axis
     * that child controls flow.
     */
    private SizeFlags get_control_size_flags_flow(Control control) {
        return (
            this.Vertical ?
            control.SizeFlagsVertical :
            control.SizeFlagsHorizontal
        );
    }
    
    /**
     * Helper to get a control's SizeFlags along the axis
     * that child controls are aligned or scaled to fit.
     */
    private SizeFlags get_control_size_flags_fit(Control control) {
        return (
            this.Vertical ?
            control.SizeFlagsHorizontal :
            control.SizeFlagsVertical
        );
    }
    
    private Vector2 get_inner_margin() {
        float proportional_axis = (
            this.Vertical ?
            this.Size.X :
            this.Size.Y
        );
        return (
            this.InnerMarginPixels +
            (proportional_axis * this.InnerMarginProportional)
        );
    }
    
    private float get_separation_size(float this_size_fit) {
        return (
            this.ControlSeparationPixels +
            (this_size_fit * this.ControlSeparationProportional) +
            this.GetThemeConstant("separation") +
            (
                this.BoxContainerThemeSeparation ?
                this.GetThemeConstant("separation", "BoxContainer") :
                0
            )
        );
    }
    
    private void fit_control(Control child_control, float this_size_fit) {
        var flags = this.get_control_size_flags_fit(child_control);
        if((flags & SizeFlags.Expand) != 0) {
            this.fit_control_expand(child_control, this_size_fit);
        }
        else if((flags & SizeFlags.Fill) != 0) {
            this.fit_control_fill(child_control, this_size_fit);
        }
        else if((flags & SizeFlags.ShrinkEnd) == SizeFlags.ShrinkEnd) {
            this.fit_control_shrink_end(child_control, this_size_fit);
        }
        else if((flags & SizeFlags.ShrinkCenter) == SizeFlags.ShrinkCenter) {
            this.fit_control_shrink_center(child_control, this_size_fit);
        }
        else {
            // Treat as ShrinkBegin
            this.fit_control_shrink_begin(child_control, this_size_fit);
        }
    }
    
    private void fit_align_control(Control child_control, float this_size_fit) {
        var flags = this.get_control_size_flags_fit(child_control);
        if((flags & SizeFlags.ShrinkEnd) == SizeFlags.ShrinkEnd) {
            this.fit_control_align_end(child_control, this_size_fit);
        }
        else if((flags & SizeFlags.ShrinkCenter) == SizeFlags.ShrinkCenter) {
            this.fit_control_align_center(child_control, this_size_fit);
        }
        else {
            // All other flags align to position 0
            this.fit_control_align_begin(child_control);
        }
    }
    
    private void fit_control_fill(Control child_control, float this_size_fit) {
        Vector2 child_size_min = child_control.GetCombinedMinimumSize();
        float child_size_fit = this.get_fit_axis(child_size_min);
        float child_scale = this_size_fit / child_size_fit;
        Vector2 child_size = child_scale * child_size_min;
        child_control.Size = child_size;
    }
    
    private void fit_control_expand(Control child_control, float this_size_fit) {
        Vector2 child_size_min = child_control.GetCombinedMinimumSize();
        child_control.Size = this.set_fit_axis(child_size_min, this_size_fit);
    }
    
    private void fit_control_shrink_begin(Control child_control, float this_size_fit) {
        this.fit_control_shrink(child_control, this_size_fit);
        this.fit_control_align_begin(child_control);
    }
    
    private void fit_control_shrink_end(Control child_control, float this_size_fit) {
        this.fit_control_shrink(child_control, this_size_fit);
        this.fit_control_align_end(child_control, this_size_fit);
    }
    
    private void fit_control_shrink_center(Control child_control, float this_size_fit) {
        this.fit_control_shrink(child_control, this_size_fit);
        this.fit_control_align_center(child_control, this_size_fit);
    }
    
    private void fit_control_align_begin(Control child_control) {
        child_control.Position = this.set_fit_axis(child_control.Position, 0.0f);
    }
    
    private void fit_control_align_end(Control child_control, float this_size_fit) {
        child_control.Position = this.set_fit_axis(
            child_control.Position,
            this_size_fit - this.get_fit_axis(child_control.Size)
        );
    }
    
    private void fit_control_align_center(Control child_control, float this_size_fit) {
        child_control.Position = this.set_fit_axis(
            child_control.Position,
            0.5f * (this_size_fit - this.get_fit_axis(child_control.Size))
        );
    }
    
    private void fit_control_shrink(Control child_control, float this_size_fit) {
        Vector2 child_size_min = child_control.GetCombinedMinimumSize();
        float child_size_fit = this.get_fit_axis(child_size_min);
        float child_scale = Mathf.Min(1.0f, this_size_fit / child_size_fit);
        Vector2 child_size = child_scale * child_size_min;
        child_control.SetSize(child_size);
    }
    
    /**
     * Helper to scale controls to fully fill the container's size
     * along the flow axis.
     */
    private void scale_controls_to_fit(
        float controls_size_flow,
        float this_size_flow,
        float this_size_fit
    ) {
        float scale = this_size_flow / controls_size_flow;
        Vector2 pos_scale_vector = this.scale_flow_axis(Vector2.One, scale);
        foreach(Control child_control in this.GetControls()) {
            child_control.Size *= scale;
            child_control.Position *= pos_scale_vector;
            this.fit_align_control(child_control, this_size_fit);
        }
    }
    
    /**
     * This helper takes the amount of remaining margin in the container
     * along its flow axis and distributes it among those children with
     * an Expand flag set for its sizing along the flow axis.
     */
    private void apply_flow_axis_expand(
        float controls_size_flow,
        float this_size_flow,
        float child_stretch_ratio_sum
    ) {
        float flow_margin = this_size_flow - controls_size_flow;
        Vector2 child_pos_add_vector = new Vector2(0.0f, 0.0f);
        if(flow_margin <= 0.0f) {
            return;
        }
        foreach(Control child_control in this.GetControls()) {
            var child_size_flags = (
                this.get_control_size_flags_flow(child_control)
            );
            child_control.Position += child_pos_add_vector;
            if((child_size_flags & SizeFlags.Expand) != 0) {
                float child_ratio = Mathf.Max(
                    0.0f, child_control.SizeFlagsStretchRatio
                );
                float child_size_flow_add = (
                    flow_margin * (child_ratio / child_stretch_ratio_sum)
                );
                Vector2 child_size_flow_add_vector = (
                    this.get_flow_vector(child_size_flow_add)
                );
                if((child_size_flags & SizeFlags.Fill) != 0) {
                    child_control.Size += child_size_flow_add_vector;
                }
                child_pos_add_vector += child_size_flow_add_vector;
            }
        }
    }
    
    private void apply_flow_axis_alignment(float controls_size_flow, float this_size_flow) {
        float child_offset_add = this.FlowAlignment switch {
            BoxContainer.AlignmentMode.Center => 0.5f * (this_size_flow - controls_size_flow),
            BoxContainer.AlignmentMode.End => this_size_flow - controls_size_flow,
            _ => 0.0f,
        };
        if(child_offset_add == 0.0f) {
            return;
        }
        Vector2 pos_add_vector = this.get_flow_vector(child_offset_add);
        foreach(Control child_control in this.GetControls()) {
            child_control.Position += pos_add_vector;
        }
    }
}
