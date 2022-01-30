# Copyright 2008-2009  Segher Boessenkool  <segher@kernel.crashing.org>
# This code is licensed to you under the terms of the GNU GPL, version 2;
# see file COPYING or http://www.gnu.org/licenses/old-licenses/gpl-2.0.txt


	.globl _start
_start:
	
	bl __OSStopAudioSystem
	bl GXAbortFrame;
	# Set MEM2 to be able to execute custom code
	lis     4, 0x1000;	ori     4, 4, 0x0002 
	lis     3, 0x9000;	ori     3, 3, 0x1FFF
	mtspr   0x230, 3;	mtspr   0x231, 4; isync	

	# Disable interrupts, enable FP.
	mfmsr 3 ; rlwinm 3,3,0,17,15 ; ori 3,3,0x2000 ; mtmsr 3 ; isync

	# Setup stack.
	lis 1,_stack_top@ha ; addi 1,1,_stack_top@l ; li 0,0 ; stwu 0,-64(1)

	# Clear BSS.
	lis 3,__bss_start@ha ; addi 3,3,__bss_start@l
	li 4,0
	lis 5,__bss_end@ha ; addi 5,5,__bss_end@l ; sub 5,5,3
	bl memset

	# Go!
	bl main

	# If it returns, hang.  Shouldn't happen.
	b .
