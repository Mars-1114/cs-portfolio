#include <stdio.h>
#include <stdlib.h>
#include <iostream>
#include <string.h>
#include "dialogue.h"
#include "room.h"

Room* Generate() {
	Room* map = new Room[17];
	map[0] = { 1, 0, -1, -1, 0, 2, 2 };
	map[1] = { 2, 0, -1, -1, 1 };
	map[2] = { 15, 1, 3, -1, 2, 1 };
	map[3] = { -1, 4, 5, 2, 3 };
	map[4] = { 3, -1, -1, -1, 4 };
	map[5] = { -1, -1, 6, 3, 5 };
	map[6] = { 7, -1, -1, 5, 6 };
	map[7] = { 8, 6, -1, 9, 7 };
	map[8] = { -1, 7, -1, -1, 8 };
	map[9] = { 11, -1, 7, 10, 9, 4 };
	map[10] = { -1, -1, 9, -1, 10 };
	map[11] = { -1, 9, -1, 12, 11 };
	map[12] = { 13, -1, 11, 14, 12 };
	map[13] = { -1, 12, -1, -1, 13 };
	map[14] = { -1, 15, 12, -1, 14 };
	map[15] = { 14, 2, -1, 16, 15, 2, 4 };
	map[16] = { -1, -1, 15, -1, 16 };

	return map;
}