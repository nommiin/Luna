if (keyboard_check(vk_left) == true) {
	h = max(h - 0.1, -2);	
} else if (keyboard_check(vk_right) == true) {
	h = min(h + 0.1, 2);
} else {
	if (h < 0) {
		h = min(h + 0.05, 0);	
	} else if (h > 0) {
		h = max(h - 0.05, 0);	
	}
}

if (keyboard_check(vk_up) == true) {
	v = max(v - 0.1, -2);	
} else if (keyboard_check(vk_down) == true) {
	v = min(v + 0.1, 2);	
} else {
	if (v < 0) {
		v = min(v + 0.05, 0);	
	} else if (v > 0) {
		v = max(v - 0.05, 0);	
	}
}


date_get_day()

x += h;
y += v;

if (keyboard_check(vk_space) == true) {
	instance_create_depth(x, y, 0, Object2);	
}
show_debug_message(1);