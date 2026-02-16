extends Node3D

@export var receiver: Receiver

# Called when the node enters the scene tree for the first time.
func _ready():
	print("Hello Friend")
	receiver.OnCalled()
