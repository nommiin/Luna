var h = keyboard_check(vk_right)-keyboard_check(vk_left);
var v = keyboard_check(vk_down)-keyboard_check(vk_up);
show_debug_message(string(ord("B")));
if (keyboard_check(ord("B"))) { image_blend = 0xFF0000; }
if (keyboard_check(ord("G"))) { image_blend = 0x00FF00; }
if (keyboard_check(ord("R"))) image_blend = 0x0000FF;
if (keyboard_check(ord("W"))) image_blend = 0xFFFFFF;

x += h;
y += v;

image_angle += 1;
image_angle %= 360;