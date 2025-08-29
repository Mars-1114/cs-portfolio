#define _CRT_SECURE_NO_WARNINGS
#include <stdio.h>
#include <stdlib.h>
#include <iostream>
#include <string.h>
#include "dialogue.h"
#include "Dungeon.h"

Dungeon dungeon = Dungeon();

int main() {
	dungeon.RunGame();
	return 0;
}