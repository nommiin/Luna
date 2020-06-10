var inst = instance_create_depth(x, y, 0, Object2);
inst.test = 100;

score = 100;
show_message(game_save_id);
game_save("lol.bin");