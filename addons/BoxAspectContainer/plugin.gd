@tool
extends EditorPlugin

func _enter_tree():
    add_custom_type(
        "BoxAspectContainer",
        "Container",
        preload("BoxAspectContainer.cs"),
        preload("BoxAspectContainer.svg")
    )

func _exit_tree():
    remove_custom_type("BoxAspectContainer")
