Use Compression: 
	grid contracts and expands
Spin Mode: 
	amount of rotation relative to compression.
	negative values: faster compression
	positive values:  faster rotation
Kernel Angle: 
	Amount of rotation (0=no rotation)
<x> Formula: 
	x rotation around x axis
	y rotation around y axis
	z rotation around z axis
	There are two modes: 
	1. With brackets <> (see SpinLab doc!)
		< rotation around y
		> rotation around -y
		between brackets, use x and z.
		Examples: <x>, <z>, <xz>
		The rotations are in sequence:
		<xz> y, then x, then z, then -y
		All combinations are "valid" and don't break the "grid"
	2. Without brackets 
		Combination of xyz
		Back and forth "twist" around specified axis
		