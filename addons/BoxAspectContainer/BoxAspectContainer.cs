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
        IEnumerable<Node> childNodes = (
            this.Reverse ?
            ((IEnumerable<Node>) this.GetChildren()).Reverse() :
            this.GetChildren()
        );
        foreach(Node childNode in childNodes) {
            if(childNode is Control childControl) {
                if(!childControl.Visible && !this.LayoutHiddenControls) {
                    continue;
                }
                yield return childControl;
            }
        }
    }
    
    /**
     * Assign positions and sizes to child controls, according to
     * the container's layout settings.
     */
    public void Resort() {
        Vector2 innerMargin = this.getInnerMargin();
        Vector2 thisSizeLessMargin = this.Size - (2 * innerMargin);
        float thisSizeFlow = this.getFlowAxis(thisSizeLessMargin);
        float thisSizeFit = this.getFitAxis(thisSizeLessMargin);
        float sepSize = this.getSeparationSize(thisSizeFit);
        float childStretchRatioSum = 0.0f;
        float childOffset = 0.0f;
        bool firstChildControl = true;
        // Initially scale and set position of each child control.
        foreach(Control childControl in this.GetControls()) {
            if(firstChildControl) {
                firstChildControl = false;
            }
            else {
                childOffset += sepSize;
            }
            if(this.OverrideRotation) {
                childControl.Rotation = 0;
            }
            if(this.OverrideScale) {
                childControl.Scale = Vector2.One;
            }
            childControl.Position = (
                innerMargin +
                this.getFlowVector(childOffset)
            );
            this.fitControl(childControl, thisSizeFit);
            childOffset += this.getFlowAxis(childControl.Size);
            if(this.getControlSizeFlagsFlow(childControl) is
                SizeFlags.Expand or SizeFlags.ExpandFill
            ) {
                childStretchRatioSum += Mathf.Max(
                    0.0f, childControl.SizeFlagsStretchRatio
                );
            }
        }
        // Finalize layout when children fit within the container
        // along its flow axis.
        if(childOffset <= thisSizeFlow) {
            if(childStretchRatioSum > 0.0f) {
                this.applyFlowAxisExpand(
                    childOffset,
                    thisSizeFlow,
                    childStretchRatioSum
                );
            }
            else if(this.ExpandToFit) {
                this.scaleControlsToFit(
                    childOffset,
                    thisSizeFlow,
                    thisSizeFit
                );
            }
            else {
                this.applyFlowAxisAlignment(
                    childOffset,
                    thisSizeFlow
                );
            }
        }
        // Finalize layout when children exceeded the container
        // size along its flow axis.
        else {
            if(this.ShrinkToFit) {
                this.scaleControlsToFit(
                    childOffset,
                    thisSizeFlow,
                    thisSizeFit
                );
            }
            else {
                this.applyFlowAxisAlignment(
                    childOffset,
                    thisSizeFlow
                );
            }
        }
    }
    
    /**
     * Helper to get X or width when horizontal, Y or height
     * when vertical.
     * Child controls flow along this axis.
     */
    private float getFlowAxis(Vector2 vector) {
        return this.Vertical ? vector.Y : vector.X;
    }
    
    /**
     * Helper to get Y or height when horizontal, X or width
     * when vertical.
     * Child controls are aligned or scaled to fit the
     * container on this axis.
     */
    private float getFitAxis(Vector2 vector) {
        return this.Vertical ? vector.X : vector.Y;
    }
    
    /**
     * Helper to assign X or width when horizontal, Y or height
     * when vertical.
     */
    private Vector2 setFlowAxis(Vector2 vector, float value) {
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
    private Vector2 setFitAxis(Vector2 vector, float value) {
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
    private Vector2 scaleFlowAxis(Vector2 vector, float value) {
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
    private Vector2 scaleFitAxis(Vector2 vector, float value) {
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
    private Vector2 getFlowVector() {
        return this.Vertical ? Vector2.Down : Vector2.Right;
    }
    
    /**
     * Helper to get a horizontal when horizontal,
     * a vertical vector when vertical.
     */
    private Vector2 getFlowVector(float scale) {
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
    private Vector2 getFitVector() {
        return this.Vertical ? Vector2.Right : Vector2.Down;
    }
    
    /**
     * Helper to get a vertical when horizontal,
     * a horizontal vector when vertical.
     */
    private Vector2 getFitVector(float scale) {
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
    private SizeFlags getControlSizeFlagsFlow(Control control) {
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
    private SizeFlags getControlSizeFlagsFit(Control control) {
        return (
            this.Vertical ?
            control.SizeFlagsHorizontal :
            control.SizeFlagsVertical
        );
    }
    
    private Vector2 getInnerMargin() {
        float proportionalAxis = (
            this.Vertical ?
            this.Size.X :
            this.Size.Y
        );
        return (
            this.InnerMarginPixels +
            (proportionalAxis * this.InnerMarginProportional)
        );
    }
    
    private float getSeparationSize(float thisSizeFit) {
        return (
            this.ControlSeparationPixels +
            (thisSizeFit * this.ControlSeparationProportional) +
            this.GetThemeConstant("separation") +
            (
                this.BoxContainerThemeSeparation ?
                this.GetThemeConstant("separation", "BoxContainer") :
                0
            )
        );
    }
    
    private void fitControl(Control childControl, float thisSizeFit) {
        var flags = this.getControlSizeFlagsFit(childControl);
        if((flags & SizeFlags.Expand) != 0) {
            this.fitControlExpand(childControl, thisSizeFit);
        }
        else if((flags & SizeFlags.Fill) != 0) {
            this.fitControlFill(childControl, thisSizeFit);
        }
        else if((flags & SizeFlags.ShrinkEnd) == SizeFlags.ShrinkEnd) {
            this.fitControlShrinkEnd(childControl, thisSizeFit);
        }
        else if((flags & SizeFlags.ShrinkCenter) == SizeFlags.ShrinkCenter) {
            this.fitControlShrinkCenter(childControl, thisSizeFit);
        }
        else {
            // Treat as ShrinkBegin
            this.fitControlShrinkBegin(childControl, thisSizeFit);
        }
    }
    
    private void fitAlignControl(Control childControl, float thisSizeFit) {
        var flags = this.getControlSizeFlagsFit(childControl);
        if((flags & SizeFlags.ShrinkEnd) == SizeFlags.ShrinkEnd) {
            this.fitControlAlignEnd(childControl, thisSizeFit);
        }
        else if((flags & SizeFlags.ShrinkCenter) == SizeFlags.ShrinkCenter) {
            this.fitControlAlignCenter(childControl, thisSizeFit);
        }
        else {
            // All other flags align to position 0
            this.fitControlAlignBegin(childControl);
        }
    }
    
    private void fitControlFill(Control childControl, float thisSizeFit) {
        Vector2 childSizeMin = childControl.GetCombinedMinimumSize();
        float childSizeFit = this.getFitAxis(childSizeMin);
        float childScale = thisSizeFit / childSizeFit;
        Vector2 childSize = childScale * childSizeMin;
        childControl.Size = childSize;
    }
    
    private void fitControlExpand(Control childControl, float thisSizeFit) {
        Vector2 childSizeMin = childControl.GetCombinedMinimumSize();
        childControl.Size = this.setFitAxis(childSizeMin, thisSizeFit);
    }
    
    private void fitControlShrinkBegin(Control childControl, float thisSizeFit) {
        this.fitControlShrink(childControl, thisSizeFit);
        this.fitControlAlignBegin(childControl);
    }
    
    private void fitControlShrinkEnd(Control childControl, float thisSizeFit) {
        this.fitControlShrink(childControl, thisSizeFit);
        this.fitControlAlignEnd(childControl, thisSizeFit);
    }
    
    private void fitControlShrinkCenter(Control childControl, float thisSizeFit) {
        this.fitControlShrink(childControl, thisSizeFit);
        this.fitControlAlignCenter(childControl, thisSizeFit);
    }
    
    private void fitControlAlignBegin(Control childControl) {
        childControl.Position = this.setFitAxis(childControl.Position, 0.0f);
    }
    
    private void fitControlAlignEnd(Control childControl, float thisSizeFit) {
        childControl.Position = this.setFitAxis(
            childControl.Position,
            thisSizeFit - this.getFitAxis(childControl.Size)
        );
    }
    
    private void fitControlAlignCenter(Control childControl, float thisSizeFit) {
        childControl.Position = this.setFitAxis(
            childControl.Position,
            0.5f * (thisSizeFit - this.getFitAxis(childControl.Size))
        );
    }
    
    private void fitControlShrink(Control childControl, float thisSizeFit) {
        Vector2 childSizeMin = childControl.GetCombinedMinimumSize();
        float childSizeFit = this.getFitAxis(childSizeMin);
        float childScale = Mathf.Min(1.0f, thisSizeFit / childSizeFit);
        Vector2 childSize = childScale * childSizeMin;
        childControl.SetSize(childSize);
    }
    
    /**
     * Helper to scale controls to fully fill the container's size
     * along the flow axis.
     */
    private void scaleControlsToFit(
        float controlsSizeFlow,
        float thisSizeFlow,
        float thisSizeFit
    ) {
        float scale = thisSizeFlow / controlsSizeFlow;
        Vector2 posScaleVector = this.scaleFlowAxis(Vector2.One, scale);
        foreach(Control childControl in this.GetControls()) {
            childControl.Size *= scale;
            childControl.Position *= posScaleVector;
            this.fitAlignControl(childControl, thisSizeFit);
        }
    }
    
    /**
     * This helper takes the amount of remaining margin in the container
     * along its flow axis and distributes it among those children with
     * an Expand flag set for its sizing along the flow axis.
     */
    private void applyFlowAxisExpand(
        float controlsSizeFlow,
        float thisSizeFlow,
        float childStretchRatioSum
    ) {
        float flowMargin = thisSizeFlow - controlsSizeFlow;
        Vector2 childPosAddVector = new Vector2(0.0f, 0.0f);
        if(flowMargin <= 0.0f) {
            return;
        }
        foreach(Control childControl in this.GetControls()) {
            var childSizeFlags = (
                this.getControlSizeFlagsFlow(childControl)
            );
            childControl.Position += childPosAddVector;
            if((childSizeFlags & SizeFlags.Expand) != 0) {
                float childRatio = Mathf.Max(
                    0.0f, childControl.SizeFlagsStretchRatio
                );
                float childSizeFlowAdd = (
                    flowMargin * (childRatio / childStretchRatioSum)
                );
                Vector2 childSizeFlowAddVector = (
                    this.getFlowVector(childSizeFlowAdd)
                );
                if((childSizeFlags & SizeFlags.Fill) != 0) {
                    childControl.Size += childSizeFlowAddVector;
                }
                childPosAddVector += childSizeFlowAddVector;
            }
        }
    }
    
    private void applyFlowAxisAlignment(float controlsSizeFlow, float thisSizeFlow) {
        float childOffsetAdd = this.FlowAlignment switch {
            BoxContainer.AlignmentMode.Center => 0.5f * (thisSizeFlow - controlsSizeFlow),
            BoxContainer.AlignmentMode.End => thisSizeFlow - controlsSizeFlow,
            _ => 0.0f,
        };
        if(childOffsetAdd == 0.0f) {
            return;
        }
        Vector2 posAddVector = this.getFlowVector(childOffsetAdd);
        foreach(Control childControl in this.GetControls()) {
            childControl.Position += posAddVector;
        }
    }
}
